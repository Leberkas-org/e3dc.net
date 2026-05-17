# HA Tags (Home Automation)

Namespace prefix: `0x09`

The HA namespace provides access to home automation datapoints and actuator control. Unlike other namespaces, HA uses a discovery pattern — query the datapoint list first, then interact with individual datapoints.

## Discovery

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `HA_REQ_DATAPOINT_LIST` | `0x09000001` | None | Request all registered datapoints |
| `HA_DATAPOINT_LIST` | `0x09800001` | Container | List of datapoint containers |
| `HA_DATAPOINT` | `0x09800002` | Container | Individual datapoint entry |
| `HA_DATAPOINT_INDEX` | `0x09800003` | UInt16 | Datapoint index within the list |
| `HA_DATAPOINT_STATE` | `0x09800011` | — | Current state of a datapoint |

## Actuator Control

| Tag | Hex | Type | Description |
|-----|-----|------|-------------|
| `HA_REQ_ACTUATOR_STATES` | `0x09000010` | None | Request current actuator states |
| `HA_ACTUATOR_STATES` | `0x09800010` | Container | Current actuator states |

## Usage

```csharp
// Discover all datapoints
var request = RscpRequest.Create()
    .Read(Ha.DatapointList);

// Query actuator states
var request = RscpRequest.Create()
    .Read(Ha.ActuatorStates);
```

The response to `HA_REQ_DATAPOINT_LIST` is a container of `HA_DATAPOINT` entries, each containing an index and state information. The exact structure depends on the home automation devices connected to the E3DC system.

::: tip
Not all E3DC systems have home automation devices configured. If no datapoints are registered, the response will be empty or contain an error.
:::
