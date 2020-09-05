using UnityEngine;

namespace SimpleKeplerOrbits.Examples
{
	/// <summary>
	/// Simple camera controller for the example scene.
	/// </summary>
	public class CameraController : MonoBehaviour
	{
		[SerializeField]
		private float _arrowsMovementSpeedMlt = 1f;

		[SerializeField]
		private float _arrowsMovementBoostMlt = 10f;

		[SerializeField]
		private float _rotationSpeedMlt = 1f;

		[SerializeField]
		private Transform _camTransform;

		[SerializeField]
		private float _scrollSpeedMlt = 1f;

		[SerializeField]
		private float _minCamDistance = -50f;

		[SerializeField]
		private float _maxCamDistance = -1f;

		private readonly Vector3 _slideMotionNormal = new Vector3(0, 0, 1);

		private Transform _transform;

		private bool _isRMBPressed;

		private void Awake()
		{
			_transform = transform;
		}

		private void Update()
		{
			UpdateArrowsMovement();
			UpdateMouseRotation();
			UpdateZoom();
		}

		private void UpdateArrowsMovement()
		{
			var horizontal = Input.GetAxis("Horizontal");
			var vertical   = Input.GetAxis("Vertical");
			if (Mathf.Approximately(horizontal, 0) && Mathf.Approximately(vertical, 0)) return;

			var boostMlt = Input.GetKey(KeyCode.LeftShift) ? _arrowsMovementBoostMlt : 1f;
			var speedX   = horizontal * _arrowsMovementSpeedMlt * 2 * Time.deltaTime * boostMlt;
			var speedY   = vertical * _arrowsMovementSpeedMlt * 2 * Time.deltaTime * boostMlt;
			var right    = Vector3.Cross(_slideMotionNormal, _transform.forward).normalized;
			var forward  = Vector3.Cross(right, _slideMotionNormal).normalized;
			transform.Translate(speedY * forward + speedX * right, Space.World);
		}

		private void UpdateMouseRotation()
		{
			if (Input.GetMouseButton(1))
			{
				if (!_isRMBPressed)
				{
					_isRMBPressed = true;
					OnRMBPressStart();
				}

				OnRMBUpdate();
			}
			else
			{
				if (_isRMBPressed)
				{
					_isRMBPressed = false;
					OnRMBRelease();
				}
			}
		}

		private void OnRMBUpdate()
		{
			var axisX = Input.GetAxis("Mouse X");
			var axisY = Input.GetAxis("Mouse Y");
			if (Mathf.Approximately(axisX, 0) && Mathf.Approximately(axisY, 0)) return;

			var scaleMlt = _rotationSpeedMlt * 50 * Time.deltaTime;
			var rotX     = axisX * 5 * scaleMlt;
			var rotY     = axisY * 5 * scaleMlt;
			var rotation = Quaternion.LookRotation(_transform.forward, _slideMotionNormal);
			var delta    = Quaternion.Euler(rotY, -rotX, 0);
			_transform.localRotation = rotation * delta;
		}

		private void OnRMBPressStart()
		{
			Cursor.visible   = false;
			Cursor.lockState = CursorLockMode.Confined;
		}

		private void OnRMBRelease()
		{
			Cursor.visible   = true;
			Cursor.lockState = CursorLockMode.None;
		}

		private void UpdateZoom()
		{
			var axis = Input.GetAxis("Mouse ScrollWheel");
			if (Mathf.Approximately(axis, 0)) return;

			var currentPos = _camTransform.localPosition.z;
			var pos        = currentPos + axis * _scrollSpeedMlt * Mathf.Abs(currentPos);
			_camTransform.localPosition = new Vector3(0, 0, Mathf.Clamp(pos, _minCamDistance, _maxCamDistance));
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.position, transform.position + transform.forward * 10);
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(transform.position, transform.position + transform.up * 2);

			var r = Vector3.Cross(_slideMotionNormal, transform.forward).normalized;
			var f = Vector3.Cross(r, _slideMotionNormal).normalized;

			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(transform.position - r * 1, transform.position + r * 1);
			Gizmos.color = Color.green;
			Gizmos.DrawLine(transform.position, transform.position + f * 4);

			Gizmos.color = Color.red;
			Gizmos.DrawLine(_camTransform.position, _camTransform.position + _camTransform.forward * 6f);
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(_camTransform.position, _camTransform.position + _camTransform.up * 1.2f);

			Gizmos.color = Color.gray;
			Gizmos.DrawLine(new Vector3(), _slideMotionNormal * 10);
		}
	}
}