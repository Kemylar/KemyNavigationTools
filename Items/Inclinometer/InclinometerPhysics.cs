using UnityEngine;

namespace KemyNavTools
{
    public class InclinometerPhysics : MonoBehaviour
    {
        private ShipItem shipItem;
        private Transform indicatorArmTransform;

        [Header("Pendulum Smoothness")]
        // Increased from 4.0f to 18.0f for snappy, responsive tracking
        public float swingSmoothness = 18.0f;

        private void Start()
        {
            shipItem = GetComponent<ShipItem>();
            indicatorArmTransform = transform.Find("Arm");

            if (indicatorArmTransform == null)
            {
                InclinometerPlugin.DiagLogger.LogError("[NAV SUITE] Critical: Couldn't find the 'Arm' child component!");
            }
        }

        private void LateUpdate()
        {
            if (shipItem == null || indicatorArmTransform == null) return;

            // Pure world gravity calculation
            Vector3 itemRight = transform.right;
            float targetRoll = Mathf.Atan2(Vector3.Dot(Vector3.up, itemRight), Vector3.Dot(Vector3.up, Vector3.up)) * Mathf.Rad2Deg;

            // Apply rotation with the newly accelerated snappy responsiveness
            Quaternion targetArmRot = Quaternion.Euler(0f, 0f, -targetRoll);
            indicatorArmTransform.localRotation = Quaternion.Slerp(indicatorArmTransform.localRotation, targetArmRot, Time.deltaTime * swingSmoothness);
        }
    }
}