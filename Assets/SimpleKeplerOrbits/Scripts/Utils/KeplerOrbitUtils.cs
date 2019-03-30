using System;
using System.Collections.Generic;
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
		/// Project vector onto plane.
		/// </summary>
		/// <param name="point">Input vector.</param>
		/// <param name="planeNormal">Plane normal.</param>
		/// <returns>Result vector.</returns>
		public static Vector3d ProjectPointOnPlane(Vector3d point, Vector3d planeNormal)
		{
			return point - planeNormal * DotProduct(point, planeNormal);
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
			float dotProd = DotProduct(rayDirection, normal);
			if (Math.Abs(dotProd) < Epsilon)
			{
				return new Vector3();
			}
			Vector3 p = rayOrigin + rayDirection * DotProduct((pointOnPlane - rayOrigin), normal) / dotProd;

			//Projection. For better precision.
			p = p - normal * DotProduct(p - pointOnPlane, normal);
			return p;
		}

		/// <summary>
		/// Rotate vector around another vector.
		/// </summary>
		/// <param name="v">Vector to rotate.</param>
		/// <param name="angleRad">angle in radians.</param>
		/// <param name="n">normalized vector to rotate around, or normal of rotation plane.</param>
		public static Vector3 RotateVectorByAngle(Vector3 v, float angleRad, Vector3 n)
		{
			float cosT = Mathf.Cos(angleRad);
			float sinT = Mathf.Sin(angleRad);
			float oneMinusCos = 1f - cosT;
			// Rotation matrix:
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
		/// Rotate vector around another vector (double).
		/// </summary>
		/// <param name="v">Vector to rotate.</param>
		/// <param name="angleRad">angle in radians.</param>
		/// <param name="n">normalized vector to rotate around, or normal of rotation plane.</param>
		public static Vector3d RotateVectorByAngle(Vector3d v, double angleRad, Vector3d n)
		{
			double cosT = Math.Cos(angleRad);
			double sinT = Math.Sin(angleRad);
			double oneMinusCos = 1f - cosT;
			// Rotation matrix:
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

		public static double Abs(double a)
		{
			return a < 0 ? -a : a;
		}

		public static float Abs(float a)
		{
			return a < 0 ? -a : a;
		}

		/// <summary>
		/// Calc velocity vector for circle orbit.
		/// </summary>
		public static Vector3 CalcCircleOrbitVelocity(Vector3 attractorPos, Vector3 bodyPos, double attractorMass, double bodyMass, Vector3 orbitNormal, double gConst)
		{
			Vector3 distanceVector = bodyPos - attractorPos;
			float dist = distanceVector.magnitude;
			double MG = attractorMass * gConst;
			double vScalar = Math.Sqrt(MG / dist);
			return CrossProduct(distanceVector, -orbitNormal).normalized * (float)vScalar;
		}

		/// <summary>
		/// Calc velocity vector for circle orbit.
		/// </summary>
		public static Vector3d CalcCircleOrbitVelocity(Vector3d attractorPos, Vector3d bodyPos, double attractorMass, double bodyMass, Vector3d orbitNormal, double gConst)
		{
			Vector3d distanceVector = bodyPos - attractorPos;
			double dist = distanceVector.magnitude;
			double MG = attractorMass * gConst;
			double vScalar = Math.Sqrt(MG / dist);
			return CrossProduct(distanceVector, -orbitNormal).normalized * vScalar;
		}

		/// <summary>
		/// Calc orbit curve points with provided precision.
		/// </summary>
		public static Vector3[] CalcOrbitPoints(Vector3 attractorPos, Vector3 bodyPos, double attractorMass, double bodyMass, Vector3 relVelocity, double gConst, int pointsCount)
		{
			if (pointsCount < 3 || pointsCount > 1000000)
			{
				return new Vector3[0];
			}
			Vector3 focusPoint = CalcCenterOfMass(attractorPos, attractorMass, bodyPos, bodyMass);
			Vector3 radiusVector = bodyPos - focusPoint;
			float radiusVectorMagnitude = radiusVector.magnitude;
			Vector3 orbitNormal = CrossProduct(radiusVector, relVelocity);
			double MG = (attractorMass + bodyMass) * gConst;
			Vector3 eccVector = CrossProduct(relVelocity, orbitNormal) / (float)MG - radiusVector / radiusVectorMagnitude;
			double focalParameter = orbitNormal.sqrMagnitude / MG;
			float eccentricity = eccVector.magnitude;
			Vector3 minorAxisNormal = -CrossProduct(orbitNormal, eccVector).normalized;
			Vector3 majorAxisNormal = -CrossProduct(orbitNormal, minorAxisNormal).normalized;
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
				orbitCompressionRatio = eccentricity * eccentricity - 1.0;
				semiMajorAxys = focalParameter / orbitCompressionRatio;
				semiMinorAxys = semiMajorAxys * System.Math.Sqrt(orbitCompressionRatio);
				relFocusPoint = -(float)semiMajorAxys * eccVector;
				centerPoint = focusPoint - relFocusPoint;
			}

			Vector3[] points = new Vector3[pointsCount];
			double eccVar = 0f;
			for (int i = 0; i < pointsCount; i++)
			{
				Vector3 result = eccentricity < 1 ?
					new Vector3((float)(System.Math.Sin(eccVar) * semiMinorAxys), -(float)(System.Math.Cos(eccVar) * semiMajorAxys)) :
					new Vector3((float)(System.Math.Sinh(eccVar) * semiMinorAxys), (float)(System.Math.Cosh(eccVar) * semiMajorAxys));
				eccVar += Mathf.PI * 2f / (pointsCount - 1);
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
				double cosE = System.Math.Cos(eccentricAnomaly);
				double tAnom = System.Math.Acos((cosE - eccentricity) / (1d - eccentricity * cosE));
				if (eccentricAnomaly > Mathf.PI)
				{
					tAnom = PI_2 - tAnom;
				}
				return tAnom;
			}
			else
			{
				double tAnom = System.Math.Atan2(
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
				double cosT2 = Math.Cos(trueAnomaly);
				double eccAnom = Math.Acos((eccentricity + cosT2) / (1d + eccentricity * cosT2));
				if (trueAnomaly > Math.PI)
				{
					eccAnom = PI_2 - eccAnom;
				}
				return eccAnom;
			}
			else
			{
				double cosT = Math.Cos(trueAnomaly);
				double eccAnom = Acosh((eccentricity + cosT) / (1d + eccentricity * cosT)) * System.Math.Sign(trueAnomaly);
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
			// One stable method.
			var iterations = eccentricity < 0.2 ? 2 : 3;
			double e = meanAnomaly;
			double esinE;
			double ecosE;
			double deltaE;
			double n;
			for (int i = 0; i < iterations; i++)
			{
				esinE = eccentricity * System.Math.Sin(e);
				ecosE = eccentricity * System.Math.Cos(e);
				deltaE = e - esinE - meanAnomaly;
				n = 1.0 - ecosE;
				e += -5d * deltaE / (n + System.Math.Sign(n) * System.Math.Sqrt(System.Math.Abs(16d * n * n - 20d * deltaE * esinE)));
			}
			return e;
		}

		/// <summary>
		/// Kepler solver for hyperbolic case.
		/// </summary>
		/// <param name="meanAnomaly">The mean anomaly.</param>
		/// <param name="eccentricity">The eccentricity.</param>
		/// <returns>Eccentric anomaly in radians.</returns>
		public static double KeplerSolverHyperbolicCase(double meanAnomaly, double eccentricity)
		{
			double delta = 1d;
			// Danby guess.
			double F = System.Math.Log(2d * System.Math.Abs(meanAnomaly) / eccentricity + 1.8d);
			if (double.IsNaN(F) || double.IsInfinity(F))
			{
				return meanAnomaly;
			}
			int i = 0;
			while (delta > 1e-8 || delta < -1e-8)
			{
				i++;
				delta = (eccentricity * System.Math.Sinh(F) - F - meanAnomaly) / (eccentricity * System.Math.Cosh(F) - 1d);
				F -= delta;
			}
			return F;
		}

		/// <summary>
		/// Find attractor transform, which is parent for both A and B.
		/// If mutual attractor is not direct parent of both A and B, then look in upper hierarchy of attractors.
		/// </summary>
		/// <param name="a">Body A.</param>
		/// <param name="b">Body B.</param>
		/// <param name="isGetFullChain">If true, attractors chains will include full hierarchy. In opposite case, chains will end before mutual attractor.</param>
		/// <param name="attractorsAChain">Chain of parent attractors for A.</param>
		/// <param name="attractorsBChain">Chain of parent attractors for B.</param>
		/// <param name="gConst">Attractor gravity const.</param>
		/// <param name="mass">Attractor mass.</param>
		/// <returns>Mutual attractor transform or null if not found.</returns>
		/// <remarks>
		/// Chain of attractors is constructed from attractors transforms, which also have own KeplerOrbitMover component.
		/// 
		/// Note: this method also retreaving g and mass of attractor. Because these values can be different for any KeplerOrbitMover, those, what belong to body A are preffered.
		/// </remarks>
		public static Transform FindMutualAttractor(KeplerOrbitMover a, KeplerOrbitMover b, bool isGetFullChain, ref List<KeplerOrbitMover> attractorsAChain, ref List<KeplerOrbitMover> attractorsBChain, ref double mass, ref double gConst)
		{
			if (attractorsAChain == null)
			{
				attractorsAChain = new List<KeplerOrbitMover>();
			}
			else
			{
				attractorsAChain.Clear();
			}
			if (attractorsBChain == null)
			{
				attractorsBChain = new List<KeplerOrbitMover>();
			}
			else
			{
				attractorsBChain.Clear();
			}
			int maxChainLen = 1000;
			Transform mutualAttractor = null;
			if (a != null && b != null && a != b)
			{
				var attrTransform = a.AttractorSettings.AttractorObject;
				while (attrTransform != null && attractorsAChain.Count < maxChainLen)
				{
					var attrOrbitMover = attrTransform.GetComponent<KeplerOrbitMover>();
					attrTransform = null;
					if (attrOrbitMover != null && !attractorsAChain.Contains(attrOrbitMover))
					{
						attrTransform = attrOrbitMover.AttractorSettings.AttractorObject;
						attractorsAChain.Add(attrOrbitMover);
					}
				}

				attrTransform = b.AttractorSettings.AttractorObject;
				while (attrTransform != null && attractorsBChain.Count < maxChainLen)
				{
					var attrOrbitMover = attrTransform.GetComponent<KeplerOrbitMover>();
					attrTransform = null;
					if (attrOrbitMover != null && !attractorsBChain.Contains(attrOrbitMover))
					{
						attrTransform = attrOrbitMover.AttractorSettings.AttractorObject;
						attractorsBChain.Add(attrOrbitMover);
					}
				}

				if (a.AttractorSettings.AttractorObject == b.AttractorSettings.AttractorObject)
				{
					mutualAttractor = a.AttractorSettings.AttractorObject;
					gConst = a.AttractorSettings.GravityConstant;
					mass = a.AttractorSettings.AttractorMass;
				}
				else
				{
					for (int i = 0; i < attractorsAChain.Count && mutualAttractor == null; i++)
					{
						for (int n = 0; n < attractorsBChain.Count; n++)
						{
							if (attractorsAChain[i].AttractorSettings.AttractorObject == attractorsBChain[n].transform ||
								attractorsAChain[i].AttractorSettings.AttractorObject == attractorsBChain[i].AttractorSettings.AttractorObject)
							{
								mutualAttractor = attractorsAChain[i].AttractorSettings.AttractorObject;
								gConst = attractorsAChain[i].AttractorSettings.GravityConstant;
								mass = attractorsAChain[i].AttractorSettings.AttractorMass;
							}
							else if (attractorsBChain[i].AttractorSettings.AttractorObject == attractorsAChain[i].transform)
							{
								mutualAttractor = attractorsBChain[i].AttractorSettings.AttractorObject;
								gConst = attractorsAChain[i].AttractorSettings.GravityConstant;
								mass = attractorsAChain[i].AttractorSettings.AttractorMass;
							}
							else
							{
								continue;
							}
							break;
						}
					}
				}
				if (!isGetFullChain && mutualAttractor != null)
				{
					int mutualAttractorIndex = -1;
					for (int i = 0; i < attractorsAChain.Count; i++)
					{
						if (attractorsAChain[i].transform == mutualAttractor)
						{
							mutualAttractorIndex = i;
							break;
						}
					}
					if (mutualAttractorIndex >= 0)
					{
						//mutualAttractorIndex++;
						while (attractorsAChain.Count > mutualAttractorIndex)
						{
							attractorsAChain.RemoveAt(attractorsAChain.Count - 1);
						}
					}
					mutualAttractorIndex = -1;
					for (int i = 0; i < attractorsBChain.Count; i++)
					{
						if (attractorsBChain[i].transform == mutualAttractor)
						{
							mutualAttractorIndex = i;
							break;
						}
					}
					if (mutualAttractorIndex >= 0)
					{
						//mutualAttractorIndex++;
						while (attractorsBChain.Count > mutualAttractorIndex)
						{
							attractorsBChain.RemoveAt(attractorsBChain.Count - 1);
						}
					}
				}
				return mutualAttractor;
			}
			attractorsAChain.Clear();
			attractorsBChain.Clear();
			return null;
		}
	}
}