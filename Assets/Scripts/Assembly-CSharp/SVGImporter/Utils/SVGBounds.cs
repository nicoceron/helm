using System;
using UnityEngine;

namespace SVGImporter.Utils
{
	[Serializable]
	public struct SVGBounds
	{
		[HideInInspector]
		[SerializeField]
		private float _minX;

		[HideInInspector]
		[SerializeField]
		private float _minY;

		[HideInInspector]
		[SerializeField]
		private float _maxX;

		[HideInInspector]
		[SerializeField]
		private float _maxY;

		[SerializeField]
		private Vector2 _center;

		[SerializeField]
		private Vector2 _size;

		[HideInInspector]
		[SerializeField]
		private Vector2 _extents;

		public float minX
		{
			get
			{
				return _minX;
			}
			set
			{
				if (_minX != value)
				{
					_minX = value;
					UpdateSizeExtentsCenter();
				}
			}
		}

		public float maxX
		{
			get
			{
				return _maxX;
			}
			set
			{
				if (_maxX != value)
				{
					_maxX = value;
					UpdateSizeExtentsCenter();
				}
			}
		}

		public float minY
		{
			get
			{
				return _minY;
			}
			set
			{
				if (_minY != value)
				{
					_minY = value;
					UpdateSizeExtentsCenter();
				}
			}
		}

		public float maxY
		{
			get
			{
				return _maxY;
			}
			set
			{
				if (_maxY != value)
				{
					_maxY = value;
					UpdateSizeExtentsCenter();
				}
			}
		}

		public Vector2 min
		{
			get
			{
				return new Vector2(_minX, _minY);
			}
			set
			{
				if (_minX != value.x || _minY != value.y)
				{
					_minX = value.x;
					_minY = value.y;
					UpdateSizeExtentsCenter();
				}
			}
		}

		public Vector2 max
		{
			get
			{
				return new Vector2(_maxX, _maxY);
			}
			set
			{
				if (_maxX != value.x || _maxY != value.y)
				{
					_maxX = value.x;
					_maxY = value.y;
					UpdateSizeExtentsCenter();
				}
			}
		}

		public Vector2 size
		{
			get
			{
				return _size;
			}
			set
			{
				if (!(_size == value))
				{
					_size = value;
					_extents = value * 0.5f;
					UpdateMinMax();
				}
			}
		}

		public Vector2 extents
		{
			get
			{
				return _extents;
			}
			set
			{
				if (!(_extents == value))
				{
					_size = value * 2f;
					_extents = value;
					UpdateMinMax();
				}
			}
		}

		public Vector2 center
		{
			get
			{
				return _center;
			}
			set
			{
				if (!(_center == value))
				{
					_center = value;
					UpdateMinMax();
				}
			}
		}

		public Rect rect => new Rect(_minX, _minY, _size.x, _size.y);

		public static SVGBounds InfiniteInverse => new SVGBounds(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);

		public bool isInfiniteInverse
		{
			get
			{
				if (_minX == float.MaxValue && _minY == float.MaxValue && _maxX == float.MinValue)
				{
					return _maxY == float.MinValue;
				}
				return false;
			}
		}

		public SVGBounds(float minX, float minY, float maxX, float maxY)
		{
			_minX = minX;
			_minY = minY;
			_maxX = maxX;
			_maxY = maxY;
			_center = Vector2.zero;
			_size = Vector2.one;
			_extents = _size * 0.5f;
			UpdateSizeExtentsCenter();
		}

		public SVGBounds(Vector2 center, Vector2 size)
		{
			_minX = 0f;
			_minY = 0f;
			_maxX = 0f;
			_maxY = 0f;
			_center = center;
			_size = size;
			_extents = _size * 0.5f;
			UpdateMinMax();
		}

		public SVGBounds(Bounds bounds)
		{
			_minX = 0f;
			_minY = 0f;
			_maxX = 0f;
			_maxY = 0f;
			_center = bounds.center;
			_size = bounds.size;
			_extents = _size * 0.5f;
			UpdateMinMax();
		}

		public bool Contains(Vector2 point)
		{
			if (point.x >= _minX && point.x <= _maxX && point.y >= _minY)
			{
				return point.y <= _maxY;
			}
			return false;
		}

		public bool Contains(SVGBounds bounds)
		{
			if (bounds._minX >= _minX && bounds._minY >= _minY && bounds._maxX <= _maxX)
			{
				return bounds._maxY <= _maxY;
			}
			return false;
		}

		public bool Contains(Vector2 center, Vector2 size)
		{
			size *= 0.5f;
			if (center.x - size.x >= _minX && center.y - size.y >= _minY && center.x + size.x <= _maxX)
			{
				return center.y + size.y <= _maxY;
			}
			return false;
		}

		public SVGBounds Encapsulate(Vector2 point)
		{
			bool flag = false;
			if (point.x < _minX)
			{
				_minX = point.x;
				flag = true;
			}
			if (point.x > _maxX)
			{
				_maxX = point.x;
				flag = true;
			}
			if (point.y < _minY)
			{
				_minY = point.y;
				flag = true;
			}
			if (point.y > _maxY)
			{
				_maxY = point.y;
				flag = true;
			}
			if (flag)
			{
				UpdateSizeExtentsCenter();
			}
			return this;
		}

		public SVGBounds Encapsulate(float minX, float minY, float maxX, float maxY)
		{
			bool flag = false;
			if (minX < _minX)
			{
				_minX = minX;
				flag = true;
			}
			if (maxX > _maxX)
			{
				_maxX = maxX;
				flag = true;
			}
			if (minY < _minY)
			{
				_minY = minY;
				flag = true;
			}
			if (maxY > _maxY)
			{
				_maxY = maxY;
				flag = true;
			}
			if (flag)
			{
				UpdateSizeExtentsCenter();
			}
			return this;
		}

		public SVGBounds Encapsulate(Vector2 center, Vector2 size)
		{
			size *= 0.5f;
			return Encapsulate(center.x - size.x, center.y - size.y, center.x + size.x, center.y + size.y);
		}

		public SVGBounds Encapsulate(SVGBounds bounds)
		{
			return Encapsulate(bounds._minX, bounds._minY, bounds._maxX, bounds._maxY);
		}

		public SVGBounds Encapsulate(Bounds bounds)
		{
			return Encapsulate(bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y);
		}

		public SVGBounds Expand(float amount)
		{
			if (amount == 1f)
			{
				return this;
			}
			_size *= amount;
			_extents = _size * 0.5f;
			UpdateMinMax();
			return this;
		}

		public SVGBounds Expand(Vector2 amount)
		{
			if (amount.x == 1f && amount.y == 1f)
			{
				return this;
			}
			_size.x *= amount.x;
			_size.y *= amount.y;
			_extents = _size * 0.5f;
			UpdateMinMax();
			return this;
		}

		public bool Intersects(SVGBounds bounds)
		{
			if (!(_minX > bounds._maxX) && !(_maxX < bounds._minX) && !(_minY > bounds._maxY))
			{
				return !(_maxY < bounds._minY);
			}
			return false;
		}

		public SVGBounds SetMinMax(float minX, float minY, float maxX, float maxY)
		{
			_minX = minX;
			_minY = minY;
			_maxX = maxX;
			_maxY = maxY;
			UpdateSizeExtentsCenter();
			return this;
		}

		public void ApplyBounds(SVGBounds bounds)
		{
			SetMinMax(bounds._minX, bounds._minY, bounds._maxX, bounds._maxY);
		}

		public SVGBounds Reset()
		{
			_minX = (_maxX = (_minY = (_maxY = 0f)));
			_center = Vector2.zero;
			_size = Vector2.zero;
			_extents = Vector2.zero;
			return this;
		}

		public bool Compare(SVGBounds bounds)
		{
			if (_minX == bounds._minX && _minY == bounds._minY && _maxX == bounds._maxX)
			{
				return _maxY == bounds._maxY;
			}
			return false;
		}

		public SVGBounds ResetToInfiniteInverse()
		{
			SetMinMax(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
			return this;
		}

		public override string ToString()
		{
			return $"[SVGBounds: minX={minX}, maxX={maxX}, minY={minY}, maxY={maxY}, size={size}, extents={extents}, center={center}, rect={rect}]";
		}

		public Bounds ToBounds()
		{
			return new Bounds(_center, _size);
		}

		private void UpdateMinMax()
		{
			_minX = _center.x - _extents.x;
			_minY = _center.y - _extents.y;
			_maxX = _center.x + _extents.x;
			_maxY = _center.y + _extents.y;
		}

		private void UpdateSizeExtentsCenter()
		{
			_size.x = Mathf.Abs(_maxX - _minX);
			_size.y = Mathf.Abs(_maxY - _minY);
			_extents.x = _size.x * 0.5f;
			_extents.y = _size.y * 0.5f;
			_center.x = _minX + _extents.x;
			_center.y = _minY + _extents.y;
		}
	}
}
