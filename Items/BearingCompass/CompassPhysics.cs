using UnityEngine;

namespace KemyNavTools
{
    public class CompassPhysics : MonoBehaviour
    {
        private ShipItem shipItem;
        private Transform compassCardTransform;
        private Transform sightingVaneTransform;

        [Header("Physics Smoothness")]
        // Decreased from 8.0f to 2.5f to give the dial an authentic, dampened fluid drag
        public float cardSmoothness = 2.5f;
        public float vaneSmoothness = 5.0f;

        private readonly Vector3 vaneUpRotation = new Vector3(0f, 0f, 0f);
        private readonly Vector3 vaneFoldedRotation = new Vector3(270f, 0f, 0f);

        private float currentVanePitch = 270f;

        private void Start()
        {
            shipItem = GetComponent<ShipItem>();
            compassCardTransform = transform.Find("CompassCard");
            sightingVaneTransform = transform.Find("SightingVane");

            if (sightingVaneTransform != null)
            {
                sightingVaneTransform.localRotation = Quaternion.Euler(vaneFoldedRotation);
            }
        }

        private void LateUpdate()
        {
            if (shipItem == null) return;

            // 1. DAMPENED COMPASS DIAL NORTH TRACKING
            if (compassCardTransform != null)
            {
                float currentHeading = transform.eulerAngles.y;
                Quaternion targetCardRot = Quaternion.Euler(0f, -currentHeading, 0f);

                // Slerp will now take longer to catch up, mimicking the vanilla weight
                compassCardTransform.localRotation = Quaternion.Slerp(compassCardTransform.localRotation, targetCardRot, Time.deltaTime * cardSmoothness);
            }

            // 2. DYNAMIC SIGHTING VANE FLIP
            if (sightingVaneTransform != null)
            {
                float targetPitch = shipItem.held ? vaneUpRotation.x : vaneFoldedRotation.x;
                currentVanePitch = Mathf.MoveTowardsAngle(currentVanePitch, targetPitch, vaneSmoothness * 50f * Time.deltaTime);
                sightingVaneTransform.localRotation = Quaternion.Euler(currentVanePitch, 0f, 0f);
            }
        }
    }
}