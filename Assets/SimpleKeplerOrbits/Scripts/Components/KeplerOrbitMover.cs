using UnityEngine;

namespace SimpleKeplerOrbits
{
    [ExecuteInEditMode]
    public class KeplerOrbitMover : MonoBehaviour
    {
        public AttractorData AttractorSettings = new AttractorData();

        public Transform VelocityHandle;

        public KeplerOrbitData OrbitData = new KeplerOrbitData();

        public float TimeScale = 1f;

        private void Update()
        {
            if (!Application.isPlaying)
            {
                OrbitData.position = new Vector3d(transform.position - AttractorSettings.AttractorObject.position);
                var r = VelocityHandle.position - transform.position;
                OrbitData.velocity = new Vector3d(r);
                OrbitData.CalculateNewOrbitData();
            }
            else
            {
                OrbitData.UpdateOrbitDataByTime(Time.deltaTime * TimeScale);
                transform.position = AttractorSettings.AttractorObject.position + (Vector3)OrbitData.position;
                if (VelocityHandle != null)
                {
                    VelocityHandle.position = transform.position + (Vector3)OrbitData.velocity;
                }
            }
        }

        public void UpdateOrbitData()
        {
            OrbitData.attractorMass = AttractorSettings.AttractorMass;
            OrbitData.gravConst = AttractorSettings.GravityConstant;
            OrbitData.position = new Vector3d(transform.position - AttractorSettings.AttractorObject.position);
            OrbitData.velocity = new Vector3d((VelocityHandle.position - transform.position));
            OrbitData.CalculateNewOrbitData();

        }

        [ContextMenu("Circulize orbit")]
        public void SetAutoCircleOrbit()
        {
            OrbitData.velocity = KeplerOrbitUtils.CalcCircleOrbitVelocity(Vector3d.zero, OrbitData.position, OrbitData.attractorMass, 1f, OrbitData.orbitNormal, OrbitData.gravConst);
            OrbitData.CalculateNewOrbitData();
        }
    }
}