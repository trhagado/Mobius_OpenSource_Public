using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Mobius.SpotfireDocument
{

	/// <summary>
	/// VisualmSX
	/// </summary>

	// Includes below needed to be able to serialize/deserialize the listed subclasses

	[XmlInclude(typeof(TablePlotMsx))]
	[XmlInclude(typeof(BarChartMsx))]
	[XmlInclude(typeof(ScatterPlotMsx))]
	[XmlInclude(typeof(TreemapMsx))]
	[XmlInclude(typeof(HeatMapMsx))]
	[XmlInclude(typeof(TrellisCardVisualMsx))]
	public class VisualMsx : NodeMsx

	{
		public string Id = ""; // guid
		public string Title = "";
		public bool ShowTitle = true;
		public TypeIdentifierMsx TypeId;

		public string Description = "";
		public bool ShowDescription = true;

		public VisualizationDataMsx Data = new VisualizationDataMsx();


/// <summary>
/// Serialize a visual
/// </summary>
/// <returns></returns>

		public string Serialize()
		{
			UpdatePreSerializationSecondaryReferences();

			string serializedText = SerializeMsx.Serialize(this); // serialize
			return serializedText;
		}

		/// <summary>
		/// Deserialize a visual returning the appropriate visual object that is derived from 
		/// the base VisualMsx object.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>

		public static VisualMsx Deserialize(
			string data,
			AnalysisApplicationMsx app)
		{
			Type visType = null;

			if (data.IndexOf("<TablePlotMsx", StringComparison.OrdinalIgnoreCase) == 0)
				visType = typeof(TablePlotMsx);

			else if (data.IndexOf("<BarChartMsx", StringComparison.OrdinalIgnoreCase) == 0)
				visType = typeof(BarChartMsx);

			else if (data.IndexOf("<ScatterPlotMsx", StringComparison.OrdinalIgnoreCase) == 0)
				visType = typeof(ScatterPlotMsx);

			else if (data.IndexOf("<TreemapMsx", StringComparison.OrdinalIgnoreCase) == 0)
				visType = typeof(TreemapMsx);

			else if (data.IndexOf("<HeatMapMsx", StringComparison.OrdinalIgnoreCase) == 0)
				visType = typeof(HeatMapMsx);

			else if (data.IndexOf("<TrellisCardvisualMsx", StringComparison.OrdinalIgnoreCase) == 0)
				visType = typeof(TrellisCardVisualMsx);

			else visType = typeof(VisualMsx); // shouldn't happen

			XmlSerializer xmlSer = new XmlSerializer(visType);
			StringReader reader = new StringReader(data);

			VisualMsx visMsx = (VisualMsx)(xmlSer.Deserialize(reader));

			visMsx.SetApp(app);

			visMsx.UpdatePostDeserializationSecondaryReferences();

			return visMsx;
		}

		/// <summary>
		/// Update secondary object xxxMsx class object references to the
		/// associated Guid Ids prior to serialization.
		/// This method is normally overridden by each xxxMsx class to update 
		/// the references for that class.
		/// </summary>

		public override void UpdatePreSerializationSecondaryReferences()
		{
			Data.UpdatePreSerializationSecondaryReferences();

			return;
		}


		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// Note that this base method should be done for each Visual type class (ScatterPlot, BarChart...)
		/// </summary>

		public override void UpdatePostDeserializationSecondaryReferences()
		{
			ValidateNode();

			UpdatePostDeserializationSecondaryReferences(Data);

			return;
		}

	}

	/// <summary>
	/// TypeIdentifier
	/// </summary>

	public class TypeIdentifierMsx
	{
		public string Description = "";
		public string DisplayName = "";
		public string Name = "";
	}

	public class VisualCollectionMsx : NodeMsx
	{
		public List<VisualMsx> VisualList = new List<VisualMsx>();

		//public VisualNavigationMode NavigationMode { get; set; }
		//public VisualizationAreaSize VisualizationAreaSize { get; }

		public bool TryGetVisual(
			string id,
			out VisualMsx visual)
		{
			foreach (VisualMsx vis in VisualList)
			{
				if (vis.Id == id)
				{
					visual = vis;
					return true;
				}
			}
			visual = null;
			return false;
		}

		/// <summary>
		/// Update secondary object xxxMsx class object references to the
		/// associated Guid Ids prior to serialization.
		/// This method is normally overridden by each xxxMsx class to update 
		/// the references for that class.
		/// </summary>

		public override void UpdatePreSerializationSecondaryReferences()
		{
			return; // this method should be overridden by the derived visuals
		}

		/// <summary>
		/// Adjust secondary references for each Mobius.SpotfireDocument.xxxMsx class 
		/// including DataTableMsx, DataColumnMsx, VisualMsx, PageMsx ...
		/// </summary>

		public override void UpdatePostDeserializationSecondaryReferences()
		{
			ValidateNode();

			return; // this method should be overridden by the derived visuals
		}

	}

	/// <summary>
	/// Color class that works properly with XmlSerializer
	/// </summary>
	public class XmlColor
	{
		private Color color_ = Color.Black;

		public XmlColor() { }
		public XmlColor(Color c) { color_ = c; }


		public static implicit operator Color(XmlColor x)
		{
			return x.color_;
		}

		public static implicit operator XmlColor(Color c)
		{
			return new XmlColor(c);
		}

		public static string FromColor(Color color)
		{
			if (color.IsNamedColor)
				return color.Name;

			int colorValue = color.ToArgb();

			if (((uint)colorValue >> 24) == 0xFF)
				return String.Format("#{0:X6}", colorValue & 0x00FFFFFF);
			else
				return String.Format("#{0:X8}", colorValue);
		}

		public static Color ToColor(string value)
		{
			try
			{
				if (value[0] == '#')
				{
					return Color.FromArgb((value.Length <= 7 ? unchecked((int)0xFF000000) : 0) +
							Int32.Parse(value.Substring(1), System.Globalization.NumberStyles.HexNumber));
				}
				else
				{
					return Color.FromName(value);
				}
			}
			catch (Exception)
			{
			}

			return Color.Black;
		}

		[XmlText]
		public string Default
		{
			get { return FromColor(color_); }
			set { color_ = ToColor(value); }
		}
	}

}
