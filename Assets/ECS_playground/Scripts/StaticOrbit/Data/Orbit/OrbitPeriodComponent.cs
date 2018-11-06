using Unity.Entities;

namespace ECS_Sandbox
{
	public struct OrbitPeriodData : IComponentData
	{
		public double Period;
	}

	public class OrbitPeriodComponent : ComponentDataWrapper<OrbitPeriodData>
	{
	}
}