using UnityEngine;
using UnityEditor;

namespace SimpleKeplerOrbits
{
	[CustomEditor(typeof(EllipticInterceptionSolver), isFallback = false)]
	[CanEditMultipleObjects]
	public class EllipticInterceptionSolverEditor : Editor
	{
		private EllipticInterceptionSolver _target;

		private void OnEnable()
		{
			_target = target as EllipticInterceptionSolver;
		}

		public override void OnInspectorGUI()
		{
			if (_target.TargetDuration < 0)
			{
				_target.TargetDuration = 0;
			}
			DrawDefaultInspector();
			GUILayout.BeginVertical("box");
			{
				EditorGUILayout.LabelField("Transition duration", (_target.CurrentTransition == null ? "0" : _target.CurrentTransition.Duration.ToString()));
				EditorGUILayout.LabelField("Departure delta-v required", (_target.CurrentTransition == null || _target.CurrentTransition.ImpulseDifferences == null || _target.CurrentTransition.ImpulseDifferences.Count < 1 ? "-" : _target.CurrentTransition.ImpulseDifferences[0].magnitude.ToString()));
				EditorGUILayout.LabelField("Arrival delta-v required", (_target.CurrentTransition == null || _target.CurrentTransition.ImpulseDifferences == null || _target.CurrentTransition.ImpulseDifferences.Count < 1 ? "-" : _target.CurrentTransition.ImpulseDifferences[1].magnitude.ToString()));
				EditorGUILayout.LabelField("Total delta-v required", (_target.CurrentTransition == null ? "0" : _target.CurrentTransition.TotalDeltaV.ToString()));
				EditorGUILayout.LabelField("Eccentricity", (_target.CurrentTransition == null || _target.CurrentTransition.Orbit == null  ? "0" : _target.CurrentTransition.Orbit.Eccentricity.ToString()));
				EditorGUILayout.LabelField("SemiMajor axis", (_target.CurrentTransition == null || _target.CurrentTransition.Orbit == null  ? "0" : _target.CurrentTransition.Orbit.SemiMajorAxis.ToString()));
			}
			GUILayout.EndVertical();
			if (_target.CurrentTransition == null)
			{
				GUI.enabled = false;
			}
			if (GUILayout.Button(new GUIContent( "Set real target duration", "Assing calculated transition duration to preferred Duration property. May require multiple iterations (button presses) to find equilibrium between these two values.")))
			{
				_target.TargetDuration = _target.CurrentTransition.Duration;
				EditorUtility.SetDirty(_target);
			}

			GUI.enabled = true;

			if (_target.CurrentTransition == null)
			{
				GUI.enabled = false;
			}

			if (GUILayout.Button(new GUIContent( "Spawn body from current trajectory data", "Spawn body from template or default empty gameobject")))
			{
				_target.TrySpawnOrbitingBodyForCurrentTrajectory();
			}

			GUI.enabled = true;

			if (!Application.isPlaying || _target.CurrentTransition == null)
			{
				GUI.enabled = false;
			}

			GUI.enabled = true;

			if (_target.CurrentTransition == null && _target.Target != null)
			{
				if (_target.Target.gameObject == _target.gameObject)
				{
					EditorGUILayout.HelpBox("Target can not be self.", MessageType.Info);
				}
				else
				{
					EditorGUILayout.HelpBox("Can not calculate transition.", MessageType.Info);
				}
			}
		}
	}
}