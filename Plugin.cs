using System;
using System.Collections;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace LookingGlassCommandFix
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.bryndan.lookingglasscommandfix";
        public const string PluginName = "LookingGlass Command Fix";
        public const string PluginVersion = "1.0.0";

        internal static ManualLogSource Log;

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;
            _harmony = new Harmony(PluginGuid);

            try
            {
                Type targetType = AccessTools.TypeByName("LookingGlass.CommandItemCount.CommandItemCountClass");
                if (targetType == null)
                {
                    Log.LogInfo("LookingGlass type not found. No patch applied (expected if LookingGlass is not installed).");
                    return;
                }

                MethodInfo submitChoice = AccessTools.Method(targetType, "SubmitChoice");
                if (submitChoice == null)
                {
                    Log.LogWarning("LookingGlass SubmitChoice method not found. Compatibility patch was not applied.");
                    return;
                }

                MethodInfo prefix = AccessTools.Method(typeof(SubmitChoicePatch), nameof(SubmitChoicePatch.Prefix));
                _harmony.Patch(submitChoice, prefix: new HarmonyMethod(prefix));
                Log.LogInfo("Successfully applied LookingGlass Command selection compatibility patch.");
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to apply LookingGlass compatibility patch: {ex}");
            }
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }

    internal static class SubmitChoicePatch
    {
        public static bool Prefix(object __instance, Delegate orig, object self, int index)
        {
            if (self == null)
            {
                return false;
            }

            Array options = AccessTools.Field(self.GetType(), "options")?.GetValue(self) as Array;
            if (options == null)
            {
                Plugin.Log?.LogWarning("SubmitChoice patch: picker controller options were null; skipping submission.");
                return false;
            }

            int optionCount = options.Length;
            bool originalInRange = IsValidOptionIndex(index, optionCount);
            int finalIndex = index;

            try
            {
                FieldInfo optionMapField = AccessTools.Field(__instance?.GetType(), "optionMap");
                if (optionMapField != null)
                {
                    object optionMapRaw = optionMapField.GetValue(__instance);
                    if (TryGetMappedIndex(optionMapRaw, index, out int mappedIndex) && IsValidOptionIndex(mappedIndex, optionCount))
                    {
                        finalIndex = mappedIndex;
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"SubmitChoice patch: failed to read LookingGlass optionMap, falling back to original index. Error: {ex.Message}");
                finalIndex = index;
            }

            bool finalInRange = IsValidOptionIndex(finalIndex, optionCount);
            if (!finalInRange && originalInRange)
            {
                finalIndex = index;
                finalInRange = true;
            }

            if (!finalInRange)
            {
                Plugin.Log?.LogWarning($"SubmitChoice patch: blocking invalid selection. original={index}, final={finalIndex}, options={optionCount}.");
                return false;
            }

            if (orig == null)
            {
                Plugin.Log?.LogWarning("SubmitChoice patch: orig delegate was null; cannot submit selection.");
                return false;
            }

            try
            {
                orig.DynamicInvoke(self, finalIndex);
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"SubmitChoice patch: invoking original SubmitChoice delegate failed: {ex}");
            }

            return false;
        }

        private static bool TryGetMappedIndex(object optionMapRaw, int index, out int mappedIndex)
        {
            mappedIndex = default;

            if (optionMapRaw == null || index < 0)
            {
                return false;
            }

            if (optionMapRaw is IList list)
            {
                if (index >= list.Count)
                {
                    return false;
                }

                if (list[index] is int i)
                {
                    mappedIndex = i;
                    return true;
                }

                return false;
            }

            if (optionMapRaw is Array array)
            {
                if (index >= array.Length)
                {
                    return false;
                }

                if (array.GetValue(index) is int i)
                {
                    mappedIndex = i;
                    return true;
                }
            }

            return false;
        }

        private static bool IsValidOptionIndex(int i, int optionCount)
        {
            return i >= 0 && i < optionCount;
        }
    }
}
