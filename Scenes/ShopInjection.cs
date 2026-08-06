using System;
using UnityEngine;

namespace KemyNavTools
{
    public static class ShopInjection
    {
        private static bool spawnedGRC = false;
        private static bool spawnedAestrin = false;
        private static bool spawnedDragonCliffs = false;

        public static void TrySpawnGRC(GameObject sceneryRoot)
        {
            if (spawnedGRC || sceneryRoot == null) return;

            try
            {
                // Inclinometer (830)
                DeploySpawner(sceneryRoot, "GRC_Inclinometer", PreloadDirectoryPatch.InclinometerPrefabRef,
                    new Vector3(1520.0f, 7.52f, -380.5f), Quaternion.Euler(77.5f, 241f, 0f));

                // Compass (831)
                DeploySpawner(sceneryRoot, "GRC_Compass", PreloadDirectoryPatch.CompassPrefabRef,
                    new Vector3(1522.568f, 7.520f, -383.791f), Quaternion.Euler(343.5f, 237.5f, 0.0f));

                // Binnacle (832)
                DeploySpawner(sceneryRoot, "GRC_Binnacle", PreloadDirectoryPatch.BinnaclePrefabRef,
                    new Vector3(1521.5f, 5.55f, -378.0f), Quaternion.Euler(0f, -90f, 0f));

                spawnedGRC = true;
                InclinometerPlugin.DiagLogger.LogInfo("[NAV SUITE] GRC shop spawners cleanly deployed via IslandStreetlightsManager.");
            }
            catch (Exception ex)
            {
                InclinometerPlugin.DiagLogger.LogError($"[NAV SUITE] Failed GRC injection: {ex}");
            }
        }

        public static void TrySpawnFortAestrin(GameObject sceneryRoot)
        {
            if (spawnedAestrin || sceneryRoot == null) return;

            try
            {
                // Compass (831)
                DeploySpawner(sceneryRoot, "FA_Compass", PreloadDirectoryPatch.CompassPrefabRef,
                    new Vector3(-76.904f, 2.870f, 44.645f), Quaternion.Euler(271.0f, 180.0f, 0.0f));

                // Inclinometer (830)
                DeploySpawner(sceneryRoot, "FA_Inclinometer", PreloadDirectoryPatch.InclinometerPrefabRef,
                    new Vector3(-73.704f, 2.870f, 44.620f), Quaternion.Euler(0.0f, 180.0f, 0.0f));

                // Binnacle (832)
                DeploySpawner(sceneryRoot, "FA_Binnacle", PreloadDirectoryPatch.BinnaclePrefabRef,
                    new Vector3(-72.504f, 2.170f, 44.820f), Quaternion.Euler(0.0f, 135.0f, 0.0f));

                spawnedAestrin = true;
                InclinometerPlugin.DiagLogger.LogInfo("[NAV SUITE] Fort Aestrin shop spawners cleanly deployed via IslandStreetlightsManager.");
            }
            catch (Exception ex)
            {
                InclinometerPlugin.DiagLogger.LogError($"[NAV SUITE] Failed Fort Aestrin injection: {ex}");
            }
        }

        public static void TrySpawnDragonCliffs(GameObject sceneryRoot)
        {
            if (spawnedDragonCliffs || sceneryRoot == null) return;

            try
            {
                // Compass (831)
                DeploySpawner(sceneryRoot, "DC_Compass", PreloadDirectoryPatch.CompassPrefabRef,
                    new Vector3(-91.312f, 5.272f, -541.350f), Quaternion.Euler(89.0f, 44.6f, 0.0f));

                // Inclinometer (830)
                DeploySpawner(sceneryRoot, "DC_Inclinometer", PreloadDirectoryPatch.InclinometerPrefabRef,
                    new Vector3(-91.329f, 4.762f, -541.365f), Quaternion.Euler(0.0f, 224.6f, 0.0f));

                // Binnacle (832)
                DeploySpawner(sceneryRoot, "DC_Binnacle", PreloadDirectoryPatch.BinnaclePrefabRef,
                    new Vector3(-91.729f, 3.662f, -541.765f), Quaternion.Euler(0.0f, 134.6f, 0.0f));

                spawnedDragonCliffs = true;
                InclinometerPlugin.DiagLogger.LogInfo("[NAV SUITE] Dragon Cliffs shop spawners cleanly deployed via IslandStreetlightsManager.");
            }
            catch (Exception ex)
            {
                InclinometerPlugin.DiagLogger.LogError($"[NAV SUITE] Failed Dragon Cliffs injection: {ex}");
            }
        }

        private static void DeploySpawner(GameObject sceneryRoot, string label, GameObject targetPrefab, Vector3 localPos, Quaternion localRot)
        {
            if (targetPrefab == null) return;

            GameObject spawnerNode = new GameObject($"shop item spawner ({label})");
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
        }

        public static void ResetFlags()
        {
            spawnedGRC = false;
            spawnedAestrin = false;
            spawnedDragonCliffs = false;
        }
    }
}