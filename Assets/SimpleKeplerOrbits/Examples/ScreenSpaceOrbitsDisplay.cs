using System.Collections.Generic;
using UnityEngine;

namespace SimpleKeplerOrbits.Examples
{
	/// <summary>
	/// Scene orbits lines display on camera projection for the demo scene.
	/// </summary>
	/// <remarks>
	/// Crude implementation of centralized orbit lines display system that solves line width issue of the regular orbits lines components.
	/// Utilizes internal pooling of allocated objects to minimize GC usage.
	///
	/// Orbit lines intersection with sides of the screen will have small blinking gaps which caused by intentional flaw in the algorithm.
	/// </remarks>
	public class ScreenSpaceOrbitsDisplay : MonoBehaviour
	{
		public class TargetItem
		{
			public KeplerOrbitMover       Body;
			public KeplerOrbitLineDisplay LineDisplay;
			public Vector3d[]             OrbitPoints;
		}

		[SerializeField] private Camera       _targetCamera;
		[SerializeField] private LineRenderer _lineTemplate;
		[SerializeField] private float        _camDistance;

		private List<LineRenderer>  _instances = new List<LineRenderer>(50);
		private List<TargetItem>    _targets   = new List<TargetItem>(20);
		private List<List<Vector3>> _paths     = new List<List<Vector3>>();
		private List<List<Vector3>> _pool      = new List<List<Vector3>>();

		private void Awake()
		{
			var spawner = GetComponent<ISpawner>();
			if (spawner != null)
			{
				spawner.OnBodySpawnedEvent += AddTargetBody;
			}

			var bodies = GameObject.FindObjectsOfType<KeplerOrbitMover>();
			foreach (var item in bodies)
			{
				AddTargetBody(item);
			}
		}

		private void AddTargetBody(KeplerOrbitMover obj)
		{
			if (obj.AttractorSettings.AttractorObject == null || obj.OrbitData.MeanMotion <= 0) return;

			var lineDisplay = obj.GetComponent<KeplerOrbitLineDisplay>();
			if (lineDisplay != null)
			{
				AddTargetBody(obj, lineDisplay);
			}
		}

		private void AddTargetBody(KeplerOrbitMover body, KeplerOrbitLineDisplay lineDisplay)
		{
			_targets.Add(new TargetItem()
				{
					Body        = body,
					LineDisplay = lineDisplay,
					OrbitPoints = new Vector3d[0],
				}
			);
			lineDisplay.enabled = false;
		}

		private void LateUpdate()
		{
			foreach (var item in _paths)
			{
				ReleaseList(item);
			}

			_paths.Clear();

			List<Vector3> _currentPath = null;
			foreach (var item in _targets)
			{
				var orbitPoints = item.OrbitPoints;
				item.Body.OrbitData.GetOrbitPointsNoAlloc(ref orbitPoints, item.LineDisplay.OrbitPointsCount, new Vector3d(), item.LineDisplay.MaxOrbitWorldUnitsDistance);
				item.OrbitPoints = orbitPoints;
				var attrPos = item.Body.AttractorSettings.AttractorObject.position;
				foreach (var p in orbitPoints)
				{
					var projectedPoint = _targetCamera.WorldToScreenPoint(attrPos + new Vector3((float)p.x, (float)p.y, (float)p.z));
					if (projectedPoint.z > 0)
					{
						projectedPoint.z = _camDistance;
					}
					else
					{
						continue;
					}

					var worldPoint = _targetCamera.ScreenToWorldPoint(projectedPoint);
					if (_currentPath == null)
					{
						_currentPath = GetListFromPool();
					}

					_currentPath.Add(worldPoint);

					if (!IsPointInsideScreen(projectedPoint))
					{
						if (_currentPath.Count > 1)
						{
							_paths.Add(_currentPath);
						}
						else
						{
							ReleaseList(_currentPath);
						}

						_currentPath = null;
					}
				}

				if (_currentPath != null)
				{
					if (_currentPath.Count > 1)
					{
						_paths.Add(_currentPath);
					}
					else
					{
						ReleaseList(_currentPath);
					}

					_currentPath = null;
				}
			}

			int i = 0;
			foreach (var path in _paths)
			{
				LineRenderer instance;
				if (i >= _instances.Count)
				{
					instance = CreateLineRendererInstance();
					_instances.Add(instance);
				}
				else
				{
					instance = _instances[i];
				}

				instance.positionCount = path.Count;
				for (int j = 0; j < path.Count; j++)
				{
					instance.SetPosition(j, path[j]);
				}

				instance.enabled = true;
				i++;
			}

			for (int j = i; j < _instances.Count; j++)
			{
				_instances[j].enabled = false;
			}
		}

		private LineRenderer CreateLineRendererInstance()
		{
			var result = Instantiate(_lineTemplate, _targetCamera.transform);
			result.name = "line";
			result.gameObject.SetActive(true);
			return result;
		}

		private List<Vector3> GetListFromPool()
		{
			if (_pool.Count == 0)
			{
				return new List<Vector3>(100);
			}

			int last   = _pool.Count - 1;
			var result = _pool[last];
			_pool.RemoveAt(last);
			result.Clear();
			return result;
		}

		private void ReleaseList(List<Vector3> list)
		{
			_pool.Add(list);
		}

		private bool IsPointInsideScreen(Vector3 p)
		{
			return p.x >= 0 && p.x < Screen.width && p.y >= 0 && p.y < Screen.height;
		}
	}
}