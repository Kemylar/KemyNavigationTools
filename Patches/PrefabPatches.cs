using HarmonyLib;
using System;
using UnityEngine;

namespace KemyNavTools
{
    [HarmonyPatch(typeof(PrefabsDirectory), "PopulateShipItems")]
    public static class PreloadDirectoryPatch
    {
        private const int INCLINOMETER_INDEX = 830;
        private const int COMPASS_INDEX = 831;

        private static GameObject inclinometerPrefab;
        private static GameObject compassPrefab;

        public static GameObject InclinometerPrefabRef => inclinometerPrefab;
        public static GameObject CompassPrefabRef => compassPrefab;

        [HarmonyPrefix]
        public static void Prefix(PrefabsDirectory __instance)
        {
            if (__instance.directory == null) return;

            try
            {
                if (__instance.directory.Length <= COMPASS_INDEX)
                {
                    Array.Resize(ref __instance.directory, COMPASS_INDEX + 10);
                }
            }
            catch (Exception ex)
            {
                InclinometerPlugin.DiagLogger.LogError($"[NAV SUITE] Prefix array resize failed: {ex}");
            }
        }

        [HarmonyPostfix]
        public static void Postfix(PrefabsDirectory __instance)
        {
            if (InclinometerPlugin.MainAssetBundle == null) return;

            try
            {
                // 1. Inclinometer Injection
                if (inclinometerPrefab == null)
                {
                    inclinometerPrefab = InclinometerPlugin.MainAssetBundle.LoadAsset<GameObject>("Inclinometer");
                    InclinometerSetup.Configure(inclinometerPrefab);
                }
                __instance.directory[INCLINOMETER_INDEX] = inclinometerPrefab;

                // 2. Reverted Bearing Compass Injection (Back to original asset name)
                if (compassPrefab == null)
                {
                    compassPrefab = InclinometerPlugin.MainAssetBundle.LoadAsset<GameObject>("BearingCompass");

                    if (compassPrefab != null)
                    {
                        CompassSetup.Configure(compassPrefab);
                    }
                    else
                    {
                        InclinometerPlugin.DiagLogger.LogError("[NAV SUITE] Critical Error: BearingCompass prefab not found in bundle!");
                    }
                }
                __instance.directory[COMPASS_INDEX] = compassPrefab;
            }
            catch (Exception ex)
            {
                InclinometerPlugin.DiagLogger.LogError($"[NAV SUITE] Postfix directory mapping failed: {ex}");
            }
        }
    }
}