using System.Buffers.Binary;
using System.Text;
using E3dcConnector.Messages.Responses;
using E3dcConnector.Protocol;
using E3dcConnector.Tags;
using E3dcConnector.Typed.Bat;
using E3dcConnector.Typed.Dcdc;
using E3dcConnector.Typed.Ems;
using E3dcConnector.Typed.Ep;
using E3dcConnector.Typed.Info;
using E3dcConnector.Typed.Pm;
using E3dcConnector.Typed.Pvi;
using E3dcConnector.Typed.Wb;

namespace E3dcConnector.Typed;

public static class RscpResponseExtensions
{
    public static EmsPowerSnapshot? ToEmsPowerSnapshot(this RscpDataResponse response)
    {
        int pv = 0, bat = 0, grid = 0, home = 0, add = 0;
        float soc = 0, autarky = 0, selfCons = 0;
        var found = false;

        foreach (var item in Flatten(response.Items))
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

        foreach (var item in Flatten(response.Items))
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

        foreach (var item in Flatten(response.Items))
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

    public static InverterSnapshot? ToInverterSnapshot(this RscpDataResponse response)
    {
        float acP1 = 0, acP2 = 0, acP3 = 0;
        float acV1 = 0, acV2 = 0, acV3 = 0;
        float dcP = 0, dcV = 0, dcI = 0, freq = 0;
        var found = false;

        foreach (var item in Flatten(response.Items))
        {
            switch ((RscpTag)item.Tag)
            {
                case RscpTag.PVI_AC_POWER:     acP1 = ReadValue(item); found = true; break;
                case RscpTag.PVI_AC_VOLTAGE:   acV1 = ReadValue(item); found = true; break;
                case RscpTag.PVI_DC_POWER:     dcP = ReadValue(item); found = true; break;
                case RscpTag.PVI_DC_VOLTAGE:   dcV = ReadValue(item); found = true; break;
                case RscpTag.PVI_DC_CURRENT:   dcI = ReadValue(item); found = true; break;
                case RscpTag.PVI_AC_FREQUENCY: freq = ReadValue(item); found = true; break;
            }
        }

        return found ? new InverterSnapshot(acP1, acP2, acP3, acV1, acV2, acV3, dcP, dcV, dcI, freq) : null;
    }

    public static PowerMeterSnapshot? ToPowerMeterSnapshot(this RscpDataResponse response)
    {
        float pL1 = 0, pL2 = 0, pL3 = 0;
        float vL1 = 0, vL2 = 0, vL3 = 0;
        double eL1 = 0, eL2 = 0, eL3 = 0;
        var found = false;

        foreach (var item in Flatten(response.Items))
        {
            switch ((RscpTag)item.Tag)
            {
                case RscpTag.PM_POWER_L1:   pL1 = ReadValue(item); found = true; break;
                case RscpTag.PM_POWER_L2:   pL2 = ReadValue(item); found = true; break;
                case RscpTag.PM_POWER_L3:   pL3 = ReadValue(item); found = true; break;
                case RscpTag.PM_VOLTAGE_L1: vL1 = ReadValue(item); found = true; break;
                case RscpTag.PM_VOLTAGE_L2: vL2 = ReadValue(item); found = true; break;
                case RscpTag.PM_VOLTAGE_L3: vL3 = ReadValue(item); found = true; break;
                case RscpTag.PM_ENERGY_L1:  eL1 = ReadValue(item); found = true; break;
                case RscpTag.PM_ENERGY_L2:  eL2 = ReadValue(item); found = true; break;
                case RscpTag.PM_ENERGY_L3:  eL3 = ReadValue(item); found = true; break;
            }
        }

        return found ? new PowerMeterSnapshot(pL1, pL2, pL3, vL1, vL2, vL3, eL1, eL2, eL3) : null;
    }

    public static DcdcSnapshot? ToDcdcSnapshot(this RscpDataResponse response)
    {
        float iBat = 0, uBat = 0, pBat = 0;
        var found = false;

        foreach (var item in Flatten(response.Items))
        {
            switch ((RscpTag)item.Tag)
            {
                case RscpTag.DCDC_I_BAT: iBat = ReadValue(item); found = true; break;
                case RscpTag.DCDC_U_BAT: uBat = ReadValue(item); found = true; break;
                case RscpTag.DCDC_P_BAT: pBat = ReadValue(item); found = true; break;
            }
        }

        return found ? new DcdcSnapshot(iBat, uBat, pBat) : null;
    }

    public static EmergencyPowerSnapshot? ToEmergencyPowerSnapshot(this RscpDataResponse response)
    {
        bool ready = false, grid = false, island = false;
        var found = false;

        foreach (var item in Flatten(response.Items))
        {
            switch ((RscpTag)item.Tag)
            {
                case RscpTag.EP_IS_READY_FOR_SWITCH: ready = item.Value.Span[0] != 0; found = true; break;
                case RscpTag.EP_IS_GRID_CONNECTED:   grid = item.Value.Span[0] != 0; found = true; break;
                case RscpTag.EP_IS_ISLAND_GRID:      island = item.Value.Span[0] != 0; found = true; break;
            }
        }

        return found ? new EmergencyPowerSnapshot(ready, grid, island) : null;
    }

    public static WallboxSnapshot? ToWallboxSnapshot(this RscpDataResponse response)
    {
        double energyAll = 0, energySolar = 0;
        int status = 0, errorCode = 0, mode = 0;
        float pL1 = 0, pL2 = 0, pL3 = 0;
        var found = false;

        foreach (var item in Flatten(response.Items))
        {
            switch ((RscpTag)item.Tag)
            {
                case RscpTag.WB_ENERGY_ALL:   energyAll = ReadDouble(item); found = true; break;
                case RscpTag.WB_ENERGY_SOLAR: energySolar = ReadDouble(item); found = true; break;
                case RscpTag.WB_STATUS:       status = ReadInt32(item); found = true; break;
                case RscpTag.WB_ERROR_CODE:   errorCode = ReadInt32(item); found = true; break;
                case RscpTag.WB_MODE:         mode = ReadInt32(item); found = true; break;
                case RscpTag.WB_PM_POWER_L1:  pL1 = ReadValue(item); found = true; break;
                case RscpTag.WB_PM_POWER_L2:  pL2 = ReadValue(item); found = true; break;
                case RscpTag.WB_PM_POWER_L3:  pL3 = ReadValue(item); found = true; break;
            }
        }

        return found ? new WallboxSnapshot(energyAll, energySolar, status, errorCode, mode, pL1, pL2, pL3) : null;
    }

    // Reads a value from an item that may be a direct scalar or a container wrapping {INDEX, VALUE}
    private static float ReadValue(RscpDataItem item)
    {
        if (item.DataType == RscpDataType.Container)
            return ExtractContainerFloat(item) ?? 0;
        return ReadFloat(item);
    }

    private static float? ExtractContainerFloat(RscpDataItem container)
    {
        foreach (var child in container.ParseContainerChildren())
        {
            if (child.DataType is RscpDataType.Float32 or RscpDataType.Double64
                or RscpDataType.Int32 or RscpDataType.UInt32)
                return ReadFloat(child);
        }
        return null;
    }

    private static IEnumerable<RscpDataItem> Flatten(IReadOnlyList<RscpDataItem> items)
    {
        foreach (var item in items)
        {
            yield return item;
            if (item.DataType == RscpDataType.Container)
            {
                foreach (var child in Flatten(item.ParseContainerChildren()))
                    yield return child;
            }
        }
    }

    private static int ReadInt32(RscpDataItem item) => item.DataType switch
    {
        RscpDataType.Int32  => BinaryPrimitives.ReadInt32LittleEndian(item.Value.Span),
        RscpDataType.UInt32 => (int)BinaryPrimitives.ReadUInt32LittleEndian(item.Value.Span),
        RscpDataType.Int16  => BinaryPrimitives.ReadInt16LittleEndian(item.Value.Span),
        RscpDataType.UInt16 => BinaryPrimitives.ReadUInt16LittleEndian(item.Value.Span),
        RscpDataType.UChar8 => item.Value.Span[0],
        RscpDataType.Char8  => (sbyte)item.Value.Span[0],
        _ => BinaryPrimitives.ReadInt32LittleEndian(item.Value.Span),
    };

    private static float ReadFloat(RscpDataItem item) => item.DataType switch
    {
        RscpDataType.Float32 => BinaryPrimitives.ReadSingleLittleEndian(item.Value.Span),
        RscpDataType.Double64 => (float)BinaryPrimitives.ReadDoubleLittleEndian(item.Value.Span),
        RscpDataType.Int32   => BinaryPrimitives.ReadInt32LittleEndian(item.Value.Span),
        RscpDataType.UChar8  => item.Value.Span[0],
        RscpDataType.UInt16  => BinaryPrimitives.ReadUInt16LittleEndian(item.Value.Span),
        _ => BinaryPrimitives.ReadSingleLittleEndian(item.Value.Span),
    };

    private static double ReadDouble(RscpDataItem item) => item.DataType switch
    {
        RscpDataType.Double64 => BinaryPrimitives.ReadDoubleLittleEndian(item.Value.Span),
        RscpDataType.Float32  => BinaryPrimitives.ReadSingleLittleEndian(item.Value.Span),
        RscpDataType.Int32    => BinaryPrimitives.ReadInt32LittleEndian(item.Value.Span),
        _ => BinaryPrimitives.ReadDoubleLittleEndian(item.Value.Span),
    };

    private static string ReadString(RscpDataItem item)
        => Encoding.UTF8.GetString(item.Value.Span);
}
