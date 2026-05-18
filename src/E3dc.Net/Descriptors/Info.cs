using E3dc.Messages;
using E3dc.Tags;

namespace E3dc.Descriptors;

public static class Info
{
    public static readonly TagDescriptor SerialNumber = new(RscpTag.INFO_REQ_SERIAL_NUMBER);
    public static readonly TagDescriptor ProductionDate = new(RscpTag.INFO_REQ_PRODUCTION_DATE);
    public static readonly TagDescriptor SwRelease = new(RscpTag.INFO_REQ_SW_RELEASE);
    public static readonly TagDescriptor IpAddress = new(RscpTag.INFO_REQ_IP_ADDRESS);
    public static readonly TagDescriptor SubnetMask = new(RscpTag.INFO_REQ_SUBNET_MASK);
    public static readonly TagDescriptor Gateway = new(RscpTag.INFO_REQ_GATEWAY);
    public static readonly TagDescriptor Dns = new(RscpTag.INFO_REQ_DNS);
    public static readonly TagDescriptor Time = new(RscpTag.INFO_REQ_TIME);
    public static readonly TagDescriptor TimeZone = new(RscpTag.INFO_REQ_TIME_ZONE);

    public static readonly TagDescriptor[] All = [SerialNumber, ProductionDate, SwRelease, IpAddress, SubnetMask, Gateway, Dns, Time, TimeZone];
}
