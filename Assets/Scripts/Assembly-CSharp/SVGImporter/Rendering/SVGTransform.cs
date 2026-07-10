using System;
using System.Globalization;
using SVGImporter.Utils;
using UnityEngine;

namespace SVGImporter.Rendering
{
	public class SVGTransform
	{
		private SVGTransformMode _type;

		private SVGMatrix _matrix;

		private double _angle;

		public SVGMatrix matrix => _matrix;

		public float angle
		{
			get
			{
				SVGTransformMode sVGTransformMode = _type;
				if (sVGTransformMode - 4 <= SVGTransformMode.Translate)
				{
					return (float)_angle;
				}
				return 0f;
			}
		}

		public SVGTransformMode type => _type;

		public SVGTransform()
		{
			_matrix = SVGMatrix.identity;
			_type = SVGTransformMode.Matrix;
		}

		public SVGTransform(SVGMatrix matrix)
		{
			_type = SVGTransformMode.Matrix;
			_matrix = matrix;
		}

		public SVGTransform(string strKey, string strValue)
		{
			string[] array = SVGStringExtractor.ExtractTransformValue(strValue);
			int num = array.Length;
			float[] array2 = new float[num];
			for (int i = 0; i < num; i++)
			{
				try
				{
					array2.SetValue(float.Parse(array[i], CultureInfo.InvariantCulture), i);
				}
				catch (Exception ex)
				{
					Debug.Log("SVGTransform: e: " + ex);
				}
			}
			switch (strKey)
			{
			case "translate":
				switch (num)
				{
				case 1:
					SetTranslate(array2[0], 0f);
					break;
				case 2:
					SetTranslate(array2[0], array2[1]);
					break;
				default:
					throw new ApplicationException("Wrong number of arguments in translate transform");
				}
				break;
			case "rotate":
				switch (num)
				{
				case 1:
					SetRotate(array2[0]);
					break;
				case 3:
					SetRotate(array2[0], array2[1], array2[2]);
					break;
				default:
					throw new ApplicationException("Wrong number of arguments in rotate transform");
				}
				break;
			case "scale":
				switch (num)
				{
				case 1:
					SetScale(array2[0], array2[0]);
					break;
				case 2:
					SetScale(array2[0], array2[1]);
					break;
				default:
					throw new ApplicationException("Wrong number of arguments in scale transform");
				}
				break;
			case "skewX":
				if (num != 1)
				{
					throw new ApplicationException("Wrong number of arguments in skewX transform");
				}
				SetSkewX(array2[0]);
				break;
			case "skewY":
				if (num != 1)
				{
					throw new ApplicationException("Wrong number of arguments in skewY transform");
				}
				SetSkewY(array2[0]);
				break;
			case "matrix":
				if (num != 6)
				{
					throw new ApplicationException("Wrong number of arguments in matrix transform");
				}
				SetMatrix(new SVGMatrix(array2[0], array2[1], array2[2], array2[3], array2[4], array2[5]));
				break;
			default:
				_type = SVGTransformMode.Unknown;
				break;
			}
		}

		public void SetMatrix(SVGMatrix matrix)
		{
			_type = SVGTransformMode.Matrix;
			_matrix = matrix;
		}

		public void SetTranslate(float tx, float ty)
		{
			_type = SVGTransformMode.Translate;
			_matrix = SVGMatrix.identity.Translate(tx, ty);
		}

		public void SetScale(float sx, float sy)
		{
			_type = SVGTransformMode.Scale;
			_matrix = SVGMatrix.identity.Scale(sx, sy);
		}

		public void SetRotate(float angle)
		{
			_type = SVGTransformMode.Rotate;
			_angle = angle;
			_matrix = SVGMatrix.identity.Rotate(angle);
		}

		public void SetRotate(float angle, float cx, float cy)
		{
			_type = SVGTransformMode.Rotate;
			_angle = angle;
			_matrix = SVGMatrix.identity.Translate(cx, cy).Rotate(angle).Translate(0f - cx, 0f - cy);
		}

		public void SetSkewX(float angle)
		{
			_type = SVGTransformMode.SkewX;
			_angle = angle;
			_matrix = SVGMatrix.identity.SkewX(angle);
		}

		public void SetSkewY(float angle)
		{
			_type = SVGTransformMode.SkewY;
			_angle = angle;
			_matrix = SVGMatrix.identity.SkewY(angle);
		}
	}
}
