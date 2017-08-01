using UnityEngine;

namespace SimpleKeplerOrbits
{
    [RequireComponent(typeof(KeplerOrbitMover))]
    [ExecuteInEditMode]
    public class KeplerOrbitLineDisplay : MonoBehaviour
    {
        public int orbitPointsCount = 50;

        public float MaxOrbitWorkUnitsDistance = 1000f;
        public LineRenderer linerend;

        private KeplerOrbitMover _moverReference;

        void OnEnable()
        {
            if (_moverReference == null)
            {
                _moverReference = GetComponent<KeplerOrbitMover>();
            }
        }

        private void Update()
        {
            if (linerend != null)
            {
                var points = _moverReference.OrbitData.GetOrbitPoints(orbitPointsCount, MaxOrbitWorkUnitsDistance);
                linerend.positionCount = points.Length;
                for (int i = 0; i < points.Length; i++)
                {
                    linerend.SetPosition(i, _moverReference.AttractorSettings.AttractorObject.position + (Vector3)points[i]);
                }
            }
        }
        void OnDrawGizmos()
        {
            if (_moverReference.VelocityHandle != null)
            {
                if (!Application.isPlaying)
                {
                    _moverReference.UpdateOrbitData();
                }
                ShowVelocity();
                ShowOrbit();
                ShowNodes();
            }
        }

        void ShowVelocity()
        {
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)_moverReference.OrbitData.GetVelocityAtEccentricAnomaly(_moverReference.OrbitData.eccentricAnomaly));
        }

        void ShowOrbit()
        {
            var points = _moverReference.OrbitData.GetOrbitPoints(orbitPointsCount, (double)MaxOrbitWorkUnitsDistance);
            Gizmos.color = new Color(1, 1, 1, 0.3f);
            for (int i = 0; i < points.Length - 1; i++)
            {
                Gizmos.DrawLine(_moverReference.AttractorSettings.AttractorObject.position + (Vector3)points[i], _moverReference.AttractorSettings.AttractorObject.position + (Vector3)points[i + 1]);
            }
        }

        void ShowNodes()
        {
            Vector3 asc;
            if (_moverReference.OrbitData.GetAscendingNode(out asc))
            {
                Gizmos.color = new Color(0.9f, 0.4f, 0.2f, 0.5f);
                Gizmos.DrawLine(_moverReference.AttractorSettings.AttractorObject.position, _moverReference.AttractorSettings.AttractorObject.position + asc);
            }
            Vector3 desc;
            if (_moverReference.OrbitData.GetDescendingNode(out desc))
            {
                Gizmos.color = new Color(0.2f, 0.4f, 0.78f, 0.5f);
                Gizmos.DrawLine(_moverReference.AttractorSettings.AttractorObject.position, _moverReference.AttractorSettings.AttractorObject.position + desc);
            }
        }
    }
}