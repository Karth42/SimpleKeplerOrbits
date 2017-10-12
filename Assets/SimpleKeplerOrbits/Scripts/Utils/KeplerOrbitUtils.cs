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
    /// Math utility methods for help in orbits calculations.
    /// </summary>
    public static class KeplerOrbitUtils
    {
        /// <summary>
        /// Two PI.
        /// </summary>
        public const double PI_2 = 6.2831853071796d;
        public const double Deg2Rad = 0.017453292519943d;
        public const double Rad2Deg = 57.295779513082d;
        public const double Epsilon = 1.401298E-45d;

        /// <summary>
        /// Regular Acosh, but without exception when out of possible range.
        /// </summary>
        /// <param name="x">The input value.</param>
        /// <returns>Calculated Acos value or 0.</returns>
        public static double Acosh(double x)
        {
            if (x < 1.0)
            {
                return 0;
            }

            return Math.Log(x + System.Math.Sqrt(x * x - 1.0));
        }

        /// <summary>
        /// Gets the ray plane intersection point.
        /// </summary>
        /// <param name="pointOnPlane">The point on plane.</param>
        /// <param name="normal">The normal vector.</param>
        /// <param name="rayOrigin">The ray origin point.</param>
        /// <param name="rayDirection">The ray direction vector.</param>
        /// <returns>Point on a plane, where ray is intersected with it.</returns>
        public static Vector3 GetRayPlaneIntersectionPoint(Vector3 pointOnPlane, Vector3 normal, Vector3 rayOrigin, Vector3 rayDirection)
        {
            var dotProd = DotProduct(rayDirection, normal);
            if (Math.Abs(dotProd) < 1e-5)
            {
                return new Vector3();
            }

            var p = rayOrigin + rayDirection * DotProduct((pointOnPlane - rayOrigin), normal) / dotProd;

            //Projection. for better precision.
            p = p - normal * DotProduct(p - pointOnPlane, normal);
            return p;
        }
        
        /// <summary>
        /// Rotate vector around another vector
        /// </summary>
        /// <param name="v">Vector to rotate</param>
        /// <param name="angleRad">angle in radians</param>
        /// <param name="n">normalized vector to rotate around, or normal of rotation plane</param>
        public static Vector3 RotateVectorByAngle(Vector3 v, float angleRad, Vector3 n)
        {
            float cosT = Mathf.Cos(angleRad);
            float sinT = Mathf.Sin(angleRad);
            float oneMinusCos = 1f - cosT;
            //rotation matrix:
            float a11 = oneMinusCos * n.x * n.x + cosT;
            float a12 = oneMinusCos * n.x * n.y - n.z * sinT;
            float a13 = oneMinusCos * n.x * n.z + n.y * sinT;
            float a21 = oneMinusCos * n.x * n.y + n.z * sinT;
            float a22 = oneMinusCos * n.y * n.y + cosT;
            float a23 = oneMinusCos * n.y * n.z - n.x * sinT;
            float a31 = oneMinusCos * n.x * n.z - n.y * sinT;
            float a32 = oneMinusCos * n.y * n.z + n.x * sinT;
            float a33 = oneMinusCos * n.z * n.z + cosT;
            return new Vector3(
                v.x * a11 + v.y * a12 + v.z * a13,
                v.x * a21 + v.y * a22 + v.z * a23,
                v.x * a31 + v.y * a32 + v.z * a33
                );
        }

        /// <summary>
        /// Rotate vector around another vector (double)
        /// </summary>
        /// <param name="v">Vector to rotate</param>
        /// <param name="angleRad">angle in radians</param>
        /// <param name="n">normalized vector to rotate around, or normal of rotation plane</param>
        public static Vector3d RotateVectorByAngle(Vector3d v, double angleRad, Vector3d n)
        {
            double cosT = Math.Cos(angleRad);
            double sinT = Math.Sin(angleRad);
            double oneMinusCos = 1f - cosT;
            //rotation matrix:
            double a11 = oneMinusCos * n.x * n.x + cosT;
            double a12 = oneMinusCos * n.x * n.y - n.z * sinT;
            double a13 = oneMinusCos * n.x * n.z + n.y * sinT;
            double a21 = oneMinusCos * n.x * n.y + n.z * sinT;
            double a22 = oneMinusCos * n.y * n.y + cosT;
            double a23 = oneMinusCos * n.y * n.z - n.x * sinT;
            double a31 = oneMinusCos * n.x * n.z - n.y * sinT;
            double a32 = oneMinusCos * n.y * n.z + n.x * sinT;
            double a33 = oneMinusCos * n.z * n.z + cosT;
            return new Vector3d(
                v.x * a11 + v.y * a12 + v.z * a13,
                v.x * a21 + v.y * a22 + v.z * a23,
                v.x * a31 + v.y * a32 + v.z * a33
                );
        }

        /// <summary>
        /// Dot product of two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>Dot product.</returns>
        public static float DotProduct(Vector3 a, Vector3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        /// <summary>
        /// Dot product of two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>Dot product.</returns>
        public static double DotProduct(Vector3d a, Vector3d b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        /// <summary>
        /// Cross product of two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>Perpendicular vector.</returns>
        public static Vector3 CrossProduct(Vector3 a, Vector3 b)
        {
            return new Vector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        }

        /// <summary>
        /// Cross product of two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>Perpendicular vector.</returns>
        public static Vector3d CrossProduct(Vector3d a, Vector3d b)
        {
            return new Vector3d(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        }

        /// <summary>
        /// Calc velocity vector for circle orbit.
        /// </summary>
        public static Vector3 CalcCircleOrbitVelocity(Vector3 attractorPos, Vector3 bodyPos, double attractorMass, double bodyMass, Vector3 orbitNormal, double gConst)
        {
            var distanceVector = bodyPos - attractorPos;
            var dist = distanceVector.magnitude;
            var MG = attractorMass * gConst;
            var vScalar = System.Math.Sqrt(MG / dist);
            return CrossProduct(distanceVector, -orbitNormal).normalized * (float)vScalar;
        }

        /// <summary>
        /// Calc velocity vector for circle orbit.
        /// </summary>
        public static Vector3d CalcCircleOrbitVelocity(Vector3d attractorPos, Vector3d bodyPos, double attractorMass, double bodyMass, Vector3d orbitNormal, double gConst)
        {
            var distanceVector = bodyPos - attractorPos;
            var dist = distanceVector.magnitude;
            var MG = attractorMass * gConst;
            var vScalar = System.Math.Sqrt(MG / dist);
            return CrossProduct(distanceVector, -orbitNormal).normalized * vScalar;
        }

        /// <summary>
        /// Calc orbit curve points with provided precision.
        /// </summary>
        public static Vector3[] CalcOrbitPoints(Vector3 attractorPos, Vector3 bodyPos, double attractorMass, double bodyMass, Vector3 relVelocity, double gConst, int pointsCount)
        {
            if (pointsCount < 3 || pointsCount > 10000)
            {
                return new Vector3[0];
            }
            var focusPoint = CalcCenterOfMass(attractorPos, attractorMass, bodyPos, bodyMass);
            var radiusVector = bodyPos - focusPoint;
            var radiusVectorMagnitude = radiusVector.magnitude;
            var orbitNormal = CrossProduct(radiusVector, relVelocity);
            var MG = (attractorMass + bodyMass) * gConst;
            var eccVector = CrossProduct(relVelocity, orbitNormal) / (float)MG - radiusVector / radiusVectorMagnitude;
            var focalParameter = orbitNormal.sqrMagnitude / MG;
            var eccentricity = eccVector.magnitude;
            var minorAxisNormal = -CrossProduct(orbitNormal, eccVector).normalized;
            var majorAxisNormal = -CrossProduct(orbitNormal, minorAxisNormal).normalized;
            orbitNormal.Normalize();
            double orbitCompressionRatio;
            double semiMajorAxys;
            double semiMinorAxys;
            Vector3 relFocusPoint;
            Vector3 centerPoint;
            if (eccentricity < 1)
            {
                orbitCompressionRatio = 1 - eccentricity * eccentricity;
                semiMajorAxys = focalParameter / orbitCompressionRatio;
                semiMinorAxys = semiMajorAxys * System.Math.Sqrt(orbitCompressionRatio);
                relFocusPoint = (float)semiMajorAxys * eccVector;
                centerPoint = focusPoint - relFocusPoint;
            }
            else
            {
                orbitCompressionRatio = eccentricity * eccentricity - 1f;
                semiMajorAxys = focalParameter / orbitCompressionRatio;
                semiMinorAxys = semiMajorAxys * System.Math.Sqrt(orbitCompressionRatio);
                relFocusPoint = -(float)semiMajorAxys * eccVector;
                centerPoint = focusPoint - relFocusPoint;
            }

            var points = new Vector3[pointsCount];
            double eccVar = 0f;
            for (int i = 0; i < pointsCount; i++)
            {
                Vector3 result = eccentricity < 1 ?
                    new Vector3((float)(System.Math.Sin(eccVar) * semiMinorAxys), -(float)(System.Math.Cos(eccVar) * semiMajorAxys)) :
                    new Vector3((float)(System.Math.Sinh(eccVar) * semiMinorAxys), (float)(System.Math.Cosh(eccVar) * semiMajorAxys));
                eccVar += Mathf.PI * 2f / (float)(pointsCount - 1);
                points[i] = minorAxisNormal * result.x + majorAxisNormal * result.y + centerPoint;
            }
            return points;
        }

        /// <summary>
        /// Calculates the center of mass.
        /// </summary>
        /// <param name="pos1">The posistion 1.</param>
        /// <param name="mass1">The mass 1.</param>
        /// <param name="pos2">The position 2.</param>
        /// <param name="mass2">The mass 2.</param>
        /// <returns>Center of mass postion vector.</returns>
        public static Vector3 CalcCenterOfMass(Vector3 pos1, double mass1, Vector3 pos2, double mass2)
        {
            return ((pos1 * (float)mass1) + (pos2 * (float)mass2)) / (float)(mass1 + mass2);
        }

        /// <summary>
        /// Calculates the center of mass.
        /// </summary>
        /// <param name="pos1">The posistion 1.</param>
        /// <param name="mass1">The mass 1.</param>
        /// <param name="pos2">The position 2.</param>
        /// <param name="mass2">The mass 2.</param>
        /// <returns>Center of mass postion vector.</returns>
        public static Vector3d CalcCenterOfMass(Vector3d pos1, double mass1, Vector3d pos2, double mass2)
        {
            return ((pos1 * mass1) + (pos2 * mass2)) / (mass1 + mass2);
        }

        /// <summary>
        /// Converts the eccentric to true anomaly.
        /// </summary>
        /// <param name="eccentricAnomaly">The eccentric anomaly.</param>
        /// <param name="eccentricity">The eccentricity.</param>
        /// <returns>True anomaly in radians.</returns>
        public static double ConvertEccentricToTrueAnomaly(double eccentricAnomaly, double eccentricity)
        {
            if (eccentricity < 1d)
            {
                var cosE = System.Math.Cos(eccentricAnomaly);
                var tAnom = System.Math.Acos((cosE - eccentricity) / (1d - eccentricity * cosE));
                if (eccentricAnomaly > Mathf.PI)
                {
                    tAnom = PI_2 - tAnom;
                }
                return tAnom;
            }
            else
            {
                var tAnom = System.Math.Atan2(
                    System.Math.Sqrt(eccentricity * eccentricity - 1d) * System.Math.Sinh(eccentricAnomaly),
                    eccentricity - System.Math.Cosh(eccentricAnomaly)
                );
                return tAnom;
            }
        }

        /// <summary>
        /// Converts the true to eccentric anomaly.
        /// </summary>
        /// <param name="trueAnomaly">The true anomaly.</param>
        /// <param name="eccentricity">The eccentricity.</param>
        /// <returns>Eccentric anomaly in radians.</returns>
        public static double ConvertTrueToEccentricAnomaly(double trueAnomaly, double eccentricity)
        {
            if (double.IsNaN(eccentricity) || double.IsInfinity(eccentricity))
            {
                return trueAnomaly;
            }
            trueAnomaly = trueAnomaly % PI_2;
            if (eccentricity < 1d)
            {
                if (trueAnomaly < 0)
                {
                    trueAnomaly = trueAnomaly + PI_2;
                }
                var cosT2 = Math.Cos(trueAnomaly);
                var eccAnom = Math.Acos((eccentricity + cosT2) / (1d + eccentricity * cosT2));
                if (trueAnomaly > Math.PI)
                {
                    eccAnom = PI_2 - eccAnom;
                }
                return eccAnom;
            }
            else
            {
                var cosT = Math.Cos(trueAnomaly);
                if (double.IsNaN(trueAnomaly))
                {
                    Debug.Log("Ec " + eccentricity);
                }
                var eccAnom = Acosh((eccentricity + cosT) / (1d + eccentricity * cosT)) * System.Math.Sign(trueAnomaly);
                return eccAnom;
            }
        }

        /// <summary>
        /// Converts the mean to eccentric anomaly.
        /// </summary>
        /// <param name="meanAnomaly">The mean anomaly.</param>
        /// <param name="eccentricity">The eccentricity.</param>
        /// <returns>Eccentric anomaly in radians.</returns>
        public static double ConvertMeanToEccentricAnomaly(double meanAnomaly, double eccentricity)
        {
            if (eccentricity < 1)
            {
                return KeplerSolver(meanAnomaly, eccentricity);
            }
            else
            {
                return KeplerSolverHyperbolicCase(meanAnomaly, eccentricity);
            }
        }

        /// <summary>
        /// Converts the eccentric to mean anomaly.
        /// </summary>
        /// <param name="eccentricAnomaly">The eccentric anomaly.</param>
        /// <param name="eccentricity">The eccentricity.</param>
        /// <returns>Mean anomaly in radians.</returns>
        public static double ConvertEccentricToMeanAnomaly(double eccentricAnomaly, double eccentricity)
        {
            if (eccentricity < 1)
            {
                return eccentricAnomaly - eccentricity * System.Math.Sin(eccentricAnomaly);
            }
            else
            {
                return System.Math.Sinh(eccentricAnomaly) * eccentricity - eccentricAnomaly;
            }
        }

        /// <summary>
        /// Gets the True anomaly value from current distance from the focus (attractor).
        /// </summary>
        /// <param name="distance">The distance from attractor.</param>
        /// <param name="eccentricity">The eccentricity.</param>
        /// <param name="semiMajorAxis">The semi major axis.</param>
        /// <returns></returns>
        public static double CalcTrueAnomalyForDistance(double distance, double eccentricity, double semiMajorAxis)
        {
            if (eccentricity < 1)
            {
                return Math.Acos((semiMajorAxis * (1d - eccentricity * eccentricity) - distance) / (distance * eccentricity));
            }
            else
            {
                return Math.Acos((semiMajorAxis * (eccentricity * eccentricity - 1d) - distance) / (distance * eccentricity));
            }
        }

        /// <summary>
        /// A classic Kepler solver.
        /// </summary>
        /// <param name="meanAnomaly">The mean anomaly in radians.</param>
        /// <param name="eccentricity">The eccentricity.</param>
        /// <returns>Eccentric anomaly in radians.</returns>
        public static double KeplerSolver(double meanAnomaly, double eccentricity)
        {
            //one stable method
            int iterations = eccentricity < 0.4d ? 2 : 4;
            double E = meanAnomaly;
            for (int i = 0; i < iterations; i++)
            {
                double esinE = eccentricity * System.Math.Sin(E);
                double ecosE = eccentricity * System.Math.Cos(E);
                double deltaE = E - esinE - meanAnomaly;
                double n = 1.0 - ecosE;
                E += -5d * deltaE / (n + System.Math.Sign(n) * System.Math.Sqrt(System.Math.Abs(16d * n * n - 20d * deltaE * esinE)));
            }
            return E;
        }

        /// <summary>
        /// Kepler solver for hyperbolic case.
        /// </summary>
        /// <param name="meanAnomaly">The mean anomaly.</param>
        /// <param name="eccentricity">The eccentricity.</param>
        /// <returns>Eccentric anomaly in radians.</returns>
        public static double KeplerSolverHyperbolicCase(double meanAnomaly, double eccentricity)
        {
            double epsilon = 1e-005d;
            double delta = 1d;
            double F = System.Math.Log(2d * System.Math.Abs(meanAnomaly) / eccentricity + 1.8d);//danby guess
            if (double.IsNaN(F) || double.IsInfinity(F))
            {
                return meanAnomaly;
            }
            while (System.Math.Abs(delta) > epsilon)
            {
                delta = (eccentricity * (float)System.Math.Sinh(F) - F - meanAnomaly) / (eccentricity * (float)System.Math.Cosh(F) - 1d);
                if (double.IsNaN(delta) || double.IsInfinity(delta))
                {
                    return F;
                }
                F -= delta;
            }
            return F;
        }
    }
}
