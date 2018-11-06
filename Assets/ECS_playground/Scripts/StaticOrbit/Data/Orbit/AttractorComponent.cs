using Unity.Entities;

namespace ECS_Sandbox
{
	public struct AttractorData : IComponentData
	{
		public double AttractorMass;
	}

	public class AttractorMassComponent : ComponentDataWrapper<AttractorData>
	{
	}
}