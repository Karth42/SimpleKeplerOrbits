using UnityEngine;
using UnityEngine.UI;

namespace ECS_Sandbox
{
	public class FPS : MonoBehaviour
	{
		public static FPS Instance;

		public Text countText;
		public Text fpsText;

		public Transform RotationRoot;
		public float RotationSpeed = 100f;

		private float _deltaTime;
		private int _totalCount = 0;
		private int _lastTotalCount = 0;

		public void SetElementCount(int count)
		{
			_totalCount = count;
		}

		private void Awake()
		{
			Instance = this;
			DisplayCount();
		}

		private void Update()
		{
			_deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
			DisplayFPS();
			if (_lastTotalCount != _totalCount)
			{
				_lastTotalCount = _totalCount;
				DisplayCount();
			}
			if (RotationRoot != null)
			{
				float x = Input.GetAxis("Vertical");
				float y = Input.GetAxis("Horizontal");
				if (x != 0 || y != 0)
				{
					float dt = Time.deltaTime * RotationSpeed;
					RotationRoot.Rotate(new Vector3(x * dt, y * dt, 0));
				}
			}
		}

		private void DisplayFPS()
		{
			float msec = _deltaTime * 1000.0f;
			float fps = 1.0f / _deltaTime;
			fpsText.text = string.Format("FPS: {0:00.} ({1:00.0} ms)", fps, msec);
		}

		private void DisplayCount()
		{
			countText.text = "Entities count: " + _totalCount.ToString();
		}
	}
}