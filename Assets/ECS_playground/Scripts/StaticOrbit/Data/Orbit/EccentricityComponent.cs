using Unity.Entities;

namespace ECS_Sandbox
{
	[System.Serializable]
	public struct EccentricityData : IComponentData
	{
		public double Eccentricity;
		public double EccentricitySqr;
	}

	public class EccentricityComponent : ComponentDataWrapper<EccentricityData>
	{
	}
}