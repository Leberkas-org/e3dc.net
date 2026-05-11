using System.Buffers.Binary;
using System.Text;
using E3dcConnector.Messages.Responses;
using E3dcConnector.Protocol;
using E3dcConnector.Tags;
using E3dcConnector.Typed.Bat;
using E3dcConnector.Typed.Ems;
using E3dcConnector.Typed.Info;

namespace E3dcConnector.Typed;

public static class RscpResponseExtensions
{
    public static EmsPowerSnapshot? ToEmsPowerSnapshot(this RscpDataResponse response)
    {
        int pv = 0, bat = 0, grid = 0, home = 0, add = 0;
        float soc = 0, autarky = 0, selfCons = 0;
        var found = false;

        foreach (var item in response.Items)
        {
            switch ((RscpTag)item.Tag)
            {
                case RscpTag.EMS_POWER_PV:         pv = ReadInt32(item); found = true; break;
                case RscpTag.EMS_POWER_BAT:        bat = ReadInt32(item); found = true; break;
                case RscpTag.EMS_POWER_GRID:       grid = ReadInt32(item); found = true; break;
                case RscpTag.EMS_POWER_HOME:       home = ReadInt32(item); found = true; break;
                case RscpTag.EMS_POWER_ADD:        add = ReadInt32(item); found = true; break;
                case RscpTag.EMS_BAT_SOC:          soc = ReadFloat(item); found = true; break;
                case RscpTag.EMS_AUTARKY:          autarky = ReadFloat(item); found = true; break;
                case RscpTag.EMS_SELF_CONSUMPTION: selfCons = ReadFloat(item); found = true; break;
            }
        }

        return found ? new EmsPowerSnapshot(pv, bat, grid, home, add, soc, autarky, selfCons) : null;
    }

    public static BatterySnapshot? ToBatterySnapshot(this RscpDataResponse response)
    {
        float rsoc = 0, voltage = 0, current = 0;
        int cycles = 0, status = 0, error = 0;
        var found = false;

        foreach (var item in response.Items)
        {
            switch ((RscpTag)item.Tag)
            {
                case RscpTag.BAT_RSOC:          rsoc = ReadFloat(item); found = true; break;
                case RscpTag.BAT_MODULE_VOLTAGE: voltage = ReadFloat(item); found = true; break;
                case RscpTag.BAT_CURRENT:        current = ReadFloat(item); found = true; break;
                case RscpTag.BAT_CHARGE_CYCLES:  cycles = ReadInt32(item); found = true; break;
                case RscpTag.BAT_STATUS_CODE:    status = ReadInt32(item); found = true; break;
                case RscpTag.BAT_ERROR_CODE:     error = ReadInt32(item); found = true; break;
            }
        }

        return found ? new BatterySnapshot(rsoc, voltage, current, cycles, status, error) : null;
    }

    public static DeviceInfo? ToDeviceInfo(this RscpDataResponse response)
    {
        string serial = "", prod = "", sw = "", ip = "", mask = "", gw = "";
        var found = false;

        foreach (var item in response.Items)
        {
            switch ((RscpTag)item.Tag)
            {
                case RscpTag.INFO_SERIAL_NUMBER:   serial = ReadString(item); found = true; break;
                case RscpTag.INFO_PRODUCTION_DATE: prod = ReadString(item); found = true; break;
                case RscpTag.INFO_SW_RELEASE:      sw = ReadString(item); found = true; break;
                case RscpTag.INFO_IP_ADDRESS:      ip = ReadString(item); found = true; break;
                case RscpTag.INFO_SUBNET_MASK:     mask = ReadString(item); found = true; break;
                case RscpTag.INFO_GATEWAY:         gw = ReadString(item); found = true; break;
            }
        }

        return found ? new DeviceInfo(serial, prod, sw, ip, mask, gw) : null;
    }

    private static int ReadInt32(RscpDataItem item)
        => BinaryPrimitives.ReadInt32LittleEndian(item.Value.Span);

    private static float ReadFloat(RscpDataItem item)
        => BinaryPrimitives.ReadSingleLittleEndian(item.Value.Span);

    private static string ReadString(RscpDataItem item)
        => Encoding.UTF8.GetString(item.Value.Span);
}
