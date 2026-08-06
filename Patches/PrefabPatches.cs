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
        private const int BINNACLE_INDEX = 832; // Added slot for the test piece

        private static GameObject inclinometerPrefab;
        private static GameObject compassPrefab;
        private static GameObject binnaclePrefab; // Added reference

        public static GameObject InclinometerPrefabRef => inclinometerPrefab;
        public static GameObject CompassPrefabRef => compassPrefab;
        public static GameObject BinnaclePrefabRef => binnaclePrefab; // Added reference

        [HarmonyPrefix]
        public static void Prefix(PrefabsDirectory __instance)
        {
            if (__instance.directory == null) return;

            try
            {
                // Resizing the array safely to accommodate the new index 832
                if (__instance.directory.Length <= BINNACLE_INDEX)
                {
                    Array.Resize(ref __instance.directory, BINNACLE_INDEX + 10);
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

                // 2. Bearing Compass Injection
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

                // 3. Binnacle Injection
                if (binnaclePrefab == null)
                {
                    binnaclePrefab = InclinometerPlugin.MainAssetBundle.LoadAsset<GameObject>("Binnacle");
                    if (binnaclePrefab != null)
                    {
                        BinnacleSetup.Configure(binnaclePrefab);
                    }
                    else
                    {
                        InclinometerPlugin.DiagLogger.LogError("[NAV SUITE] Critical Error: Binnacle prefab not found in bundle!");
                    }
                }
                __instance.directory[BINNACLE_INDEX] = binnaclePrefab;
            }
            catch (Exception ex)
            {
                InclinometerPlugin.DiagLogger.LogError($"[NAV SUITE] Postfix directory mapping failed: {ex}");
            }
        }
    }
}