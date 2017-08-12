#region Copyright
/// Copyright © 2017 Vlad Kirpichenko
/// 
/// Author: Vlad Kirpichenko 'itanksp@gmail.com'
/// Licensed under the MIT License.
/// License: http://opensource.org/licenses/MIT
#endregion

using UnityEngine;
using UnityEditor;

namespace SimpleKeplerOrbits
{
    [CustomEditor(typeof(KeplerOrbitMover))]
    public class KeplerOrbitMoverEditor : Editor
    {
        private KeplerOrbitMover _target;

        private void OnEnable()
        {
            _target = target as KeplerOrbitMover;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!_target.OrbitData.IsValidOrbit)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Circularize orbit"))
            {
                _target.SetAutoCircleOrbit();
            }
            if (_target.OrbitData.Eccentricity >= 1.0)
            {
                GUI.enabled = false;
            }
            if (_target.OrbitData.Eccentricity < 1.0)
            {
                var meanAnomaly = EditorGUILayout.Slider("Mean anomaly", (float)_target.OrbitData.MeanAnomaly, 0, (float)KeplerOrbitUtils.PI_2);
                if (meanAnomaly != (float)_target.OrbitData.MeanAnomaly)
                {
                    _target.OrbitData.SetMeanAnomaly(meanAnomaly);
                    _target.ForceUpdateViewFromInternalState();
                    EditorUtility.SetDirty(_target);
                }
            }
            else
            {
                EditorGUILayout.LabelField("Mean anomaly", _target.OrbitData.MeanAnomaly.ToString());
            }
            if (!GUI.enabled)
            {
                GUI.enabled = true;
            }
            if (_target.AttractorSettings != null && _target.AttractorSettings.AttractorObject == _target.gameObject)
            {
                _target.AttractorSettings.AttractorObject = null;
                EditorUtility.SetDirty(_target);
            }
            if (_target.AttractorSettings.GravityConstant < 0)
            {
                _target.AttractorSettings.GravityConstant = 0;
                EditorUtility.SetDirty(_target);
            }
            if (_target.OrbitData.GravConst < 0)
            {
                _target.OrbitData.GravConst = 0;
                EditorUtility.SetDirty(_target);
            }
        }
    }
}