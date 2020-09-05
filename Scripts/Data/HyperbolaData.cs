namespace SimpleKeplerOrbits
{
	public class HyperbolaData
	{
		public double   A;
		public double   B;
		public double   C;
		public double   Eccentricity;
		public Vector3d Center;
		public Vector3d FocusDistance;
		public Vector3d Focus0;
		public Vector3d Focus1;
		public Vector3d AxisMain;
		public Vector3d AxisSecondary;

		public Vector3d Normal
		{
			get { return Vector3d.Cross(AxisMain, AxisSecondary).normalized; }
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
			Focus0        = focus0;
			Focus1        = focus1;
			FocusDistance = Focus1 - Focus0;
			AxisMain      = FocusDistance.normalized;
			var tempNormal = Vector3d.Cross(AxisMain, p0 - Focus0).normalized;
			AxisSecondary = Vector3d.Cross(AxisMain, tempNormal).normalized;
			C             = FocusDistance.magnitude * 0.5;
			A             = System.Math.Abs(((p0 - Focus0).magnitude - (p0 - Focus1).magnitude)) * 0.5;
			Eccentricity  = C / A;
			B             = System.Math.Sqrt(C * C - A * A);
			Center        = focus0 + FocusDistance * 0.5;
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
			double   x      = A * System.Math.Cosh(hyperbolicCoordinate);
			double   y      = B * System.Math.Sinh(hyperbolicCoordinate);
			Vector3d result = Center + (isMainBranch ? AxisMain : -AxisMain) * x + AxisSecondary * y;

			return result;
		}
	}
}