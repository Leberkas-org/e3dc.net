namespace E3dc.Messages;

public interface IRscpMessage;

public interface IRscpResponse : IRscpMessage
{
    string CorrelationId { get; }
}
