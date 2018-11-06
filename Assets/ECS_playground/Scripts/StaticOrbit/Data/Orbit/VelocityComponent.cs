using Unity.Entities;
using Unity.Mathematics;

namespace ECS_Sandbox
{
	[System.Serializable]
	public struct VelocityData : IComponentData
	{
		public double3 Velocity;
	}

	public class VelocityComponent : ComponentDataWrapper<VelocityData>
	{
	}
}