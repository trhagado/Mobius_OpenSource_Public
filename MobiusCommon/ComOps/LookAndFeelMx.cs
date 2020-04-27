using DevExpress.XtraEditors;
using DevExpress.LookAndFeel;
using DevExpress.Skins;

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Reflection;

namespace Mobius.ComOps
{
	/// <summary>
	/// Common look and feel functionaligy
	/// </summary>

	public class LookAndFeelMx
	{

		// List of skins with some added attributes.
		// The index of the skin in the array matches the index of the image in Bitmaps16x16.
		// .Bmp images were created from DevExpress .png images by opening in Visual Studio and
		// then saving as a .bmp file.
		// BonusSkinsDllName = "DevExpress.BonusSkins.v<major.minor>.dll"; // change with new version

		static SkinInfoMx[] Skins = {
			new SkinInfoMx("Black", true),
			new SkinInfoMx("Blue", true),
			new SkinInfoMx("Blueprint", true),
			new SkinInfoMx("Caramel", true),
			new SkinInfoMx("Coffee", true),
			new SkinInfoMx("Dark Side", true),
			new SkinInfoMx("Darkroom", true),
			new SkinInfoMx("DevExpress Style", false, "Dx Style"),
			new SkinInfoMx("Foggy", true),
			new SkinInfoMx("Glass Oceans", true),
			new SkinInfoMx("High Contrast", true, DateTime.MaxValue, DateTime.MaxValue, "High Contrast"), // hidden
			new SkinInfoMx("iMaginary", true),
			new SkinInfoMx("Lilian", true),
			new SkinInfoMx("Liquid Sky", true),
			new SkinInfoMx("London Liquid Sky", true),
			new SkinInfoMx("McSkin", true),
			new SkinInfoMx("Metropolis", false),
			new SkinInfoMx("Money Twins", true),
			new SkinInfoMx("Office 2007 Black", true),
			new SkinInfoMx("Office 2007 Blue", true),
			new SkinInfoMx("Office 2007 Green", true),
			new SkinInfoMx("Office 2007 Pink", true),
			new SkinInfoMx("Office 2007 Silver", true),
			new SkinInfoMx("Office 2010 Black", false),
			new SkinInfoMx("Office 2010 Blue", false),
			new SkinInfoMx("Office 2010 Silver", false),
			new SkinInfoMx("Pumpkin", true, new DateTime(2000, 10, 1), new DateTime(2000, 10, 31), "October"),
			new SkinInfoMx("Sharp", true),
			new SkinInfoMx("Sharp Plus", true),
			new SkinInfoMx("Springtime", true, new DateTime(2000, 3, 20), new DateTime(2000, 6, 20), "Springtime"),
			new SkinInfoMx("Stardust", true), // 30
			new SkinInfoMx("Summer 2008", true, new DateTime(2000, 6, 21), new DateTime(2000, 9, 20), "Summer"),
			new SkinInfoMx("The Asphalt World", true),
			new SkinInfoMx("Valentine", true, new DateTime(2000, 2, 7), new DateTime(2000, 2, 14), "Valentine"),
			new SkinInfoMx("VS2010", false, "VS10"),
			new SkinInfoMx("Whiteprint", true),
			new SkinInfoMx("Seven", true, "Windows 7"),
			new SkinInfoMx("Seven Classic", false, DateTime.MaxValue, DateTime.MaxValue, "Windows 7 Classic"), // hidden
			new SkinInfoMx("Classic Windows 3D Style", false, "Windows XP"), // Note: not a skin
			new SkinInfoMx("Xmas 2008 Blue", true, new DateTime(2000, 12, 1), new DateTime(2000, 12, 25), "December"),
						new SkinInfoMx("Office 2016 Colorful"),
						new SkinInfoMx("Office 2016 Dark")


				};

		static bool SkinningEnabled = false;
		static bool BonusSkinsAvailable = true;
		static bool BonusSkinsRegistered = false;

		/// <summary>
		/// Get list of available skins
		/// </summary>
		/// <returns></returns>

		public static List<SkinInfoMx> GetSkins()
		{

			List<SkinInfoMx> skinList = new List<SkinInfoMx>();
			for (int si = 0; si < Skins.Length; si++)
			{
				SkinInfoMx sInf = Skins[si];
				if (sInf.LowDate == DateTime.MaxValue) continue; // hidden

				else if (sInf.Bonus && !BonusSkinsAvailable) continue; // unavailable bonus skin

				else if (sInf.LowDate != DateTime.MinValue) // check date range if defined
				{
					DateTime lowDate = new DateTime(DateTime.Now.Year, sInf.LowDate.Month, sInf.LowDate.Day);
					DateTime highDate = new DateTime(DateTime.Now.Year, sInf.HighDate.Month, sInf.HighDate.Day);
					if (lowDate.CompareTo(DateTime.Now.Date) <= 0 &&
						highDate.CompareTo(DateTime.Now.Date) >= 0) { } // show only if within date range
																														//else if (Security.IsAdministrator(SS.I.UserName)) { }

					else continue; // outside of date range
				}

				skinList.Add(sInf);
			}

			return skinList;
		}

		/// <summary>
		/// GetSkinManagerDefaultSkins
		/// </summary>
		/// <returns></returns>

		public static string GetSkinManagerDefaultSkins()
		{
			//SkinManager sm = new SkinManager();
			//SkinContainerCollection allSkins = sm.Skins;
			SkinContainerCollection skins = SkinManager.Default.Skins;
			int skinCnt = skins.Count;
			string skinList = "";
			foreach (SkinContainer skin in skins)
				skinList += skin.SkinName + "\n";
			return skinList;
		}

		/// <summary>
		/// Set app look and feel
		/// </summary>
		/// <param name="lookAndFeelName"></param>
		/// <param name="userLookAndFeel"></param>

		public static Color SetLookAndFeel(
			string lookAndFeelName,
			UserLookAndFeel userLookAndFeel)
		{
			if (!SkinningEnabled)
			{
				//DevExpress.UserSkins.OfficeSkins.Register(); // (obsolete in V12)
				//BonusSkinsRegister();
				SkinManager.EnableFormSkins(); // paint XtraForms with current skin
				SkinningEnabled = true;
			}

			Color fontColor = SystemColors.ControlText; // default font color

			if (lookAndFeelName == "Classic Windows 3D Style")
			{
				userLookAndFeel.Style = LookAndFeelStyle.Style3D;
				userLookAndFeel.UseWindowsXPTheme = true;
			}

			else if (lookAndFeelName == "Flat") // not used
			{
				userLookAndFeel.Style = LookAndFeelStyle.Flat;
				userLookAndFeel.UseWindowsXPTheme = true;
			}

			else if (lookAndFeelName == "UltraFlat") // not used
			{
				userLookAndFeel.Style = LookAndFeelStyle.UltraFlat;
				userLookAndFeel.UseWindowsXPTheme = true;
			}

			else // use a skin
			{
				SkinInfoMx si = GetSkinFromInternalName(lookAndFeelName);
				if (si == null)
					si = GetSkinFromInternalName("Blue");
				if (si == null) return fontColor; // shouldn't happen

				// Dynamically load the BonusSkinsAssembly if requested and not already loaded

				if (si.Bonus && !BonusSkinsRegistered) // be sure bonus skins are registered
				{
					if (!BonusSkinsRegister())
					{
						lookAndFeelName = "Blue"; // default to blue on error
						si = GetSkinFromInternalName(lookAndFeelName);
					}
				}

				userLookAndFeel.Style = LookAndFeelStyle.Skin;
				userLookAndFeel.SetSkinStyle(lookAndFeelName);

				//UserLookAndFeel.Default.Style = LookAndFeelStyle.Skin; // set for static default object
				//UserLookAndFeel.Default.SetSkinStyle(lookAndFeelName);

				DevExpress.Skins.Skin currentSkin = DevExpress.Skins.CommonSkins.GetSkin(userLookAndFeel);
				Color captionColor = currentSkin.Colors.GetColor(DevExpress.Skins.CommonSkins.SkinGroupPanelCaptionTop);
				fontColor = currentSkin.Colors.GetColor(DevExpress.Skins.CommonColors.ControlText);

				if (Lex.Contains(lookAndFeelName, "Black") || // if a black look set white font color
				 Lex.Contains(lookAndFeelName, "Dark") || Lex.Contains(lookAndFeelName, "Sharp"))
					fontColor = Color.White;
			}

			return fontColor;
		}

		/// <summary>
		/// Register bonus skins
		/// </summary>
		/// <returns></returns>

		public static bool BonusSkinsRegister()
		{
			try
			{ // load bonus skins assembly if needed, i.e. call DevExpress.UserSkins.BonusSkins.Register();

				DevExpress.UserSkins.BonusSkins.Register();

				//string dxSkins = GetSkinManagerDefaultSkins(); // debug

				BonusSkinsRegistered = true;
				return true;
			}
			catch (Exception ex)
			{
				DebugLog.Message("Error registering BonusSkins:\n" + DebugLog.FormatExceptionMessage(ex));
				return false;

				//DebugLog.Message("Error loading skin: " + lookAndFeelName + "\n" + DebugLog.FormatExceptionMessage(ex));
				//lookAndFeelName = "Blue"; // default to blue
				//si = GetSkinFromInternalName(lookAndFeelName);
			}
		}

		// private Color GetElementColor(string elementName)
		//{
		//    DevExpress.Skins.SkinElement element = DevExpress.Skins.GridSkins.GetSkin(gridControl1.LookAndFeel)[elementName];
		//    return element.Color.BackColor;
		//}

		//public void GetColors()
		//{
		//    Color evenRowColor = GetElementColor(DevExpress.Skins.GridSkins.SkinGridEvenRow);
		//    Color oddRowColor = GetElementColor(DevExpress.Skins.GridSkins.SkinGridOddRow);
		//}

		/// <summary>
		/// GetInternalSkinName
		/// </summary>
		/// <param name="extName"></param>
		/// <returns></returns>

		public static string GetInternalSkinName(string extName)
		{
			foreach (SkinInfoMx si in Skins)
			{
				if (Lex.Eq(si.ExternalName, extName))
					return si.InternalName;
			}

			return "Blue";
		}

		/// <summary>
		/// GetExternalSkinName
		/// </summary>
		/// <param name="intName"></param>
		/// <returns></returns>

		public static string GetExternalSkinName(string intName)
		{
			foreach (SkinInfoMx si in Skins)
			{
				if (Lex.Eq(si.InternalName, intName))
					return si.ExternalName;
			}

			return "Blue";
		}

		/// <summary>
		/// GetSkinFromInternalName
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static SkinInfoMx GetSkinFromInternalName(string name)
		{
			foreach (SkinInfoMx si in Skins)
			{
				if (Lex.Eq(si.InternalName, name))
					return si;
			}

			return null;
		}

	}

	/// <summary>
	/// List of skins available via Mobius
	/// </summary>

	public class SkinInfoMx
	{
		public string InternalName; // internal DevExpress name
		public bool Bonus; // true if from BonusSkins
		public DateTime LowDate; // low date limit to show this item in menu
		public DateTime HighDate; // high date limit to show this item in menu
		public string ExternalName; // external name to show to user
		public int ImageIndex; // index of skin image

		static int ImageCount = 0;

		public SkinInfoMx(string name)
		{
			InternalName = ExternalName = name;
			ImageIndex = ImageCount++;
		}

		public SkinInfoMx(string name, bool bonus)
		{
			InternalName = ExternalName = name;
			Bonus = bonus;
			ImageIndex = ImageCount++;
		}

		public SkinInfoMx(string internalName, bool bonus, string externalName)
		{
			InternalName = internalName;
			Bonus = bonus;
			ExternalName = externalName;
			ImageIndex = ImageCount++;
		}

		public SkinInfoMx(string internalName, bool bonus, DateTime lowDate, DateTime highDate, string externalName)
		{
			InternalName = internalName;
			Bonus = bonus;
			LowDate = lowDate;
			HighDate = highDate;
			ExternalName = externalName;
			ImageIndex = ImageCount++;
		}

	}
}
