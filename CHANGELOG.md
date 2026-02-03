# Changelog

All notable changes to this project will be documented in this file.

## [1.3.0] - 2026-02-03
### Added
- `Log Mode (PlayersOnly/AdminsOnly/Everyone)` setting to control who gets logged.
  - `PlayersOnly` – only non-admin players.
  - `AdminsOnly` – only staff (OwnerID / ModeratorID).
  - `Everyone` – both players and staff.
- Config validation and normalization for `LogMode`.

### Improved
- Config handling for ignored commands (deduplication, trimming, case-insensitive).

---

## [1.2.1] - 2026-02-02
### Fixed
- Duplicated default entries in `Ignored Commands` after multiple reloads.
- Now deduplicates ignored commands and seeds defaults only when needed.

---

## [1.2.0] - 2026-02-02
### Added
- `Wipe Data File On Server Wipe` option.
- Automatically clears the data file when `OnNewSave` (server wipe) occurs.

---

## [1.1.0] - 2026-02-02
### Added
- `Ignored Commands (without /)` config list.
- Default ignored commands:
  - `mymini`
  - `nomini`
  - `fmini`

---

## [1.0.0] - 2026-02-02
### Initial
- Initial release of CommandChatLogger.
- Logs player chat `/commands` to a JSON data file with basic enable/disable toggle.
