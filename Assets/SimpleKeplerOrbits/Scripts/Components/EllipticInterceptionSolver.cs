using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleKeplerOrbits
{
	/// <summary>
	/// Component for searching interception trajectory between two bodies, and it is considered as trajectory calculation helper in first approximation.
	/// Goals: calc transition elliptic orbit from current orbit to target point, calculating transition duration, calculating velocity budget (delta-v);
	/// Limitations: 
	/// hyperbolic transition orbits not supported, acceleration is considered instant, 
	/// algorithm doesn't handle gravity loses when departuring from strong gravity well, 
	/// collisions avoiding not supported, 
	/// external perturbations and energy losses are not handled,
	/// only one revolution around attractor is supported.
	/// </summary>
	[RequireComponent(typeof(KeplerOrbitMover))]
	[RequireComponent(typeof(SpawnNotifier))]
	[ExecuteInEditMode]
	public class EllipticInterceptionSolver : MonoBehaviour
	{
		/// <summary>
		/// Container for all required references for the algorithm.
		/// </summary>
		[Serializable]
		public struct BodiesReferencesData
		{
			public Transform          Attractor;
			public KeplerOrbitMover   Origin;
			public KeplerOrbitMover   Target;
			public KeplerOrbitMover[] OriginAttractorsChain;
			public KeplerOrbitMover[] TargetAttractorsChain;
			public double             AttractorMass;
			public double             GConst;
		}

		/// <summary>
		/// Container for calculated trajectory data.
		/// </summary>
		public struct TrajectoryData
		{
			public KeplerOrbitData orbit;
			public double          Duration;
			public double          EccAnomStart;
			public double          EccAnomEnd;
		}

		/// <summary>
		/// Target body reference. Should not be attractor or self.
		/// </summary>
		/// <remarks>
		/// Hint: If you want to park at circular orbit around target planet, then do not set this planet as target, 
		/// instead create empty kepler mover object with preffered circular orbit and assign it as target. After transition calculation this object may be deleted (manually, or by custom script). 
		/// And additionaly, if you disable mover component on temp object, then it will not move, which may be convenient.
		/// </remarks>
		public KeplerOrbitMover Target;

		/// <summary>
		/// Departure time.
		/// </summary>
		[Tooltip("Departure time.")]
		public double StartTimeOffset;

		/// <summary>
		/// Preferred transition duration. If set to 0 then minimal possible duration will be calculated.
		/// </summary>
		/// <remarks>
		/// Note that minimal transition duration is dependent on maximal semi major axis limit of transition orbit.
		/// How closer semi major axis is to infinity is how close elliptic transition orbit to parabolic transition.
		/// Brachistochrone (or very close to strait lines) transition orbits will be hyperbolic trajectory, which is not supported in this component.
		/// </remarks>
		[Tooltip("Preferred transition duration. If set to 0 then minimal possible duration will be calculated.")]
		public double TargetDuration;

		/// <summary>
		/// Option to calculate transition trajectory, which passes behind attractor before reaching target.
		/// In other words, difference between departure mean anomaly and arrival mean anomaly is greater than 180 degrees.
		/// </summary>
		[Tooltip("Pass behind attractor when transitioning to target.")]
		public bool IsTransitionPassBehindAttractor;

		/// <summary>
		/// Upper limit for transition orbit's ellipse.
		/// </summary>
		[Tooltip("Upper limit for transition ellipse.")]
		public double MaxTransitionSemiMajorAxis = 1e8;

		/// <summary>
		/// Precision for transition duration calculation. The smaller the value the more calculation iterations will be required.
		/// </summary>
		[Tooltip("Precision for transition duration calculation. The smaller the value the more calculation iterations will be required.")]
		public double TargetDurationPrecision = 1e-4;

		/// <summary>
		/// Draw gizmo curve toggle.
		/// </summary>
		public bool IsEnabledDrawTrajectoryInEditor = true;

		private KeplerOrbitMover _orbitMover;

		private SpawnNotifier _spawnNotifier;

		private TransitionOrbitData _currentTransition;

		[SerializeField]
		private KeplerOrbitMover _instantiateBodyTemplate;

		/// <summary>
		/// Reference to current transition data.
		/// </summary>
		/// <remarks>
		/// Value is updated in Update loop and may be null if failed to calculate proper transition.
		/// </remarks>
		public TransitionOrbitData CurrentTransition
		{
			get { return _currentTransition; }
		}

		private void Awake()
		{
			_orbitMover    = GetComponent<KeplerOrbitMover>();
			_spawnNotifier = GetComponent<SpawnNotifier>();
		}

		private void Update()
		{
			CalculateTransitionState();
		}

		private void OnDrawGizmos()
		{
			if (IsEnabledDrawTrajectoryInEditor && _currentTransition != null)
			{
				GizmosDrawSingleTransition(_currentTransition);
			}
		}

		private void GizmosDrawSingleTransition(TransitionOrbitData transition)
		{
			Gizmos.color = Color.green;
			int   steps     = 50;
			float delta     = transition.EccAnomalyEnd - transition.EccAnomalyStart;
			var   lastPoint = transition.Orbit.GetFocalPositionAtEccentricAnomaly(transition.EccAnomalyStart);
			var   point     = lastPoint;
			for (int i = 1; i <= steps; i++)
			{
				var ratio = i / (float)steps;
				point = transition.Orbit.GetFocalPositionAtEccentricAnomaly(transition.EccAnomalyStart + delta * ratio);
				var attractorPos = transition.Attractor.position;
				Gizmos.DrawLine(
					attractorPos + new Vector3((float)lastPoint.x, (float)lastPoint.y, (float)lastPoint.z),
					attractorPos + new Vector3((float)point.x,     (float)point.y,     (float)point.z));
				lastPoint = point;
			}
		}

		private List<Vector3d> CalculateVelocityDifference(KeplerOrbitMover a, KeplerOrbitMover b, KeplerOrbitData transitionOrbit, double departureTime, double duration, double eccAnomalyDeparture,
			double                                                            eccAnomalyArrival)
		{
			var aMeanAnomalyAtDeparture = a.OrbitData.MeanAnomaly + a.OrbitData.MeanMotion * departureTime;
			var aEccAnomAtDeparture     = KeplerOrbitUtils.ConvertMeanToEccentricAnomaly(aMeanAnomalyAtDeparture, a.OrbitData.Eccentricity);
			var aVelocityAtDeparture    = a.OrbitData.GetVelocityAtEccentricAnomaly(aEccAnomAtDeparture);

			var bMeanAnomalyAtArrival = b.OrbitData.MeanAnomaly + b.OrbitData.MeanMotion * (departureTime + duration);
			var bEccAnomAtArrival     = KeplerOrbitUtils.ConvertMeanToEccentricAnomaly(aMeanAnomalyAtDeparture, a.OrbitData.Eccentricity);
			var bVelocityAtArrival    = a.OrbitData.GetVelocityAtEccentricAnomaly(aEccAnomAtDeparture);

			var transitionVeloctyStart = transitionOrbit.GetVelocityAtEccentricAnomaly(eccAnomalyDeparture);
			var transitionVelcityEnd   = transitionOrbit.GetVelocityAtEccentricAnomaly(eccAnomalyArrival);

			var result = new List<Vector3d>();
			result.Add(transitionVeloctyStart - aVelocityAtDeparture);
			result.Add(bVelocityAtArrival - transitionVelcityEnd);
			return result;
		}

		/// <summary>
		/// Recalculate CurrentTransition value.
		/// </summary>
		public void CalculateTransitionState()
		{
			var refs = GetBodiesReferences();
			if (refs.Attractor != null)
			{
				Vector3d startPoint       = GetPositionAtGivenTime(refs.Origin, (StartTimeOffset),                  refs.OriginAttractorsChain);
				Vector3d endPoint         = GetPositionAtGivenTime(refs.Target, (StartTimeOffset + TargetDuration), refs.TargetAttractorsChain);
				var      attractorPosHalf = refs.Attractor.position;
				var      attractorPos     = new Vector3d(attractorPosHalf.x, attractorPosHalf.y, attractorPosHalf.z);
				var      hyperbola        = new HyperbolaData(new Vector3d(startPoint.x, startPoint.y, startPoint.z), endPoint, attractorPos);
				var trajectory = CalcTransitionTrajectory(
					startPoint,
					endPoint,
					attractorPos,
					hyperbola,
					this.TargetDuration,
					isReverseOrbit: this.IsTransitionPassBehindAttractor,
					attrMass: refs.AttractorMass,
					g: refs.GConst,
					precision: this.TargetDurationPrecision,
					semiMajorAxisUpperLimit: this.MaxTransitionSemiMajorAxis);

				if (trajectory.orbit != null && trajectory.orbit.IsValidOrbit && trajectory.Duration > 0 && !double.IsInfinity(trajectory.Duration))
				{
					var velocityDiff = CalculateVelocityDifference(refs.Origin, refs.Target, trajectory.orbit, StartTimeOffset, trajectory.Duration, trajectory.EccAnomStart, trajectory.EccAnomEnd);
					var totalDeltaV  = 0.0;
					for (int i = 0; i < velocityDiff.Count; i++)
					{
						totalDeltaV += velocityDiff[i].magnitude;
					}

					_currentTransition = new TransitionOrbitData()
					{
						Attractor          = refs.Attractor,
						Duration           = (float)trajectory.Duration,
						EccAnomalyStart    = (float)trajectory.EccAnomStart,
						EccAnomalyEnd      = (float)trajectory.EccAnomEnd,
						ImpulseDifferences = velocityDiff,
						Orbit              = trajectory.orbit,
						TotalDeltaV        = (float)totalDeltaV,
					};
				}
				else
				{
					_currentTransition = null;
				}
			}
			else
			{
				_currentTransition = null;
			}
		}

		/// <summary>
		/// Find and return referneces to three bodies: self, target and mutual attractor.
		/// Additional attractors chain arrays are used when mutual attractor is not direct parent of target body.
		/// If mutual attractor not existent, or target not assigned, then return empty data.
		/// </summary>
		/// <returns>All references data, or empty data if not found.</returns>
		public BodiesReferencesData GetBodiesReferences()
		{
			if (_orbitMover == null)
			{
				_orbitMover = GetComponent<KeplerOrbitMover>();
			}

			if (_orbitMover == null || Target == null || _orbitMover == Target)
			{
				return default(BodiesReferencesData);
			}

			List<KeplerOrbitMover> attractorsA = new List<KeplerOrbitMover>();
			List<KeplerOrbitMover> attractorsB = new List<KeplerOrbitMover>();
			double                 mass        = 0;
			double                 g           = 0;
			Transform mutualAttractor = FindMutualAttractor(
				a: _orbitMover,
				b: Target,
				isGetFullChain: false,
				attractorsAChain: ref attractorsA,
				attractorsBChain: ref attractorsB,
				mass: ref mass,
				gConst: ref g);

			if (mutualAttractor != null)
			{
				return new BodiesReferencesData()
				{
					Origin                = _orbitMover,
					Target                = Target,
					Attractor             = mutualAttractor,
					OriginAttractorsChain = attractorsA.ToArray(),
					TargetAttractorsChain = attractorsB.ToArray(),
					AttractorMass         = mass,
					GConst                = g,
				};
			}

			return default(BodiesReferencesData);
		}

		/// <summary>
		/// Get world space position vector at given time.
		/// </summary>
		/// <param name="target">Target body.</param>
		/// <param name="time">Time, relative to current time.</param>
		/// <param name="attractors">Optional chain of attractors. Order of attractors must be from closest to furthest in hierarchy.</param>
		/// <returns>Position at given time.</returns>
		/// <remarks>
		/// Zero time is considered current state.
		/// For example, at time 0 result position vector will be equal to current target position.
		/// This method allows to progress orbit in time forward (or backward, if passed time is negative) and get position of body at that time.
		/// If attractors collection is not null or empty, then evaluation process will propagate through all attractors, which will affect result.
		/// </remarks>
		public static Vector3d GetPositionAtGivenTime(KeplerOrbitMover target, double time, KeplerOrbitMover[] attractorsChain = null)
		{
			if (target == null)
			{
				return new Vector3d();
			}

			if (!target.OrbitData.IsValidOrbit || target.AttractorSettings.AttractorObject == null)
			{
				var result = target.transform.position;
				return new Vector3d(result.x, result.y, result.z);
			}

			if (attractorsChain == null || attractorsChain.Length == 0)
			{
				if (!target.enabled || target.TimeScale == 0f)
				{
					var result = target.transform.position;
					return new Vector3d(result.x, result.y, result.z);
				}
				else
				{
					var finalMeanAnom = target.OrbitData.MeanAnomaly + target.OrbitData.MeanMotion * time;
					var finalEccAnom  = KeplerOrbitUtils.ConvertMeanToEccentricAnomaly(finalMeanAnom, target.OrbitData.Eccentricity);
					var pos           = target.AttractorSettings.AttractorObject.transform.position;
					var result        = new Vector3d(pos.x, pos.y, pos.z) + target.OrbitData.GetFocalPositionAtEccentricAnomaly(finalEccAnom);
					return result;
				}
			}
			else
			{
				var relativePosition = new Vector3d();
				for (int i = 0; i < attractorsChain.Length; i++)
				{
					bool isLast = i == attractorsChain.Length - 1;
					if (attractorsChain[i].OrbitData.IsValidOrbit && attractorsChain[i].AttractorSettings.AttractorObject != null)
					{
						if (attractorsChain[i].enabled)
						{
							var attrMeanAnom = attractorsChain[i].OrbitData.MeanAnomaly + attractorsChain[i].OrbitData.MeanMotion * attractorsChain[i].TimeScale * time;
							var attrEccAnom  = KeplerOrbitUtils.ConvertMeanToEccentricAnomaly(attrMeanAnom, attractorsChain[i].OrbitData.Eccentricity);
							relativePosition += attractorsChain[i].OrbitData.GetFocalPositionAtEccentricAnomaly(attrEccAnom);
						}
						else
						{
							var pos = attractorsChain[i].transform.position - attractorsChain[i].AttractorSettings.AttractorObject.transform.position;
							relativePosition += new Vector3d(pos.x, pos.y, pos.z);
						}

						if (isLast)
						{
							var pos = attractorsChain[i].AttractorSettings.AttractorObject.position;
							relativePosition += new Vector3d(pos.x, pos.y, pos.z);
						}
					}
					else
					{
						if (isLast || attractorsChain[i].AttractorSettings.AttractorObject == null)
						{
							var pos = attractorsChain[i].transform.position;
							relativePosition += new Vector3d(pos.x, pos.y, pos.z);
						}
						else
						{
							relativePosition += attractorsChain[i].OrbitData.Position;
						}
					}
				}

				if (!target.enabled || target.TimeScale == 0f)
				{
					var pos = target.transform.position - target.AttractorSettings.AttractorObject.position;
					relativePosition += new Vector3d(pos.x, pos.y, pos.z);
				}
				else
				{
					var finalMeanAnom = target.OrbitData.MeanAnomaly + target.OrbitData.MeanMotion * time;
					var finalEccAnom  = KeplerOrbitUtils.ConvertMeanToEccentricAnomaly(finalMeanAnom, target.OrbitData.Eccentricity);
					relativePosition += target.OrbitData.GetFocalPositionAtEccentricAnomaly(finalEccAnom);
				}

				return relativePosition;
			}
		}

		/// <summary>
		/// Caclulate transition tranjectory from point p0 to point p1 around attractor f0.
		/// </summary>
		/// <param name="p0">First position vector.</param>
		/// <param name="p1">Second position vector.</param>
		/// <param name="f0">Attractor position vector.</param>
		/// <param name="hyperbola">Trajectory hyperbola.</param>
		/// <param name="targetDuration">Preferred duration.</param>
		/// <param name="isReverseOrbit">Is transfer orbit plane flipped.</param>
		/// <param name="attrMass">Attractor mass.</param>
		/// <param name="g">Gravity constant.</param>
		/// <param name="precision">Calculation precision.</param>
		/// <param name="semiMajorAxisUpperLimit">Transition ellipse semi major axis limit.</param>
		/// <returns>Calculated trajectory data.</returns>
		/// <remarks>
		/// Main task of this component is to find elliptic trajectory between any 2 points, orbiting around single attractor.
		/// The core problem can be described in more formal form: 
		/// let's take a body A and body B; this two bodies rotating around attractor F. The Goal is to find an orbit around attractor F, which passes through vector A and B.
		/// This problem is equivalent to to problem of finding of an ellipse when only one focus and two points on ellipse are given,
		/// which is known as Lambert's problem (but only with one allowed revolution around attractor). The answer to this problem actually is quite simple. In simplified form the solution can be described in these steps:
		/// 1. Build a hyperbola, using given points A and B as focuses of hyperbola, and attractor F as point on one branch of hyperbola.
		/// 2. Place a new point F2 anywhere on branch opposite to main branch of hyperbola ( where main branch is branch, on which F is located)
		/// 3. Build an ellipse using 3 points: F as first focus, new point F2 as second focus, A or B as one point on ellipse.
		/// 4. As result, created ellipse will always exactly correspond to transition orbit, and it always will pass through A and B.
		/// Eccentricity of ellipse is dependent on where was placed point F2, which is convenient as it can be tweaked for better orbit parameters.
		/// 
		/// Each step of this solution is pretty strait forward. With help of hyperbola, given data of 1 focus and 2 points is converting to 2 focuses and 1 point.
		/// 
		/// With known ellipse parameters it is easy to construct orbit data of transition, and additionally it is possible to calculate mean anomaly of departure and arrival.
		/// Difference between these mean anomalies multiplied by mean motion gives exact duration of transition.
		/// Because time of transition is depending from second focus point F2 (which is parametric), it is possible to set some specific transition time value
		/// and then adjust F2 to match target time as close as possible.
		/// </remarks>
		public static TrajectoryData CalcTransitionTrajectory(Vector3d p0,        Vector3d p1, Vector3d f0, HyperbolaData hyperbola, double targetDuration, bool isReverseOrbit, double attrMass, double g,
			double                                                       precision, double   semiMajorAxisUpperLimit)
		{
			TrajectoryData result                = new TrajectoryData();
			double         hyperbolaValue        = 0.0;
			int            lastDeltaSign         = 0;
			int            changedDeltaSignCount = 0;
			float          delta                 = 0.8f;
			double         lastDuration          = 0;
			int            tmp                   = 0;
			Vector3d       ellipseSecondFocus    = new Vector3d();

			// Calculate transition multiple times until optimal transition duration not found.
			// Usually steps count is not larger than a hundred, so 1000 iterations limit is for fail checking.
			while (true && tmp < 1e3)
			{
				tmp++;
				bool isBranch0 = (p0 - f0).magnitude < (p1 - f0).magnitude;
				ellipseSecondFocus = hyperbola.GetSamplePointOnBranch(hyperbolicCoordinate: hyperbolaValue, isMainBranch: isBranch0);
				var transitionOrbitEllipse = new EllipseData(focus0: f0, focus1: ellipseSecondFocus, p0: p0);
				if (Vector3d.Dot(transitionOrbitEllipse.Normal, hyperbola.Normal) <= 0)
				{
					transitionOrbitEllipse.AxisSecondary *= -1;
				}

				if (transitionOrbitEllipse.A > semiMajorAxisUpperLimit)
				{
					break;
				}

				if (isReverseOrbit)
				{
					transitionOrbitEllipse.AxisSecondary = -transitionOrbitEllipse.AxisSecondary;
				}

				result.orbit = new KeplerOrbitData(
					eccentricity: transitionOrbitEllipse.Eccentricity,
					semiMajorAxis: transitionOrbitEllipse.AxisMain * transitionOrbitEllipse.A,
					semiMinorAxis: transitionOrbitEllipse.AxisSecondary * transitionOrbitEllipse.B,
					meanAnomalyDeg: 0,
					attractorMass: attrMass,
					gConst: g);
				result.EccAnomStart = transitionOrbitEllipse.GetEccentricAnomalyForPoint(p0);
				result.EccAnomEnd   = transitionOrbitEllipse.GetEccentricAnomalyForPoint(p1);

				if (result.EccAnomStart > result.EccAnomEnd)
				{
					result.EccAnomEnd += KeplerOrbitUtils.PI_2;
				}

				var meanAnomStart = KeplerOrbitUtils.ConvertEccentricToMeanAnomaly(result.EccAnomStart, eccentricity: result.orbit.Eccentricity);
				var meanAnomEnd   = KeplerOrbitUtils.ConvertEccentricToMeanAnomaly(result.EccAnomEnd,   eccentricity: result.orbit.Eccentricity);
				var meanAnomDiff  = meanAnomEnd - meanAnomStart;
				result.Duration = meanAnomEnd <= meanAnomStart ? 0.0 : meanAnomDiff / result.orbit.MeanMotion;
				var diff = result.Duration - targetDuration;
				int sign = diff >= 0 ? -1 : 1;
				if (KeplerOrbitUtils.Abs(diff) < precision)
				{
					break;
				}

				if (sign != lastDeltaSign)
				{
					lastDeltaSign = sign;
					changedDeltaSignCount++;
				}

				if (changedDeltaSignCount >= 2)
				{
					delta *= 0.5f;
				}

				int conicShapeAligmentSign = Vector3d.Dot(transitionOrbitEllipse.Normal, hyperbola.Normal) >= 0 ? 1 : -1;
				hyperbolaValue += delta * sign * conicShapeAligmentSign;
				var stepDurationDiff = result.Duration - lastDuration;
				if (KeplerOrbitUtils.Abs(stepDurationDiff) < precision)
				{
					break;
				}

				lastDuration = result.Duration;
			}

			return result;
		}

		/// <summary>
		/// Find attractor transform, which is parent for both A and B.
		/// If mutual attractor is not direct parent of both A and B, then look in upper hierarchy of attractors.
		/// </summary>
		/// <param name="a">Body A.</param>
		/// <param name="b">Body B.</param>
		/// <param name="isGetFullChain">If true, attractors chains will include full hierarchy. In opposite case, chains will end before mutual attractor.</param>
		/// <param name="attractorsAChain">Chain of parent attractors for A.</param>
		/// <param name="attractorsBChain">Chain of parent attractors for B.</param>
		/// <param name="gConst">Attractor gravity const.</param>
		/// <param name="mass">Attractor mass.</param>
		/// <returns>Mutual attractor transform or null if not found.</returns>
		/// <remarks>
		/// Chain of attractors is constructed from attractors transforms, which also have own KeplerOrbitMover component.
		/// 
		/// Note: this method also retreaving g and mass of attractor. Because these values can be different for any KeplerOrbitMover, those, what belong to body A are preffered.
		/// </remarks>
		public static Transform FindMutualAttractor(KeplerOrbitMover a, KeplerOrbitMover b, bool isGetFullChain, ref List<KeplerOrbitMover> attractorsAChain, ref List<KeplerOrbitMover> attractorsBChain,
			ref double                                                 mass, ref double gConst)
		{
			if (attractorsAChain == null)
			{
				attractorsAChain = new List<KeplerOrbitMover>();
			}
			else
			{
				attractorsAChain.Clear();
			}

			if (attractorsBChain == null)
			{
				attractorsBChain = new List<KeplerOrbitMover>();
			}
			else
			{
				attractorsBChain.Clear();
			}

			int       maxChainLen     = 1000;
			Transform mutualAttractor = null;
			if (a != null && b != null && a != b)
			{
				var attrTransform = a.AttractorSettings.AttractorObject;
				while (attrTransform != null && attractorsAChain.Count < maxChainLen)
				{
					var attrOrbitMover = attrTransform.GetComponent<KeplerOrbitMover>();
					attrTransform = null;
					if (attrOrbitMover != null && !attractorsAChain.Contains(attrOrbitMover))
					{
						attrTransform = attrOrbitMover.AttractorSettings.AttractorObject;
						attractorsAChain.Add(attrOrbitMover);
					}
				}

				attrTransform = b.AttractorSettings.AttractorObject;
				while (attrTransform != null && attractorsBChain.Count < maxChainLen)
				{
					var attrOrbitMover = attrTransform.GetComponent<KeplerOrbitMover>();
					attrTransform = null;
					if (attrOrbitMover != null && !attractorsBChain.Contains(attrOrbitMover))
					{
						attrTransform = attrOrbitMover.AttractorSettings.AttractorObject;
						attractorsBChain.Add(attrOrbitMover);
					}
				}

				if (a.AttractorSettings.AttractorObject == b.AttractorSettings.AttractorObject)
				{
					mutualAttractor = a.AttractorSettings.AttractorObject;
					gConst          = a.AttractorSettings.GravityConstant;
					mass            = a.AttractorSettings.AttractorMass;
				}
				else
				{
					for (int i = 0; i < attractorsAChain.Count && mutualAttractor == null; i++)
					{
						for (int n = 0; n < attractorsBChain.Count; n++)
						{
							if (attractorsAChain[i].AttractorSettings.AttractorObject == attractorsBChain[n].transform ||
							    attractorsAChain[i].AttractorSettings.AttractorObject == attractorsBChain[i].AttractorSettings.AttractorObject)
							{
								mutualAttractor = attractorsAChain[i].AttractorSettings.AttractorObject;
								gConst          = attractorsAChain[i].AttractorSettings.GravityConstant;
								mass            = attractorsAChain[i].AttractorSettings.AttractorMass;
							}
							else if (attractorsBChain[i].AttractorSettings.AttractorObject == attractorsAChain[i].transform)
							{
								mutualAttractor = attractorsBChain[i].AttractorSettings.AttractorObject;
								gConst          = attractorsAChain[i].AttractorSettings.GravityConstant;
								mass            = attractorsAChain[i].AttractorSettings.AttractorMass;
							}
							else
							{
								continue;
							}

							break;
						}
					}
				}

				if (!isGetFullChain && mutualAttractor != null)
				{
					int mutualAttractorIndex = -1;
					for (int i = 0; i < attractorsAChain.Count; i++)
					{
						if (attractorsAChain[i].transform == mutualAttractor)
						{
							mutualAttractorIndex = i;
							break;
						}
					}

					if (mutualAttractorIndex >= 0)
					{
						//mutualAttractorIndex++;
						while (attractorsAChain.Count > mutualAttractorIndex)
						{
							attractorsAChain.RemoveAt(attractorsAChain.Count - 1);
						}
					}

					mutualAttractorIndex = -1;
					for (int i = 0; i < attractorsBChain.Count; i++)
					{
						if (attractorsBChain[i].transform == mutualAttractor)
						{
							mutualAttractorIndex = i;
							break;
						}
					}

					if (mutualAttractorIndex >= 0)
					{
						//mutualAttractorIndex++;
						while (attractorsBChain.Count > mutualAttractorIndex)
						{
							attractorsBChain.RemoveAt(attractorsBChain.Count - 1);
						}
					}
				}

				return mutualAttractor;
			}

			attractorsAChain.Clear();
			attractorsBChain.Clear();
			return null;
		}

		[ContextMenu("Spawn transfer body.")]
		public bool TrySpawnOrbitingBodyForCurrentTrajectory()
		{
			if (_currentTransition == null) return false;
			if (_currentTransition.Attractor == null) return false;
			if (_currentTransition.Orbit == null) return false;
			if (!_currentTransition.Orbit.IsValidOrbit) return false;

			KeplerOrbitMover instance;
			if (_instantiateBodyTemplate != null)
			{
				instance = Instantiate(_instantiateBodyTemplate);
			}
			else
			{
				instance = new GameObject("transition_body", new Type[] { typeof(KeplerOrbitMover), typeof(KeplerOrbitLineDisplay) }).GetComponent<KeplerOrbitMover>();
			}

			instance.AttractorSettings.AttractorMass   = (float)_currentTransition.Orbit.AttractorMass;
			instance.AttractorSettings.AttractorObject = _currentTransition.Attractor;
			instance.AttractorSettings.GravityConstant = (float)_currentTransition.Orbit.GravConst;
			instance.OrbitData                         = _currentTransition.Orbit.CloneOrbit();
			instance.LockOrbitEditing                  = true;
			instance.OrbitData.SetEccentricAnomaly(_currentTransition.EccAnomalyStart);
			instance.ForceUpdateViewFromInternalState();
			if (_spawnNotifier != null) _spawnNotifier.NotifyBodySpawned(instance);
			return true;
		}
	}
}