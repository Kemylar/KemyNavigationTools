using HarmonyLib;
using UnityEngine;

namespace KemyNavTools
{
    [HarmonyPatch(typeof(IslandStreetlightsManager), "Awake")]
    public static class StreetlightManagerSpawnPatch
    {
        [HarmonyPostfix]
        public static void Postfix(IslandStreetlightsManager __instance)
        {
            if (__instance == null) return;

            // Get the root scenery parent for the island
            GameObject sceneryRoot = __instance.transform.root.gameObject;
            string rootName = sceneryRoot.name.ToLower();

            if (rootName.Contains("gold rock"))
            {
                ShopInjection.TrySpawnGRC(sceneryRoot);
            }
            else if (rootName.Contains("island 15 m (fort)"))
            {
                ShopInjection.TrySpawnFortAestrin(sceneryRoot);
            }
            else if (rootName.Contains("island 9 e (dragon cliffs)"))
            {
                ShopInjection.TrySpawnDragonCliffs(sceneryRoot);
            }
        }
    }
}