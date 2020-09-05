using System;

namespace SimpleKeplerOrbits
{
	[System.Serializable]
	public struct Vector3d
	{
		public double x;
		public double y;
		public double z;
		private const double EPSILON = 1.401298E-45;

		public Vector3d normalized
		{
			get { return Vector3d.Normalize(this); }
		}

		public double magnitude
		{
			get { return Math.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z); }
		}

		public double sqrMagnitude
		{
			get { return this.x * this.x + this.y * this.y + this.z * this.z; }
		}

		public static Vector3d zero
		{
			get { return new Vector3d(0d, 0d, 0d); }
		}

		public static Vector3d one
		{
			get { return new Vector3d(1d, 1d, 1d); }
		}

		public Vector3d(double x, double y, double z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public Vector3d(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public Vector3d(double x, double y)
		{
			this.x = x;
			this.y = y;
			this.z = 0d;
		}

		public static Vector3d operator +(Vector3d a, Vector3d b)
		{
			return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z);
		}

		public static Vector3d operator -(Vector3d a, Vector3d b)
		{
			return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
		}

		public static Vector3d operator -(Vector3d a)
		{
			return new Vector3d(-a.x, -a.y, -a.z);
		}

		public static Vector3d operator *(Vector3d a, double d)
		{
			return new Vector3d(a.x * d, a.y * d, a.z * d);
		}

		public static Vector3d operator *(double d, Vector3d a)
		{
			return new Vector3d(a.x * d, a.y * d, a.z * d);
		}

		public static Vector3d operator /(Vector3d a, double d)
		{
			return new Vector3d(a.x / d, a.y / d, a.z / d);
		}

		public static bool operator ==(Vector3d lhs, Vector3d rhs)
		{
			return Vector3d.SqrMagnitude(lhs - rhs) < 0.0 / 1.0;
		}

		public static bool operator !=(Vector3d lhs, Vector3d rhs)
		{
			return Vector3d.SqrMagnitude(lhs - rhs) >= 0.0 / 1.0;
		}

		public static Vector3d Lerp(Vector3d from, Vector3d to, double t)
		{
			t = t < 0 ? 0 : (t > 1.0 ? 1.0 : t);
			return new Vector3d(from.x + (to.x - from.x) * t, from.y + (to.y - from.y) * t, from.z + (to.z - from.z) * t);
		}

		public static Vector3d MoveTowards(Vector3d current, Vector3d target, double maxDistanceDelta)
		{
			Vector3d vector3   = target - current;
			double   magnitude = vector3.magnitude;
			if (magnitude <= maxDistanceDelta || magnitude == 0.0)
			{
				return target;
			}
			else
			{
				return current + vector3 / magnitude * maxDistanceDelta;
			}
		}

		public void Set(double new_x, double new_y, double new_z)
		{
			this.x = new_x;
			this.y = new_y;
			this.z = new_z;
		}

		public static Vector3d Scale(Vector3d a, Vector3d b)
		{
			return new Vector3d(a.x * b.x, a.y * b.y, a.z * b.z);
		}

		public void Scale(Vector3d scale)
		{
			this.x *= scale.x;
			this.y *= scale.y;
			this.z *= scale.z;
		}

		public static Vector3d Cross(Vector3d a, Vector3d b)
		{
			return new Vector3d(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
		}

		public override int GetHashCode()
		{
			return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2;
		}

		public override bool Equals(object other)
		{
			if (!(other is Vector3d))
			{
				return false;
			}

			Vector3d vector3d = (Vector3d)other;
			if (this.x.Equals(vector3d.x) && this.y.Equals(vector3d.y))
			{
				return this.z.Equals(vector3d.z);
			}
			else
			{
				return false;
			}
		}

		public static Vector3d Reflect(Vector3d inDirection, Vector3d inNormal)
		{
			return -2d * Vector3d.Dot(inNormal, inDirection) * inNormal + inDirection;
		}

		public static Vector3d Normalize(Vector3d value)
		{
			double num = Vector3d.Magnitude(value);
			if (num > EPSILON)
			{
				return value / num;
			}
			else
			{
				return Vector3d.zero;
			}
		}

		public void Normalize()
		{
			double num = Vector3d.Magnitude(this);
			if (num > EPSILON)
			{
				this = this / num;
			}
			else
			{
				this = Vector3d.zero;
			}
		}

		public override string ToString()
		{
			return "(" + this.x + "; " + this.y + "; " + this.z + ")";
		}

		public string ToString(string format)
		{
			return "(" + this.x.ToString(format) + "; " + this.y.ToString(format) + "; " + this.z.ToString(format) + ")";
		}

		public static double Dot(Vector3d a, Vector3d b)
		{
			return a.x * b.x + a.y * b.y + a.z * b.z;
		}

		public static Vector3d Project(Vector3d vector, Vector3d onNormal)
		{
			double num = Vector3d.Dot(onNormal, onNormal);
			if (num < 1.40129846432482E-45d)
			{
				return Vector3d.zero;
			}
			else
			{
				return onNormal * Vector3d.Dot(vector, onNormal) / num;
			}
		}

		public static Vector3d Exclude(Vector3d excludeThis, Vector3d fromThat)
		{
			return fromThat - Vector3d.Project(fromThat, excludeThis);
		}

		public static double Distance(Vector3d a, Vector3d b)
		{
			Vector3d vector3d = new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z);
			return Math.Sqrt(vector3d.x * vector3d.x + vector3d.y * vector3d.y + vector3d.z * vector3d.z);
		}

		public static Vector3d ClampMagnitude(Vector3d vector, double maxLength)
		{
			if (vector.sqrMagnitude > maxLength * maxLength)
			{
				return vector.normalized * maxLength;
			}
			else
			{
				return vector;
			}
		}

		public static double Angle(Vector3d from, Vector3d to)
		{
			double dot = Dot(from.normalized, to.normalized);
			return Math.Acos(dot < -1.0 ? -1.0 : (dot > 1.0 ? 1.0 : dot)) * 57.29578d;
		}

		public static double Magnitude(Vector3d a)
		{
			return Math.Sqrt(a.x * a.x + a.y * a.y + a.z * a.z);
		}

		public static double SqrMagnitude(Vector3d a)
		{
			return a.x * a.x + a.y * a.y + a.z * a.z;
		}

		public static Vector3d Min(Vector3d a, Vector3d b)
		{
			return new Vector3d(Math.Min(a.x, b.x), Math.Min(a.y, b.y), Math.Min(a.z, b.z));
		}

		public static Vector3d Max(Vector3d a, Vector3d b)
		{
			return new Vector3d(Math.Max(a.x, b.x), Math.Max(a.y, b.y), Math.Max(a.z, b.z));
		}
	}
}