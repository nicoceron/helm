using UnityEngine;
using UnityEngine.Serialization;

namespace SVGImporter
{
	public class SVGDocumentAsset : ScriptableObject
	{
		[FormerlySerializedAs("errors")]
		[SerializeField]
		protected SVGError[] _errors;

		[FormerlySerializedAs("svgFile")]
		[SerializeField]
		protected string _svgFile;

		[FormerlySerializedAs("title")]
		[SerializeField]
		protected string _title;

		[FormerlySerializedAs("description")]
		[SerializeField]
		protected string _description;

		public SVGError[] errors
		{
			get
			{
				return _errors;
			}
			set
			{
				_errors = value;
			}
		}

		public string svgFile
		{
			get
			{
				return _svgFile;
			}
			set
			{
				_svgFile = value;
			}
		}

		public string title
		{
			get
			{
				return _title;
			}
			set
			{
				_title = value;
			}
		}

		public string description
		{
			get
			{
				return _description;
			}
			set
			{
				_description = value;
			}
		}

		public static SVGDocumentAsset CreateInstance(string svgFile, SVGError[] errors = null, string title = null, string description = null)
		{
			SVGDocumentAsset sVGDocumentAsset = ScriptableObject.CreateInstance<SVGDocumentAsset>();
			sVGDocumentAsset._description = description;
			sVGDocumentAsset._title = title;
			sVGDocumentAsset._svgFile = svgFile;
			sVGDocumentAsset._errors = errors;
			return sVGDocumentAsset;
		}
	}
}
