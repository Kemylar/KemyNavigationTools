using UnityEngine;

namespace KemyNavTools
{
    public class BinnaclePhysics : MonoBehaviour
    {
        // Works seamlessly whether the root has ShipItem or ShipItemLight attached
        private Component shipItemComponent;

        private Transform gimbalRing1;
        private Transform compassBody;
        private Transform compassFace;
        private Transform inclinometerArm; // New reference for the arm child

        [Header("Gimbal Responsiveness")]
        public float gimbalSmoothness = 5.0f;
        public float compassSmoothness = 2.5f;
        public float inclinometerSmoothness = 18.0f; // Snappy responsiveness brought from standalone

        private void Start()
        {
            // Dynamic look-up to support the new ShipItemLight structure smoothly
            shipItemComponent = GetComponent<ShipItemLight>() ?? (Component)GetComponent<ShipItem>();

            // Find the inclinometer arm attached directly to the binnacle object root
            inclinometerArm = transform.Find("InclinometerArm");

            gimbalRing1 = transform.Find("GimbalRing1");
            if (gimbalRing1 != null)
            {
                compassBody = gimbalRing1.Find("CompassBody");
                if (compassBody != null)
                {
                    compassFace = compassBody.Find("CompassFace");
                }
            }

            // Expanded diagnostic log to include the inclinometer verification
            if (gimbalRing1 == null || compassBody == null || compassFace == null || inclinometerArm == null)
            {
                InclinometerPlugin.DiagLogger.LogError("[NAV SUITE] Binnacle is missing one or more required child transforms in its expanded hierarchy!");
            }
        }

        private void LateUpdate()
        {
            if (shipItemComponent == null) return;

            // 1. GIMBAL RING 1 (Pitch): Leveling on the local X-axis
            if (gimbalRing1 != null)
            {
                Vector3 localUpProjected = Vector3.ProjectOnPlane(Vector3.up, transform.right).normalized;
                float pitchAngle = Vector3.SignedAngle(transform.up, localUpProjected, transform.right);

                Quaternion targetRing1Rot = Quaternion.Euler(pitchAngle, 0f, 0f);
                gimbalRing1.localRotation = Quaternion.Slerp(gimbalRing1.localRotation, targetRing1Rot, Time.deltaTime * gimbalSmoothness);
            }

            // 2. COMPASS BODY (Roll): Direct child of Ring 1, leveling on the local Z-axis
            if (compassBody != null)
            {
                Vector3 localUpProjected = Vector3.ProjectOnPlane(Vector3.up, gimbalRing1.forward).normalized;
                float rollAngle = Vector3.SignedAngle(gimbalRing1.up, localUpProjected, gimbalRing1.forward);

                Quaternion targetBodyRot = Quaternion.Euler(0f, 0f, rollAngle);
                compassBody.localRotation = Quaternion.Slerp(compassBody.localRotation, targetBodyRot, Time.deltaTime * gimbalSmoothness);
            }

            // 3. COMPASS FACE: Continuous flat North tracking
            if (compassFace != null)
            {
                float currentHeading = compassBody.eulerAngles.y;
                Quaternion targetFaceRot = Quaternion.Euler(0f, -currentHeading, 0f);
                compassFace.localRotation = Quaternion.Slerp(compassFace.localRotation, targetFaceRot, Time.deltaTime * compassSmoothness);
            }

            // 4. INCLINOMETER ARM: Pure world gravity tracking relative to the binnacle body
            if (inclinometerArm != null)
            {
                Vector3 itemRight = transform.right;
                float targetRoll = Mathf.Atan2(Vector3.Dot(Vector3.up, itemRight), Vector3.Dot(Vector3.up, Vector3.up)) * Mathf.Rad2Deg;

                Quaternion targetArmRot = Quaternion.Euler(0f, 0f, -targetRoll);
                inclinometerArm.localRotation = Quaternion.Slerp(inclinometerArm.localRotation, targetArmRot, Time.deltaTime * inclinometerSmoothness);
            }
        }
    }
}