using Mobius.SpotfireDocument;
using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml;
using System.Xml.Serialization;

namespace Mobius.SpotfireDocument
{


	/// <summary>
	/// ColoringMsx
	/// </summary>

	public class ColoringMsx : NodeMsx
	{
		public string MsxId = new Guid().ToString(); // used to internally reference colorings

		public string DisplayName { get; set; }

		public XmlColor EmptyColor { get; set; }
		public bool EvaluatePerColumn { get; set; }
		public bool EvaluatePerTrellis { get; set; }
		public XmlColor DefaultColor { get; set; }

		public bool SetColorOnText { get; set; }

		public List<ColorRuleMsx> Rules = new List<ColorRuleMsx>();
	}

	/// <summary>
	/// ColorRuleMsx
	/// </summary>

	public class ColorRuleMsx : NodeMsx
	{
		public string ManualDisplayName { get; set; }
	}

	/// <summary>
	/// CategoricalColorRuleMsx
	/// </summary>

	public class CategoricalColorRuleMsx : ColorRuleMsx
	{
		// todo
	}

	/// <summary>
	/// ContinuousColorRuleMsx
	/// </summary>

	public class ContinuousColorRuleMsx : ColorRuleMsx
	{
		// todo
	}

	/// <summary>
	/// ConditionalColorRuleMsx - Other classes based on ConditionalColorRule:
	/// 
	///  public ExpressionColorRule ecr;
	///  public RangeColorRule rcr;
	///  public StringColorRule scr;
	///  public ThresholdColorRule tcr;
	///  public TopBottomColorRule tbcr;
	/// </summary>

	public abstract class ConditionalColorRuleMsx : ColorRuleMsx
	{
		public XmlColor Color { get; set; }
	}

	/// <summary>
	/// StringColorRuleMsx
	/// </summary>

	public class StringColorRuleMsx : ConditionalColorRuleMsx
	{
		// todo
	}

	/// <summary>
	/// RangeColorRuleMsx
	/// </summary>

	public class RangeColorRuleMsx : ConditionalColorRuleMsx
	{
		// todo
	}

	/// <summary>
	/// ThresholdColorRuleMsx
	/// </summary>

	public class ThresholdColorRuleMsx : ConditionalColorRuleMsx
	{
		// todo
	}

	/// <summary>
	/// TopBottomColorRuleMsx
	/// </summary>

	public class TopBottomColorRuleMsx : ConditionalColorRuleMsx
	{
		// todo
	}

	/// <summary>
	/// ExpressionColorRuleMsx
	/// </summary>

	public class ExpressionColorRuleMsx : ConditionalColorRuleMsx
	{
		// todo
	}

	/// <summary>
	/// ColoringCollectionMsx
	/// </summary>

	public class ColoringCollectionMsx : NodeMsx
	{
		public List<ColoringMsx> Colorings = new List<ColoringMsx>();

		// Mappings property
		[XmlIgnore] // Can't XmlSerialize a Dictionary, pass as list defined below
		public Dictionary<CategoryKeyMsx, string> CategoryToGuidDict = new Dictionary<CategoryKeyMsx, string>();

		public List<KeyValuePair<CategoryKeyMsx, string>> CategoryToGuidList
		{
			get { return CategoryToGuidDict.ToList(); }
			set { CategoryToGuidDict = value.ToDictionary(x => x.Key, x => x.Value); }
		}

		// Items property
		[XmlIgnore] // Can't XmlSerialize a Dictionary, pass as list defined below
		public Dictionary<string, ColoringMsx> GuidToColoringDict = new Dictionary<string, ColoringMsx>();
		public List<KeyValuePair<string, ColoringMsx>> GuidToColoringList
		{
			get { return GuidToColoringDict.ToList(); }
			set { GuidToColoringDict = value.ToDictionary(x => x.Key, x => x.Value); }
		}

		// OrderedItems property:		base.CreateProperty<System.Guid>(ColoringCollection.PrivatePropertyNames.OrderedItems, out this.orderedItems);
		[XmlIgnore]
		public List<string> orderedItems; // ordered by guid

		public List<CategoryKeyMsx> GetCategories()
		{
			List<CategoryKeyMsx> list = new List<CategoryKeyMsx>();
			list.AddRange(CategoryToGuidDict.Keys);
			return list;
		}

		public ColoringMsx GetColoringForCategory(CategoryKeyMsx category)
		{
			string key;
			if (CategoryToGuidDict.TryGetValue(category, out key))
			{
				return this.GuidToColoringDict[key];
			}

			return null;
		}

		public ColoringMsx AddNew(string displayName)
		{
			ColoringMsx newColoring = new ColoringMsx();
			newColoring.DisplayName = displayName;
			GuidToColoringDict.Add(newColoring.MsxId, newColoring);
			orderedItems.Add(newColoring.MsxId);
			return newColoring;
		}

		public void AddMapping(CategoryKeyMsx category, ColoringMsx coloring)
		{
			CategoryToGuidDict[category] = coloring.MsxId;
		}


		public List<CategoryKeyMsx> GetCategoriesForColoring(ColoringMsx coloring)
		{
			List<CategoryKeyMsx> list = new List<CategoryKeyMsx>();
			foreach (KeyValuePair<string, ColoringMsx> current in this.GuidToColoringDict)
			{
				if (object.ReferenceEquals(coloring, current.Value))
				{
					foreach (KeyValuePair<CategoryKeyMsx, string> current2 in this.CategoryToGuidDict)
					{
						if (current2.Value == current.Key)
						{
							list.Add(current2.Key);
						}
					}
				}
			}
			return list;
		}
	}

	/// <summary>
	/// ColorMapMsx - Mapping of category value (object array of values to Color)
	/// </summary>

	public class ColorMapMsx : NodeMsx
	{
		[XmlIgnore] // Can't XmlSerialize a Dictionary, pass as list defined below
		public Dictionary<CategoryKeyMsx, XmlColor> CategoryToColorDict = new Dictionary<CategoryKeyMsx, XmlColor>();

		public List<KeyValuePair<CategoryKeyMsx, XmlColor>> CategoryToColorList
		{
			get { return CategoryToColorDict.ToList(); }
			set { CategoryToColorDict = value.ToDictionary(x => x.Key, x => x.Value); }
		}
	}


}
