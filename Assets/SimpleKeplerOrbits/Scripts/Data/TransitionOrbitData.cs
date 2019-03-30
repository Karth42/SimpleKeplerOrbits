using System.Collections.Generic;
using UnityEngine;

namespace SimpleKeplerOrbits
{
	[System.Serializable]
	public class TransitionOrbitData
	{
		public Transform Attractor;
		public KeplerOrbitData Orbit;
		public List<Vector3d> ImpulseDifferences;
		public float TotalDeltaV;
		public float Duration;
		public float EccAnomalyStart;
		public float EccAnomalyEnd;
	}
}