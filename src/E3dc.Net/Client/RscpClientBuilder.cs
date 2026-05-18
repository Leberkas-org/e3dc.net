using Akka.Actor;
using E3dc.Messages;
using E3dc.Reactive;
using E3dc.Reactive.Internal;

namespace E3dc.Client;

public sealed class RscpClientBuilder
{
    private string _host = "localhost";
    private int _port = 5033;
    private string _user = "";
    private string _password = "";
    private string _encryptionKey = "";
    private RscpRequest? _pollingRequest;
    private RscpFlowSettings _settings = new();

    public RscpClientBuilder Connect(string host, int port = 5033)
    {
        _host = host;
        _port = port;
        return this;
    }

    public RscpClientBuilder WithCredentials(string user, string password)
    {
        _user = user;
        _password = password;
        return this;
    }

    public RscpClientBuilder WithEncryptionKey(string key)
    {
        _encryptionKey = key;
        return this;
    }

    public RscpClientBuilder WithPolling(TimeSpan interval, RscpRequest request)
    {
        _pollingRequest = request;
        _settings = _settings with { PollingInterval = interval };
        return this;
    }

    public RscpClientBuilder WithPolling(TimeSpan interval, TagDescriptor[] tags)
        => WithPolling(interval, RscpRequest.Create().Read(tags));

    public RscpClientBuilder WithReconnect(TimeSpan min, TimeSpan max)
    {
        _settings = _settings with { MinReconnectBackoff = min, MaxReconnectBackoff = max };
        return this;
    }

    public RscpClient Build(ActorSystem? actorSystem = null)
    {
        var host = _host;
        var port = _port;
        var user = _user;
        var password = _password;
        var encKey = _encryptionKey;
        var pollingRequest = _pollingRequest;
        var settings = _settings;

        return new RscpClient(
            () => RscpFlow.Create(
                () => new RscpConnection(host, port, user, password, encKey),
                pollingRequest,
                settings),
            actorSystem);
    }
}
