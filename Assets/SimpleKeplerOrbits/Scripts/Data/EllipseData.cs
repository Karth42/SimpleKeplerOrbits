using UnityEngine;

namespace SimpleKeplerOrbits
{
	public class EllipseData
	{
		public double A;
		public double B;
		public double Eccentricity;
		public Vector3d FocusDistance;
		public Vector3d AxisMain;
		public Vector3d AxisSecondary;
		public Vector3d Center;
		public Vector3d Focus0;
		public Vector3d Focus1;

		public Vector3d Normal
		{
			get
			{
				return KeplerOrbitUtils.CrossProduct(AxisMain, AxisSecondary).normalized;
			}
		}

		public EllipseData(Vector3d focus0, Vector3d focus1, Vector3d p0)
		{
			Focus0 = focus0;
			Focus1 = focus1;
			FocusDistance = Focus0 - Focus1;
			A = ((Focus0 - p0).magnitude + (focus1 - p0).magnitude) * 0.5;
			if (A < 0)
			{
				A = -A;
			}
			Eccentricity = (FocusDistance.magnitude * 0.5) / A;
			B = A * System.Math.Sqrt(1 - Eccentricity * Eccentricity);

			AxisMain = FocusDistance.normalized;
			var tempNormal = KeplerOrbitUtils.CrossProduct(AxisMain, p0 - Focus0).normalized;
			AxisSecondary = KeplerOrbitUtils.CrossProduct(AxisMain, tempNormal).normalized;
			Center = Focus1 + FocusDistance * 0.5;
		}

		/// <summary>
		/// Get point on ellipse at specified angle from center.
		/// </summary>
		/// <param name="eccentricAnomaly">Angle from center in radians</param>
		/// <returns></returns>
		public Vector3d GetSamplePoint(double eccentricAnomaly)
		{
			return Center + AxisMain * (A * System.Math.Cos(eccentricAnomaly)) + AxisSecondary * (B * System.Math.Sin(eccentricAnomaly));
		}

		/// <summary>
		/// Calculate eccentric anomaly in radians for point.
		/// </summary>
		/// <param name="point">Point in plane of elliptic shape.</param>
		/// <returns>Eccentric anomaly radians.</returns>
		public double GetEccentricAnomalyForPoint(Vector3d point)
		{
			var vector = point - Focus0;
			var trueAnomaly = Vector3d.Angle(vector, AxisMain) * KeplerOrbitUtils.Deg2Rad;
			if (KeplerOrbitUtils.DotProduct(vector, AxisSecondary) > 0)
			{
				trueAnomaly = KeplerOrbitUtils.PI_2 - trueAnomaly;
			}
			var result = KeplerOrbitUtils.ConvertTrueToEccentricAnomaly(trueAnomaly, Eccentricity);
			return result;
		}

		public void DebugDrawEllipse(Color color)
		{
			Vector3 lastPoint = (Vector3)(Center + AxisMain * A);
			Vector3 point = lastPoint;
			int points = 100;
			for (int i = 1; i < points; i++)
			{
				float angle = (float)(KeplerOrbitUtils.PI_2 * (i / (float)(points - 1)));
				point = (Vector3)(Center + AxisMain * A * System.Math.Cos(angle) + AxisSecondary * B * System.Math.Sin(angle));
				Debug.DrawLine(lastPoint, point, color);
				lastPoint = point;
			}
		}
	}
}