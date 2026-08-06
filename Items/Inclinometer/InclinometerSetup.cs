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

            // 3. Sailwind Save System Registration
            var saveComp = prefab.GetComponent<SaveablePrefab>() ?? prefab.AddComponent<SaveablePrefab>();
            saveComp.prefabIndex = 830;

            // 4. Physics Component Injection
            if (prefab.GetComponent<InclinometerPhysics>() == null)
            {
                prefab.AddComponent<InclinometerPhysics>();
            }

            // 5. Clean Interactivity Outlines
            var outlines = prefab.GetComponentsInChildren<cakeslice.Outline>(true);
            foreach (var outline in outlines)
            {
                UnityEngine.Object.DestroyImmediate(outline, true);
            }
        }
    }
}