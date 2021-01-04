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
	/// </remarks>
	[RequireComponent(typeof(SpawnNotifier))]
	public class ScreenSpaceOrbitsDisplay : MonoBehaviour
	{
		public class TargetItem
		{
			public KeplerOrbitMover       Body;
			public KeplerOrbitLineDisplay LineDisplay;
			public Vector3d[]             OrbitPoints;
		}

		private struct SegmentPointData
		{
			public Vector3 worldPoint;
			public bool    isVisible;
			public bool    isProcessed;

			public SegmentPointData(Vector3 worldPoint, bool isVisible, bool isProcessed)
			{
				this.worldPoint  = worldPoint;
				this.isVisible   = isVisible;
				this.isProcessed = isProcessed;
			}
		}

		[SerializeField] private Camera       _targetCamera;
		[SerializeField] private LineRenderer _lineTemplate;
		[SerializeField] private float        _camDistance;
		[SerializeField] private bool         _isDebugMode;

		private List<LineRenderer>                      _instances = new List<LineRenderer>(50);
		private List<TargetItem>                        _targets   = new List<TargetItem>(20);
		private Dictionary<string, List<List<Vector3>>> _paths     = new Dictionary<string, List<List<Vector3>>>();
		private List<List<Vector3>>                     _pool      = new List<List<Vector3>>();
		private SpawnNotifier                           _spawnerNotifier;
		private List<SegmentPointData>                  _tempPoints = new List<SegmentPointData>(150);

		private void Awake()
		{
			_spawnerNotifier                    =  GetComponent<SpawnNotifier>();
			_spawnerNotifier.onBodySpawnedEvent += AddTargetBody;

			var bodies = GameObject.FindObjectsOfType<KeplerOrbitMover>();
			foreach (var item in bodies)
			{
				AddTargetBody(item);
			}
		}

		private void OnDestroy()
		{
			if (_spawnerNotifier != null)
			{
				_spawnerNotifier.onBodySpawnedEvent -= AddTargetBody;
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
			var allVisibleSegments = _paths;

			foreach (var kv in allVisibleSegments)
			{
				foreach (var points in kv.Value)
				{
					ReleaseList(points);
				}

				kv.Value.Clear();
			}

			foreach (var item in _targets)
			{
				if (!item.Body.enabled || !item.Body.gameObject.activeInHierarchy) continue;
				var orbitPoints = item.OrbitPoints;
				item.Body.OrbitData.GetOrbitPointsNoAlloc(ref orbitPoints, item.LineDisplay.OrbitPointsCount, new Vector3d(), item.LineDisplay.MaxOrbitWorldUnitsDistance);
				item.OrbitPoints = orbitPoints;
				var attrPos         = item.Body.AttractorSettings.AttractorObject.position;
				var projectedPoints = _tempPoints;

				// Project all orbit points onto current camera view plane and assign isVisible flag for each point.
				ConvertOrbitPointsToProjectedPoints(orbitPoints, attrPos, _targetCamera, _camDistance, projectedPoints);

				// Mark isVisible to all not visible points at distance 1 from nearest initially visible point.
				ExpandVisibleSegmentsSizeByOne(projectedPoints);

				var bodyName = _isDebugMode ? item.Body.name : "";

				// Convert set of projected points (visible and not visible) into list of unbroken visible segments. 
				ExtractVisibleSegmentsFromProjectedOrbitPoints(bodyName, allVisibleSegments, projectedPoints);
			}

			RefreshLineRenderersForCurrentSegments(_isDebugMode, allVisibleSegments, _instances);
		}

		private static void ConvertOrbitPointsToProjectedPoints(Vector3d[] orbitPoints, Vector3 attractorPos, Camera targetCamera, float camDistance, List<SegmentPointData> projectedPoints)
		{
			projectedPoints.Clear();
			if (projectedPoints.Capacity < orbitPoints.Length)
			{
				projectedPoints.Capacity = orbitPoints.Length;
			}

			var maxDistance = new Vector3();
			foreach (var p in orbitPoints)
			{
				var halfP       = new Vector3((float)p.x, (float)p.y, (float)p.z);
				var worldPoint  = attractorPos + halfP;
				var screenPoint = targetCamera.WorldToScreenPoint(worldPoint);
				if (screenPoint.z > 0)
				{
					screenPoint.z = camDistance;
					var projectedPoint = targetCamera.ScreenToWorldPoint(screenPoint);
					var isVisible      = IsPointInsideScreen(screenPoint);
					projectedPoints.Add(new SegmentPointData(projectedPoint, isVisible, isProcessed: false));
				}
				else
				{
					projectedPoints.Add(new SegmentPointData(new Vector3(), false, false));
				}

				var diff = projectedPoints[projectedPoints.Count - 1].worldPoint - projectedPoints[0].worldPoint;

				if (diff.x > maxDistance.x) maxDistance.x = diff.x;
				if (diff.y > maxDistance.y) maxDistance.y = diff.y;
				if (diff.z > maxDistance.z) maxDistance.z = diff.z;
			}

			if (maxDistance.magnitude < 0.001f)
			{
				projectedPoints.Clear();
			}
		}

		private static void ExpandVisibleSegmentsSizeByOne(List<SegmentPointData> projectedPoints)
		{
			int segmentCount = 0;
			for (var i = 0; i < projectedPoints.Count; i++)
			{
				var pointData = projectedPoints[i];
				if (pointData.isProcessed)
				{
					segmentCount = 0;
					continue;
				}

				if (pointData.isVisible)
				{
					if (segmentCount == 0)
					{
						if (i > 0)
						{
							var prevPointData = projectedPoints[i - 1];
							if (prevPointData.worldPoint != Vector3.zero)
							{
								segmentCount++;
								prevPointData.isVisible   = true;
								prevPointData.isProcessed = true;
								projectedPoints[i - 1]    = prevPointData;
							}
						}
					}

					segmentCount++;
					pointData.isProcessed = true;
					projectedPoints[i]    = pointData;
				}
				else
				{
					if (segmentCount > 0)
					{
						if (pointData.worldPoint != Vector3.zero)
						{
							pointData.isVisible   = true;
							pointData.isProcessed = true;
							projectedPoints[i]    = pointData;
						}

						segmentCount = 0;
					}
				}
			}

			for (var i = 0; i < projectedPoints.Count; i++)
			{
				var pointData = projectedPoints[i];
				pointData.isProcessed = false;
				projectedPoints[i]    = pointData;
			}
		}

		private void ExtractVisibleSegmentsFromProjectedOrbitPoints(
			string                                  bodyName,
			Dictionary<string, List<List<Vector3>>> allProjectedPaths,
			List<SegmentPointData>                  projectedPoints)
		{
			List<Vector3> currentSegment = null;

			for (var i = 0; i < projectedPoints.Count; i++)
			{
				var pointData = projectedPoints[i];
				if (pointData.isVisible && !pointData.isProcessed)
				{
					if (currentSegment == null)
					{
						currentSegment = GetListFromPool();
					}

					if (currentSegment.Capacity < projectedPoints.Count)
					{
						currentSegment.Capacity = projectedPoints.Count;
					}

					currentSegment.Add(pointData.worldPoint);
					pointData.isProcessed = true;
					projectedPoints[i]    = pointData;
				}
				else
				{
					FinilizeCurrentSegment(bodyName, ref currentSegment, allProjectedPaths);
				}
			}

			FinilizeCurrentSegment(bodyName, ref currentSegment, allProjectedPaths);
		}

		private void FinilizeCurrentSegment(string bodyName, ref List<Vector3> currentSegment, Dictionary<string, List<List<Vector3>>> targetPaths)
		{
			if (currentSegment != null)
			{
				if (currentSegment.Count > 1)
				{
					List<List<Vector3>> list = null;

					if (targetPaths.ContainsKey(bodyName))
					{
						list = targetPaths[bodyName];
					}
					else
					{
						list                  = new List<List<Vector3>>();
						targetPaths[bodyName] = list;
					}

					list.Add(currentSegment);
				}
				else
				{
					ReleaseList(currentSegment);
				}

				currentSegment = null;
			}
		}

		private void RefreshLineRenderersForCurrentSegments(
			bool                                    isDebugMode,
			Dictionary<string, List<List<Vector3>>> allSegments,
			List<LineRenderer>                      instances)
		{
			var i = 0;
			foreach (var kv in allSegments)
			{
				var bodyName = kv.Key;
				foreach (var segment in kv.Value)
				{
					LineRenderer instance;
					if (i >= instances.Count)
					{
						instance = CreateLineRendererInstance();
						instances.Add(instance);
					}
					else
					{
						instance = instances[i];
					}

					instance.positionCount = segment.Count;
					for (int j = 0; j < segment.Count; j++)
					{
						instance.SetPosition(j, segment[j]);
					}

					instance.enabled = true;

					if (isDebugMode)
					{
						instance.name = "line_" + bodyName;
						instance.gameObject.SetActive(true);
					}

					i++;
				}
			}

			for (int j = i; j < instances.Count; j++)
			{
				instances[j].enabled = false;
				if (isDebugMode)
				{
					instances[j].name = "line";
					instances[j].gameObject.SetActive(false);
				}
			}
		}

		private LineRenderer CreateLineRendererInstance()
		{
			var result = Instantiate(_lineTemplate, _targetCamera.transform);
			result.gameObject.SetActive(true);
			return result;
		}

		private List<Vector3> GetListFromPool()
		{
			if (_pool.Count == 0)
			{
				return new List<Vector3>();
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

		private static bool IsPointInsideScreen(Vector3 p)
		{
			return p.x >= 0 && p.x < Screen.width && p.y >= 0 && p.y < Screen.height;
		}
	}
}