using UnityEngine;

namespace SimpleKeplerOrbits
{
	/// <summary>
	/// Component for displaying current orbit curve in editor and in game.
	/// </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	[RequireComponent(typeof(KeplerOrbitMover))]
	[ExecuteAlways]
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

		public bool ShowOrbitGizmoWhileInPlayMode       = true;
		public bool ShowVelocityGizmoInEditor           = true;
		public bool ShowPeriapsisApoapsisGizmosInEditor = true;
		public bool ShowAscendingNodeInEditor           = true;
		public bool ShowAxisGizmosInEditor              = false;

		[Range(0f, 1f)]
		public float GizmosAlphaMain = 1f;

		[Range(0f, 1f)]
		public float GizmosAlphaSecondary = 0.3f;
#endif

		private KeplerOrbitMover _moverReference;
		private Vector3d[]       _orbitPoints;

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
			if (LineRendererReference != null && _moverReference.AttractorSettings.AttractorObject != null)
			{
				var attractorPosHalf = _moverReference.AttractorSettings.AttractorObject.position;

				_moverReference.OrbitData.GetOrbitPointsNoAlloc(
					ref _orbitPoints,
					OrbitPointsCount,
					new Vector3d(attractorPosHalf.x, attractorPosHalf.y, attractorPosHalf.z),
					MaxOrbitWorldUnitsDistance);
				LineRendererReference.positionCount = _orbitPoints.Length;
				for (int i = 0; i < _orbitPoints.Length; i++)
				{
					var point = _orbitPoints[i];
					LineRendererReference.SetPosition(i, new Vector3((float)point.x, (float)point.y, (float)point.z));
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

						if (ShowAscendingNodeInEditor)
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
			var origin = _moverReference.AttractorSettings.AttractorObject.position + new Vector3((float)_moverReference.OrbitData.CenterPoint.x, (float)_moverReference.OrbitData.CenterPoint.y,
				(float)_moverReference.OrbitData.CenterPoint.z);
			Gizmos.color = new Color(0, 1, 0.5f, GizmosAlphaSecondary);
			var semiMajorAxis = new Vector3(
				(float)_moverReference.OrbitData.SemiMajorAxisBasis.x,
				(float)_moverReference.OrbitData.SemiMajorAxisBasis.y,
				(float)_moverReference.OrbitData.SemiMajorAxisBasis.z);
			Gizmos.DrawLine(origin, origin + semiMajorAxis);
			Gizmos.color = new Color(1, 0.8f, 0.2f, GizmosAlphaSecondary);
			Gizmos.DrawLine(origin,
				origin + new Vector3((float)_moverReference.OrbitData.SemiMinorAxisBasis.x, (float)_moverReference.OrbitData.SemiMinorAxisBasis.y, (float)_moverReference.OrbitData.SemiMinorAxisBasis.z));
			Gizmos.color = new Color(0.9f, 0.1f, 0.2f, GizmosAlphaSecondary);
			Gizmos.DrawLine(origin, origin + new Vector3((float)_moverReference.OrbitData.OrbitNormal.x, (float)_moverReference.OrbitData.OrbitNormal.y, (float)_moverReference.OrbitData.OrbitNormal.z));
		}

		private void ShowAscNode()
		{
			if (GizmosAlphaSecondary <= 0) return;
			Vector3 origin = _moverReference.AttractorSettings.AttractorObject.position;
			Gizmos.color = new Color(0.29f, 0.42f, 0.64f, GizmosAlphaSecondary);
			Vector3d ascNode;
			if (_moverReference.OrbitData.GetAscendingNode(out ascNode))
			{
				Gizmos.DrawLine(origin, origin + new Vector3((float)ascNode.x, (float)ascNode.y, (float)ascNode.z));
			}
		}

		private void ShowVelocity()
		{
			if (GizmosAlphaSecondary <= 0) return;
			Gizmos.color = new Color(1, 1, 1, GizmosAlphaSecondary);
			var velocity = _moverReference.OrbitData.GetVelocityAtEccentricAnomaly(_moverReference.OrbitData.EccentricAnomaly);
			if (_moverReference.VelocityHandleLengthScale > 0)
			{
				velocity *= _moverReference.VelocityHandleLengthScale;
			}

			var pos = transform.position;
			Gizmos.DrawLine(pos, pos + new Vector3((float)velocity.x, (float)velocity.y, (float)velocity.z));
		}

		private void ShowOrbit()
		{
			var attractorPosHalf = _moverReference.AttractorSettings.AttractorObject.position;
			var attractorPos     = new Vector3d(attractorPosHalf.x, attractorPosHalf.y, attractorPosHalf.z);
			_moverReference.OrbitData.GetOrbitPointsNoAlloc(ref _orbitPoints, OrbitPointsCount, attractorPos, MaxOrbitWorldUnitsDistance);
			Gizmos.color = new Color(1, 1, 1, GizmosAlphaMain);
			for (int i = 0; i < _orbitPoints.Length - 1; i++)
			{
				var p1 = _orbitPoints[i];
				var p2 = _orbitPoints[i + 1];
				Gizmos.DrawLine(new Vector3((float)p1.x, (float)p1.y, (float)p1.z), new Vector3((float)p2.x, (float)p2.y, (float)p2.z));
			}
		}

		private void ShowNodes()
		{
			if (GizmosAlphaSecondary <= 0) return;
			if (!_moverReference.OrbitData.IsValidOrbit) return;

			Gizmos.color = new Color(0.9f, 0.4f, 0.2f, GizmosAlphaSecondary);
			var     periapsis    = new Vector3((float)_moverReference.OrbitData.Periapsis.x, (float)_moverReference.OrbitData.Periapsis.y, (float)_moverReference.OrbitData.Periapsis.z);
			var     attractorPos = _moverReference.AttractorSettings.AttractorObject.position;
			Vector3 point        = attractorPos + periapsis;
			Gizmos.DrawLine(attractorPos, point);

			if (_moverReference.OrbitData.Eccentricity < 1)
			{
				Gizmos.color = new Color(0.2f, 0.4f, 0.78f, GizmosAlphaSecondary);
				var apoapsis = new Vector3((float)_moverReference.OrbitData.Apoapsis.x, (float)_moverReference.OrbitData.Apoapsis.y, (float)_moverReference.OrbitData.Apoapsis.z);
				point = _moverReference.AttractorSettings.AttractorObject.position + apoapsis;
				Gizmos.DrawLine(attractorPos, point);
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