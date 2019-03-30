using UnityEngine;

namespace SimpleKeplerOrbits
{
	/// <summary>
	/// Attractor data, necessary for calculation orbit.
	/// </summary>
	[System.Serializable]
	public class AttractorData
	{
		public Transform AttractorObject;
		public float AttractorMass = 1000;
		public float GravityConstant = 0.1f;
	}
}