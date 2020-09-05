using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleKeplerOrbits.Examples
{
	/// <summary>
	/// Controller for spawning bodies by orbital elements values. Supports JPL database input.
	/// </summary>
	[RequireComponent(typeof(SpawnNotifier))]
	public class JPLOrbitsLoader : MonoBehaviour
	{
		[Serializable]
		public class JPLListContainer
		{
			public JPLElementsData[] OrbitsData = new JPLElementsData[0];
		}

		/// <summary>
		/// Data container for single body orbit.
		/// </summary>
		[Serializable]
		public class JPLElementsData
		{
			public string BodyName;
			public string AttractorName;
			public float  AttractorMass;

			/// <summary>
			/// Eccentricity.
			/// </summary>
			[Tooltip("Eccentricity")]
			public double EC;

			/// <summary>
			/// Inclination (degrees).
			/// </summary>
			[Tooltip("Inclination")]
			public double IN;

			/// <summary>
			/// Longitude of Ascending Node (degrees).
			/// </summary>
			[Tooltip("Ascending node longitude")]
			public double OM;

			/// <summary>
			/// Argument of perifocus (degrees).
			/// </summary>
			[Tooltip("Argument of periapsis")]
			public double W;

			/// <summary>
			/// Mean anomaly (degrees).
			/// </summary>
			[Tooltip("Mean anomaly")]
			public double MA;

			/// <summary>
			/// Semi-major axis (au).
			/// </summary>
			[Tooltip("Semi-major axis")]
			public double A;

			/// <summary>
			/// Diameter for the body transform scale calulation.
			/// Scale is logarithmic.
			/// </summary>
			public double Diameter = 1;

			/// <summary>
			/// Distance multiplier;
			/// </summary>
			public double RangeMlt = 1;

			/// <summary>
			/// Material tint color.
			/// </summary>
			public Color Color = Color.white;

			/// <summary>
			/// Custom type param. Used to switch mesh material for body.
			/// </summary>
			public int Type = 0;
		}

		public enum LoadingType
		{
			Json,
			Scene,
		}

		public LoadingType LoadingDataSource;

		/// <summary>
		/// Gravitational constant. In this context plays role of speed muliplier.
		/// </summary>
		public double GConstant = 100;

		/// <summary>
		/// Orbit scale multiplier: world units per 1 au.
		/// </summary>
		public float UnitsPerAU = 1f;

		/// <summary>
		/// Body scale multiplier: world scale unit per diameter unit.
		/// </summary>
		public float ScalePerDiameter = 1f;

		/// <summary>
		/// Each body can have it's own multiplier value for semi major axis for better visualization.
		/// </summary>
		public bool IsAllowedRangeScaleMltPerBody = true;

		/// <summary>
		/// Astronomical unit in SI units.
		/// </summary>
		/// <remarks>
		/// Used to calculate real scale orbit periods.
		/// </remarks>
		public double AU = 1.495978707e11;

		public KeplerOrbitMover BodyTemplate;

		public TextAsset JsonData;

		public JPLElementsData[] ElementsTable;

		public Material MainAttractorMaterial;

		private readonly List<KeplerOrbitMover> _spawnedInstances = new List<KeplerOrbitMover>(20);
		private SpawnNotifier _spawnNotifier;

		private void Start()
		{
			_spawnNotifier = GetComponent<SpawnNotifier>();
			switch (LoadingDataSource)
			{
				case LoadingType.Json:
					SpawnFromJsonData();
					break;
				case LoadingType.Scene:
					SpawnFromSceneData();
					break;
			}
		}

		[ContextMenu("Spawn from json")]
		private void SpawnFromJsonData()
		{
			if (JsonData != null)
			{
				var data = JsonUtility.FromJson<JPLListContainer>(JsonData.text);
				if (data != null)
				{
					SpawnAll(data.OrbitsData);
				}
			}
		}

		[ContextMenu("Spawn from component")]
		private void SpawnFromSceneData()
		{
			SpawnAll(ElementsTable);
		}

		[ContextMenu("Destroy all bodies")]
		private void DestroyAll()
		{
			var instances = GameObject.FindObjectsOfType<KeplerOrbitMover>();
			for (int i = 0; i < instances.Length; i++)
			{
				if (instances[i].gameObject.activeSelf)
				{
					if (Application.isPlaying)
					{
						Destroy(instances[i].gameObject);
					}
					else
					{
						DestroyImmediate(instances[i].gameObject);
					}
				}
			}
		}

		private void SpawnAll(JPLElementsData[] inputData)
		{
			// Spawn all bodies in multiple passes to resolve parent-child connections.
			// All spawned bodies are instantiated from signle template, which has no visual components attached,
			// because this example is designed only for simplest orbits loading process demonstration.

			if (inputData == null || inputData.Length == 0) return;
			List<JPLElementsData> spawnOrder = new List<JPLElementsData>(inputData);
			ClearAllInstances();
			bool isAnySpawned = true;

			while (spawnOrder.Count > 0 && isAnySpawned)
			{
				isAnySpawned = false;
				for (int i = 0; i < spawnOrder.Count; i++)
				{
					var spawnItem     = spawnOrder[0];
					var attractorName = spawnItem.AttractorName != null ? spawnItem.AttractorName.Trim() : "";

					bool Predicate(KeplerOrbitMover s)
					{
						return s.name == attractorName;
					}

					bool isAttractorSpawned = string.IsNullOrEmpty(attractorName) || _spawnedInstances.Any(Predicate);
					if (isAttractorSpawned)
					{
						KeplerOrbitMover body = SpawnBody(spawnItem, attractorName);
						spawnOrder.RemoveAt(0);
						i--;
						_spawnedInstances.Add(body);
						_spawnNotifier.NotifyBodySpawned(body);
						isAnySpawned = true;
					}
					else
					{
						// If attractor not spawned yet, then wait for next spawn cycle pass.
					}
				}
			}

			if (!isAnySpawned && spawnOrder.Count > 0)
			{
				Debug.LogError("Couldn't spawn " + spawnOrder.Count + " because assigned attractor was not found");
			}
		}

		private KeplerOrbitMover FindBodyInstance(string str)
		{
			bool FindPredicate(KeplerOrbitMover s)
			{
				return s.name == str;
			}

			var result = string.IsNullOrEmpty(str)
				? null
				: _spawnedInstances.First(FindPredicate);
			return result;
		}

		private KeplerOrbitMover SpawnBody(JPLElementsData data, string attractorName)
		{
			KeplerOrbitMover attractor = FindBodyInstance(attractorName);
			KeplerOrbitMover body      = Instantiate(BodyTemplate, parent: attractor == null ? null : attractor.transform);
			if (!string.IsNullOrEmpty(data.BodyName))
			{
				body.name = data.BodyName.Trim();
			}

			Transform bodyTransform = body.transform;

			if (attractor != null)
			{
				body.AttractorSettings.AttractorMass = data.AttractorMass;
			}

			double unitsPerAU = UnitsPerAU;
			if (IsAllowedRangeScaleMltPerBody)
			{
				// By default MLT value is 1,
				// but for moons it may be larger than 1 for better visualization.
				unitsPerAU *= data.RangeMlt;
			}

			// G constant is used as free parameter to fixate orbits periods values while SemiMajor axis parameter is adjusted for the scene.
			double compensatedGConst = GConstant / Math.Pow(AU / unitsPerAU, 3d);

			body.AttractorSettings.GravityConstant = (float)compensatedGConst;
			body.AttractorSettings.AttractorObject = attractor == null ? null : attractor.transform;
			body.OrbitData = new KeplerOrbitData(
				eccentricity: data.EC,
				semiMajorAxis: data.A * unitsPerAU,
				meanAnomalyDeg: data.MA,
				inclinationDeg: data.IN,
				argOfPerifocusDeg: data.W,
				ascendingNodeDeg: data.OM,
				attractorMass: body.AttractorSettings.AttractorMass,
				gConst: compensatedGConst);
			if (attractor != null && data.A > 0)
			{
				body.ForceUpdateViewFromInternalState();
			}
			else
			{
				body.enabled = false;
			}

			var mat = data.Type == 1 ? MainAttractorMaterial : null;
			SetBodyColorAndDiameter(bodyTransform, data.Color, mat, (float)data.Diameter, ScalePerDiameter);
			body.gameObject.SetActive(true);
			return body;
		}

		private static void SetBodyColorAndDiameter(Transform body, Color col, Material mat, float diameter, float scaleMlt)
		{
			var renderer = body.GetComponentInChildren<MeshRenderer>();
			if (renderer != null)
			{
				renderer.material       = mat;
				renderer.material.color = col;
				if (diameter <= 0)
				{
					renderer.enabled = false;
				}
				else
				{
					var scale = Mathf.Log10(diameter + 1.3f) * scaleMlt;
					renderer.transform.localScale = new Vector3(scale, scale, scale);
				}
			}
		}

		private void ClearAllInstances()
		{
			foreach (var item in _spawnedInstances)
			{
				if (item != null)
				{
					Destroy(item.gameObject);
				}
			}

			_spawnedInstances.Clear();
		}
	}
}