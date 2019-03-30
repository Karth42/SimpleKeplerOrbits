using UnityEngine;

namespace SimpleKeplerOrbits
{
	public class HyperbolaData
	{
		public double A;
		public double B;
		public double C;
		public double Eccentricity;
		public Vector3d Center;
		public Vector3d FocusDistance;
		public Vector3d Focus0;
		public Vector3d Focus1;
		public Vector3d AxisMain;
		public Vector3d AxisSecondary;

		public Vector3d Normal
		{
			get
			{
				return KeplerOrbitUtils.CrossProduct(AxisMain, AxisSecondary).normalized;
			}
		}

		/// <summary>
		/// Construct new hyperbola from 2 focuses and a point on one of branches.
		/// </summary>
		/// <param name="focus0">Focus of branch 0.</param>
		/// <param name="focus1">Focus of branch 1.</param>
		/// <param name="p0">Point on hyperbola branch 0.</param>
		public HyperbolaData(Vector3 focus0, Vector3 focus1, Vector3 p0)
		{
			Initialize(new Vector3d(focus0), new Vector3d(focus1), new Vector3d(p0));
		}

		/// <summary>
		/// Construct new hyperbola from 2 focuses and a point on one of branches.
		/// </summary>
		/// <param name="focus0">Focus of branch 0.</param>
		/// <param name="focus1">Focus of branch 1.</param>
		/// <param name="p0">Point on hyperbola branch 0.</param>
		public HyperbolaData(Vector3d focus0, Vector3d focus1, Vector3d p0)
		{
			Initialize(focus0, focus1, p0);
		}

		private void Initialize(Vector3d focus0, Vector3d focus1, Vector3d p0)
		{
			Focus0 = focus0;
			Focus1 = focus1;
			FocusDistance = Focus1 - Focus0;
			AxisMain = FocusDistance.normalized;
			var tempNormal = KeplerOrbitUtils.CrossProduct(AxisMain, p0 - Focus0).normalized;
			AxisSecondary = KeplerOrbitUtils.CrossProduct(AxisMain, tempNormal).normalized;
			C = FocusDistance.magnitude * 0.5;
			A = System.Math.Abs(((p0 - Focus0).magnitude - (p0 - Focus1).magnitude)) * 0.5;
			Eccentricity = C / A;
			B = System.Math.Sqrt(C * C - A * A);
			Center = focus0 + FocusDistance * 0.5;
		}

		/// <summary>
		/// Get point on hyperbola curve.
		/// </summary>
		/// <param name="hyperbolicCoordinate">Hyperbola's parametric function time parameter.</param>
		/// <param name="isMainBranch">Is taking first branch, or, if false, second branch.</param>
		/// <returns>Point on hyperbola at given time (-inf..inf).</returns>
		/// <remarks>
		/// First branch is considered the branch, which was specified in constructor of hyperboal with a point, laying on that branch.
		/// Therefore second branch is always opposite from that.
		/// </remarks>
		public Vector3d GetSamplePointOnBranch(double hyperbolicCoordinate, bool isMainBranch)
		{
			double x = A * System.Math.Cosh(hyperbolicCoordinate);
			double y = B * System.Math.Sinh(hyperbolicCoordinate);
			Vector3d result = Center + (isMainBranch ? AxisMain : -AxisMain) * x + AxisSecondary * y;
			return result;
		}

		public void DebugDrawHyperbola(Color col)
		{
			var lastP00 = GetSamplePointOnBranch(0, true);
			var lastP01 = GetSamplePointOnBranch(0, false);
			var lastP10 = GetSamplePointOnBranch(0, true);
			var lastP11 = GetSamplePointOnBranch(0, false);

			float step = 0.1f;
			for (int i = 0; i < 100; i++)
			{
				var p00 = GetSamplePointOnBranch(i * step, true);
				var p01 = GetSamplePointOnBranch(i * step, false);
				var p10 = GetSamplePointOnBranch(-i * step, true);
				var p11 = GetSamplePointOnBranch(-i * step, false);
				Debug.DrawLine((Vector3)lastP00, (Vector3)p00, col);
				Debug.DrawLine((Vector3)lastP01, (Vector3)p01, col);
				Debug.DrawLine((Vector3)lastP10, (Vector3)p10, col);
				Debug.DrawLine((Vector3)lastP11, (Vector3)p11, col);
				lastP00 = p00;
				lastP01 = p01;
				lastP10 = p10;
				lastP11 = p11;
			}
		}
	}
}
