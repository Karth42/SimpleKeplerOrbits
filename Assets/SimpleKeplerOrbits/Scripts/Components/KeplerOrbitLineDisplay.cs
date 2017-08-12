#region Copyright
/// Copyright © 2017 Vlad Kirpichenko
/// 
/// Author: Vlad Kirpichenko 'itanksp@gmail.com'
/// Licensed under the MIT License.
/// License: http://opensource.org/licenses/MIT
#endregion

using UnityEngine;

namespace SimpleKeplerOrbits
{
    /// <summary>
    /// Component for displaying current orbit curve in editor and in game.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    [RequireComponent(typeof(KeplerOrbitMover))]
    [ExecuteInEditMode]
    public class KeplerOrbitLineDisplay : MonoBehaviour
    {
        /// <summary>
        /// The orbit curve precision.
        /// </summary>
        public int OrbitPointsCount = 50;

        /// <summary>
        /// The maximum orbit distance of orbit display in world units.
        /// </summary>
        public float MaxOrbitWorldUnitsDistance = 1000f;

        /// <summary>
        /// The line renderer reference.
        /// </summary>
        public LineRenderer LineRendererReference;

        [Header("Gizmo display options:")]
        public bool ShowOrbitGizmoInEditor = true;
        public bool ShowOrbitGizmoWhileInPlayMode = true;

        private KeplerOrbitMover _moverReference;

        private void OnEnable()
        {
            if (_moverReference == null)
            {
                _moverReference = GetComponent<KeplerOrbitMover>();
            }
        }

        private void LateUpdate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif
            if (LineRendererReference != null)
            {
                var points = _moverReference.OrbitData.GetOrbitPoints(OrbitPointsCount, MaxOrbitWorldUnitsDistance);
                LineRendererReference.positionCount = points.Length;
                for (int i = 0; i < points.Length; i++)
                {
                    LineRendererReference.SetPosition(i, _moverReference.AttractorSettings.AttractorObject.position + (Vector3)points[i]);
                }
                LineRendererReference.loop = _moverReference.OrbitData.Eccentricity < 1.0;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (ShowOrbitGizmoInEditor && _moverReference != null)
            {
                if (!Application.isPlaying || ShowOrbitGizmoWhileInPlayMode)
                {
                    //if (!Application.isPlaying)
                    //{
                    //    _moverReference.ForceUpdateOrbitData();
                    //}
                    if (_moverReference.AttractorSettings != null && _moverReference.AttractorSettings.AttractorObject != null)
                    {
                        ShowVelocity();
                        ShowOrbit();
                        ShowNodes();
                    }
                }
            }
        }

        private void ShowVelocity()
        {
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)_moverReference.OrbitData.GetVelocityAtEccentricAnomaly(_moverReference.OrbitData.EccentricAnomaly));
        }

        private void ShowOrbit()
        {
            var points = _moverReference.OrbitData.GetOrbitPoints(OrbitPointsCount, (double)MaxOrbitWorldUnitsDistance);
            Gizmos.color = new Color(1, 1, 1, 0.3f);
            for (int i = 0; i < points.Length - 1; i++)
            {
                Gizmos.DrawLine(_moverReference.AttractorSettings.AttractorObject.position + (Vector3)points[i], _moverReference.AttractorSettings.AttractorObject.position + (Vector3)points[i + 1]);
            }
        }

        private void ShowNodes()
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

        [ContextMenu("AutoFind LineRenderer")]
        private void AutoFindLineRenderer()
        {
            if (LineRendererReference == null)
            {
                LineRendererReference = GetComponent<LineRenderer>();
            }
        }
#endif
    }
}