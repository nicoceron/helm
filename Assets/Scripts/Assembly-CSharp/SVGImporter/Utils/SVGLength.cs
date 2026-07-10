namespace SVGImporter.Utils
{
	public struct SVGLength
	{
		private SVGLengthType _unitType;

		private float _valueInSpecifiedUnits;

		private float _value;

		public float value => _value;

		public SVGLengthType unitType => _unitType;

		public SVGLength(SVGLengthType unitType, float valueInSpecifiedUnits)
		{
			_unitType = unitType;
			_valueInSpecifiedUnits = valueInSpecifiedUnits;
			_value = SVGLengthConvertor.ConvertToPX(_valueInSpecifiedUnits, _unitType);
		}

		public SVGLength(float valueInSpecifiedUnits)
		{
			_unitType = SVGLengthType.Number;
			_valueInSpecifiedUnits = valueInSpecifiedUnits;
			_value = SVGLengthConvertor.ConvertToPX(_valueInSpecifiedUnits, _unitType);
		}

		public SVGLength(string valueText)
		{
			float valueInSpecifiedUnits = 0f;
			SVGLengthType lengthType = SVGLengthType.Unknown;
			SVGLengthConvertor.ExtractType(valueText, ref valueInSpecifiedUnits, ref lengthType);
			_unitType = lengthType;
			_valueInSpecifiedUnits = valueInSpecifiedUnits;
			_value = SVGLengthConvertor.ConvertToPX(_valueInSpecifiedUnits, _unitType);
		}

		public void NewValueSpecifiedUnits(float valueInSpecifiedUnits)
		{
			_unitType = SVGLengthType.Unknown;
			_valueInSpecifiedUnits = valueInSpecifiedUnits;
			_value = SVGLengthConvertor.ConvertToPX(_valueInSpecifiedUnits, _unitType);
		}

		public static float GetPXLength(string valueText)
		{
			float num = 0f;
			SVGLengthType lengthType = SVGLengthType.Unknown;
			SVGLengthConvertor.ExtractType(valueText, ref num, ref lengthType);
			return SVGLengthConvertor.ConvertToPX(num, lengthType);
		}

		public SVGLength Multiply(SVGLength svglength)
		{
			if (unitType == SVGLengthType.Percentage && svglength.unitType == SVGLengthType.Percentage)
			{
				return new SVGLength(SVGLengthType.Percentage, value * svglength.value);
			}
			return new SVGLength(SVGLengthType.PX, value * svglength.value);
		}

		public override string ToString()
		{
			return value.ToString();
		}
	}
}
