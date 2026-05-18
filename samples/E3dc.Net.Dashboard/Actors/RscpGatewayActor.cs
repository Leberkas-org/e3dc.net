using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Channels;
using Akka.Actor;
using Akka.Streams;
using E3dc;
using E3dc.Client;
using E3dc.Messages;
using E3dc.Messages.Responses;
using E3dc.Protocol;
using E3dc.Reactive;
using E3dc.Reactive.Internal;
using E3dc.Tags;
using E3dc.Dashboard.Configuration;
using Generated = E3dc.Dashboard.Controllers.Generated;
using D = E3dc.Descriptors;

namespace E3dc.Dashboard.Actors;

public sealed class RscpGatewayActor : ReceiveActor, IWithTimers
{
    // Internal message for correlation timeout
    private sealed record AdHocTimeout(string CorrelationId);

    // Internal message for routing responses back from the consumer loop
    private sealed record ResponseReceived(RscpDataResponse Data);

    public ITimerScheduler Timers { get; set; } = null!;

    private readonly IActorRef _snapshotActor;
    private readonly ChannelWriter<IRscpCommand> _commands;
    private readonly ConcurrentDictionary<string, PendingRequest> _pendingRequests = new();

    private sealed record PendingRequest(IActorRef Sender, Func<RscpDataResponse, object> Convert);

    public RscpGatewayActor(E3dcOptions options, IActorRef snapshotActor, IMaterializer materializer)
    {
        _snapshotActor = snapshotActor;

        var flow = RscpFlow.Create(
            () => new RscpConnection(options.Host, user: options.User, password: options.Password, encryptionKey: options.RscpKey),
            pollingRequest: null,
            new RscpFlowSettings());

        var (commands, messages) = flow.Materialize(materializer);
        _commands = commands;

        var self = Self;

        // Response consumer loop — routes responses back into the actor via Tell
        _ = Task.Run(async () =>
        {
            await foreach (var msg in messages.ReadAllAsync())
            {
                if (msg is RscpDataResponse data)
                    self.Tell(new ResponseReceived(data));
            }
        });

        Receive<ResponseReceived>(msg => HandleResponse(msg.Data));
        Receive<SendPollingCommand>(msg => HandleSendPollingCommand(msg));
        Receive<SendTagsRequest>(msg => HandleSendTagsRequest(msg));
        Receive<HistoryQueryMessage>(msg => HandleHistoryQueryMessage(msg));
        Receive<AdHocTimeout>(msg => HandleTimeout(msg));
    }

    // ── Message handlers ────────────────────────────────────────────────────

    private void HandleResponse(RscpDataResponse data)
    {
        if (_pendingRequests.TryRemove(data.CorrelationId, out var pending))
        {
            Timers.Cancel(data.CorrelationId);
            try
            {
                pending.Sender.Tell(pending.Convert(data));
            }
            catch (Exception ex)
            {
                pending.Sender.Tell(new Status.Failure(ex));
            }
            return;
        }

        // Polling response: parse and forward to SnapshotActor
        _snapshotActor.Tell(new UpdateRawDump(DumpItems(data.Items)));
        _snapshotActor.Tell(new UpdateRawItems(ItemsToJson(data.Items)));

        var info = data.ToDeviceInfo();
        if (info is not null) _snapshotActor.Tell(new UpdateDeviceInfo(info));

        var ems = data.ToEmsPowerSnapshot();
        if (ems is not null) _snapshotActor.Tell(new UpdateEms(ems));

        var bat = data.ToBatterySnapshot();
        if (bat is not null) _snapshotActor.Tell(new UpdateBat(bat));

        var pvi = data.ToInverterSnapshot();
        if (pvi is not null) _snapshotActor.Tell(new UpdatePvi(pvi));

        var pm = data.ToPowerMeterSnapshot();
        if (pm is not null) _snapshotActor.Tell(new UpdatePm(pm));

        var dcdc = data.ToDcdcSnapshot();
        if (dcdc is not null) _snapshotActor.Tell(new UpdateDcdc(dcdc));

        var ep = data.ToEmergencyPowerSnapshot();
        if (ep is not null) _snapshotActor.Tell(new UpdateEp(ep));

        var wb = data.ToWallboxSnapshot();
        if (wb is not null) _snapshotActor.Tell(new UpdateWb(wb));
    }

    private void HandleSendPollingCommand(SendPollingCommand msg)
    {
        _ = _commands.WriteAsync(msg.Command).AsTask();
    }

    private void HandleSendTagsRequest(SendTagsRequest msg)
    {
        var body = msg.Request;
        if (body?.Tags is null || body.Tags.Count == 0)
        {
            Sender.Tell(new SendTagsResponse(null, "tags required"));
            return;
        }

        var request = RscpRequest.Create();
        var hasDevice = body.DeviceNamespace is not null && body.DeviceIndex is not null;
        var tagCount = 0;

        if (hasDevice)
        {
            var device = body.DeviceNamespace!.ToUpperInvariant() switch
            {
                "BAT" => D.Bat.Device,
                "PVI" => D.Pvi.Device,
                "PM"  => D.Pm.Device,
                "WB"  => D.Wb.Device,
                _     => (DeviceDescriptor?)null
            };

            if (device is null)
            {
                Sender.Tell(new SendTagsResponse(null, $"Unknown device namespace: {body.DeviceNamespace}"));
                return;
            }

            request.FromDevice(device.Value, body.DeviceIndex!.Value, b =>
            {
                foreach (var tagStr in body.Tags)
                {
                    if (TryParseTag(tagStr, out var tag))
                    {
                        b.Read(new IndexedTag(tag));
                        tagCount++;
                    }
                }
            });
        }
        else
        {
            foreach (var tagStr in body.Tags)
            {
                if (TryParseTag(tagStr, out var tag))
                {
                    request.Read(new TagDescriptor(tag));
                    tagCount++;
                }
            }
        }

        if (tagCount == 0)
        {
            Sender.Tell(new SendTagsResponse(null, "No valid tags found"));
            return;
        }

        var correlationId = request.Options.CorrelationId;
        _pendingRequests[correlationId] = new PendingRequest(Sender, data =>
            new SendTagsResponse(new Generated.SendResponse { Items = ItemsToJson(data.Items) }, null));
        _ = _commands.WriteAsync(request).AsTask();
        Timers.StartSingleTimer(correlationId, new AdHocTimeout(correlationId), TimeSpan.FromSeconds(5));
    }

    private void HandleHistoryQueryMessage(HistoryQueryMessage msg)
    {
        var body = msg.Request;
        var raw = body.Start ?? DateTimeOffset.UtcNow.Date;
        var periodStr = body.Period?.ToString()?.ToUpperInvariant() ?? "DAY";

        // Snap start to beginning of period
        var start = periodStr switch
        {
            "WEEK"  => raw.AddDays(-(((int)raw.DayOfWeek + 6) % 7)),
            "MONTH" => new DateTimeOffset(raw.Year, raw.Month, 1, 0, 0, 0, raw.Offset),
            "YEAR"  => new DateTimeOffset(raw.Year, 1, 1, 0, 0, 0, raw.Offset),
            _       => raw,
        };

        var daysInMonth = DateTime.DaysInMonth(start.Year, start.Month);
        var (reqTag, span, interval) = periodStr switch
        {
            "WEEK"  => (RscpTag.DB_REQ_HISTORY_DATA_WEEK,  TimeSpan.FromDays(7), TimeSpan.FromDays(1)),
            "MONTH" => (RscpTag.DB_REQ_HISTORY_DATA_MONTH, TimeSpan.FromDays(daysInMonth), TimeSpan.FromDays(1)),
            "YEAR"  => (RscpTag.DB_REQ_HISTORY_DATA_YEAR,  TimeSpan.FromDays(365), TimeSpan.FromDays(31)),
            _       => (RscpTag.DB_REQ_HISTORY_DATA_DAY,   TimeSpan.FromDays(1), TimeSpan.FromMinutes(15)),
        };

        var container = RscpDataItem.CreateContainer((uint)reqTag, [
            MakeUInt64((uint)RscpTag.DB_REQ_HISTORY_TIME_START,    (ulong)start.ToUnixTimeSeconds()),
            MakeUInt64((uint)RscpTag.DB_REQ_HISTORY_TIME_INTERVAL, (ulong)interval.TotalSeconds),
            MakeUInt64((uint)RscpTag.DB_REQ_HISTORY_TIME_SPAN,     (ulong)span.TotalSeconds),
        ]);
        var request = new RawCommand([container]);

        var correlationId = request.Options.CorrelationId;
        var startStr = start.ToString("yyyy-MM-dd");
        var periodOut = body.Period?.ToString() ?? "Day";
        _pendingRequests[correlationId] = new PendingRequest(Sender, data =>
        {
            Generated.HistoryDataPoint? summary = null;
            var dataPoints = new List<Generated.HistoryDataPoint>();
            foreach (var item in data.Items)
            {
                if (item.DataType != RscpDataType.Container) continue;
                foreach (var child in item.ParseContainerChildren())
                {
                    if (child.DataType != RscpDataType.Container) continue;
                    var tag = (RscpTag)child.Tag;
                    if (tag == RscpTag.DB_SUM_CONTAINER)
                        summary = ParseDbValueContainer(child);
                    else if (tag == RscpTag.DB_VALUE_CONTAINER)
                    {
                        var dp = ParseDbValueContainer(child);
                        if (dp is not null) dataPoints.Add(dp);
                    }
                }
            }
            return new HistoryQueryResult(new Generated.HistoryQueryResponse
            {
                Period = periodOut,
                Start = startStr,
                Summary = summary,
                DataPoints = dataPoints,
                Count = dataPoints.Count,
            }, null);
        });
        _ = _commands.WriteAsync(request).AsTask();
        Timers.StartSingleTimer(correlationId, new AdHocTimeout(correlationId), TimeSpan.FromSeconds(10));
    }

    private void HandleTimeout(AdHocTimeout msg)
    {
        if (_pendingRequests.TryRemove(msg.CorrelationId, out var pending))
            pending.Sender.Tell(new Status.Failure(new TimeoutException($"RSCP request timed out (correlationId={msg.CorrelationId})")));
    }

    // ── Called by SnapshotActor when it receives a RscpDataResponse from pending requests ──
    // (Routing is already done in HandleResponse; these helpers produce the typed DTOs
    //  for ad-hoc callers that receive a RscpDataResponse and need to convert it.)

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static bool TryParseTag(string tagStr, out RscpTag tag)
    {
        if (Enum.TryParse(tagStr, true, out tag))
            return true;

        if (tagStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            && uint.TryParse(tagStr[2..], System.Globalization.NumberStyles.HexNumber, null, out var hex))
        {
            tag = (RscpTag)hex;
            return true;
        }

        tag = default;
        return false;
    }

    private static string DumpItems(IReadOnlyList<RscpDataItem> items, int indent = 0)
    {
        var sb = new StringBuilder();
        var pad = new string(' ', indent * 2);
        foreach (var item in items)
        {
            var tagName = Enum.IsDefined(typeof(RscpTag), item.Tag)
                ? ((RscpTag)item.Tag).ToString()
                : $"0x{item.Tag:X8}";
            var hex = BitConverter.ToString(item.Value.ToArray()).Replace("-", " ");
            sb.AppendLine($"{pad}{tagName}  Type={item.DataType}  Len={item.Value.Length}  Val=[{hex}]");
            if (item.DataType == RscpDataType.Container)
                sb.Append(DumpItems(item.ParseContainerChildren(), indent + 1));
        }
        return sb.ToString();
    }

    public static List<Generated.RscpItem> ItemsToJson(IReadOnlyList<RscpDataItem> items)
    {
        var result = new List<Generated.RscpItem>();
        foreach (var item in items)
        {
            var tagName = Enum.IsDefined(typeof(RscpTag), item.Tag)
                ? ((RscpTag)item.Tag).ToString()
                : $"0x{item.Tag:X8}";
            var hex = BitConverter.ToString(item.Value.ToArray()).Replace("-", " ");
            object? parsed = item.DataType switch
            {
                RscpDataType.Bool      => item.Value.Span[0] != 0,
                RscpDataType.UChar8    => item.Value.Span[0],
                RscpDataType.Char8     => (sbyte)item.Value.Span[0],
                RscpDataType.Int16     => BinaryPrimitives.ReadInt16LittleEndian(item.Value.Span),
                RscpDataType.UInt16    => BinaryPrimitives.ReadUInt16LittleEndian(item.Value.Span),
                RscpDataType.Int32     => BinaryPrimitives.ReadInt32LittleEndian(item.Value.Span),
                RscpDataType.UInt32    => BinaryPrimitives.ReadUInt32LittleEndian(item.Value.Span),
                RscpDataType.Float32   => BinaryPrimitives.ReadSingleLittleEndian(item.Value.Span),
                RscpDataType.Double64  => BinaryPrimitives.ReadDoubleLittleEndian(item.Value.Span),
                RscpDataType.CString   => Encoding.UTF8.GetString(item.Value.Span),
                RscpDataType.Timestamp when item.Value.Length >= 12 => item.ToTimestamp().ToString("o"),
                _ => null,
            };

            var entry = new Generated.RscpItem
            {
                Tag  = tagName,
                Type = item.DataType.ToString(),
                Hex  = hex,
            };
            if (parsed is not null) entry.Value = parsed;
            if (item.DataType == RscpDataType.Container)
                entry.Children = ItemsToJson(item.ParseContainerChildren());
            result.Add(entry);
        }
        return result;
    }

    public static Generated.HistoryDataPoint? ParseDbValueContainer(RscpDataItem container)
    {
        int idx = 0;
        double batIn = 0, batOut = 0, gridIn = 0, gridOut = 0, dcPow = 0, cons = 0;
        var found = false;
        foreach (var child in container.ParseContainerChildren())
        {
            switch ((RscpTag)child.Tag)
            {
                case RscpTag.DB_GRAPH_INDEX:    idx = (int)Math.Round(ReadDbDouble(child)); break;
                case RscpTag.DB_BAT_POWER_IN:   batIn = ReadDbDouble(child); found = true; break;
                case RscpTag.DB_BAT_POWER_OUT:  batOut = ReadDbDouble(child); found = true; break;
                case RscpTag.DB_DC_POWER:       dcPow = ReadDbDouble(child); found = true; break;
                case RscpTag.DB_GRID_POWER_IN:  gridIn = ReadDbDouble(child); found = true; break;
                case RscpTag.DB_GRID_POWER_OUT: gridOut = ReadDbDouble(child); found = true; break;
                case RscpTag.DB_CONSUMPTION:    cons = ReadDbDouble(child); found = true; break;
            }
        }
        if (!found) return null;
        return new Generated.HistoryDataPoint
        {
            Index       = idx,
            BatIn       = batIn,
            BatOut      = batOut,
            GridIn      = gridIn,
            GridOut     = gridOut,
            Solar       = dcPow,
            Consumption = cons,
        };
    }

    private static RscpDataItem MakeUInt64(uint tag, ulong value)
    {
        var buf = new byte[8];
        BinaryPrimitives.WriteUInt64LittleEndian(buf, value);
        return new RscpDataItem(tag, RscpDataType.UInt64, buf);
    }

    private static double ReadDbDouble(RscpDataItem item) => item.DataType switch
    {
        RscpDataType.Double64 => BinaryPrimitives.ReadDoubleLittleEndian(item.Value.Span),
        RscpDataType.Float32  => BinaryPrimitives.ReadSingleLittleEndian(item.Value.Span),
        _                     => 0,
    };

    // ── Factory ──────────────────────────────────────────────────────────────

    public static Props Props(E3dcOptions options, IActorRef snapshotActor, IMaterializer materializer)
        => Akka.Actor.Props.Create(() => new RscpGatewayActor(options, snapshotActor, materializer));
}

// ── RawCommand ───────────────────────────────────────────────────────────────

internal sealed class RawCommand(IReadOnlyList<RscpDataItem> items) : IRawItemsCommand
{
    public RscpRequestOptions Options { get; } = new();
    public IReadOnlyList<RscpDataItem> Items => items;
}
