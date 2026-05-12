using E3dcConnector.Tags;

namespace E3dcConnector.Messages.Descriptors;

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
}
