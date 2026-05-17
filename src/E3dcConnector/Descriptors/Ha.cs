using E3dcConnector.Messages;
using E3dcConnector.Tags;

namespace E3dcConnector.Descriptors;

public static class Ha
{
    public static readonly TagDescriptor DatapointList = new(RscpTag.HA_REQ_DATAPOINT_LIST);
    public static readonly TagDescriptor ActuatorStates = new(RscpTag.HA_REQ_ACTUATOR_STATES);
}
