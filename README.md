# Rust-Command-Chat-Logger
Rust uMod/Oxide plugin that logs player chat /commands to a data file with filters, staff-only mode, and automatic wipe cleanup.

It supports:

- Logging **only players**, **only staff**, or **everyone**
- Ignoring spammy/irrelevant commands (e.g. `/mymini`)
- Automatically wiping the log file on server wipes
- Low performance impact and simple configuration

---

## Features

- ✅ Logs any chat message starting with `/` (e.g. `/vanish`, `/tpm`, `/kit`)
- ✅ Flexible log mode:
  - `PlayersOnly` – only non-admin players
  - `AdminsOnly` – only staff (OwnerID / ModeratorID in `users.cfg`)
  - `Everyone` – both players and staff
- ✅ Configurable **ignored commands** so spammy commands don’t clutter logs
- ✅ Option to **wipe logs automatically** on server wipe (`OnNewSave`)
- ✅ Stores logs in a structured JSON data file under `oxide/data`
- ✅ Designed to be safe and very low overhead

---

## Requirements

- Rust server using uMod/Oxide
- .cs plugin support (standard for Oxide/uMod)

---

## Installation

1. Download or copy `CommandChatLogger.cs`.
2. Place the file in your server’s: oxide/plugins/

## Reload the plugin via console or RCON
Type oxide.reload CommandChatLogger

## The configuration file will be generated at:
oxide/config/CommandChatLogger.json

## The data file (logs) will be stored at:
oxide/data/CommandChatLogger.json

## Performance Considerations
Hooks into OnPlayerChat, which is common and lightweight.
- Only processes messages starting with /.
- Ignores non-players, console, and bots.
- Command logging involves:
  - A few string operations and list lookups.
  - Writing the data list back to a JSON data file.
- The plugin is designed to be very low overhead for typical server use.
- Log data can be automatically wiped each server wipe to prevent unbounded growth.
