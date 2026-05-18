using E3dc.Messages;
using E3dc.Tags;

namespace E3dc.Descriptors;

public static class Ha
{
    public static readonly TagDescriptor DatapointList = new(RscpTag.HA_REQ_DATAPOINT_LIST);
    public static readonly TagDescriptor ActuatorStates = new(RscpTag.HA_REQ_ACTUATOR_STATES);
}
