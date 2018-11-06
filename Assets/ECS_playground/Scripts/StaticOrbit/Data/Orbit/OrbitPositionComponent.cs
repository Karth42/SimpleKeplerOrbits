using Unity.Entities;
using Unity.Mathematics;

namespace ECS_Sandbox
{
	[System.Serializable]
	public struct OrbitalPositionData : IComponentData
	{
		public double3 Position;
	}

	public class OrbitalPositionComponent : ComponentDataWrapper<OrbitalPositionData>
	{
	}
}