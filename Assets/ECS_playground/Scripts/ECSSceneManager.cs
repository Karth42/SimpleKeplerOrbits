using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;
using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.ECS.Rendering;

namespace ECS_Sandbox
{
	public class ECSSceneManager : MonoBehaviour
	{
		public static ECSSceneManager Instance { get; private set; }

		public StaticOrbitConfig Config;
		private EntityManager _entityManager;
		private List<ScriptBehaviourManager> _gameSystemlist = new List<ScriptBehaviourManager>();

		private void Awake()
		{
			Instance = this;
		}

		private void OnEnable()
		{
			if (Instance == null)
			{
				Instance = this;
			}

			_entityManager = World.Active.GetOrCreateManager<EntityManager>();

			CreateGameSystems();
		}

		private void CreateGameSystems()
		{
			//_gameSystemlist.Add(World.Active.GetOrCreateManager(typeof(StaticOrbitMotionSystem)));
			ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.Active);
		}

		private void OnDisable()
		{
			if (World.Active != null)
			{
				for (int i = 0; i < _gameSystemlist.Count; i++)
				{
					World.Active.DestroyManager(_gameSystemlist[i]);
				}

				ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.Active);
				_gameSystemlist.Clear();
			}

			//if (_entityManager.IsCreated)
			//{
			//	_entityManager.DestroyEntity(_entityManager.GetAllEntities(Unity.Collections.Allocator.Temp));
			//}
		}

		private void OnDestroy()
		{
			if (Instance == this)
			{
				Instance = null;
			}
		}
	}
}
