using System;
using System.Reflection;
using UnityEngine;

namespace KemyNavTools
{
    public static class CompassSetup
    {
        public static void Configure(GameObject prefab)
        {
            if (prefab == null) return;

            // 1. Scale configuration
            prefab.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            // 2. Unity Physics Configuration on Root
            var rb = prefab.GetComponent<Rigidbody>() ?? prefab.AddComponent<Rigidbody>();
            rb.mass = 2.0f;
            rb.isKinematic = true;

            // 3. Sailwind Save System Registration
            var saveComp = prefab.GetComponent<SaveablePrefab>() ?? prefab.AddComponent<SaveablePrefab>();
            saveComp.prefabIndex = 831;

            // 4. Shop Profile and Economy Parameters
            ShipItem shipItem = prefab.GetComponent<ShipItem>() ?? prefab.AddComponent<ShipItem>();
            shipItem.name = "bearing compass";
            shipItem.value = 250;
            shipItem.mass = 2.0f;
            shipItem.category = TransactionCategory.toolsAndSupplies;
            shipItem.wallAttachment = false;
            shipItem.holdDistance = 0.4f;
            shipItem.furniturePlaceHeight = 0.05f;

            // 5. Physics Component Injection on the root object
            if (prefab.GetComponent<CompassPhysics>() == null)
            {
                prefab.AddComponent<CompassPhysics>();
            }

            // 6. Clean Interactivity Outlines
            var outlines = prefab.GetComponentsInChildren<cakeslice.Outline>(true);
            foreach (var outline in outlines)
            {
                UnityEngine.Object.DestroyImmediate(outline, true);
            }

            // 7. Standard Mesh Renderer Hook
            var meshRenderer = prefab.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                var rendererField = typeof(ShipItem).GetField("renderer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                rendererField?.SetValue(shipItem, meshRenderer);
            }
        }
    }
}