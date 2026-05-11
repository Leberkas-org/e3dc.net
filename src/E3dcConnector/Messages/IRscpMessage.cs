namespace E3dcConnector.Messages;

public interface IRscpMessage;

public interface IRscpResponse : IRscpMessage
{
    string CorrelationId { get; }
}
