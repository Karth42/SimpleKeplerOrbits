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

#if UNITY_EDITOR
		[Header("Gizmo display options:")]
		public bool ShowOrbitGizmoInEditor = true;
		public bool ShowOrbitGizmoWhileInPlayMode = true;
		public bool ShowVelocityGizmoInEditor = true;
		public bool ShowPeriapsisApoapsisGizmosInEditor = true;
		public bool ShowAscendingNodeIneditor = true;
		public bool ShowAxisGizmosInEditor = false;

		[Range(0f, 1f)]
		public float GizmosAlphaMain = 1f;

		[Range(0f,1f)]
		public float GizmosAlphaSecondary = 0.3f;
#endif

		private KeplerOrbitMover _moverReference;
		private Vector3[] _orbitPoints;

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
				_moverReference.OrbitData.GetOrbitPointsNoAlloc(ref _orbitPoints, OrbitPointsCount, _moverReference.AttractorSettings.AttractorObject.position, MaxOrbitWorldUnitsDistance);
				LineRendererReference.positionCount = _orbitPoints.Length;
				for (int i = 0; i < _orbitPoints.Length; i++)
				{
					LineRendererReference.SetPosition(i, _orbitPoints[i]);
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
					if (_moverReference.AttractorSettings != null && _moverReference.AttractorSettings.AttractorObject != null)
					{
						if (ShowVelocityGizmoInEditor)
						{
							ShowVelocity();
						}
						ShowOrbit();
						if (ShowPeriapsisApoapsisGizmosInEditor)
						{
							ShowNodes();
						}
						if (ShowAxisGizmosInEditor)
						{
							ShowAxis();
						}
						if (ShowAscendingNodeIneditor)
						{
							ShowAscNode();
						}
					}
				}
			}
		}

		private void ShowAxis()
		{
			if (GizmosAlphaSecondary <= 0) return;
			Vector3 origin = _moverReference.AttractorSettings.AttractorObject.position + (Vector3)_moverReference.OrbitData.CenterPoint;
			Gizmos.color = new Color(0, 1, 0.5f, GizmosAlphaSecondary);
			Gizmos.DrawLine(origin, origin + (Vector3)_moverReference.OrbitData.SemiMajorAxisBasis);
			Gizmos.color = new Color(1, 0.8f, 0.2f, GizmosAlphaSecondary);
			Gizmos.DrawLine(origin, origin + (Vector3)_moverReference.OrbitData.SemiMinorAxisBasis);
			Gizmos.color = new Color(0.9f, 0.1f, 0.2f, GizmosAlphaSecondary);
			Gizmos.DrawLine(origin, origin + (Vector3)_moverReference.OrbitData.OrbitNormal);
		}

		private void ShowAscNode()
		{
			if (GizmosAlphaSecondary <= 0) return;
			Vector3 origin = _moverReference.AttractorSettings.AttractorObject.position;
			Gizmos.color = new Color(0.29f, 0.42f, 0.64f, GizmosAlphaSecondary);
			Vector3 ascNode;
			if (_moverReference.OrbitData.GetAscendingNode(out ascNode))
			{
				Gizmos.DrawLine(origin, origin + ascNode);
			}
		}

		private void ShowVelocity()
		{
			if (GizmosAlphaSecondary <= 0) return;
			Gizmos.color = new Color(1, 1, 1, GizmosAlphaSecondary);
			Vector3 velocity = (Vector3)_moverReference.OrbitData.GetVelocityAtEccentricAnomaly(_moverReference.OrbitData.EccentricAnomaly);
			if (_moverReference.VelocityHandleLenghtScale > 0)
			{
				velocity *= _moverReference.VelocityHandleLenghtScale;
			}
			Gizmos.DrawLine(transform.position, transform.position + velocity);
		}

		private void ShowOrbit()
		{
			_moverReference.OrbitData.GetOrbitPointsNoAlloc(ref _orbitPoints, OrbitPointsCount, _moverReference.AttractorSettings.AttractorObject.position, MaxOrbitWorldUnitsDistance);
			Gizmos.color = new Color(1, 1, 1, GizmosAlphaMain);
			for (int i = 0; i < _orbitPoints.Length - 1; i++)
			{
				Gizmos.DrawLine(_orbitPoints[i], _orbitPoints[i + 1]);
			}
		}

		private void ShowNodes()
		{
			if (GizmosAlphaSecondary <= 0) return;
			if (!_moverReference.OrbitData.IsValidOrbit) return;
			Gizmos.color = new Color(0.9f, 0.4f, 0.2f, GizmosAlphaSecondary);
			Vector3 point = _moverReference.AttractorSettings.AttractorObject.position + (Vector3)_moverReference.OrbitData.Periapsis;
			Gizmos.DrawLine(_moverReference.AttractorSettings.AttractorObject.position, point);

			if (_moverReference.OrbitData.Eccentricity < 1)
			{
				Gizmos.color = new Color(0.2f, 0.4f, 0.78f, GizmosAlphaSecondary);
				point = _moverReference.AttractorSettings.AttractorObject.position + (Vector3)_moverReference.OrbitData.Apoapsis;
				Gizmos.DrawLine(_moverReference.AttractorSettings.AttractorObject.position, point);
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