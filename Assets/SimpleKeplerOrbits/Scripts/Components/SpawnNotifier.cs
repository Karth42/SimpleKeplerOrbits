using System;
using UnityEngine;

namespace SimpleKeplerOrbits
{
	/// <summary>
	/// Component for tracking kepler bodies creation.
	/// </summary>
	public class SpawnNotifier : MonoBehaviour
	{
		private static event Action<KeplerOrbitMover> _onGlobalBodySpawnedEvent;

		public event Action<KeplerOrbitMover> onBodySpawnedEvent;

		private void Awake()
		{
			_onGlobalBodySpawnedEvent += OnGlobalNotify;
		}

		private void OnDestroy()
		{
			_onGlobalBodySpawnedEvent -= OnGlobalNotify;
		}

		private void OnGlobalNotify(KeplerOrbitMover b)
		{
			onBodySpawnedEvent?.Invoke(b);
		}

		public void NotifyBodySpawned(KeplerOrbitMover b)
		{
			_onGlobalBodySpawnedEvent?.Invoke(b);
		}
	}
}