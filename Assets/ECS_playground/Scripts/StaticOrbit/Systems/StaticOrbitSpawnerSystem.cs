using System.Linq;
using System.Collections.Generic;
using SimpleKeplerOrbits;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.ECS.Rendering;

namespace ECS_Sandbox
{
	public class StaticOrbitSpawnerSystem : ComponentSystem
	{
		private SimpleKeplerOrbits.KeplerOrbitMover[] gameobjects = new SimpleKeplerOrbits.KeplerOrbitMover[0];
		private EntityManager _entityManager;
		private EntityInstanceRenderer _sharedRendererData;

		private Dictionary<KeplerOrbitMover, List<Entity>> _cachedSpawnings = new Dictionary<KeplerOrbitMover, List<Entity>>();
		private int _totalCount = 0;

		private StaticOrbitConfig Config
		{
			get
			{
				if (ECSSceneManager.Instance == null)
				{
					return GameObject.FindObjectOfType<ECSSceneManager>().Config;
				}
				return ECSSceneManager.Instance.Config;
			}
		}

		protected override void OnCreateManager()
		{
			gameobjects = GameObject.FindObjectsOfType<SimpleKeplerOrbits.KeplerOrbitMover>();
			_entityManager = World.Active.GetOrCreateManager<EntityManager>();

			// TODO: Figure out how to do proper entities rendering
			_sharedRendererData = new EntityInstanceRenderer()
			{
				mesh = Config.BodyMesh,
				materials = new Material[] { Config.BodyMaterial },
				castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
				receiveShadows = true,
			};
		}

		protected override void OnUpdate()
		{
			if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.touchCount > 0)
			{
				SpawnEntity();
			}
			if (FPS.Instance != null)
			{
				FPS.Instance.SetElementCount(_totalCount);
			}
		}

		private void SpawnEntity()
		{
			SpawnStaticOrbitItemsFromObjectsOnScene();
		}

		private void SpawnStaticOrbitItemsFromObjectsOnScene()
		{
			_cachedSpawnings.Clear();
			var startList = new List<KeplerOrbitMover>(gameobjects);
			var currentList = new List<KeplerOrbitMover>();
			var completedList = new List<KeplerOrbitMover>();

			int t = 50;
			while (startList.Count > 0 && t > 0)
			{
				bool addRend = UnityEngine.Random.value <= Config.BodyRenderingProbability;
				t--;
				currentList.Clear();
				for (int i = 0; i < startList.Count; i++)
				{
					bool hasParent = startList[i].transform.parent != null && startList[i].transform.parent.GetComponent<KeplerOrbitMover>() != null;
					bool isParentProcessed = hasParent && completedList.Any(v => v.transform == startList[i].transform.parent);
					if (!hasParent || isParentProcessed)
					{
						currentList.Add(startList[i]);
						startList.RemoveAt(i);
						i--;
					}
				}
				List<Entity> entityIteration = new List<Entity>();
				foreach (var item in currentList)
				{
					var parent = item.transform.parent != null ? item.transform.parent.GetComponent<KeplerOrbitMover>() : null;
					var config = Config;
					entityIteration.Clear();
					if (config.EntitiesPerSpawn > 1)
					{
						if (config.RandomPositionDeviationRange != Vector3.zero || config.RandomVelocityDeviationRange != Vector3.zero)
						{
							for (int i = 0; i < config.EntitiesPerSpawn; i++)
							{
								var entity = CreateEntity(addRend);
								_totalCount++;
								entityIteration.Add(entity);
								AssignOrbitDataToEntity(entity, ModifyOrbitDataRandomly(item.OrbitData, Config), addRend);
							}
						}
						else
						{
							var entity = CreateEntity(addRend);
							_totalCount++;
							entityIteration.Add(entity);
							AssignOrbitDataToEntity(entity, item.OrbitData, addRend);
						}
					}
					else
					{
						var entity = CreateEntity(addRend);
						_totalCount++;
						entityIteration.Add(entity);
						AssignOrbitDataToEntity(entity, ModifyOrbitDataRandomly(item.OrbitData, Config), addRend);
					}
					_cachedSpawnings[item] = entityIteration;
					if (parent != null)
					{
						for (int i = 0; i < entityIteration.Count; i++)
						{
							AssignParentToEntity(entityIteration[i], _cachedSpawnings[parent][i]);
						}
					}
					completedList.Add(item);
				}
			}
			_cachedSpawnings.Clear();
		}

		private Entity CreateEntity(bool addRend)
		{
			var componentsTypes = new ComponentType[]{
					//Orbital state components
					typeof(EccentricityData),
					typeof(AnomalyData),
					typeof(AttractorData),
					typeof(OrbitPeriodData),
					typeof(SemiMinorMajorAxisData),
					typeof(OrbitalPositionData)
			};
			if (addRend)
			{
				var arr = new ComponentType[componentsTypes.Length + 3];
				for (int i = 0; i < componentsTypes.Length; i++)
				{
					arr[i] = componentsTypes[i];
				}
				arr[arr.Length - 3] = typeof(ECS_SpaceShooterDemo.EntityInstanceRenderData);
				arr[arr.Length - 2] = typeof(EntityInstanceRendererTransform);
				arr[arr.Length - 1] = typeof(EntityInstanceRenderer);
				componentsTypes = arr;
			}
			return _entityManager.CreateEntity(componentsTypes);
		}

		/// <summary>
		/// Add parent to entity.
		/// </summary>
		/// <remarks>
		/// Local to world child entities position update doesn't work yet.
		/// </remarks>
		private void AssignParentToEntity(Entity entity, Entity parent)
		{
			// TODO: Figure out how to do proper transform hierarchy updates
			_entityManager.AddComponent(entity, typeof(Parent));
			_entityManager.AddComponent(entity, typeof(LocalToParent));
			_entityManager.SetComponentData<Parent>(entity, new Parent() { Value = parent });
		}

		private void AssignOrbitDataToEntity(Entity entity, KeplerOrbitData orbitData, bool addRend)
		{
			_entityManager.SetComponentData<EccentricityData>(entity, new EccentricityData()
			{
				Eccentricity = orbitData.Eccentricity
			});
			_entityManager.SetComponentData<AnomalyData>(entity, new AnomalyData()
			{
				EccentricAnomaly = orbitData.EccentricAnomaly,
				TrueAnomaly = orbitData.TrueAnomaly,
				MeanAnomaly = orbitData.MeanAnomaly,
			});
			_entityManager.SetComponentData<AttractorData>(entity, new AttractorData()
			{
				AttractorMass = orbitData.AttractorMass
			});
			_entityManager.SetComponentData<OrbitPeriodData>(entity, new OrbitPeriodData()
			{
				Period = orbitData.Period,
			});
			_entityManager.SetComponentData<SemiMinorMajorAxisData>(entity, new SemiMinorMajorAxisData()
			{
				SemiMajorAxisBasis = new double3(orbitData.SemiMajorAxisBasis.x, orbitData.SemiMajorAxisBasis.y, orbitData.SemiMajorAxisBasis.z),
				SemiMinorAxisBasis = new double3(orbitData.SemiMinorAxisBasis.x, orbitData.SemiMinorAxisBasis.y, orbitData.SemiMinorAxisBasis.z),
				SemiMinorAxis = orbitData.SemiMinorAxis,
				SemiMajorAxis = orbitData.SemiMajorAxis,
				CenterPoint = new double3(orbitData.CenterPoint.x, orbitData.CenterPoint.y, orbitData.CenterPoint.z),
				SemiMajorAxisPow3 = math.pow(orbitData.SemiMajorAxis, 3)
			});
			_entityManager.SetComponentData<OrbitalPositionData>(entity, new OrbitalPositionData()
			{
				Position = new double3(orbitData.Position.x, orbitData.Position.y, orbitData.Position.z)
			});
			if (addRend)
			{
				_entityManager.SetComponentData<ECS_SpaceShooterDemo.EntityInstanceRenderData>(entity, new ECS_SpaceShooterDemo.EntityInstanceRenderData()
				{
					position = new float3((float)orbitData.Position.x, (float)orbitData.Position.y, (float)orbitData.Position.z),
					forward = math.forward(quaternion.identity),
					up = math.up()
				});
				_entityManager.SetSharedComponentData<EntityInstanceRenderer>(entity, _sharedRendererData);
			}
		}

		private static KeplerOrbitData ModifyOrbitDataRandomly(KeplerOrbitData orbitData, StaticOrbitConfig config)
		{
			bool isChanged = false;
			if (config.RandomPositionDeviationRange != Vector3.zero)
			{
				orbitData = orbitData.Clone();
				orbitData.Position += RandomVectorFromRange(config.RandomPositionDeviationRange);
				isChanged = true;
			}
			if (config.RandomVelocityDeviationRange != Vector3.zero)
			{
				if (!isChanged)
				{
					orbitData = orbitData.Clone();
				}
				orbitData.Velocity += RandomVectorFromRange(config.RandomVelocityDeviationRange);
				isChanged = true;
			}
			if (isChanged)
			{
				orbitData.CalculateNewOrbitData();
			}
			return orbitData;
		}

		private static Vector3d RandomVectorFromRange(Vector3 range)
		{
			var maxX = range.x;
			var minX = maxX - range.x * 2;

			var maxY = range.y;
			var minY = maxY - range.y * 2;

			var maxZ = range.z;
			var minZ = maxZ - range.z * 2;

			return new Vector3d(
					UnityEngine.Random.Range(minX, maxX),
					UnityEngine.Random.Range(minY, maxY),
					UnityEngine.Random.Range(minZ, maxZ));
		}
	}
}