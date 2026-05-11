using Akka.Actor;
using E3dcConnector.Reactive;
using E3dcConnector.Reactive.Internal;
using E3dcConnector.Tags;

namespace E3dcConnector.Client;

public sealed class RscpClientBuilder
{
    private string _host = "localhost";
    private int _port = 5033;
    private string _user = "";
    private string _password = "";
    private string _encryptionKey = "";
    private RscpTag[]? _pollingTags;
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

    public RscpClientBuilder WithPolling(TimeSpan interval, RscpTag[] tags)
    {
        _pollingTags = tags;
        _settings = _settings with { PollingInterval = interval };
        return this;
    }

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
        var pollingTags = _pollingTags;
        var settings = _settings;

        return new RscpClient(
            () => RscpFlow.Create(
                () => new RscpConnection(host, port, user, password, encKey),
                pollingTags,
                settings),
            actorSystem);
    }
}
