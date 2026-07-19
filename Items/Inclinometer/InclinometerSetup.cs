using System;
using System.Reflection;
using UnityEngine;

namespace KemyNavTools
{
    public static class InclinometerSetup
    {
        public static void Configure(GameObject prefab)
        {
            if (prefab == null) return;

            // 1. Scale configuration
            prefab.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            // 2. Unity Physics Configuration on Root
            var rb = prefab.GetComponent<Rigidbody>() ?? prefab.AddComponent<Rigidbody>();
            rb.mass = 3.0f;
            rb.isKinematic = true;

            // 3. Sailwind Save System Registration (Fixed: Mapped directly to slot 830)
            var saveComp = prefab.GetComponent<SaveablePrefab>() ?? prefab.AddComponent<SaveablePrefab>();
            saveComp.prefabIndex = 830;

            // 4. Shop Profile and Economy Parameters
            ShipItem shipItem = prefab.GetComponent<ShipItem>() ?? prefab.AddComponent<ShipItem>();
            shipItem.name = "inclinometer";
            shipItem.value = 180;
            shipItem.mass = 3.0f;
            shipItem.category = TransactionCategory.toolsAndSupplies;
            shipItem.wallAttachment = true;
            shipItem.holdDistance = 0.5f;
            shipItem.furniturePlaceHeight = 0.05f;

            // 5. Physics Component Injection
            if (prefab.GetComponent<InclinometerPhysics>() == null)
            {
                prefab.AddComponent<InclinometerPhysics>();
            }

            // 6. Clean Interactivity Outlines
            var outlines = prefab.GetComponentsInChildren<cakeslice.Outline>(true);
            foreach (var outline in outlines)
            {
                UnityEngine.Object.DestroyImmediate(outline, true);
            }

            // 7. Synchronize Renderers
            var meshRenderer = prefab.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                var rendererField = typeof(ShipItem).GetField("renderer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                rendererField?.SetValue(shipItem, meshRenderer);
            }
        }
    }
}