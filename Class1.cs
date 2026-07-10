using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InclinometerMod
{
    // Main plugin entry point registering the mod lifecycle with BepInEx
    [BepInPlugin("com.larsonlogistics.sailwind.inclinometer", "Inclinometer Mod", "1.0.0")]
    public class InclinometerPlugin : BaseUnityPlugin
    {
        public static GameObject InclinometerPrefab;
        private static string assetName = "inclinometer_asset";
        public const int FIXED_PREFAB_INDEX = 830;
        public static ManualLogSource DiagLogger;

        // Tracking flags to guarantee single-instance deployment per scene load
        private static bool spawnedGRC = false;
        private static bool spawnedAestrin = false;
        private static bool spawnedDragonCliffs = false;

        // BepInEx initialization lifecycle handling file system resolution and Harmony activation
        private void Awake()
        {
            DiagLogger = Logger;
            DiagLogger.LogInfo("[INCLINOMETER DIAG] Awake lifecycle activated.");

            string dllPath = Assembly.GetExecutingAssembly().Location;
            string modFolder = Path.GetDirectoryName(dllPath);
            string primaryTarget = Path.Combine(modFolder, assetName);

            if (File.Exists(primaryTarget))
            {
                LoadAssetBundle(primaryTarget);
            }

            if (InclinometerPrefab != null)
            {
                var harmony = new Harmony("com.larsonlogistics.sailwind.inclinometer");
                harmony.PatchAll();
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        // Synchronous file reader handling the extraction of compiled Unity asset assemblies
        private void LoadAssetBundle(string path)
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(path);
            if (bundle != null)
            {
                InclinometerPrefab = bundle.LoadAsset<GameObject>("Inclinometer");
                bundle.Unload(false);
            }
        }

        // Scene routing controller directing spatial spawner execution paths based on active region
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            string sceneName = scene.name.ToLower();

            // Reset execution tracking states when returning to the primary main menu layout
            if (sceneName.Contains("main menu"))
            {
                spawnedGRC = false;
                spawnedAestrin = false;
                spawnedDragonCliffs = false;
                return;
            }

            // 1. Gold Rock City (Al'Ankh Archipelago)
            if (sceneName.Contains("gold rock") && !spawnedGRC)
            {
                StartCoroutine(DelayedInjectionRoutine("island 1 A (gold rock) scenery", "GRC",
                    new Vector3(1520.0f, 7.52f, -380.5f), Quaternion.Euler(77.5f, 241f, 0f)));
            }

            // 2. Fort Aestrin (Aestrin Archipelago)
            if (sceneName.Contains("island 15 m (fort)") && !spawnedAestrin)
            {
                StartCoroutine(DelayedInjectionRoutine("island 15 M (Fort) scenery", "Aestrin",
                    new Vector3(-75.316f, 2.7f, 44.2995f), Quaternion.Euler(0f, 180f, 0f)));
            }

            // 3. Dragon Cliffs (Emerald Archipelago)
            if (sceneName.Contains("island 9 e dragon cliffs") && !spawnedDragonCliffs)
            {
                StartCoroutine(DelayedInjectionRoutine("island 9 E (dragon cliffs) scenery", "DragonCliffs",
                    new Vector3(-89.268f, 4.477f, -544.36f), Quaternion.Euler(80f, 230f, 0f)));
            }
        }

        // Asynchronous structural injection engine anchoring custom spawner nodes inside vanilla scenery hierarchies
        private IEnumerator DelayedInjectionRoutine(string parentSceneryName, string regionKey, Vector3 localPos, Quaternion localRot)
        {
            yield return new WaitForSeconds(3f);

            if (regionKey == "GRC" && spawnedGRC) yield break;
            if (regionKey == "Aestrin" && spawnedAestrin) yield break;
            if (regionKey == "DragonCliffs" && spawnedDragonCliffs) yield break;

            if (InclinometerPrefab == null) yield break;

            var sceneryRoot = GameObject.Find(parentSceneryName);
            if (sceneryRoot == null) yield break;

            try
            {
                GameObject spawnerNode = new GameObject($"shop item spawner (inclinometer {regionKey})");
                spawnerNode.transform.parent = sceneryRoot.transform;

                spawnerNode.transform.localPosition = localPos;
                spawnerNode.transform.localRotation = localRot;

                var masterFilter = InclinometerPrefab.GetComponent<MeshFilter>() ?? InclinometerPrefab.GetComponentInChildren<MeshFilter>();
                if (masterFilter != null)
                {
                    var filter = spawnerNode.AddComponent<MeshFilter>();
                    filter.mesh = masterFilter.mesh;
                    spawnerNode.AddComponent<MeshRenderer>();
                }

                var nativeSpawner = spawnerNode.AddComponent<ShopItemSpawner>();
                nativeSpawner.itemPrefab = InclinometerPrefab;

                if (regionKey == "GRC") spawnedGRC = true;
                else if (regionKey == "Aestrin") spawnedAestrin = true;
                else if (regionKey == "DragonCliffs") spawnedDragonCliffs = true;

                DiagLogger.LogInfo($"[INCLINOMETER DIAG] Spawner cleanly deployed at {regionKey}: {localPos}");
            }
            catch (Exception ex)
            {
                DiagLogger.LogError($"[INCLINOMETER DIAG] Failed injection for {regionKey}: {ex}");
            }
        }
    }

    // Harmony interface intercepting the master database array during item preloading sequences
    [HarmonyPatch(typeof(PrefabsDirectory), "PopulateShipItems")]
    public static class PreloadDirectoryPatch
    {
        // Allocation handling extending internal game array storage sizes to prevent save loading overflows
        [HarmonyPrefix]
        public static void Prefix(PrefabsDirectory __instance)
        {
            try
            {
                if (__instance.directory == null) return;

                if (__instance.directory.Length <= InclinometerPlugin.FIXED_PREFAB_INDEX)
                {
                    Array.Resize(ref __instance.directory, InclinometerPlugin.FIXED_PREFAB_INDEX + 5);
                }
            }
            catch (Exception ex)
            {
                InclinometerPlugin.DiagLogger.LogError($"[INCLINOMETER DIAG] Prefix array resize failed: {ex}");
            }
        }

        // Prefab initialization configuring component behaviors, mechanical values, and child asset bindings
        [HarmonyPostfix]
        public static void Postfix(PrefabsDirectory __instance)
        {
            if (InclinometerPlugin.InclinometerPrefab == null) return;

            try
            {
                __instance.directory[InclinometerPlugin.FIXED_PREFAB_INDEX] = InclinometerPlugin.InclinometerPrefab;

                InclinometerPlugin.InclinometerPrefab.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

                var rb = InclinometerPlugin.InclinometerPrefab.GetComponent<Rigidbody>() ?? InclinometerPlugin.InclinometerPrefab.AddComponent<Rigidbody>();
                rb.mass = 3.0f;
                rb.isKinematic = true;

                var saveComp = InclinometerPlugin.InclinometerPrefab.GetComponent<SaveablePrefab>() ?? InclinometerPlugin.InclinometerPrefab.AddComponent<SaveablePrefab>();
                saveComp.prefabIndex = InclinometerPlugin.FIXED_PREFAB_INDEX;

                ShipItem shipItem = InclinometerPlugin.InclinometerPrefab.GetComponent<ShipItem>() ?? InclinometerPlugin.InclinometerPrefab.AddComponent<ShipItem>();
                shipItem.name = "inclinometer";
                shipItem.value = 150;
                shipItem.mass = 3.0f;
                shipItem.category = TransactionCategory.toolsAndSupplies;
                shipItem.wallAttachment = true;
                shipItem.holdDistance = 0.5f;
                shipItem.furniturePlaceHeight = 0.1f;

                Transform armTransform = InclinometerPlugin.InclinometerPrefab.transform.Find("Arm");
                if (armTransform != null)
                {
                    if (armTransform.gameObject.GetComponent<InclinometerPhysics>() == null)
                    {
                        armTransform.gameObject.AddComponent<InclinometerPhysics>();
                    }
                }

                var outlines = InclinometerPlugin.InclinometerPrefab.GetComponentsInChildren<cakeslice.Outline>(true);
                foreach (var outline in outlines)
                {
                    UnityEngine.Object.DestroyImmediate(outline, true);
                }

                var childFilter = InclinometerPlugin.InclinometerPrefab.GetComponentInChildren<MeshFilter>();
                var childRenderer = InclinometerPlugin.InclinometerPrefab.GetComponentInChildren<MeshRenderer>();

                if (childFilter != null && childRenderer != null)
                {
                    var rootFilter = InclinometerPlugin.InclinometerPrefab.GetComponent<MeshFilter>() ?? InclinometerPlugin.InclinometerPrefab.AddComponent<MeshFilter>();
                    rootFilter.mesh = childFilter.mesh;

                    var rootRenderer = InclinometerPlugin.InclinometerPrefab.GetComponent<MeshRenderer>() ?? InclinometerPlugin.InclinometerPrefab.AddComponent<MeshRenderer>();
                    rootRenderer.sharedMaterials = childRenderer.sharedMaterials;

                    var rendererField = typeof(ShipItem).GetField("renderer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (rendererField != null)
                    {
                        rendererField.SetValue(shipItem, rootRenderer);
                    }
                }
            }
            catch (Exception ex)
            {
                InclinometerPlugin.DiagLogger.LogError($"[INCLINOMETER DIAG] Postfix execution breakdown: {ex}");
            }
        }
    }

    // Real-world vector math projection translating physics gravity rotations to the moving needle child asset
    public class InclinometerPhysics : MonoBehaviour
    {
        public float smoothness = 10.0f;
        public bool invertDirection = true;

        void Update()
        {
            if (transform.parent == null) return;

            // Simple world gravity tracking loop
            Vector3 currentForward = transform.parent.forward;
            Vector3 currentUp = transform.parent.up;

            Vector3 worldUp = Vector3.up;
            Vector3 projectedUp = worldUp - Vector3.Project(worldUp, currentForward);

            float tiltAngle = Vector3.SignedAngle(projectedUp, currentUp, currentForward);

            if (invertDirection)
            {
                tiltAngle = -tiltAngle;
            }

            tiltAngle = Mathf.Clamp(tiltAngle, -55f, 55f);

            Quaternion targetRotation = Quaternion.Euler(0, 0, tiltAngle);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smoothness);
        }
    }
}