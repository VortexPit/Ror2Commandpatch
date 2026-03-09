# LookingGlassCommandFix

A small standalone BepInEx compatibility patch for **Risk of Rain 2** that fixes command essence selections when using **LookingGlass** together with **CommandQueueExtended**.

## Thunderstore Summary
Compatibility fix for LookingGlass + CommandQueueExtended command selections (prevents invalid index remap from dropping rewards).

## What this fixes
When Artifact of Command selection is processed through queued command paths, LookingGlass can receive a stale/mismatched index and attempt to remap it through a private `optionMap`. If that remap result is invalid, the item selection fails and no reward is granted.

This plugin patches LookingGlass at runtime and safely validates both mapped and original indices before submission.

## Who this is for
Use this mod if you:
- play Risk of Rain 2 with **LookingGlass**;
- use **CommandQueueExtended**;
- observe command essence/item choice failures tied to LookingGlass `SubmitChoice` logic.

## Dependencies
- BepInExPack (for Risk of Rain 2)
- Harmony (0Harmony)
- LookingGlass (target being patched)
- CommandQueueExtended (the queueing behavior this patch is intended to keep compatible)

## Installation (r2modman)
1. Build `LookingGlassCommandFix.dll`.
2. In your profile path, create (if needed):
   `C:\Users\SwanerPc\AppData\Roaming\r2modmanPlus-local\RiskOfRain2\profiles\Vannilaplus\BepInEx\plugins\LookingGlassCommandFix`
3. Place `LookingGlassCommandFix.dll` in that folder.
4. Launch the game from r2modman.
5. Check BepInEx logs for plugin startup and patch status messages.

## Manual Thunderstore package contents
Your release zip should contain:
- `plugins/LookingGlassCommandFix.dll`
- `manifest.json`
- `README.md`
- `CHANGELOG.md`
- `icon.png` (optional but recommended)

## Notes
- This patch does **not** modify `LookingGlass.dll` directly.
- If LookingGlass is absent, the plugin exits gracefully and logs an informational message.
- The patch favors normal behavior and only blocks submission when no valid index exists.
