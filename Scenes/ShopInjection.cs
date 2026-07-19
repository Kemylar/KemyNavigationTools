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

            if (sceneName.Contains("main menu"))
            {
                spawnedGRC = false;
                spawnedAestrin = false;
                spawnedDragonCliffs = false;
                return;
            }

            GameObject runner = GameObject.Find("ShopItemSpawnerRunner");
            if (runner == null)
            {
                runner = new GameObject("ShopItemSpawnerRunner");
                UnityEngine.Object.DontDestroyOnLoad(runner);
            }
            var component = runner.GetComponent<CoroutineRunner>() ?? runner.AddComponent<CoroutineRunner>();

            if (sceneName.Contains("gold rock") && !spawnedGRC)
            {
                // 1. Spawn Inclinometer (Slot 830)
                component.StartCoroutine(DelayedInjectionRoutine("island 1 A (gold rock) scenery", "GRC",
                    new Vector3(1520.0f, 7.52f, -380.5f), Quaternion.Euler(77.5f, 241f, 0f), 830));

                // 2. Spawn Bearing Compass (Slot 831)
                component.StartCoroutine(DelayedInjectionRoutine("island 1 A (gold rock) scenery", "GRC",
                    new Vector3(1520.4f, 7.52f, -380.5f), Quaternion.Euler(0f, 0f, 0f), 831));
            }

            if (sceneName.Contains("island 15 m (fort)") && !spawnedAestrin)
            {
                component.StartCoroutine(DelayedInjectionRoutine("island 15 M (Fort) scenery", "Aestrin",
                    new Vector3(-75.316f, 2.7f, 44.2995f), Quaternion.Euler(0f, 180f, 0f), 830));
            }

            if (sceneName.Contains("island 9 e dragon cliffs") && !spawnedDragonCliffs)
            {
                component.StartCoroutine(DelayedInjectionRoutine("island 9 E (dragon cliffs) scenery", "DragonCliffs",
                    new Vector3(-89.268f, 4.477f, -544.36f), Quaternion.Euler(80f, 230f, 0f), 830));
            }
        }

        private static IEnumerator DelayedInjectionRoutine(string parentSceneryName, string regionKey, Vector3 localPos, Quaternion localRot, int itemIndex)
        {
            yield return new WaitForSeconds(3f);

            // Fetch the target prefab instance fields tracking inside your new PrefabPatches runtime file
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