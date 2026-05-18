using E3dc.Messages;
using E3dc.Tags;

namespace E3dc.Descriptors;

public static class Ep
{
    public static readonly TagDescriptor IsReadyForSwitch = new(RscpTag.EP_REQ_IS_READY_FOR_SWITCH);
    public static readonly TagDescriptor IsGridConnected = new(RscpTag.EP_REQ_IS_GRID_CONNECTED);
    public static readonly TagDescriptor IsIslandGrid = new(RscpTag.EP_REQ_IS_ISLAND_GRID);
}
