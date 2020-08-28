using System.Collections;
using UnityEngine;

namespace SimpleKeplerOrbits
{
	/// <summary>
	/// Component for moving game object in eliptic or hyperbolic path around attractor body.
	/// </summary>
	/// <seealso cref="UnityEngine.MonoBehaviour" />
	[ExecuteAlways]
	[SelectionBase]
	[DisallowMultipleComponent]
	public class KeplerOrbitMover : MonoBehaviour
	{
		/// <summary>
		/// The attractor settings data.
		/// Attractor object reference must be assigned or orbit mover will not work.
		/// </summary>
		public AttractorData AttractorSettings = new AttractorData();

		/// <summary>
		/// The velocity handle object.
		/// Assign object and use it as velocity control handle in scene view.
		/// </summary>
		[Tooltip("The velocity handle object. Assign object and use it as velocity control handle in scene view.")]
		public Transform VelocityHandle;

		/// <summary>
		/// The velocity handle lenght scale parameter.
		/// </summary>
		[Range(0f, 10f)]
		[Tooltip("Velocity handle scale parameter.")]
		public float VelocityHandleLenghtScale = 0f;

		/// <summary>
		/// The time scale multiplier.
		/// </summary>
		[Tooltip("The time scale multiplier.")]
		public float TimeScale = 1f;

		/// <summary>
		/// The orbit data.
		/// Internal state of orbit.
		/// </summary>
		[Header("Orbit state details:")]
		[Tooltip("Internal state of orbit.")]
		public KeplerOrbitData OrbitData = new KeplerOrbitData();

		/// <summary>
		/// Disable continious editing orbit in update loop, if you don't need it.
		/// It is also very useful in cases, when orbit is not stable due to float precision limits.
		/// </summary>
		/// <remarks>
		/// Internal orbit data uses double prevision vectors, but every update it is compared with unity scene vectors, which are float precision.
		/// In result, if unity vectors precision is not enough for current values, then orbit become unstable.
		/// To avoid this issue, you can disable comparison, and then orbit motion will be nice and stable, but you will no longer be able to change orbit by moving objects in editor.
		/// </remarks>
		[Tooltip("Disable continious editing orbit in update loop, if you don't need it, or you need to fix Kraken issue on large scale orbits.")]
		public bool LockOrbitEditing = false;

#if UNITY_EDITOR
		/// <summary>
		/// The debug error displayed flag.
		/// Used to avoid errors spamming.
		/// </summary>
		private bool _debugErrorDisplayed = false;
#endif

		private Coroutine _updateRoutine;

		private bool IsReferencesAsigned
		{
			get { return AttractorSettings != null && AttractorSettings.AttractorObject != null; }
		}

		private void OnEnable()
		{
			if (!LockOrbitEditing)
			{
				ForceUpdateOrbitData();
			}
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				return;
			}
#endif
			if (_updateRoutine != null)
			{
				StopCoroutine(_updateRoutine);
			}

			_updateRoutine = StartCoroutine(OrbitUpdateLoop());
		}

		private void OnDisable()
		{
			if (_updateRoutine != null)
			{
				StopCoroutine(_updateRoutine);
				_updateRoutine = null;
			}
		}

		/// <summary>
		/// Updates orbit internal data.
		/// </summary>
		/// <remarks>
		/// In this method orbit data is updating from view state:
		/// If you change body position, attractor mass or any other vital orbit parameter, 
		/// this change will be noticed and applyed to internal OrbitData state in this method.
		/// If you need to change orbitData state directly, by script, you need to change OrbitData state and then call ForceUpdateOrbitData
		/// </remarks>
		private void Update()
		{
			if (IsReferencesAsigned)
			{
				if (!LockOrbitEditing)
				{
					var      pos      = transform.position - AttractorSettings.AttractorObject.position;
					Vector3d position = new Vector3d(pos.x, pos.y, pos.z);

					bool velocityHandleChanged = false;
					if (VelocityHandle != null)
					{
						Vector3 velocity = GetVelocityHandleDisplayedVelocity();
						if (velocity != new Vector3((float)OrbitData.Velocity.x, (float)OrbitData.Velocity.y, (float)OrbitData.Velocity.z))
						{
							velocityHandleChanged = true;
						}
					}

					if (position != OrbitData.Position ||
					    velocityHandleChanged ||
					    OrbitData.GravConst != AttractorSettings.GravityConstant ||
					    OrbitData.AttractorMass != AttractorSettings.AttractorMass)
					{
						ForceUpdateOrbitData();
					}
				}
			}
			else
			{
#if UNITY_EDITOR
				if (AttractorSettings.AttractorObject == null)
				{
					if (!_debugErrorDisplayed)
					{
						_debugErrorDisplayed = true;
						if (Application.isPlaying)
						{
							Debug.LogError("KeplerMover: Attractor reference not asigned", context: gameObject);
						}
						else
						{
							Debug.Log("KeplerMover: Attractor reference not asigned", context: gameObject);
						}
					}
				}
				else
				{
					_debugErrorDisplayed = false;
				}
#endif
			}
		}


		/// <summary>
		/// Progress orbit path motion.
		/// Actual kepler orbiting is processed here.
		/// </summary>
		/// <remarks>
		/// Orbit motion progress calculations must be placed after Update, so orbit parameters changes can be applyed,
		/// but before LateUpdate, so orbit can be displayed in same frame.
		/// Coroutine loop is best candidate for achieving this.
		/// </remarks>
		private IEnumerator OrbitUpdateLoop()
		{
			while (true)
			{
				if (IsReferencesAsigned)
				{
					if (!OrbitData.IsValidOrbit)
					{
						//try to fix orbit if we can.
						OrbitData.CalculateOrbitStateFromOrbitalVectors();
					}

					if (OrbitData.IsValidOrbit)
					{
						OrbitData.UpdateOrbitDataByTime(Time.deltaTime * TimeScale);
						ForceUpdateViewFromInternalState();
					}
				}

				yield return null;
			}
		}

		/// <summary>
		/// Updates OrbitData from new body position and velocity vectors.
		/// </summary>
		/// <param name="relativePosition">The relative position.</param>
		/// <param name="velocity">The relative velocity.</param>
		/// <remarks>
		/// This method can be useful to assign new position of body by script.
		/// Or you can directly change OrbitData state and then manually update view.
		/// </remarks>
		public void CreateNewOrbitFromPositionAndVelocity(Vector3 relativePosition, Vector3 velocity)
		{
			if (IsReferencesAsigned)
			{
				OrbitData.Position = new Vector3d((float)relativePosition.x, (float)relativePosition.y, (float)relativePosition.z);
				OrbitData.Velocity = new Vector3d((float)velocity.x,         (float)velocity.y,         (float)velocity.z);
				OrbitData.CalculateOrbitStateFromOrbitalVectors();
				ForceUpdateViewFromInternalState();
			}
		}

		/// <summary>
		/// Forces the update of body position, and velocity handler from OrbitData.
		/// Call this method after any direct changing of OrbitData.
		/// </summary>
		[ContextMenu("Update transform from orbit state")]
		public void ForceUpdateViewFromInternalState()
		{
			var pos = new Vector3((float)OrbitData.Position.x, (float)OrbitData.Position.y, (float)OrbitData.Position.z);
			transform.position = AttractorSettings.AttractorObject.position + pos;
			ForceUpdateVelocityHandleFromInternalState();
		}

		/// <summary>
		/// Forces the refresh of position of velocity handle object from actual orbit state.
		/// </summary>
		public void ForceUpdateVelocityHandleFromInternalState()
		{
			if (VelocityHandle != null)
			{
				Vector3 velocityRelativePosition = new Vector3((float)OrbitData.Velocity.x, (float)OrbitData.Velocity.y, (float)OrbitData.Velocity.z);
				if (VelocityHandleLenghtScale > 0 && !float.IsNaN(VelocityHandleLenghtScale) && !float.IsInfinity(VelocityHandleLenghtScale))
				{
					velocityRelativePosition *= VelocityHandleLenghtScale;
				}

				VelocityHandle.position = transform.position + velocityRelativePosition;
			}
		}

		/// <summary>
		/// Gets the displayed velocity vector from Velocity Handle object position if Handle reference is not null.
		/// NOTE: Displayed velocity may not be equal to actual orbit velocity.
		/// </summary>
		/// <returns>Displayed velocity vector if Handle is not null, otherwise zero vector.</returns>
		public Vector3 GetVelocityHandleDisplayedVelocity()
		{
			if (VelocityHandle != null)
			{
				Vector3 velocity = VelocityHandle.position - transform.position;
				if (VelocityHandleLenghtScale > 0 && !float.IsNaN(VelocityHandleLenghtScale) && !float.IsInfinity(VelocityHandleLenghtScale))
				{
					velocity /= VelocityHandleLenghtScale;
				}

				return velocity;
			}

			return new Vector3();
		}

		/// <summary>
		/// Forces the update of internal orbit data from current world positions of body, attractor settings and velocityHandle.
		/// </summary>
		/// <remarks>
		/// This method must be called after any manual changing of body position, velocity handler position or attractor settings.
		/// It will update internal OrbitData state from view state.
		/// </remarks>
		[ContextMenu("Update Orbit data from current vectors")]
		public void ForceUpdateOrbitData()
		{
			if (IsReferencesAsigned)
			{
				OrbitData.AttractorMass = AttractorSettings.AttractorMass;
				OrbitData.GravConst     = AttractorSettings.GravityConstant;

				// Possible loss of precision, may be a problem in some situations.
				var pos = transform.position - AttractorSettings.AttractorObject.position;
				OrbitData.Position = new Vector3d(pos.x, pos.y, pos.z);
				if (VelocityHandle != null)
				{
					Vector3 velocity = GetVelocityHandleDisplayedVelocity();
					OrbitData.Velocity = new Vector3d(velocity.x, velocity.y, velocity.z);
				}

				OrbitData.CalculateOrbitStateFromOrbitalVectors();
			}
		}

		/// <summary>
		/// Change orbit velocity vector to match circular orbit.
		/// </summary>
		[ContextMenu("Circularize orbit")]
		public void SetAutoCircleOrbit()
		{
			if (IsReferencesAsigned)
			{
				OrbitData.Velocity = KeplerOrbitUtils.CalcCircleOrbitVelocity(Vector3d.zero, OrbitData.Position, OrbitData.AttractorMass, OrbitData.OrbitNormal, OrbitData.GravConst);
				OrbitData.CalculateOrbitStateFromOrbitalVectors();
				ForceUpdateVelocityHandleFromInternalState();
			}
		}

		[ContextMenu("Inverse velocity")]
		public void InverseVelocity()
		{
			if (IsReferencesAsigned)
			{
				OrbitData.Velocity = -OrbitData.Velocity;
				OrbitData.CalculateOrbitStateFromOrbitalVectors();
				ForceUpdateVelocityHandleFromInternalState();
			}
		}

		[ContextMenu("Inverse position")]
		public void InversePositionRelativeToAttractor()
		{
			if (IsReferencesAsigned)
			{
				OrbitData.Position = -OrbitData.Position;
				OrbitData.CalculateOrbitStateFromOrbitalVectors();
				ForceUpdateVelocityHandleFromInternalState();
			}
		}

		[ContextMenu("Inverse velocity and position")]
		public void InverseOrbit()
		{
			if (IsReferencesAsigned)
			{
				OrbitData.Velocity = -OrbitData.Velocity;
				OrbitData.Position = -OrbitData.Position;
				OrbitData.CalculateOrbitStateFromOrbitalVectors();
				ForceUpdateVelocityHandleFromInternalState();
			}
		}

		[ContextMenu("Reset orbit")]
		public void ResetOrbit()
		{
			OrbitData = new KeplerOrbitData();
		}
	}
}