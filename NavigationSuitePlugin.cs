using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using KemyNavTools;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KemyNavTools

{
    [BepInPlugin("com.larsonlogistics.sailwind.navsuite", "Navigation Suite Mod", "1.0.0")]
    public class InclinometerPlugin : BaseUnityPlugin
    {
        public static ManualLogSource DiagLogger;
        public static AssetBundle MainAssetBundle;
        private static readonly string assetBundleName = "navigation_suite_assets";

        private void Awake()
        {
            DiagLogger = Logger;
            DiagLogger.LogInfo("[NAV SUITE] Initialization lifecycle activated.");

            string dllPath = Assembly.GetExecutingAssembly().Location;
            string modFolder = Path.GetDirectoryName(dllPath);
            string bundlePath = Path.Combine(modFolder, assetBundleName);

            if (File.Exists(bundlePath))
            {
                MainAssetBundle = AssetBundle.LoadFromFile(bundlePath);
                DiagLogger.LogInfo("[NAV SUITE] Asset bundle successfully loaded.");
            }
            else
            {
                DiagLogger.LogError($"[NAV SUITE] Critical Error: Asset bundle missing at {bundlePath}");
            }

            var harmony = new Harmony("com.larsonlogistics.sailwind.navsuite");
            harmony.PatchAll();

            SceneManager.sceneLoaded += ShopInjection.OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (MainAssetBundle != null)
            {
                MainAssetBundle.Unload(false);
            }
        }
    }
}