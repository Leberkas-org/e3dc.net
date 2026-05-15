using E3dcConnector.Messages;
using E3dcConnector.Tags;

namespace E3dcConnector.Descriptors;

public static class Ep
{
    public static readonly TagDescriptor IsReadyForSwitch = new(RscpTag.EP_REQ_IS_READY_FOR_SWITCH);
    public static readonly TagDescriptor IsGridConnected = new(RscpTag.EP_REQ_IS_GRID_CONNECTED);
    public static readonly TagDescriptor IsIslandGrid = new(RscpTag.EP_REQ_IS_ISLAND_GRID);
}
