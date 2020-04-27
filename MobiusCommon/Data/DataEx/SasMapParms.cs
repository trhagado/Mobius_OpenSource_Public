using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Mobius.Data
{
	/// <summary>
	/// Defines the attributes and elements of a SAS map
	/// </summary>

	public class SasMapParms
	{
		public SarMapType MapType = SarMapType.Unknown; // basic map type

		[XmlIgnore]
		public QueryColumn KeyCriteriaQc = null; // key criteria on list of CIDS to analyze
		public string _kcqcMc = ""; // for serialization
		public string _kcqcCriteria = ""; // for serialization

		[XmlIgnore]
		public MetaColumn EndpointMc = null; // endpoint being analyzed
		public string _endpointMc = ""; // for serialization

		public ActDiffCalcType ActDiffCalcType = ActDiffCalcType.SimpleDifference;

		public bool UseAbsoluteValue = true;

		public SimilaritySearchType SimilarityType = SimilaritySearchType.ECFP4; // type of similarity if sim search 
		public double MinimumSimilarity = MoleculeMx.DefaultMinSimScore; // lower sim limit

		public int MaxPairCount = 100; // maximum number of pairs to retain/display

		public const string MetaTableName = "SASMAP"; // name of associated MetaTable
		public const string ParametersMetaColumnName = "TOOL_PARAMETERS"; // Metacolumn containing parameters

		/// <summary>
		/// Serialize
		/// </summary>
		/// <returns></returns>

		public string Serialize()
		{
			_kcqcMc = _kcqcCriteria = null;
			if (KeyCriteriaQc != null)
			{
				_kcqcMc = KeyCriteriaQc.MetaTableDotMetaColumnName;
				_kcqcCriteria = KeyCriteriaQc.Criteria;
			}

			_endpointMc = null;
			if (EndpointMc != null)
				_endpointMc = EndpointMc.MetaTableDotMetaColumnName;

			string s = XmlUtil.Serialize(this);
			return s;
		}

		/// <summary>
		/// Deserialize
		/// </summary>
		/// <param name="serializedForm"></param>
		/// <returns></returns>

		public static SasMapParms Deserialize(string serializedForm)
		{
			try
			{
				SasMapParms sm = (SasMapParms)XmlUtil.Deserialize(serializedForm, typeof(SasMapParms));

				if (Lex.IsDefined(sm._kcqcMc))
				{
					MetaColumn mc = MetaColumn.ParseMetaTableMetaColumnName(sm._kcqcMc);
					sm.KeyCriteriaQc = new QueryColumn(mc);
					sm.KeyCriteriaQc.Criteria = sm._kcqcCriteria;
				}

				if (Lex.IsDefined(sm._endpointMc))
					sm.EndpointMc = MetaColumn.ParseMetaTableMetaColumnName(sm._endpointMc);

				return sm;
			}

			catch (Exception ex)
			{
				DebugLog.Message(ex);
				return new SasMapParms();
			}
		}

	}

	/// <summary>
	/// Types of SAR Maps supported
	/// </summary>

	public enum SarMapType
	{
		Unknown = 0,
		SasMap = 1, // Plot of structure similarity vs activity similarity or potency difference. 
								// Identify activity cliffs, scaffold hops and continuous regions in the SAR
	}

	/// <summary>
	/// Basic types of SarMap activity difference calculation types
	/// </summary>

	public enum ActDiffCalcType
	{
		Undefined = 0,
		SimpleDifference = 1,
		NegativeLog = 2,
		MolarNegativeLog = 3,
		Ratio = 4,
		AbsoluteValue = 8,
		Advanced = 16
	}

}
