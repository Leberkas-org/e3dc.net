# Authentication

After establishing a TCP connection and setting up encryption, the client must authenticate before sending any other requests.

## Authentication Flow

<likec4-view view-id="authFlow"></likec4-view>

## Authentication Request

Send a frame containing a single Container tag:

| Tag | Type | Value |
|-----|------|-------|
| `RSCP_REQ_AUTHENTICATION` (`0x00000001`) | Container | Contains the two items below |
| `RSCP_AUTHENTICATION_USER` (`0x00000002`) | CString | Username (from E3DC portal) |
| `RSCP_AUTHENTICATION_PASSWORD` (`0x00000003`) | CString | Password (from E3DC portal) |

## Authentication Response

The server responds with:

| Tag | Type | Value |
|-----|------|-------|
| `RSCP_AUTHENTICATION` (`0x00800001`) | UChar8 or Int32 | Authentication level |

## Auth Levels

| Level | Meaning |
|-------|---------|
| 0 | `NO_AUTH` — authentication failed |
| 10 | `USER` — standard user access |
| 20 | `INSTALL` — installer access |

If the auth level is 0, the connection should be closed and credentials checked.

## Credentials

The username and password are the same ones used to log into the E3DC portal. They can be configured on the E3DC device under Settings > User Management.

The encryption key (used for Rijndael-256, not authentication) is a separate password configured on the device under Settings > RSCP.
