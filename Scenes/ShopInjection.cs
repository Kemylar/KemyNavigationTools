using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KemyNavTools
{
    public static class ShopInjection
    {
        private static bool spawnedGRC = false;
        private static bool spawnedAestrin = false;
        private static bool spawnedDragonCliffs = false;

        public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            string sceneName = scene.name.ToLower();

            // Reset spawn flags when returning to the main menu
            if (sceneName.Contains("main menu"))
            {
                spawnedGRC = false;
                spawnedAestrin = false;
                spawnedDragonCliffs = false;
                return;
            }

            // Ensure runner component is active for coroutine context execution
            GameObject runner = GameObject.Find("ShopItemSpawnerRunner");
            if (runner == null)
            {
                runner = new GameObject("ShopItemSpawnerRunner");
                UnityEngine.Object.DontDestroyOnLoad(runner);
            }
            var component = runner.GetComponent<CoroutineRunner>() ?? runner.AddComponent<CoroutineRunner>();

            // Handle injection routing for Gold Rock City merchant stall
            if (sceneName.Contains("gold rock") && !spawnedGRC)
            {
                // Gold Rock City: Inclinometer Spawner Configuration
                component.StartCoroutine(DelayedInjectionRoutine("island 1 A (gold rock) scenery", "GRC",
                    new Vector3(1520.0f, 7.52f, -380.5f), Quaternion.Euler(77.5f, 241f, 0f), 830));

                // Gold Rock City: Bearing Compass Spawner Configuration
                component.StartCoroutine(DelayedInjectionRoutine("island 1 A (gold rock) scenery", "GRC",
                    new Vector3(1522.568f, 7.520f, -383.791f), Quaternion.Euler(343.5f, 237.5f, 0.0f), 831));
            }

            // Handle injection routing for Fort Aestrin merchant stall
            if (sceneName.Contains("island 15 m (fort)") && !spawnedAestrin)
            {
                // Fort Aestrin: Bearing Compass Spawner Configuration
                component.StartCoroutine(DelayedInjectionRoutine("island 15 M (Fort) scenery", "FA",
                    new Vector3(-76.904f, 2.870f, 44.645f), Quaternion.Euler(271.0f, 180.0f, 0.0f), 831));

                // Fort Aestrin: Inclinometer Spawner Configuration
                component.StartCoroutine(DelayedInjectionRoutine("island 15 M (Fort) scenery", "FA",
                    new Vector3(-73.704f, 2.870f, 44.620f), Quaternion.Euler(0.0f, 180.0f, 0.0f), 830));
            }

            // Handle injection routing for Dragon Cliffs merchant stall
            if (sceneName.Contains("island 9 e dragon cliffs") && !spawnedDragonCliffs)
            {
                // Dragon Cliffs: Bearing Compass Spawner Configuration
                component.StartCoroutine(DelayedInjectionRoutine("island 9 E (dragon cliffs) scenery", "DC",
                    new Vector3(-91.312f, 5.272f, -541.350f), Quaternion.Euler(89.0f, 44.6f, 0.0f), 831));

                // Dragon Cliffs: Inclinometer Spawner Configuration
                component.StartCoroutine(DelayedInjectionRoutine("island 9 E (dragon cliffs) scenery", "DC",
                    new Vector3(-91.329f, 4.762f, -541.365f), Quaternion.Euler(0.0f, 224.6f, 0.0f), 830));
            }
        }

        private static IEnumerator DelayedInjectionRoutine(string parentSceneryName, string regionKey, Vector3 localPos, Quaternion localRot, int itemIndex)
        {
            yield return new WaitForSeconds(3f);

            // Fetch target runtime asset from preload caching context
            GameObject targetPrefab = (itemIndex == 830)
                ? PreloadDirectoryPatch.InclinometerPrefabRef
                : PreloadDirectoryPatch.CompassPrefabRef;

            if ((regionKey == "GRC" && spawnedGRC) ||
                (regionKey == "Aestrin" && spawnedAestrin) ||
                (regionKey == "DragonCliffs" && spawnedDragonCliffs) ||
                targetPrefab == null)
            {
                yield break;
            }

            var sceneryRoot = GameObject.Find(parentSceneryName);
            if (sceneryRoot == null) yield break;

            try
            {
                string itemLabel = (itemIndex == 830) ? "inclinometer" : "compass";
                GameObject spawnerNode = new GameObject($"shop item spawner ({itemLabel} {regionKey})");
                spawnerNode.transform.parent = sceneryRoot.transform;
                spawnerNode.transform.localPosition = localPos;
                spawnerNode.transform.localRotation = localRot;

                var masterFilter = targetPrefab.GetComponent<MeshFilter>() ?? targetPrefab.GetComponentInChildren<MeshFilter>();
                if (masterFilter != null)
                {
                    var filter = spawnerNode.AddComponent<MeshFilter>();
                    filter.mesh = masterFilter.mesh;
                    spawnerNode.AddComponent<MeshRenderer>();
                }

                var nativeSpawner = spawnerNode.AddComponent<ShopItemSpawner>();
                nativeSpawner.itemPrefab = targetPrefab;

                if (itemIndex == 831 || regionKey != "GRC")
                {
                    switch (regionKey)
                    {
                        case "GRC": spawnedGRC = true; break;
                        case "Aestrin": spawnedAestrin = true; break;
                        case "DragonCliffs": spawnedDragonCliffs = true; break;
                    }
                }

                InclinometerPlugin.DiagLogger.LogInfo($"[NAV SUITE] {itemLabel} spawner cleanly deployed at {regionKey}: {localPos}");
            }
            catch (Exception ex)
            {
                InclinometerPlugin.DiagLogger.LogError($"[NAV SUITE] Failed injection for {regionKey} ({itemIndex}): {ex}");
            }
        }
    }

    public class CoroutineRunner : MonoBehaviour { }
}
