#region Copyright
/// Copyright © 2017 Vlad Kirpichenko
/// 
/// Author: Vlad Kirpichenko 'itanksp@gmail.com'
/// Licensed under the MIT License.
/// License: http://opensource.org/licenses/MIT
#endregion

using System;
using UnityEngine;

namespace SimpleKeplerOrbits
{
    /// <summary>
    /// Orbit data container.
    /// Also contains methods for altering and updating orbit state.
    /// </summary>
    [Serializable]
    public class KeplerOrbitData
    {
        public double GravConst = 1;
        public Vector3d EclipticNormal = new Vector3d(0, 0, 1);
        public Vector3d EclipticUp = new Vector3d(0, 1, 0);//up direction on ecliptic plane

        public Vector3d Position;
        public double AttractorDistance;
        public double AttractorMass;
        public Vector3d Velocity;

        public double SemiMinorAxis;
        public double SemiMajorAxis;
        public double FocalParameter;
        public double Eccentricity;
        public double EnergyTotal;
        public double Period;
        public double TrueAnomaly;
        public double MeanAnomaly;
        public double EccentricAnomaly;
        public double SquaresConstant;
        public Vector3d Periapsis;
        public double PeriapsisDistance;
        public Vector3d Apoapsis;
        public double ApoapsisDistance;
        public Vector3d CenterPoint;
        public double OrbitCompressionRatio;
        public Vector3d OrbitNormal;
        public Vector3d SemiMinorAxisBasis;
        public Vector3d SemiMajorAxisBasis;

        /// <summary>
        /// The orbit inclination in radians relative to ecliptic plane.
        /// </summary>
        public double Inclination;

        /// <summary>
        /// if > 0, then orbit motion is clockwise
        /// </summary>
        public double OrbitNormalDotEclipticNormal;

        /// <summary>
        /// Is orbit state valid and error-free.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid orbit; otherwise, <c>false</c>.
        /// </value>
        public bool IsValidOrbit
        {
            get
            {
                return Eccentricity >= 0 && Period > KeplerOrbitUtils.Epsilon && AttractorDistance > KeplerOrbitUtils.Epsilon && AttractorMass > KeplerOrbitUtils.Epsilon;
            }
        }

        /// <summary>
        /// Calculates the full state of orbit from current body position, attractor position, attractor mass, velocity, and gravConstant.
        /// </summary>
        public void CalculateNewOrbitData()
        {
            var MG = AttractorMass * GravConst;
            AttractorDistance = Position.magnitude;
            var angularMomentumVector = KeplerOrbitUtils.CrossProduct(Position, Velocity);
            OrbitNormal = angularMomentumVector.normalized;
            Vector3d eccVector;
            if (OrbitNormal.sqrMagnitude < 0.9 || OrbitNormal.sqrMagnitude > 1.1)
            {//check if zero lenght
                OrbitNormal = KeplerOrbitUtils.CrossProduct(Position, EclipticUp).normalized;
                eccVector = new Vector3d();
            }
            else
            {
                eccVector = KeplerOrbitUtils.CrossProduct(Velocity, angularMomentumVector) / MG - Position / AttractorDistance;
            }
            OrbitNormalDotEclipticNormal = KeplerOrbitUtils.DotProduct(OrbitNormal, EclipticNormal);
            FocalParameter = angularMomentumVector.sqrMagnitude / MG;
            Eccentricity = eccVector.magnitude;
            EnergyTotal = Velocity.sqrMagnitude - 2 * MG / AttractorDistance;
            SemiMinorAxisBasis = KeplerOrbitUtils.CrossProduct(angularMomentumVector, eccVector).normalized;
            if (SemiMinorAxisBasis.sqrMagnitude < 0.5)
            {
                SemiMinorAxisBasis = KeplerOrbitUtils.CrossProduct(OrbitNormal, Position).normalized;
            }
            SemiMajorAxisBasis = KeplerOrbitUtils.CrossProduct(OrbitNormal, SemiMinorAxisBasis).normalized;
            Inclination = Vector3d.Angle(OrbitNormal, EclipticNormal) * KeplerOrbitUtils.Deg2Rad;
            if (Eccentricity < 1)
            {
                OrbitCompressionRatio = 1 - Eccentricity * Eccentricity;
                SemiMajorAxis = FocalParameter / OrbitCompressionRatio;
                SemiMinorAxis = SemiMajorAxis * Math.Sqrt(OrbitCompressionRatio);
                CenterPoint = -SemiMajorAxis * eccVector;
                Period = KeplerOrbitUtils.PI_2 * Math.Sqrt(Math.Pow(SemiMajorAxis, 3) / MG);
                Apoapsis = CenterPoint + SemiMajorAxisBasis * SemiMajorAxis;
                Periapsis = CenterPoint - SemiMajorAxisBasis * SemiMajorAxis;
                PeriapsisDistance = Periapsis.magnitude;
                ApoapsisDistance = Apoapsis.magnitude;
                TrueAnomaly = Vector3d.Angle(Position, -SemiMajorAxisBasis) * KeplerOrbitUtils.Deg2Rad;
                if (KeplerOrbitUtils.DotProduct(KeplerOrbitUtils.CrossProduct(Position, SemiMajorAxisBasis), OrbitNormal) < 0)
                {
                    TrueAnomaly = KeplerOrbitUtils.PI_2 - TrueAnomaly;
                }
                EccentricAnomaly = KeplerOrbitUtils.ConvertTrueToEccentricAnomaly(TrueAnomaly, Eccentricity);
                MeanAnomaly = EccentricAnomaly - Eccentricity * Math.Sin(EccentricAnomaly);
            }
            else
            {
                OrbitCompressionRatio = Eccentricity * Eccentricity - 1;
                SemiMajorAxis = FocalParameter / OrbitCompressionRatio;
                SemiMinorAxis = SemiMajorAxis * Math.Sqrt(OrbitCompressionRatio);
                CenterPoint = SemiMajorAxis * eccVector;
                Period = double.PositiveInfinity;
                Apoapsis = new Vector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
                Periapsis = CenterPoint + SemiMajorAxisBasis * (SemiMajorAxis);
                PeriapsisDistance = Periapsis.magnitude;
                ApoapsisDistance = double.PositiveInfinity;
                TrueAnomaly = Vector3d.Angle(Position, eccVector) * KeplerOrbitUtils.Deg2Rad;
                if (KeplerOrbitUtils.DotProduct(KeplerOrbitUtils.CrossProduct(Position, SemiMajorAxisBasis), OrbitNormal) < 0)
                {
                    TrueAnomaly = -TrueAnomaly;
                }
                EccentricAnomaly = KeplerOrbitUtils.ConvertTrueToEccentricAnomaly(TrueAnomaly, Eccentricity);
                MeanAnomaly = Math.Sinh(EccentricAnomaly) * Eccentricity - EccentricAnomaly;
            }
        }

        /// <summary>
        /// Gets the velocity vector value at eccentric anomaly.
        /// </summary>
        /// <param name="eccentricAnomaly">The eccentric anomaly.</param>
        /// <returns>Velocity vector.</returns>
        public Vector3d GetVelocityAtEccentricAnomaly(double eccentricAnomaly)
        {
            return GetVelocityAtTrueAnomaly(KeplerOrbitUtils.ConvertEccentricToTrueAnomaly(eccentricAnomaly, Eccentricity));
        }

        /// <summary>
        /// Gets the velocity value at true anomaly.
        /// </summary>
        /// <param name="trueAnomaly">The true anomaly.</param>
        /// <returns>Velocity vector.</returns>
        public Vector3d GetVelocityAtTrueAnomaly(double trueAnomaly)
        {
            if (FocalParameter < 1e-5)
            {
                return new Vector3d();
            }
            var sqrtMGdivP = Math.Sqrt(AttractorMass * GravConst / FocalParameter);
            double vX = sqrtMGdivP * (Eccentricity + Math.Cos(trueAnomaly));
            double vY = sqrtMGdivP * Math.Sin(trueAnomaly);
            return SemiMinorAxisBasis * vX + SemiMajorAxisBasis * vY;
        }

        /// <summary>
        /// Gets the central position at true anomaly.
        /// </summary>
        /// <param name="trueAnomaly">The true anomaly.</param>
        /// <returns>Position relative to orbit center.</returns>
        /// <remarks>
        /// Note: central position is not same as focal position.
        /// </remarks>
        public Vector3d GetCentralPositionAtTrueAnomaly(double trueAnomaly)
        {
            var ecc = KeplerOrbitUtils.ConvertTrueToEccentricAnomaly(trueAnomaly, Eccentricity);
            return GetCentralPositionAtEccentricAnomaly(ecc);
        }

        /// <summary>
        /// Gets the central position at eccentric anomaly.
        /// </summary>
        /// <param name="eccentricAnomaly">The eccentric anomaly.</param>
        /// <returns>Position relative to orbit center.</returns>
        /// <remarks>
        /// Note: central position is not same as focal position.
        /// </remarks>
        public Vector3d GetCentralPositionAtEccentricAnomaly(double eccentricAnomaly)
        {
            Vector3d result = Eccentricity < 1 ?
                new Vector3d(Math.Sin(eccentricAnomaly) * SemiMinorAxis, -Math.Cos(eccentricAnomaly) * SemiMajorAxis) :
                new Vector3d(Math.Sinh(eccentricAnomaly) * SemiMinorAxis, Math.Cosh(eccentricAnomaly) * SemiMajorAxis);
            return SemiMinorAxisBasis * result.x + SemiMajorAxisBasis * result.y;
        }

        /// <summary>
        /// Gets the focal position at eccentric anomaly.
        /// </summary>
        /// <param name="eccentricAnomaly">The eccentric anomaly.</param>
        /// <returns>Position relative to attractor (focus).</returns>
        public Vector3d GetFocalPositionAtEccentricAnomaly(double eccentricAnomaly)
        {
            return GetCentralPositionAtEccentricAnomaly(eccentricAnomaly) + CenterPoint;
        }

        /// <summary>
        /// Gets the focal position at true anomaly.
        /// </summary>
        /// <param name="trueAnomaly">The true anomaly.</param>
        /// <returns>Position relative to attractor (focus).</returns>
        public Vector3d GetFocalPositionAtTrueAnomaly(double trueAnomaly)
        {
            return GetCentralPositionAtTrueAnomaly(trueAnomaly) + CenterPoint;
        }

        /// <summary>
        /// Gets the central position.
        /// </summary>
        /// <returns>Position relative to orbit center.</returns>
        /// <remarks>
        /// Note: central position is not same as focal position.
        /// </remarks>
        public Vector3d GetCentralPosition()
        {
            return Position - CenterPoint;
        }

        /// <summary>
        /// Gets calculated orbit points with defined precision.
        /// </summary>
        /// <param name="pointsCount">The points count.</param>
        /// <param name="maxDistance">The maximum distance of points.</param>
        /// <returns>Array of orbit curve points.</returns>
        public Vector3d[] GetOrbitPoints(int pointsCount = 50, double maxDistance = 1000d)
        {
            return GetOrbitPoints(pointsCount, new Vector3d(), maxDistance);
        }

        /// <summary>
        /// Gets calculated orbit points with defined precision.
        /// </summary>
        /// <param name="pointsCount">The points count.</param>
        /// <param name="origin">The origin.</param>
        /// <param name="maxDistance">The maximum distance.</param>
        /// <returns>Array of orbit curve points.</returns>
        public Vector3d[] GetOrbitPoints(int pointsCount, Vector3d origin, double maxDistance = 1000d)
        {
            if (pointsCount < 2)
            {
                return new Vector3d[0];
            }
            var result = new Vector3d[pointsCount];
            if (Eccentricity < 1)
            {
                if (ApoapsisDistance < maxDistance)
                {
                    for (var i = 0; i < pointsCount; i++)
                    {
                        result[i] = GetFocalPositionAtEccentricAnomaly(i * KeplerOrbitUtils.PI_2 / (pointsCount - 1d)) + origin;
                    }
                }
                else
                {
                    var maxAngle = KeplerOrbitUtils.CalcTrueAnomalyForDistance(maxDistance, Eccentricity, SemiMajorAxis);
                    for (int i = 0; i < pointsCount; i++)
                    {
                        result[i] = GetFocalPositionAtTrueAnomaly(-maxAngle + i * 2d * maxAngle / (pointsCount - 1)) + origin;
                    }
                }
            }
            else
            {
                if (maxDistance < PeriapsisDistance)
                {
                    return new Vector3d[0];
                }
                var maxAngle = KeplerOrbitUtils.CalcTrueAnomalyForDistance(maxDistance, Eccentricity, SemiMajorAxis);

                for (int i = 0; i < pointsCount; i++)
                {
                    result[i] = GetFocalPositionAtTrueAnomaly(-maxAngle + i * 2d * maxAngle / (pointsCount - 1)) + origin;
                }
            }
            return result;
        }

        /// <summary>
        /// Gets calculated orbit points with defined precision.
        /// </summary>
        /// <param name="pointsCount">The points count.</param>
        /// <param name="maxDistance">The maximum distance.</param>
        /// <returns>Array of orbit curve points.</returns>
        public Vector3[] GetOrbitPoints(int pointsCount = 50, float maxDistance = 1000f)
        {
            return GetOrbitPoints(pointsCount, new Vector3(), maxDistance);
        }

        /// <summary>
        /// Gets calculated orbit points with defined precision.
        /// </summary>
        /// <param name="pointsCount">The points count.</param>
        /// <param name="origin">The origin.</param>
        /// <param name="maxDistance">The maximum distance.</param>
        /// <returns>Array of orbit curve points.</returns>
        public Vector3[] GetOrbitPoints(int pointsCount, Vector3 origin, float maxDistance = 1000f)
        {
            if (pointsCount < 2)
            {
                return new Vector3[0];
            }
            var result = new Vector3[pointsCount];
            if (Eccentricity < 1)
            {
                if (ApoapsisDistance < maxDistance)
                {
                    for (var i = 0; i < pointsCount; i++)
                    {
                        result[i] = (Vector3)GetFocalPositionAtEccentricAnomaly(i * KeplerOrbitUtils.PI_2 / (pointsCount - 1d)) + origin;
                    }
                }
                else
                {
                    var maxAngle = KeplerOrbitUtils.CalcTrueAnomalyForDistance(maxDistance, Eccentricity, SemiMajorAxis);
                    for (int i = 0; i < pointsCount; i++)
                    {
                        result[i] = (Vector3)GetFocalPositionAtTrueAnomaly(-maxAngle + i * 2d * maxAngle / (pointsCount - 1)) + origin;
                    }
                }
            }
            else
            {
                if (maxDistance < PeriapsisDistance)
                {
                    return new Vector3[0];
                }
                var maxAngle = KeplerOrbitUtils.CalcTrueAnomalyForDistance(maxDistance, Eccentricity, SemiMajorAxis);

                for (int i = 0; i < pointsCount; i++)
                {
                    result[i] = (Vector3)GetFocalPositionAtTrueAnomaly(-maxAngle + i * 2d * maxAngle / (pointsCount - 1)) + origin;
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the orbit points without unnecessary memory alloc for resulting array.
        /// However, memory allocation may occur if resulting array has not correct lenght.
        /// </summary>
        /// <param name="orbitPoints">The orbit points.</param>
        /// <param name="pointsCount">The points count.</param>
        /// <param name="origin">The origin.</param>
        /// <param name="maxDistance">The maximum distance.</param>
        public void GetOrbitPointsNoAlloc(ref Vector3[] orbitPoints, int pointsCount, Vector3 origin, float maxDistance = 1000f)
        {
            if (pointsCount < 2)
            {
                orbitPoints = new Vector3[0];
                return;
            }
            if (Eccentricity < 1)
            {
                if (orbitPoints == null || orbitPoints.Length != pointsCount)
                {
                    orbitPoints = new Vector3[pointsCount];
                }
                if (ApoapsisDistance < maxDistance)
                {
                    for (var i = 0; i < pointsCount; i++)
                    {
                        orbitPoints[i] = (Vector3)GetFocalPositionAtEccentricAnomaly(i * KeplerOrbitUtils.PI_2 / (pointsCount - 1d)) + origin;
                    }
                }
                else
                {
                    var maxAngle = KeplerOrbitUtils.CalcTrueAnomalyForDistance(maxDistance, Eccentricity, SemiMajorAxis);
                    for (int i = 0; i < pointsCount; i++)
                    {
                        orbitPoints[i] = (Vector3)GetFocalPositionAtTrueAnomaly(-maxAngle + i * 2d * maxAngle / (pointsCount - 1)) + origin;
                    }
                }
            }
            else
            {
                if (maxDistance < PeriapsisDistance)
                {
                    orbitPoints = new Vector3[0];
                    return;
                }
                if (orbitPoints == null || orbitPoints.Length != pointsCount)
                {
                    orbitPoints = new Vector3[pointsCount];
                }
                var maxAngle = KeplerOrbitUtils.CalcTrueAnomalyForDistance(maxDistance, Eccentricity, SemiMajorAxis);

                for (int i = 0; i < pointsCount; i++)
                {
                    orbitPoints[i] = (Vector3)GetFocalPositionAtTrueAnomaly(-maxAngle + i * 2d * maxAngle / (pointsCount - 1)) + origin;
                }
            }
        }

        /// <summary>
        /// Gets the ascending node of orbit.
        /// </summary>
        /// <param name="asc">The asc.</param>
        /// <returns><c>true</c> if ascending node exists, otherwise <c>false</c></returns>
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

        /// <summary>
        /// Gets the ascending node of orbit.
        /// </summary>
        /// <param name="asc">The asc.</param>
        /// <returns><c>true</c> if ascending node exists, otherwise <c>false</c></returns>
        public bool GetAscendingNode(out Vector3d asc)
        {
            var norm = KeplerOrbitUtils.CrossProduct(OrbitNormal, EclipticNormal);
            var s = KeplerOrbitUtils.DotProduct(KeplerOrbitUtils.CrossProduct(norm, SemiMajorAxisBasis), OrbitNormal) < 0;
            var ecc = 0d;
            var trueAnom = Vector3d.Angle(norm, CenterPoint) * KeplerOrbitUtils.Deg2Rad;
            if (Eccentricity < 1)
            {
                var cosT = Math.Cos(trueAnom);
                ecc = Math.Acos((Eccentricity + cosT) / (1d + Eccentricity * cosT));
                if (!s)
                {
                    ecc = KeplerOrbitUtils.PI_2 - ecc;
                }
            }
            else
            {
                trueAnom = Vector3d.Angle(-norm, CenterPoint) * KeplerOrbitUtils.Deg2Rad;
                if (trueAnom >= Math.Acos(-1d / Eccentricity))
                {
                    asc = new Vector3d();
                    return false;
                }
                var cosT = Math.Cos(trueAnom);
                ecc = KeplerOrbitUtils.Acosh((Eccentricity + cosT) / (1 + Eccentricity * cosT)) * (!s ? -1 : 1);
            }
            asc = GetFocalPositionAtEccentricAnomaly(ecc);
            return true;
        }

        /// <summary>
        /// Gets the descending node of orbit.
        /// </summary>
        /// <param name="desc">The desc.</param>
        /// <returns><c>true</c> if descending node exists, otherwise <c>false</c></returns>
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

        /// <summary>
        /// Gets the descending node orbit.
        /// </summary>
        /// <param name="desc">The desc.</param>
        /// <returns><c>true</c> if descending node exists, otherwise <c>false</c></returns>
        public bool GetDescendingNode(out Vector3d desc)
        {
            var norm = KeplerOrbitUtils.CrossProduct(OrbitNormal, EclipticNormal);
            var s = KeplerOrbitUtils.DotProduct(KeplerOrbitUtils.CrossProduct(norm, SemiMajorAxisBasis), OrbitNormal) < 0;
            var ecc = 0d;
            var trueAnom = Vector3d.Angle(norm, -CenterPoint) * KeplerOrbitUtils.Deg2Rad;
            if (Eccentricity < 1)
            {
                var cosT = Math.Cos(trueAnom);
                ecc = Math.Acos((Eccentricity + cosT) / (1d + Eccentricity * cosT));
                if (s)
                {
                    ecc = KeplerOrbitUtils.PI_2 - ecc;
                }
            }
            else
            {
                trueAnom = Vector3d.Angle(norm, CenterPoint) * KeplerOrbitUtils.Deg2Rad;
                if (trueAnom >= Math.Acos(-1d / Eccentricity))
                {
                    desc = new Vector3d();
                    return false;
                }
                var cosT = Math.Cos(trueAnom);
                ecc = KeplerOrbitUtils.Acosh((Eccentricity + cosT) / (1 + Eccentricity * cosT)) * (s ? -1 : 1);
            }
            desc = GetFocalPositionAtEccentricAnomaly(ecc);
            return true;
        }

        /// <summary>
        /// Updates the kepler orbit state by defined deltatime.
        /// Orbit main parameters will remains unchanged, but all anomalies will progress in time.
        /// </summary>
        /// <param name="deltaTime">The delta time.</param>
        public void UpdateOrbitDataByTime(double deltaTime)
        {
            UpdateOrbitAnomaliesByTime(deltaTime);
            SetPositionByCurrentAnomaly();
            SetVelocityByCurrentAnomaly();
        }

        /// <summary>
        /// Updates the value of orbital anomalies by defined deltatime.
        /// </summary>
        /// <param name="deltaTime">The delta time.</param>
        /// <remarks>
        /// Only anomalies values will be changed. 
        /// Position and velocity states needs to be updated too after this method call.
        /// </remarks>
        public void UpdateOrbitAnomaliesByTime(double deltaTime)
        {
            if (Eccentricity < 1)
            {
                if (Period > 1e-5)
                {
                    MeanAnomaly += KeplerOrbitUtils.PI_2 * deltaTime / Period;
                }
                MeanAnomaly %= KeplerOrbitUtils.PI_2;
                if (MeanAnomaly < 0)
                {
                    MeanAnomaly = KeplerOrbitUtils.PI_2 - MeanAnomaly;
                }
                EccentricAnomaly = KeplerOrbitUtils.KeplerSolver(MeanAnomaly, Eccentricity);
                var cosE = Math.Cos(EccentricAnomaly);
                TrueAnomaly = Math.Acos((cosE - Eccentricity) / (1 - Eccentricity * cosE));
                if (MeanAnomaly > Math.PI)
                {
                    TrueAnomaly = KeplerOrbitUtils.PI_2 - TrueAnomaly;
                }
                if (double.IsNaN(MeanAnomaly) || double.IsInfinity(MeanAnomaly))
                {
                    Debug.Log("KeplerOrbitData: NaN(INF) MEAN ANOMALY"); //little paranoya
                    Debug.Break();
                }
                if (double.IsNaN(EccentricAnomaly) || double.IsInfinity(EccentricAnomaly))
                {
                    Debug.Log("KeplerOrbitData: NaN(INF) ECC ANOMALY");
                    Debug.Break();
                }
                if (double.IsNaN(TrueAnomaly) || double.IsInfinity(TrueAnomaly))
                {
                    Debug.Log("KeplerOrbitData: NaN(INF) TRUE ANOMALY");
                    Debug.Break();
                }
            }
            else
            {
                double n = Math.Sqrt(AttractorMass * GravConst / Math.Pow(SemiMajorAxis, 3));// * Math.Sign(OrbitNormalDotEclipticNormal);
                MeanAnomaly = MeanAnomaly + n * deltaTime;
                EccentricAnomaly = KeplerOrbitUtils.KeplerSolverHyperbolicCase(MeanAnomaly, Eccentricity);
                TrueAnomaly = Math.Atan2(Math.Sqrt(Eccentricity * Eccentricity - 1.0) * Math.Sinh(EccentricAnomaly), Eccentricity - Math.Cosh(EccentricAnomaly));
            }
        }

        /// <summary>
        /// Updates position from anomaly state.
        /// </summary>
        public void SetPositionByCurrentAnomaly()
        {
            Position = GetFocalPositionAtEccentricAnomaly(EccentricAnomaly);
        }

        /// <summary>
        /// Sets velocity by current anomaly.
        /// </summary>
        public void SetVelocityByCurrentAnomaly()
        {
            Velocity = GetVelocityAtEccentricAnomaly(EccentricAnomaly);
        }

        /// <summary>
        /// Sets the eccentricity and updates all corresponding orbit state values.
        /// </summary>
        /// <param name="e">The new eccentricity value.</param>
        public void SetEccentricity(double e)
        {
            if (!IsValidOrbit)
            {
                return;
            }
            e = Math.Abs(e);
            var _periapsis = PeriapsisDistance; // Periapsis remains constant
            Eccentricity = e;
            var compresion = Eccentricity < 1 ? (1 - Eccentricity * Eccentricity) : (Eccentricity * Eccentricity - 1);
            SemiMajorAxis = Math.Abs(_periapsis / (1 - Eccentricity));
            FocalParameter = SemiMajorAxis * compresion;
            SemiMinorAxis = SemiMajorAxis * Math.Sqrt(compresion);
            CenterPoint = SemiMajorAxis * Math.Abs(Eccentricity) * SemiMajorAxisBasis;
            if (Eccentricity < 1)
            {
                EccentricAnomaly = KeplerOrbitUtils.KeplerSolver(MeanAnomaly, Eccentricity);
                var cosE = Math.Cos(EccentricAnomaly);
                TrueAnomaly = Math.Acos((cosE - Eccentricity) / (1 - Eccentricity * cosE));
                if (MeanAnomaly > Math.PI)
                {
                    TrueAnomaly = KeplerOrbitUtils.PI_2 - TrueAnomaly;
                }
            }
            else
            {
                EccentricAnomaly = KeplerOrbitUtils.KeplerSolverHyperbolicCase(MeanAnomaly, Eccentricity);
                TrueAnomaly = Math.Atan2(Math.Sqrt(Eccentricity * Eccentricity - 1) * Math.Sinh(EccentricAnomaly), Eccentricity - Math.Cosh(EccentricAnomaly));
            }
            SetVelocityByCurrentAnomaly();
            SetPositionByCurrentAnomaly();

            CalculateNewOrbitData();
        }

        /// <summary>
        /// Sets the mean anomaly and updates all other anomalies.
        /// </summary>
        /// <param name="m">The m.</param>
        public void SetMeanAnomaly(double m)
        {
            if (!IsValidOrbit)
            {
                return;
            }
            MeanAnomaly = m % KeplerOrbitUtils.PI_2;
            if (Eccentricity < 1)
            {
                if (MeanAnomaly < 0)
                {
                    MeanAnomaly += KeplerOrbitUtils.PI_2;
                }
                EccentricAnomaly = KeplerOrbitUtils.KeplerSolver(MeanAnomaly, Eccentricity);
                TrueAnomaly = KeplerOrbitUtils.ConvertEccentricToTrueAnomaly(EccentricAnomaly, Eccentricity);
            }
            else
            {
                EccentricAnomaly = KeplerOrbitUtils.KeplerSolverHyperbolicCase(MeanAnomaly, Eccentricity);
                TrueAnomaly = KeplerOrbitUtils.ConvertEccentricToTrueAnomaly(EccentricAnomaly, Eccentricity);
            }
            SetPositionByCurrentAnomaly();
            SetVelocityByCurrentAnomaly();
        }

        /// <summary>
        /// Sets the true anomaly and updates all other anomalies.
        /// </summary>
        /// <param name="t">The t.</param>
        public void SetTrueAnomaly(double t)
        {
            if (!IsValidOrbit)
            {
                return;
            }
            t %= KeplerOrbitUtils.PI_2;

            if (Eccentricity < 1)
            {
                if (t < 0)
                {
                    t += KeplerOrbitUtils.PI_2;
                }
                EccentricAnomaly = KeplerOrbitUtils.ConvertTrueToEccentricAnomaly(t, Eccentricity);
                MeanAnomaly = EccentricAnomaly - Eccentricity * Math.Sin(EccentricAnomaly);
            }
            else
            {
                EccentricAnomaly = KeplerOrbitUtils.ConvertTrueToEccentricAnomaly(t, Eccentricity);
                MeanAnomaly = Math.Sinh(EccentricAnomaly) * Eccentricity - EccentricAnomaly;
            }
            SetPositionByCurrentAnomaly();
            SetVelocityByCurrentAnomaly();
        }

        /// <summary>
        /// Sets the eccentric anomaly and updates all other anomalies.
        /// </summary>
        /// <param name="e">The e.</param>
        public void SetEccentricAnomaly(double e)
        {
            if (!IsValidOrbit)
            {
                return;
            }
            e %= KeplerOrbitUtils.PI_2;
            EccentricAnomaly = e;
            if (Eccentricity < 1)
            {
                if (e < 0)
                {
                    e = KeplerOrbitUtils.PI_2 + e;
                }
                EccentricAnomaly = e;
                TrueAnomaly = KeplerOrbitUtils.ConvertEccentricToTrueAnomaly(e, Eccentricity);
                MeanAnomaly = EccentricAnomaly - Eccentricity * Math.Sin(EccentricAnomaly);
            }
            else
            {
                TrueAnomaly = KeplerOrbitUtils.ConvertEccentricToTrueAnomaly(e, Eccentricity);
                MeanAnomaly = Math.Sinh(EccentricAnomaly) * Eccentricity - EccentricAnomaly;
            }
            SetPositionByCurrentAnomaly();
            SetVelocityByCurrentAnomaly();
        }

        /// <summary>
        /// Rotates the relative position and velocity by same quaternion.
        /// </summary>
        /// <param name="rotation">The rotation.</param>
        public void RotateOrbit(Quaternion rotation)
        {
            Position = new Vector3d(rotation * ((Vector3)Position));
            Velocity = new Vector3d(rotation * ((Vector3)Velocity));
            CalculateNewOrbitData();
        }
    }
}
