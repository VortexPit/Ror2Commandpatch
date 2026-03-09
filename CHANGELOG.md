# Changelog

## 1.0.0
- Initial release.
- Added standalone BepInEx plugin `LookingGlass Command Fix`.
- Added Harmony runtime patch for `LookingGlass.CommandItemCount.CommandItemCountClass.SubmitChoice(...)`.
- Added safe index validation/fallback logic for mapped and original command option indices.
- Added graceful handling/logging when LookingGlass is not installed or patch targets are missing.
