using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleKeplerOrbits.Examples
{
	/// <summary>
	/// UI controller component for the example scene.
	/// </summary>
	public class TimeControlPanel : MonoBehaviour
	{
		[SerializeField] private TimeController _timeController;
		[SerializeField] private Slider         _sliderTimescale;
		[SerializeField] private InputField     _inputTimescale;
		[SerializeField] private Button         _buttonResetTimescale;
		[SerializeField] private InputField     _inputYear;
		[SerializeField] private InputField     _inputMonth;
		[SerializeField] private InputField     _inputDay;
		[SerializeField] private InputField     _inputHour;
		[SerializeField] private InputField     _inputMin;
		[SerializeField] private InputField     _inputSec;
		[SerializeField] private Button         _buttonSetNow;
		[SerializeField] private float          _maxTimeScale = 5e7f;

		private bool _isRefreshing;

		private void Awake()
		{
			_sliderTimescale.minValue = -Mathf.Abs(_maxTimeScale);
			_sliderTimescale.maxValue = Mathf.Abs(_maxTimeScale);
			_sliderTimescale.value    = 1f;
			_sliderTimescale.onValueChanged.AddListener((v) =>
			{
				if (_isRefreshing) return;

				_isRefreshing = true;
				_timeController.SetTimescale(v);
				_inputTimescale.text = v.ToString(CultureInfo.InvariantCulture);
				_isRefreshing        = false;
			});

			_buttonResetTimescale.onClick.AddListener(() =>
			{
				_isRefreshing = true;
				_timeController.SetTimescale(1f);
				_sliderTimescale.value = 1f;
				_inputTimescale.text   = "1";
				_isRefreshing          = false;
			});

			_inputTimescale.onEndEdit.AddListener((str) =>
			{
				if (_isRefreshing) return;

				_isRefreshing = true;
				var f = ParseFloat(str);
				_timeController.SetTimescale(f);
				_sliderTimescale.value = f;
				_isRefreshing          = false;
			});

			_inputYear.onEndEdit.AddListener(ApplyDateFromInputState);
			_inputMonth.onEndEdit.AddListener(ApplyDateFromInputState);
			_inputDay.onEndEdit.AddListener(ApplyDateFromInputState);
			_inputHour.onEndEdit.AddListener(ApplyDateFromInputState);
			_inputMin.onEndEdit.AddListener(ApplyDateFromInputState);
			_inputSec.onEndEdit.AddListener(ApplyDateFromInputState);

			_buttonSetNow.onClick.AddListener(() =>
			{
				_timeController.SetCurrentGlobalTime();
				RefreshTimestampDisplay();
			});
		}

		private void Update()
		{
			RefreshTimestampDisplay();
		}

		private void RefreshTimestampDisplay()
		{
			_isRefreshing = true;
			var time                                     = _timeController.CurrentTime;
			if (!_inputYear.isFocused) _inputYear.text   = time.Year.ToString();
			if (!_inputMonth.isFocused) _inputMonth.text = time.Month.ToString();
			if (!_inputDay.isFocused) _inputDay.text     = time.Day.ToString();
			if (!_inputHour.isFocused) _inputHour.text   = time.Hour.ToString();
			if (!_inputMin.isFocused) _inputMin.text     = time.Minute.ToString();
			if (!_inputSec.isFocused) _inputSec.text     = time.Second.ToString();
			_isRefreshing = false;
		}

		private void ApplyDateFromInputState(string _)
		{
			var year  = ParseInt(_inputYear.text);
			var month = ParseInt(_inputMonth.text);
			var day   = ParseInt(_inputDay.text);
			var hour  = ParseInt(_inputHour.text);
			var min   = ParseInt(_inputMin.text);
			var sec   = ParseInt(_inputSec.text);

			_timeController.SetGlobalTime(new DateTime(year, month, day, hour, min, sec));
			RefreshTimestampDisplay();
		}

		private static int ParseInt(string str)
		{
			int result = 0;
			int.TryParse(str, out result);
			return result;
		}

		private static float ParseFloat(string str)
		{
			float result = 0;
			float.TryParse(str, out result);
			return result;
		}
	}
}