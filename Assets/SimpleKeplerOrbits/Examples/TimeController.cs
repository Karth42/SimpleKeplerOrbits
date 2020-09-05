using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleKeplerOrbits.Examples
{
	/// <summary>
	/// Simple orbits time controller.
	/// Allows to change current time for all orbits on scene.
	/// </summary>
	/// <remarks>
	/// Only uses one epoch for orbits state calculations
	/// which is fine enough for demonstration purposes.
	/// </remarks>
	[RequireComponent(typeof(SpawnNotifier))]
	public class TimeController : MonoBehaviour
	{
		private struct BodyTimeData
		{
			public KeplerOrbitMover body;
			public double           initialMeanAnomaly;
		}

		private readonly List<BodyTimeData> _bodies = new List<BodyTimeData>(20);

		[Header("Epoch origin timestamp")]
		[SerializeField]
		private int _epochYear;

		[SerializeField]
		private int _epochMonth;

		[SerializeField]
		private int _epochDay;

		[SerializeField]
		private int _epochHour;

		[SerializeField]
		private int _epochMinute;

		private float _currentTimeScale = 1f;

		private SpawnNotifier _spawnNotifier;
		private DateTime      _epochDate;
		private DateTime      _currentTime;

		public DateTime CurrentTime
		{
			get { return _currentTime; }
		}

		private void Awake()
		{
			_spawnNotifier                    =  GetComponent<SpawnNotifier>();
			_spawnNotifier.onBodySpawnedEvent += OnBodySpawned;

			var instances = GameObject.FindObjectsOfType<KeplerOrbitMover>();
			foreach (var item in instances)
			{
				AddBody(item);
			}

			_epochDate = new DateTime(_epochYear, _epochMonth, _epochDay, _epochHour, _epochMinute, 0, DateTimeKind.Utc);
		}

		private void OnDestroy()
		{
			if (_spawnNotifier != null)
			{
				_spawnNotifier.onBodySpawnedEvent -= OnBodySpawned;
			}
		}

		private IEnumerator Start()
		{
			yield return null;
			SetCurrentGlobalTime();
		}

		private void Update()
		{
			_currentTime = _currentTime.AddSeconds(_currentTimeScale * Time.deltaTime);
		}

		private void OnBodySpawned(KeplerOrbitMover b)
		{
			if (b != null)
			{
				AddBody(b);
			}
		}

		private void AddBody(KeplerOrbitMover b)
		{
			// Body's initial mean anomaly is taken as origin point for the epoch.
			// And the origin timestamp for the epoch is defined in this component's parameters.
			_bodies.Add(new BodyTimeData()
			{
				body               = b,
				initialMeanAnomaly = b.OrbitData.MeanAnomaly
			});

			b.TimeScale = _currentTimeScale;
		}

		public void SetCurrentGlobalTime()
		{
			SetGlobalTime(DateTime.UtcNow);
		}

		public void SetGlobalTime(DateTime time)
		{
			bool isAnyNull = false;

			_currentTime = time;
			var elapsedTime = (time - _epochDate).TotalSeconds;

			foreach (var item in _bodies)
			{
				if (item.body == null)
				{
					isAnyNull = true;
					continue;
				}

				var value = item.initialMeanAnomaly + elapsedTime * item.body.OrbitData.MeanMotion;
				item.body.OrbitData.SetMeanAnomaly(value);
				if (item.body.AttractorSettings.AttractorObject != null)
				{
					item.body.ForceUpdateViewFromInternalState();
				}
			}

			if (isAnyNull)
			{
				bool Predicate(BodyTimeData b)
				{
					return b.body == null;
				}

				_bodies.RemoveAll(Predicate);
			}
		}

		public void SetTimescale(float timescale)
		{
			_currentTimeScale = timescale;
			foreach (var item in _bodies)
			{
				if (item.body != null)
				{
					item.body.TimeScale = timescale;
				}
			}
		}
	}
}