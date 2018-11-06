using UnityEngine;

namespace ECS_Sandbox
{
	[CreateAssetMenu(fileName = "Static Orbit Config", menuName = "CustomConfigs/Static Orbit Config", order = 0)]
	public class StaticOrbitConfig : ScriptableObject
	{
		public Mesh BodyMesh;
		public Material BodyMaterial;
		public double GravConstant = 0.1;
		public float TimeScale = 1;


		public Vector3 RandomPositionDeviationRange;
		public Vector3 RandomVelocityDeviationRange;

		public int EntitiesPerSpawn = 10;
	}
}