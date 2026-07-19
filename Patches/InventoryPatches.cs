using HarmonyLib;
using UnityEngine;

namespace KemyNavTools
{
    [HarmonyPatch(typeof(GPButtonInventorySlot), "InsertItem")]
    public static class InventoryDisplayPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ShipItem item)
        {
            if (item == null) return;

            // Fetch the save component to identify our unique mod tools
            var saveComp = item.GetComponent<SaveablePrefab>();
            if (saveComp == null) return;

            // Check if it's our Inclinometer (830) or Bearing Compass (831)
            if (saveComp.prefabIndex == 830 || saveComp.prefabIndex == 831)
            {
                // Spin the item's local rotation by 180 degrees relative to the slot UI frame
                item.transform.localRotation *= Quaternion.Euler(0f, 180f, 0f);
            }
        }
    }
}