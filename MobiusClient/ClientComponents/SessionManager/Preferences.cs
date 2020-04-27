using Mobius.Data;
using Mobius.ComOps;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Read and write user preferences
	/// </summary>

	public class Preferences
	{
		static Dictionary<string, string> PrefDict; // dictionary of preferences for user

		public Preferences()
		{
		}

		public static void Load()
		{
			string value;
			int i1;

			PrefDict = UserObjectDao.GetUserParameters(Security.UserName);

			SS.I.PreferredProjectId = // get preferred project
				 Get("PreferredProject", "DEFAULT_FOLDER");

			SS.I.TableColumnZoom = GetInt("TableColumnZoom", 100);
			SS.I.GraphicsColumnZoom = GetInt("GraphicsColumnZoom", 100);
			SS.I.ScrollGridByPixel = GetBool("ScrollGridByPixel", false); // default to row scrolling
			SS.I.AsyncImageRetrieval = GetBool("AsyncImageRetrieval", true);

			try // get preferred number format
			{
				value = Get("DefaultNumberFormat", "SigDigits");
				EnumUtil.TryParse(value, out SS.I.DefaultNumberFormat);

				SS.I.DefaultDecimals = GetInt("DefaultDecimals", 3);
			}
			catch (Exception ex) { }

			SS.I.RepeatReport = // repeating report across page 
				GetBool("RepeatReport", false);

			MqlUtil.DefaultToSingleStepQueryExecution = // default to single step query execution
				GetBool("DefaultToSingleStepQueryExecution", false);

			if (MqlUtil.DefaultToSingleStepQueryExecution)
				QueryEngine.SetParameter("DefaultToSingleStepQueryExecution", MqlUtil.DefaultToSingleStepQueryExecution.ToString()); // set in QE for current session

			SS.I.FindRelatedCpdsInQuickSearch = 
				GetBool("FindRelatedCpdsInQuickSearch", true);

			SS.I.RestoreWindowsAtStartup = // get bool for restoring windows at startup
				GetBool("RestoreWindowsAtStartup", false);

			SS.I.BreakHtmlPopupsAtPageWidth =
				GetBool("BreakHtmlPopupsAtPageWidth", true);

			try // qualified number splitting
			{
				value = Get("QualifiedNumberSplit");
				SS.I.QualifiedNumberSplit = QnfSplitControl.DeserializeQualifiedNumberSplit(value);
			}
			catch (Exception ex) { }

			SS.I.HilightCidChanges =
				GetBool("HilightCorpIdChanges", true);

			SS.I.RemoveLeadingZerosFromCids =
				GetBool("RemoveLeadingZerosFromCids", true);

			SS.I.GridMarkCheckBoxesInitially = // (always false initially now)
				GetBool("GridMarkCheckBoxesInitially", false);

			value =	Get("DefaultHorizontalAlignment", MetaColumn.SessionDefaultHAlignment.ToString());
			MetaColumn.SessionDefaultHAlignment = (HorizontalAlignmentEx)Enum.Parse(typeof(HorizontalAlignmentEx), value, true);

			value = Get("DefaultVerticalAlignment", MetaColumn.SessionDefaultVAlignment.ToString());
			MetaColumn.SessionDefaultVAlignment = (VerticalAlignmentEx)Enum.Parse(typeof(VerticalAlignmentEx), value, true);

			SS.I.EvenRowBackgroundColor =
				Color.FromArgb(GetInt("EvenRowBackgroundColor", Color.WhiteSmoke.ToArgb())); // slightly contrasting color

			SS.I.OddRowBackgroundColor =
				Color.FromArgb(GetInt("OddRowBackgroundColor", Color.White.ToArgb()));

			ClientDirs.DefaultMobiusUserDocumentsFolder = Preferences.Get("DefaultExportFolder", ClientDirs.DefaultMobiusUserDocumentsFolder);

			MoleculeFormat molFormat = MoleculeFormat.Molfile;
			value = Get("PreferredMoleculeFormat", "Molfile");
			if (Enum.TryParse(value, out molFormat))
				MoleculeMx.PreferredMoleculeFormat = molFormat;

			return;
		}

/// <summary>
/// Get a string parameter value
/// </summary>
/// <param name="parameter"></param>
/// <returns></returns>

		public static string Get (
			string parameter)
		{
			return Get(parameter, "");
		}

/// <summary>
/// Get a string parameter value
/// </summary>
/// <param name="parameter"></param>
/// <param name="defaultValue"></param>
/// <returns></returns>

		public static string Get (
			string parameter,
			string defaultValue)
		{
			string key = parameter.ToUpper();
			if (PrefDict == null) Load();

			if (PrefDict.ContainsKey(key)) return PrefDict[key];
			else return defaultValue;
		}

/// <summary>
/// Get an integer parameter value
/// </summary>
/// <param name="parameter"></param>
/// <returns></returns>

		public static int GetInt(
			string parameter)
		{
			return GetInt(parameter, -1);
		}

		public static int GetInt(
			string parameter,
			int defaultValue)
		{
			string val = Get(parameter, "");
			if (String.IsNullOrEmpty(val)) return defaultValue;

			int intVal = defaultValue;
			int.TryParse(val, out intVal);
			return intVal;
		}

/// <summary>
/// Get a boolean parameter value
/// </summary>
/// <param name="parameter"></param>
/// <returns></returns>

		public static bool GetBool(
			string parameter)
		{
			return GetBool(parameter, false);
		}

		public static bool GetBool(
			string parameter,
			bool defaultValue)
		{
			string val = Get(parameter, "");
			if (String.IsNullOrEmpty(val)) return defaultValue;

			bool boolVal = defaultValue;
			bool.TryParse(val, out boolVal);
			return boolVal;
		}

/// <summary>
/// Set value for a string parameter
/// </summary>
/// <param name="parameter"></param>
/// <param name="value"></param>

		public static void Set (
			string parameter,
			string value)
		{
			string key = parameter.ToUpper();
			PrefDict[key] = value; // update local dict

			UserObjectDao.SetUserParameter(SS.I.UserName, parameter, value);
		}

/// <summary>
/// Set value for an int parameter
/// </summary>
/// <param name="parameter"></param>
/// <param name="value"></param>

		public static void Set (
			string parameter,
			int value)
		{
			Set(parameter, value.ToString());
		}

/// <summary>
/// Set value for a bool parameter
/// </summary>
/// <param name="parameter"></param>
/// <param name="value"></param>

		public static void Set (
			string parameter,
			bool value)
		{
			Set(parameter, value.ToString());
		}

	}
}
