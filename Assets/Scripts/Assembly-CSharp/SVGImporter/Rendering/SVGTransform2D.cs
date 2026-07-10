using System;
using UnityEngine;

namespace SVGImporter.Rendering
{
	[Serializable]
	public class SVGTransform2D : ICloneable
	{
		[SerializeField]
		protected Vector2 _position;

		[SerializeField]
		protected float _rotation;

		[SerializeField]
		protected Vector2 _scale = Vector2.one;

		[HideInInspector]
		public Vector2 position
		{
			get
			{
				return _position;
			}
			set
			{
				if (!(_position == value))
				{
					_position = value;
				}
			}
		}

		public float rotation
		{
			get
			{
				return _rotation;
			}
			set
			{
				if (_rotation != value)
				{
					_rotation = value;
				}
			}
		}

		public Vector2 scale
		{
			get
			{
				return _scale;
			}
			set
			{
				if (!(_scale == value))
				{
					_scale = value;
				}
			}
		}

		public Matrix4x4 matrix4x4 => Matrix4x4.TRS(new Vector3(_position.x, _position.y, 0f), Quaternion.Euler(0f, 0f, _rotation), new Vector3(_scale.x, _scale.y, 1f));

		public SVGMatrix matrix => SVGMatrix.TRS(new Vector3(_position.x, _position.y, 0f), _rotation, new Vector2(_scale.x, _scale.y));

		public SVGTransform2D()
		{
			_position = Vector2.zero;
			_rotation = 0f;
			_scale = Vector2.one;
		}

		public SVGTransform2D(Vector2 position, float rotation, Vector2 scale)
		{
			_position = position;
			_rotation = rotation;
			_scale = scale;
		}

		public SVGTransform2D(SVGTransform2D transform)
		{
			SetTransform(transform);
		}

		public object Clone()
		{
			return new SVGTransform2D(_position, _rotation, _scale);
		}

		public void SetTransform(SVGTransform2D transform)
		{
			if (transform != null)
			{
				_position = transform._position;
				_rotation = transform._rotation;
				_scale = transform._scale;
			}
		}

		public void Reset()
		{
			_position = Vector2.zero;
			_rotation = 0f;
			_scale = Vector2.one;
		}

		public void TRS(Vector2 position, float rotation, Vector2 scale)
		{
			_position = position;
			_rotation = rotation;
			_scale = scale;
		}

		public bool Compare(SVGTransform2D transform)
		{
			if (transform == null)
			{
				return false;
			}
			if (_position == transform._position && _rotation == transform._rotation)
			{
				return _scale == transform._scale;
			}
			return false;
		}

		public static SVGTransform2D DecomposeMatrix(Matrix4x4 matrix)
		{
			return new SVGTransform2D(new Vector2(matrix[0, 3], matrix[1, 3]), Quaternion.LookRotation(new Vector3(matrix[0, 2], matrix[1, 2], matrix[2, 2]), new Vector3(matrix[0, 1], matrix[1, 1], matrix[2, 1])).eulerAngles.z, new Vector2(new Vector2(matrix[0, 0], matrix[1, 0]).magnitude, new Vector2(matrix[0, 1], matrix[1, 1]).magnitude));
		}
	}
}
