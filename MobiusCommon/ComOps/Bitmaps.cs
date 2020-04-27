using DevExpress.XtraEditors;
using DevExpress.Utils;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ComOps
{
	/// <summary>
	/// Form containing the bitmap definitions
	/// </summary>

	// DevExpress images:
	// 1. When in Visual Studio you can pick a DX control with an image and edit the image property to get a dialog that contains a DX Image Gallery tab
	// 2. DevExpress images: DevExpress.Images.ImageResourceCache.Default.GetImage("images/actions/add_16x16.png");
	// 3. DevExpress images: DevExpress.Utils.Controls.ImageHelper.CreateImageCollectionFromResources("GridColumnMenu.bmp", Assembly.GetCallingAssembly(), new Size(16, 16)); // (old)

	// Also, can look in .dlls with a resource viewer like ASMEX
	// Can also disassemble the assembly with: ildasm /out=assembly_name.il assembly_name.dll and then view the files

	public partial class Bitmaps : DevExpress.XtraEditors.XtraForm
	{
		static Bitmaps Instance; // single instance

		/// <summary>
		/// Predefined Color Sets
		/// </summary>

		public static Dictionary<string, Color[]> ColorSetImageColors = new Dictionary<string, Color[]>() {
			{ "ColorSet1", InitColorArray(0x88D09D, 0xFFE97D, 0xF6575A, 0xA0A0A0) },
			{ "ColorSet2", InitColorArray(0xF54D4F, 0xF9C1C4, 0xCAD9EE, 0x6694CC) },
			{ "ColorSet3", InitColorArray(0xF76A43, 0xFB8021, 0xFFAB16, 0xFED353) },
			{ "ColorSet4", InitColorArray(0xF65557, 0xF89294, 0xFACBCD, 0xFEF4F4) },
			{ "ColorSet5", InitColorArray(0x51B068, 0x8ECE9F, 0xC9E8D3, 0xE5F2E8) },
			{ "ColorSet6", InitColorArray(0x1CAEDB, 0x55C2E4, 0xC6EBF6, 0xE7F4FB) },

			//{ "ColorSetOld1", InitColorArray(0x84CE99, 0xBADA8D, 0xFFE97E, 0xF55557) },
			//{ "ColorSetOld6", InitColorArray(0x52B069, 0x90CB7F, 0xD2E39E, 0xFFF3B1) },
			//{ "ColorSetOld7", InitColorArray(0x7CCB92, 0xCFEAD8, 0xFACACC, 0xF65A5B) },
		};

		/// <summary>
		/// Predefined Color Scales
		/// Colors proceed from the bottom (lightest color) to the top (darkest color)
		/// </summary>

		public static Dictionary<string, Color[]> ColorScaleImageColors = new Dictionary<string, Color[]>() {
			{ "ColorScale1", InitColorArray(0xF65858, 0xFFEC89, 0x94D6A7) }, // three colors
			{ "ColorScale2", InitColorArray(0xCBD7ED, 0xEA7C85) },
			{ "ColorScale3", InitColorArray(0xFDDB74, 0xF76A43) },
			{ "ColorScale4", InitColorArray(0xFFFFFF, 0xF65759) },
			{ "ColorScale5", InitColorArray(0xFFFFFF, 0x54B26B) },
			{ "ColorScale6", InitColorArray(0xFFFFFF, 0x22B0DC) }

			// Previous
			//{ "ColorScale1", InitColorArray(0x94D6A7, 0xFFEC89, 0xF65858) }, // three colors
			//{ "ColorScale2", InitColorArray(0xEA7C85, 0xCBD7ED) },
			//{ "ColorScale3", InitColorArray(0xF76A43, 0xFDDB74) },
			//{ "ColorScale4", InitColorArray(0xF65759, 0xFFFFFF) },
			//{ "ColorScale5", InitColorArray(0x54B26B, 0xFFFFFF) },
			//{ "ColorScale6", InitColorArray(0x22B0DC, 0xFFFFFF) }
		};

		/// <summary>
		/// Predefined Data Bar Colors
		/// </summary>

		public static Dictionary<string, Color[]> DataBarsImageColors = new Dictionary<string, Color[]>() {
			{ "DataBarsBlue",    InitColorArray(0x678FC6) },
			{ "DataBarsGreen",   InitColorArray(0x70C78D) },
			{ "DataBarsRed",     InitColorArray(0xFF4F54) },
			{ "DataBarsCyan",    InitColorArray(0x008BEF) },
			{ "DataBarsYellow",  InitColorArray(0xFFB92D) },
			{ "DataBarsMagenta", InitColorArray(0xD5007D) }
		};

		public Bitmaps()
		{
			InitializeComponent();
		}

		private static Color[] InitColorArray(params int[] colorInts)
		{
			Color[] colors = new Color[colorInts.Length];

			for (int ri = 0; ri < colorInts.Length; ri++)
			{
				Color baseColor = Color.FromArgb(colorInts[ri]);
				colors[ri] = Color.FromArgb(255, baseColor); // must set alpha to opaque
			}

			return colors;
		}



		/// <summary>
		/// Get instance of bitmap collections
		/// </summary>

		public static Bitmaps I
		{
			get
			{
				if (Instance == null) Instance = new Bitmaps();
				return Instance;
			}
		}

		/// <summary>
		/// Get image list
		/// </summary>
		/// <returns></returns>

		public static ImageList Bitmaps16x16
		{
			get
			{
				if (Instance == null) Instance = new Bitmaps();
				return Instance._bitmaps16x16;
			}
		}

		/// <summary>
		/// Lookup a color set in a color set dict by name
		/// </summary>
		/// <param name="colorDict"></param>
		/// <param name="colorSetName"></param>
		/// <returns></returns>

		public static Color[] GetColorSetByName(
			Dictionary<string, Color[]> colorDict,
			string colorSetName)
		{
			if (Lex.IsUndefined(colorSetName)) return null;

			int i1 = colorSetName.IndexOf("."); // remove any file extension
			if (i1 >= 0)
				colorSetName = colorSetName.Substring(0, i1);

			foreach (string setName in colorDict.Keys)
			{
				if (Lex.Eq(setName, colorSetName))
					return colorDict[setName];
			}

			return null;
		}

		/// <summary>
		/// Lookup a color scale name by the colors that define the scale
		/// </summary>
		/// <param name="colorSet"></param>
		/// <returns></returns>

		public static string GetColorScaleNameFromColorset(Color[] colorSet)
		{
			int ci;

			foreach (string scaleName in ColorScaleImageColors.Keys)
			{
				Color[] ca = ColorScaleImageColors[scaleName];
				if (ca.Length != colorSet.Length) continue;
				for (ci = 0; ci < ca.Length; ci++)
				{
					if (ca[ci] != colorSet[ci]) break;
				}

				if (ci < ca.Length) continue; 

				return scaleName;
			}

			return ""; 
		}

		/// <summary>
		/// Retrieve image with specified name
		/// </summary>
		/// <param name="ic"></param>
		/// <param name="targetName"></param>
		/// <returns></returns>

		public static Image GetImageFromName(
			ImageCollection ic,
			string targetName)
		{
			int i1 = GetImageIndexFromName(ic, targetName);
			if (i1 >= 0) return ic.Images[i1];
			else return null;
		}

		/// <summary>
		/// Retrieve image with specified name
		/// </summary>
		/// <param name="ic"></param>
		/// <param name="targetName"></param>
		/// <returns></returns>

		public static int GetImageIndexFromName(
		ImageCollection ic,
		string targetName)
		{
			if (ic == null || ic.Images == null ||
			 Lex.IsUndefined(targetName))
				return -1;

			targetName = targetName.ToUpper();
			string targetName2 = targetName + ".";
			Images images = ic.Images;
			StringCollection names = images.Keys;
			for (int i1 = 0; i1 < names.Count; i1++)
			{
				string name = names[i1].ToUpper();
				if (name == targetName || name.StartsWith(targetName2))
				{
					return i1;
				}
			}

			return -1;
		}

		/// <summary>
		/// Get image name from index
		/// </summary>
		/// <param name="ic"></param>
		/// <param name="idx"></param>
		/// <returns></returns>

		public static string GetImageNameFromIndex(
			ImageCollection ic,
			int idx)
		{
			if (ic == null) return null;

			Images images = ic.Images;
			StringCollection names = images.Keys;
			if (idx >= 0 && idx < images.Keys.Count)
			{
				string name = names[idx];
				int i1 = name.IndexOf("."); // remove any file extension
				if (i1 >= 0)
					name = name.Substring(0, i1);
				return name;
			}

			else return null;
		}

		/// <summary>
		/// Lookup entry in Bitmaps16x16Enum by name and return any matching index
		/// </summary>
		/// <param name="bitMapName"></param>
		/// <returns></returns>

		public static int GetBitMapIndexFromName(
		string bitMapName)
	{
		Bitmaps16x16Enum bmi;

		if (Lex.IsUndefined(bitMapName))
			return -1;

		if (Enum.TryParse<Bitmaps16x16Enum>(bitMapName, true, out bmi))
			return (int)bmi;

		else return -1;
	}
}

/// <summary>
/// Small bitmap images (16 x 16) used in interface
/// </summary>

public enum Bitmaps16x16Enum
	{
		None = -1, // no image assigned
		Unknown = 0, // unknown, question mark
		Home = 1, // home
		World = 2, // the world
		Unused3 = 3,
		Folder = 4, // folder
		FolderOpen = 5, // open folder
		Table = 6, // data table
		Tables = 7, // multiple data tables
		Document = 8, // document types
		URL = 9, // web page
		CidList = 10, // compound number list
		Query = 11, // query
		Action = 12, // some action
		FolderSearch = 13,
		WorldSearch = 14,
		KeyResult = 15,
		Up = 16,
		Down = 17,
		First = 18,
		Previous = 19,
		Next = 20,
		Last = 21,
		Excel = 22,
		Print = 23,
		Books = 24,
		BoolSearch = 25,
		StructurePlus = 26,
		Tools = 27,
		Help = 28,
		Design = 29,
		SaveDoc = 30,
		OpenDoc = 31,
		NewDoc = 32,
		Run = 33,
		Key = 34, // key data type image 
		Text = 35, // text data type image
		Number = 36, // number data type image 
		QualNumber = 37, // qualified number data type image
		Date = 38, // date data type image 
		Structure = 39, // structure data type image 
		Graph = 40, // graph data type image 
		Cut = 41,
		Copy = 42,
		Paste = 43,
		Undo = 44,
		Redo = 45,
		StructSearch = 46,
		ListSearch = 47,
		Remove = 48,
		SortAsc = 49,
		SortDesc = 50,
		SortAsc1 = 51,
		SortAsc2 = 52,
		SortAsc3 = 53,
		SortDesc1 = 54,
		SortDesc2 = 55,
		SortDesc3 = 56,
		Fx = 57,
		MagFx = 58,
		SearchDoc = 59,
		Favorites = 60,
		Info = 61,
		SarTable = 62,
		DataTable = 63,
		AssayTable = 64,
		Person = 65,
		People = 66,
		CondFormat = 67,
		CondFormatPlus = 68,
		CondFormatSmall = 69,
		Target = 70,
		TargetFamily = 71,
		Project = 72,
		CalcField = 73,
		Annotation = 74,
		Sort = 75,
		Go = 76,
		DropdownBlack = 77,
		DropdownGreen = 78,
		Import = 79,
		Export = 80,
		FolderPublic = 81,
		FolderOpenPublic = 82,
		QueryPublic = 83,
		CidListPublic = 84,
		AnnotationPublic = 85,
		CalcFieldPublic = 86,
		UserCmpdDb = 87,
		SummaryOff = 88,
		SummaryOn = 89,
		AutoFilter = 90,
		Chart = 91,
		Properties = 92,
		QueryFolder = 93,
		ListFolder = 94,
		AnnotationFolder = 95,
		CalcFieldFolder = 96,
		Database = 97,
		Add = 98,
		TargetSummaryUnpivoted = 99,
		TargetSummaryPivoted = 100,
		CfSummary = 101,
		PivotTable = 102,
		Spotfire = 103,
		SpotfireLink = 104,
		ScatterPlot = 105,
		BarChart = 106,
		RadarPlot = 107,
		HeatMap = 108,
		SurfaceChart = 109,
		TargetMap = 110,
		SasMap = 111,
		CfActivityClass = 112,
		Library = 113,
		Network = 114,
		TargetTargetNetwork = 115,
		TargetTargetTable = 116,
		ResultsPage = 117,
		Group = 118
	}

	/// <summary>
	/// BitMaps5X8 for showing sorting in worksheets
	/// </summary>

	public enum SortedBitmap
	{
		Ascending = 0,
		Ascending1 = 1,
		Ascending2 = 2,
		Ascending3 = 3,
		Descending = 4,
		Descending1 = 5,
		Descending2 = 6,
		Descending3 = 7
	}

}