using System;
using SVGImporter.Document;
using UnityEngine;

namespace SVGImporter.Rendering
{
	[Serializable]
	public struct SVGMatrix
	{
		public float a;

		public float b;

		public float c;

		public float d;

		public float e;

		public float f;

		public static SVGMatrix identity => new SVGMatrix(1f, 0f, 0f, 1f, 0f, 0f);

		public Vector2 position
		{
			get
			{
				return new Vector2(e, f);
			}
			set
			{
				e = value.x;
				f = value.y;
			}
		}

		public Vector2 scale => new Vector2(Mathf.Sqrt(a * a + b * b), Mathf.Sqrt(c * c + d * d));

		public float skewX
		{
			get
			{
				Vector2 vector = DeltaTransformPoint(new Vector2(0f, 1f));
				return 180f / (float)Math.PI * Mathf.Atan2(vector.y, vector.x) - 90f;
			}
		}

		public float skewY
		{
			get
			{
				Vector2 vector = DeltaTransformPoint(new Vector2(1f, 0f));
				return 180f / (float)Math.PI * Mathf.Atan2(vector.y, vector.x);
			}
		}

		public float rotation => skewX;

		public SVGMatrix(float a, float b, float c, float d, float e, float f)
		{
			this.a = a;
			this.b = b;
			this.c = c;
			this.d = d;
			this.e = e;
			this.f = f;
		}

		private Vector2 DeltaTransformPoint(Vector2 point)
		{
			return new Vector2(point.x * a + point.y * c, point.x * b + point.y * d);
		}

		public SVGMatrix Multiply(SVGMatrix secondMatrix)
		{
			float num = secondMatrix.a;
			float num2 = secondMatrix.b;
			float num3 = secondMatrix.c;
			float num4 = secondMatrix.d;
			float num5 = secondMatrix.e;
			float num6 = secondMatrix.f;
			return new SVGMatrix(a * num + c * num2, b * num + d * num2, a * num3 + c * num4, b * num3 + d * num4, a * num5 + c * num6 + e, b * num5 + d * num6 + f);
		}

		public static SVGMatrix operator *(SVGMatrix left, SVGMatrix right)
		{
			return new SVGMatrix(left.a * right.a + left.c * right.b, left.b * right.a + left.d * right.b, left.a * right.c + left.c * right.d, left.b * right.c + left.d * right.d, left.a * right.e + left.c * right.f + left.e, left.b * right.e + left.d * right.f + left.f);
		}

		public SVGMatrix Inverse()
		{
			double num = a * d - c * b;
			if (num == 0.0)
			{
				throw new SVGException(SVGExceptionType.MatrixNotInvertable);
			}
			return new SVGMatrix((float)((double)d / num), (float)((double)(0f - b) / num), (float)((double)(0f - c) / num), (float)((double)a / num), (float)((double)(c * f - e * d) / num), (float)((double)(e * b - a * f) / num));
		}

		public SVGMatrix Scale(float scaleFactor)
		{
			return new SVGMatrix(a * scaleFactor, b * scaleFactor, c * scaleFactor, d * scaleFactor, e, f);
		}

		public SVGMatrix Scale(float scaleFactorX, float scaleFactorY)
		{
			return new SVGMatrix(a * scaleFactorX, b * scaleFactorX, c * scaleFactorY, d * scaleFactorY, e, f);
		}

		public SVGMatrix Scale(Vector2 scaleFactor)
		{
			return new SVGMatrix(a * scaleFactor.x, b * scaleFactor.x, c * scaleFactor.y, d * scaleFactor.y, e, f);
		}

		public SVGMatrix Rotate(float angle)
		{
			float num = Mathf.Cos(angle * ((float)Math.PI / 180f));
			float num2 = Mathf.Sin(angle * ((float)Math.PI / 180f));
			return new SVGMatrix(a * num + c * num2, b * num + d * num2, c * num - a * num2, d * num - b * num2, e, f);
		}

		public SVGMatrix Translate(float x, float y)
		{
			return new SVGMatrix(a, b, c, d, a * x + c * y + e, b * x + d * y + f);
		}

		public SVGMatrix Translate(Vector2 position)
		{
			return new SVGMatrix(a, b, c, d, a * position.x + c * position.y + e, b * position.x + d * position.y + f);
		}

		public SVGMatrix SkewX(float angle)
		{
			float num = Mathf.Tan(angle * ((float)Math.PI / 180f));
			return new SVGMatrix(a, b, c + a * num, d + b * num, e, f);
		}

		public SVGMatrix SkewY(float angle)
		{
			float num = Mathf.Tan(angle * ((float)Math.PI / 180f));
			return new SVGMatrix(a + c * num, b + d * num, c, d, e, f);
		}

		public Vector2 Transform(Vector2 point)
		{
			return new Vector2(a * point.x + c * point.y + e, b * point.x + d * point.y + f);
		}

		public Vector3 Transform(Vector3 point)
		{
			return new Vector3(a * point.x + c * point.y + e, b * point.x + d * point.y + f, 0f);
		}

		public static SVGMatrix TRS(Vector2 position, float rotation, Vector2 scale)
		{
			float num = Mathf.Cos(rotation * ((float)Math.PI / 180f));
			float num2 = Mathf.Sin(rotation * ((float)Math.PI / 180f));
			return new SVGMatrix((1f * num + 0f * num2) * scale.x, (0f * num + 1f * num2) * scale.x, (0f * num - 1f * num2) * scale.y, (1f * num - 0f * num2) * scale.y, 1f * position.x + 0f * position.y + 0f, 0f * position.x + 1f * position.y + 0f);
		}

		public Matrix4x4 ToMatrix4x4()
		{
			Matrix4x4 result = Matrix4x4.identity;
			result[0, 0] = a;
			result[0, 1] = b;
			result[1, 0] = c;
			result[1, 1] = d;
			result[2, 0] = e;
			result[2, 1] = f;
			return result;
		}

		public void Reset()
		{
			a = 1f;
			b = 0f;
			c = 0f;
			d = 1f;
			e = 0f;
			f = 0f;
		}

		public override string ToString()
		{
			return $"[SVGMatrix] a: {a}, b: {b}, c: {c}, d: {d}, e: {e}, f: {f}" + $"\nposition: {position}, rotation: {rotation}, skewX: {skewX}, skewY: {skewY}, scale: {scale}";
		}
	}
}
