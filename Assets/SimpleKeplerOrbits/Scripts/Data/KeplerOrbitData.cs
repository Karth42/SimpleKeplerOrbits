using System;
using UnityEngine;

namespace SimpleKeplerOrbits
{
    [Serializable]
    public class KeplerOrbitData
    {
        public double gravConst = 1;
        public Vector3d eclipticNormal = new Vector3d(0, 0, 1);
        public Vector3d eclipticUp = new Vector3d(0, 1, 0);//up direction on ecliptic plane

        public Vector3d position;
        public double attractorDistance;
        public double attractorMass;
        public Vector3d velocity;

        public double semiMinorAxis;
        public double semiMajorAxis;
        public double focalParameter;
        public double eccentricity;
        public double energyTotal;
        public double period;
        public double trueAnomaly;
        public double meanAnomaly;
        public double eccentricAnomaly;
        public double squaresConstant;
        public Vector3d periapsis;
        public double periapsisDistance;
        public Vector3d apoapsis;
        public double apoapsisDistance;
        public Vector3d centerPoint;
        public double orbitCompressionRatio;
        public Vector3d orbitNormal;
        public Vector3d semiMinorAxisBasis;
        public Vector3d semiMajorAxisBasis;
        public double inclination;
        /// <summary>
        /// if > 0, then orbit motion is clockwise
        /// </summary>
        public double orbitNormalDotEclipticNormal;

        public bool isValidOrbit
        {
            get
            {
                return eccentricity >= 0 && period > KeplerOrbitUtils.Epsilon && attractorDistance > KeplerOrbitUtils.Epsilon && attractorMass > KeplerOrbitUtils.Epsilon;
            }
        }
        public bool isDirty = false;

        public void CalculateNewOrbitData()
        {
            isDirty = false;
            var MG = attractorMass * gravConst;
            attractorDistance = position.magnitude;
            var angularMomentumVector = KeplerOrbitUtils.CrossProduct(position, velocity);
            orbitNormal = angularMomentumVector.normalized;
            Vector3d eccVector;
            if (orbitNormal.sqrMagnitude < 0.9 || orbitNormal.sqrMagnitude > 1.1)
            {//check if zero lenght
                orbitNormal = KeplerOrbitUtils.CrossProduct(position, eclipticUp).normalized;
                eccVector = new Vector3d();
            }
            else
            {
                eccVector = KeplerOrbitUtils.CrossProduct(velocity, angularMomentumVector) / MG - position / attractorDistance;
            }
            orbitNormalDotEclipticNormal = KeplerOrbitUtils.DotProduct(orbitNormal, eclipticNormal);
            focalParameter = angularMomentumVector.sqrMagnitude / MG;
            eccentricity = eccVector.magnitude;
            energyTotal = velocity.sqrMagnitude - 2 * MG / attractorDistance;
            semiMinorAxisBasis = KeplerOrbitUtils.CrossProduct(angularMomentumVector, eccVector).normalized;
            if (semiMinorAxisBasis.sqrMagnitude < 0.5)
            {
                semiMinorAxisBasis = KeplerOrbitUtils.CrossProduct(orbitNormal, position).normalized;
            }
            semiMajorAxisBasis = KeplerOrbitUtils.CrossProduct(orbitNormal, semiMinorAxisBasis).normalized;
            inclination = Vector3d.Angle(orbitNormal, eclipticNormal) * KeplerOrbitUtils.Deg2Rad;
            if (eccentricity < 1)
            {
                orbitCompressionRatio = 1 - eccentricity * eccentricity;
                semiMajorAxis = focalParameter / orbitCompressionRatio;
                semiMinorAxis = semiMajorAxis * Math.Sqrt(orbitCompressionRatio);
                centerPoint = -semiMajorAxis * eccVector;
                period = KeplerOrbitUtils.PI_2 * Math.Sqrt(Math.Pow(semiMajorAxis, 3) / MG);
                apoapsis = centerPoint + semiMajorAxisBasis * semiMajorAxis;
                periapsis = centerPoint - semiMajorAxisBasis * semiMajorAxis;
                periapsisDistance = periapsis.magnitude;
                apoapsisDistance = apoapsis.magnitude;
                trueAnomaly = Vector3d.Angle(position, -semiMajorAxisBasis) * KeplerOrbitUtils.Deg2Rad;
                if (KeplerOrbitUtils.DotProduct(KeplerOrbitUtils.CrossProduct(position, semiMajorAxisBasis), orbitNormal) < 0)
                {
                    trueAnomaly = KeplerOrbitUtils.PI_2 - trueAnomaly;
                }
                eccentricAnomaly = KeplerOrbitUtils.ConvertTrueToEccentricAnomaly(trueAnomaly, eccentricity);
                meanAnomaly = eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly);
            }
            else
            {
                orbitCompressionRatio = eccentricity * eccentricity - 1;
                semiMajorAxis = focalParameter / orbitCompressionRatio;
                semiMinorAxis = semiMajorAxis * Math.Sqrt(orbitCompressionRatio);
                centerPoint = semiMajorAxis * eccVector;
                period = double.PositiveInfinity;
                apoapsis = new Vector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
                periapsis = centerPoint + semiMajorAxisBasis * (semiMajorAxis);
                periapsisDistance = periapsis.magnitude;
                apoapsisDistance = double.PositiveInfinity;
                trueAnomaly = Vector3d.Angle(position, eccVector) * KeplerOrbitUtils.Deg2Rad;
                if (KeplerOrbitUtils.DotProduct(KeplerOrbitUtils.CrossProduct(position, semiMajorAxisBasis), orbitNormal) < 0)
                {
                    trueAnomaly = -trueAnomaly;
                }
                eccentricAnomaly = KeplerOrbitUtils.ConvertTrueToEccentricAnomaly(trueAnomaly, eccentricity);
                meanAnomaly = Math.Sinh(eccentricAnomaly) * eccentricity - eccentricAnomaly;
            }
        }

        public Vector3d GetVelocityAtEccentricAnomaly(double eccentricAnomaly)
        {
            return GetVelocityAtTrueAnomaly(KeplerOrbitUtils.ConvertEccentricToTrueAnomaly(eccentricAnomaly, eccentricity));
        }

        public Vector3d GetVelocityAtTrueAnomaly(double trueAnomaly)
        {
            if (focalParameter < 1e-5)
            {
                return new Vector3d();
            }
            var sqrtMGdivP = Math.Sqrt(attractorMass * gravConst / focalParameter);
            double vX = sqrtMGdivP * (eccentricity + Math.Cos(trueAnomaly));
            double vY = sqrtMGdivP * Math.Sin(trueAnomaly);
            return semiMinorAxisBasis * vX + semiMajorAxisBasis * vY;
        }

        public Vector3d GetCentralPositionAtTrueAnomaly(double trueAnomaly)
        {
            var ecc = KeplerOrbitUtils.ConvertTrueToEccentricAnomaly(trueAnomaly, eccentricity);
            return GetCentralPositionAtEccentricAnomaly(ecc);
        }

        public Vector3d GetCentralPositionAtEccentricAnomaly(double eccentricAnomaly)
        {
            Vector3d result = eccentricity < 1 ?
                new Vector3d(Math.Sin(eccentricAnomaly) * semiMinorAxis, -Math.Cos(eccentricAnomaly) * semiMajorAxis) :
                new Vector3d(Math.Sinh(eccentricAnomaly) * semiMinorAxis, Math.Cosh(eccentricAnomaly) * semiMajorAxis);
            return semiMinorAxisBasis * result.x + semiMajorAxisBasis * result.y;
        }

        public Vector3d GetFocalPositionAtEccentricAnomaly(double eccentricAnomaly)
        {
            return GetCentralPositionAtEccentricAnomaly(eccentricAnomaly) + centerPoint;
        }

        public Vector3d GetFocalPositionAtTrueAnomaly(double trueAnomaly)
        {
            return GetCentralPositionAtTrueAnomaly(trueAnomaly) + centerPoint;
        }

        public Vector3d GetCentralPosition()
        {
            return position - centerPoint;
        }

        public Vector3d[] GetOrbitPoints(int pointsCount = 50, double maxDistance = 1000d)
        {
            return GetOrbitPoints(pointsCount, new Vector3d(), maxDistance);
        }

        public Vector3d[] GetOrbitPoints(int pointsCount, Vector3d origin, double maxDistance = 1000d)
        {
            if (pointsCount < 2)
            {
                return new Vector3d[0];
            }
            var result = new Vector3d[pointsCount];
            if (eccentricity < 1)
            {
                if (apoapsisDistance < maxDistance)
                {
                    for (var i = 0; i < pointsCount; i++)
                    {
                        result[i] = GetFocalPositionAtEccentricAnomaly(i * KeplerOrbitUtils.PI_2 / (pointsCount - 1d)) + origin;
                    }
                }
                else
                {
                    var maxAngle = KeplerOrbitUtils.CalcTrueAnomalyForDistance(maxDistance, eccentricity, semiMajorAxis);
                    for (int i = 0; i < pointsCount; i++)
                    {
                        result[i] = GetFocalPositionAtTrueAnomaly(-maxAngle + i * 2d * maxAngle / (pointsCount - 1)) + origin;
                    }
                }
            }
            else
            {
                if (maxDistance < periapsisDistance)
                {
                    return new Vector3d[0];
                }
                var maxAngle = KeplerOrbitUtils.CalcTrueAnomalyForDistance(maxDistance, eccentricity, semiMajorAxis);

                for (int i = 0; i < pointsCount; i++)
                {
                    result[i] = GetFocalPositionAtTrueAnomaly(-maxAngle + i * 2d * maxAngle / (pointsCount - 1)) + origin;
                }
            }
            return result;
        }

        public Vector3[] GetOrbitPoints(int pointsCount = 50, float maxDistance = 1000f)
        {
            return GetOrbitPoints(pointsCount, new Vector3(), maxDistance);
        }

        public Vector3[] GetOrbitPoints(int pointsCount, Vector3 origin, float maxDistance = 1000f)
        {
            if (pointsCount < 2)
            {
                return new Vector3[0];
            }
            var result = new Vector3[pointsCount];
            if (eccentricity < 1)
            {
                if (apoapsisDistance < maxDistance)
                {
                    for (var i = 0; i < pointsCount; i++)
                    {
                        result[i] = (Vector3)GetFocalPositionAtEccentricAnomaly(i * KeplerOrbitUtils.PI_2 / (pointsCount - 1d)) + origin;
                    }
                }
                else
                {
                    var maxAngle = KeplerOrbitUtils.CalcTrueAnomalyForDistance(maxDistance, eccentricity, semiMajorAxis);
                    for (int i = 0; i < pointsCount; i++)
                    {
                        result[i] = (Vector3)GetFocalPositionAtTrueAnomaly(-maxAngle + i * 2d * maxAngle / (pointsCount - 1)) + origin;
                    }
                }
            }
            else
            {
                if (maxDistance < periapsisDistance)
                {
                    return new Vector3[0];
                }
                var maxAngle = KeplerOrbitUtils.CalcTrueAnomalyForDistance(maxDistance, eccentricity, semiMajorAxis);

                for (int i = 0; i < pointsCount; i++)
                {
                    result[i] = (Vector3)GetFocalPositionAtTrueAnomaly(-maxAngle + i * 2d * maxAngle / (pointsCount - 1)) + origin;
                }
            }
            return result;
        }

        public bool GetAscendingNode(out Vector3 asc)
        {
            Vector3d v;
            if (GetAscendingNode(out v))
            {
                asc = (Vector3)v;
                return true;
            }
            asc = new Vector3();
            return false;
        }

        public bool GetAscendingNode(out Vector3d asc)
        {
            var norm = KeplerOrbitUtils.CrossProduct(orbitNormal, eclipticNormal);
            var s = KeplerOrbitUtils.DotProduct(KeplerOrbitUtils.CrossProduct(norm, semiMajorAxisBasis), orbitNormal) < 0;
            var ecc = 0d;
            var trueAnom = Vector3d.Angle(norm, centerPoint) * KeplerOrbitUtils.Deg2Rad;
            if (eccentricity < 1)
            {
                var cosT = Math.Cos(trueAnom);
                ecc = Math.Acos((eccentricity + cosT) / (1d + eccentricity * cosT));
                if (!s)
                {
                    ecc = KeplerOrbitUtils.PI_2 - ecc;
                }
            }
            else
            {
                trueAnom = Vector3d.Angle(-norm, centerPoint) * KeplerOrbitUtils.Deg2Rad;
                if (trueAnom >= Math.Acos(-1d / eccentricity))
                {
                    asc = new Vector3d();
                    return false;
                }
                var cosT = Math.Cos(trueAnom);
                ecc = KeplerOrbitUtils.Acosh((eccentricity + cosT) / (1 + eccentricity * cosT)) * (!s ? -1 : 1);
            }
            asc = GetFocalPositionAtEccentricAnomaly(ecc);
            return true;
        }

        public bool GetDescendingNode(out Vector3 desc)
        {
            Vector3d v;
            if (GetDescendingNode(out v))
            {
                desc = (Vector3)v;
                return true;
            }
            desc = new Vector3();
            return false;
        }

        public bool GetDescendingNode(out Vector3d desc)
        {
            var norm = KeplerOrbitUtils.CrossProduct(orbitNormal, eclipticNormal);
            var s = KeplerOrbitUtils.DotProduct(KeplerOrbitUtils.CrossProduct(norm, semiMajorAxisBasis), orbitNormal) < 0;
            var ecc = 0d;
            var trueAnom = Vector3d.Angle(norm, -centerPoint) * KeplerOrbitUtils.Deg2Rad;
            if (eccentricity < 1)
            {
                var cosT = Math.Cos(trueAnom);
                ecc = Math.Acos((eccentricity + cosT) / (1d + eccentricity * cosT));
                if (s)
                {
                    ecc = KeplerOrbitUtils.PI_2 - ecc;
                }
            }
            else
            {
                trueAnom = Vector3d.Angle(norm, centerPoint) * KeplerOrbitUtils.Deg2Rad;
                if (trueAnom >= Math.Acos(-1d / eccentricity))
                {
                    desc = new Vector3d();
                    return false;
                }
                var cosT = Math.Cos(trueAnom);
                ecc = KeplerOrbitUtils.Acosh((eccentricity + cosT) / (1 + eccentricity * cosT)) * (s ? -1 : 1);
            }
            desc = GetFocalPositionAtEccentricAnomaly(ecc);
            return true;
        }

        public void UpdateOrbitDataByTime(double deltaTime)
        {
            UpdateOrbitAnomaliesByTime(deltaTime);
            SetPositionByCurrentAnomaly();
            SetVelocityByCurrentAnomaly();
        }

        public void UpdateOrbitAnomaliesByTime(double deltaTime)
        {
            if (eccentricity < 1)
            {
                if (period > 1e-5)
                {
                    meanAnomaly += KeplerOrbitUtils.PI_2 * deltaTime / period;
                }
                meanAnomaly %= KeplerOrbitUtils.PI_2;
                if (meanAnomaly < 0)
                {
                    meanAnomaly = KeplerOrbitUtils.PI_2 - meanAnomaly;
                }
                eccentricAnomaly = KeplerOrbitUtils.KeplerSolver(meanAnomaly, eccentricity);
                var cosE = Math.Cos(eccentricAnomaly);
                trueAnomaly = Math.Acos((cosE - eccentricity) / (1 - eccentricity * cosE));
                if (meanAnomaly > Math.PI)
                {
                    trueAnomaly = KeplerOrbitUtils.PI_2 - trueAnomaly;
                }
                if (double.IsNaN(meanAnomaly) || double.IsInfinity(meanAnomaly))
                {
                    Debug.Log("SpaceGravity2D: NaN(INF) MEAN ANOMALY"); //litle paranoya
                    Debug.Break();
                }
                if (double.IsNaN(eccentricAnomaly) || double.IsInfinity(eccentricAnomaly))
                {
                    Debug.Log("SpaceGravity2D: NaN(INF) ECC ANOMALY");
                    Debug.Break();
                }
                if (double.IsNaN(trueAnomaly) || double.IsInfinity(trueAnomaly))
                {
                    Debug.Log("SpaceGravity2D: NaN(INF) TRUE ANOMALY");
                    Debug.Break();
                }
            }
            else
            {
                double n = Math.Sqrt(attractorMass * gravConst / Math.Pow(semiMajorAxis, 3)) * Math.Sign(orbitNormalDotEclipticNormal);
                meanAnomaly = meanAnomaly - n * deltaTime;
                eccentricAnomaly = KeplerOrbitUtils.KeplerSolverHyperbolicCase(meanAnomaly, eccentricity);
                trueAnomaly = Math.Atan2(Math.Sqrt(eccentricity * eccentricity - 1.0) * Math.Sinh(eccentricAnomaly), eccentricity - Math.Cosh(eccentricAnomaly));
            }
        }

        public void SetPositionByCurrentAnomaly()
        {
            position = GetFocalPositionAtEccentricAnomaly(eccentricAnomaly);
        }

        public void SetVelocityByCurrentAnomaly()
        {
            velocity = GetVelocityAtEccentricAnomaly(eccentricAnomaly);
        }

        public void SetEccentricity(double e)
        {
            if (!isValidOrbit)
            {
                return;
            }
            e = Math.Abs(e);
            var _periapsis = periapsisDistance; // Periapsis remains constant
            eccentricity = e;
            var compresion = eccentricity < 1 ? (1 - eccentricity * eccentricity) : (eccentricity * eccentricity - 1);
            semiMajorAxis = Math.Abs(_periapsis / (1 - eccentricity));
            focalParameter = semiMajorAxis * compresion;
            semiMinorAxis = semiMajorAxis * Math.Sqrt(compresion);
            centerPoint = semiMajorAxis * Math.Abs(eccentricity) * semiMajorAxisBasis;
            if (eccentricity < 1)
            {
                eccentricAnomaly = KeplerOrbitUtils.KeplerSolver(meanAnomaly, eccentricity);
                var cosE = Math.Cos(eccentricAnomaly);
                trueAnomaly = Math.Acos((cosE - eccentricity) / (1 - eccentricity * cosE));
                if (meanAnomaly > Math.PI)
                {
                    trueAnomaly = KeplerOrbitUtils.PI_2 - trueAnomaly;
                }
            }
            else
            {
                eccentricAnomaly = KeplerOrbitUtils.KeplerSolverHyperbolicCase(meanAnomaly, eccentricity);
                trueAnomaly = Math.Atan2(Math.Sqrt(eccentricity * eccentricity - 1) * Math.Sinh(eccentricAnomaly), eccentricity - Math.Cosh(eccentricAnomaly));
            }
            SetVelocityByCurrentAnomaly();
            SetPositionByCurrentAnomaly();

            CalculateNewOrbitData();
        }

        public void SetMeanAnomaly(double m)
        {
            if (!isValidOrbit)
            {
                return;
            }
            meanAnomaly = m % KeplerOrbitUtils.PI_2;
            if (eccentricity < 1)
            {
                if (meanAnomaly < 0)
                {
                    meanAnomaly += KeplerOrbitUtils.PI_2;
                }
                eccentricAnomaly = KeplerOrbitUtils.KeplerSolver(meanAnomaly, eccentricity);
                trueAnomaly = KeplerOrbitUtils.ConvertEccentricToTrueAnomaly(eccentricAnomaly, eccentricity);
            }
            else
            {
                eccentricAnomaly = KeplerOrbitUtils.KeplerSolverHyperbolicCase(meanAnomaly, eccentricity);
                trueAnomaly = KeplerOrbitUtils.ConvertEccentricToTrueAnomaly(eccentricAnomaly, eccentricity);
            }
            SetPositionByCurrentAnomaly();
            SetVelocityByCurrentAnomaly();
        }

        public void SetTrueAnomaly(double t)
        {
            if (!isValidOrbit)
            {
                return;
            }
            t %= KeplerOrbitUtils.PI_2;

            if (eccentricity < 1)
            {
                if (t < 0)
                {
                    t += KeplerOrbitUtils.PI_2;
                }
                eccentricAnomaly = KeplerOrbitUtils.ConvertTrueToEccentricAnomaly(t, eccentricity);
                meanAnomaly = eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly);
            }
            else
            {
                eccentricAnomaly = KeplerOrbitUtils.ConvertTrueToEccentricAnomaly(t, eccentricity);
                meanAnomaly = Math.Sinh(eccentricAnomaly) * eccentricity - eccentricAnomaly;
            }
            SetPositionByCurrentAnomaly();
            SetVelocityByCurrentAnomaly();
        }

        public void SetEccentricAnomaly(double e)
        {
            if (!isValidOrbit)
            {
                return;
            }
            e %= KeplerOrbitUtils.PI_2;
            eccentricAnomaly = e;
            if (eccentricity < 1)
            {
                if (e < 0)
                {
                    e = KeplerOrbitUtils.PI_2 + e;
                }
                eccentricAnomaly = e;
                trueAnomaly = KeplerOrbitUtils.ConvertEccentricToTrueAnomaly(e, eccentricity);
                meanAnomaly = eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly);
            }
            else
            {
                trueAnomaly = KeplerOrbitUtils.ConvertEccentricToTrueAnomaly(e, eccentricity);
                meanAnomaly = Math.Sinh(eccentricAnomaly) * eccentricity - eccentricAnomaly;
            }
            SetPositionByCurrentAnomaly();
            SetVelocityByCurrentAnomaly();
        }

        public void RotateOrbit(Quaternion rotation)
        {
            position = new Vector3d(rotation * ((Vector3)position));
            velocity = new Vector3d(rotation * ((Vector3)velocity));
            CalculateNewOrbitData();
        }
    }
}
