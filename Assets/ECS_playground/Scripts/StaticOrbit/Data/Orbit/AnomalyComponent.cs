using Unity.Entities;

namespace ECS_Sandbox
{
	public struct AnomalyData : IComponentData
	{
		public double MeanAnomaly;
		public double EccentricAnomaly;
		public double TrueAnomaly;
	}

	public class AnomalyComponent : ComponentDataWrapper<AnomalyData>
	{
	}
}