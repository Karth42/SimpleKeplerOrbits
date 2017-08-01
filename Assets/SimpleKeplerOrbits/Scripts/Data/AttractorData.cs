using UnityEngine;

namespace SimpleKeplerOrbits
{
    [System.Serializable]
    public class AttractorData
    {
        public Transform AttractorObject;
        public float AttractorMass = 1000;
        public float MaxDistForHyperbolicCase = 100f;
        public float GravityConstant = 0.1f;
    }
}