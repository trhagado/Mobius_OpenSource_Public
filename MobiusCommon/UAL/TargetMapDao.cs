using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Net;
using System.IO;

namespace Mobius.UAL
{
	public class TargetMapDao
	{
		public static Dictionary<string, string> TargetMapNamesAndLabels; // key = name, value = label
		public static Dictionary<string, TargetMap> TargetMapDict; // indexed by name
		public static Dictionary<string, TargetMap> TargetMapDictByLabel; // indexed by label
		static string targetMapDir; // directory for target map files

/// <summary>
/// Basic constructor
/// </summary>

		public TargetMapDao ()
		{
			if (TargetMapDict == null) ReadTargetMapDict();
			return;
		}

		/// <summary>
		/// Get directory for target map files
		/// </summary>

		public static string TargetMapDir
		{
			get
			{
				if (String.IsNullOrEmpty(targetMapDir))
				{
					targetMapDir = ServicesIniFile.Read("TargetMapDirectory");
					if (String.IsNullOrEmpty(targetMapDir))
					{
						targetMapDir = ServicesIniFile.Read("MetaData");
						if (String.IsNullOrEmpty(targetMapDir))
							throw new Exception("TargetMapDirectory & MetaData not found in .ini file");
						if (!targetMapDir.EndsWith(@"\")) targetMapDir += @"\";
						targetMapDir += "TargetMaps";
					}

					if (targetMapDir.EndsWith(@"\")) 
						targetMapDir = targetMapDir.Substring(0,targetMapDir.Length - 1);
				}
				return targetMapDir;
			}
		}

		/// <summary>
		/// Get a target map image by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>

		public static byte[] GetMapImage(
			string name)
		{
			TargetMap tm = GetMap(name);
			if (tm == null) return null;

			string imageFile = TargetMapDir + @"\" + tm.ImageFile;
			if (!File.Exists(imageFile)) return null;

			FileStream fs = new FileStream(imageFile, FileMode.Open);
			byte [] image = new byte[fs.Length];
			fs.Read(image, 0, (int)fs.Length);
			fs.Close();
			return image;
		}

/// <summary>
/// Get a target map by name
/// </summary>
/// <param name="name"></param>
/// <returns></returns>

		public static TargetMap GetMap(
			string name)
		{
			if (TargetMapDict == null)
				ReadTargetMapDict();

			string key = name.ToUpper();
			if (!TargetMapDict.ContainsKey(key))
				throw new Exception("Target map doesn't exist: " + name);

			return TargetMapDict[key];
		}

/// <summary>
/// Get label for map given name
/// </summary>
/// <param name="name"></param>
/// <returns></returns>

		public static string GetMapLabel(
			string name)
		{
			try
			{
				TargetMap tm = GetMap(name);
				if (tm.Label != "") return tm.Label;
				else return name;
			}

			catch (Exception ex)
			{
				return name;
			}
		}

/// <summary>
/// Get a target map by label
/// </summary>
/// <param name="label"></param>
/// <returns></returns>

		public static TargetMap GetMapByLabel(
			string label)
		{
			if (TargetMapDict == null)
				ReadTargetMapDict();

			string key = label.ToUpper();

			if (TargetMapDict.ContainsKey(key))
				return TargetMapDict[key];

			else if (!TargetMapDictByLabel.ContainsKey(key))
				return TargetMapDictByLabel[key];

			else throw new Exception("Target map label doesn't exist: " + label);
		}

/// <summary>
/// Get a map by name with coordinates
/// </summary>
/// <param name="name"></param>
/// <returns></returns>

		public static TargetMap GetMapWithCoords(
			string name)
		{
			if (TargetMapDict == null)
				ReadTargetMapDict();

			string key = name.ToUpper();
			if (!TargetMapDict.ContainsKey(key))
				throw new Exception("Target map doesn't exist: " + name);

			TargetMap tm = TargetMapDict[key];
			if (tm.Coords != null) return tm; // have coords?

			tm.Coords = new Dictionary<int, List<TargetMapCoords>>();
			StreamReader sr = new StreamReader(TargetMapDir + @"\" + tm.CoordsFile);
			while (true)
			{
				string rec = sr.ReadLine();
				try
				{
					if (rec == null) break;
					if (rec.Trim().StartsWith(";")) continue;
					if (rec.Trim() == "") continue;
					string[] sa = rec.Split('\t');
					int targetId = int.Parse(sa[0]);
					if (!tm.Coords.ContainsKey(targetId))
						tm.Coords[targetId] = new List<TargetMapCoords>();
					List<TargetMapCoords> tml = tm.Coords[targetId];
					TargetMapCoords tmc = new TargetMapCoords();
					tml.Add(tmc);
					tmc.TargetId = targetId;
					tmc.X = int.Parse(sa[1]);
					tmc.Y = int.Parse(sa[2]);
					if (sa.Length > 3 && !String.IsNullOrEmpty(sa[3])) tmc.X2 = int.Parse(sa[3]);
					if (sa.Length > 4 && !String.IsNullOrEmpty(sa[4])) tmc.Y2 = int.Parse(sa[4]);
				}
				catch (Exception ex) { throw new Exception(ex.Message, ex); }
			}
			sr.Close();
			return tm;
		}

/// <summary>
/// Get list of common map names
/// </summary>
/// <returns></returns>

		public static List<string> GetCommonMapNames()
		{
			List<string> mapNames = new List<string>();
			string path = TargetMapDir + @"\CommonMaps.txt";
			if (File.Exists(path)) // get any common maps
			{
				StreamReader sr = new StreamReader(path);
				while (true)
				{
					string mapName = sr.ReadLine();
					if (mapName == null) break;
					mapName = mapName.Trim();
					if (mapName == "" || mapName.StartsWith(";")) continue;
					mapNames.Add(mapName.ToUpper());
				}
				sr.Close();
			}

			return mapNames;
		}

/// <summary>
/// Return the dictionary of target map names and labels
/// </summary>
/// <returns></returns>

		public static Dictionary<string, string> GetTargetNamesAndLabels()
		{
			if (TargetMapNamesAndLabels == null)
				ReadTargetMapDict();

			return TargetMapNamesAndLabels;
		}

		/// <summary>
		/// Read in dictionary of target maps
		/// </summary>

		static void ReadTargetMapDict()
		{
			TargetMapNamesAndLabels = new Dictionary<string, string>();
			TargetMapDict = new Dictionary<string, TargetMap>();
			TargetMapDictByLabel = new Dictionary<string,TargetMap>();

			string[] fNames = Directory.GetFiles(TargetMapDir, "*.map");
			foreach (string fName in fNames)
			{
				StreamReader sr = new StreamReader(fName);
				while (true)
				{
					try
					{
						string rec = sr.ReadLine();
						if (rec == null) break;
						if (rec.Trim().StartsWith(";")) continue;
						if (rec.Trim() == "") continue;

						TargetMap tm = new TargetMap();

						tm.Bounds = new Rectangle();

						string[] sa = rec.Split('\t'); // name, label, source, type, imageType, imagePath, imageWidth, imageHeight, markBounds, coordsPath
						if (sa.Length > 0) tm.Name = sa[0];
						if (sa.Length > 1) tm.Label = sa[1];
						if (sa.Length > 2) tm.Source = sa[2];
						if (sa.Length > 3) tm.Type = sa[3];
						if (sa.Length > 4) tm.ImageType = sa[4];
						if (sa.Length > 5) tm.ImageFile = sa[5];
						if (sa.Length > 6) tm.Bounds.X = int.Parse(sa[6]);
						if (sa.Length > 7) tm.Bounds.Y = int.Parse(sa[7]);
						if (sa.Length > 8) tm.Bounds.Width = int.Parse(sa[8]) - tm.Bounds.X;
						if (sa.Length > 9) tm.Bounds.Height = int.Parse(sa[9]) - tm.Bounds.Y;
						if (sa.Length > 10 && sa[10] != "") tm.MarkBounds = bool.Parse(sa[10]);
						if (sa.Length > 11) tm.CoordsFile = sa[11];

						TargetMapNamesAndLabels[tm.Name.ToUpper()] = tm.Label;
						TargetMapDict[tm.Name.ToUpper()] = tm;
						TargetMapDictByLabel[tm.Label.ToUpper()] = tm;

						sr.Close();
						break;
					}

					catch (Exception ex)
					{
							throw new Exception("Error reading " + fName + "\r\n" + DebugLog.FormatExceptionMessage(ex), ex);
					}
				}
			} // end of loop on file names

			return; 
		}

/// <summary>
/// Insert new map
/// </summary>
/// <param name="map"></param>
 
		public static void InsertMap (
			TargetMap map)
		{
			throw new Exception("Not implemented");
		}

/// <summary>
/// Delete map
/// </summary>
/// <param name="map"></param>

		public static void DeleteMap(
			string name)
		{
			throw new Exception("Not implemented");
		}

/// <summary>
/// Get image & coordinates file from KEGG FTP
/// </summary>
/// <param name="pathwayId"></param>
/// <returns></returns>

		public static TargetMap GetKeggPathway (
			string pathwayId)
		{
			String ftpPath, rec;

			if (TargetMapDict == null)
				ReadTargetMapDict();

			TargetMap tm = new TargetMap();
			tm.Name = pathwayId;
			tm.Label = pathwayId; // default label
			tm.Source = "KEGG";
			tm.Type = "Pathway";

// Build basic paths

			string ftpFolder = ServicesIniFile.Read("KeggPathWayFtpDirectory"); // get from here
			if (String.IsNullOrEmpty(ftpFolder)) throw new Exception("KeggPathWayFtpDirectory not defined");
			if (!ftpFolder.EndsWith(@"/")) ftpFolder += @"/";

// Get map_title file and read title for pathway

			string filePath = TargetMapDir + @"\map_title.tab";
			string pathwayPrefix = pathwayId.Substring(0, 3);
			string pathwayNbr = pathwayId.Substring(3); // 
			bool download = false;
			if (!File.Exists(filePath)) download = true;

			while (true)
			{
				if (download)
				{
					ftpPath = ftpFolder + "map_title.tab";
					Download(ftpPath, filePath);
				}

				StreamReader sr = new StreamReader(filePath);
				while (true)
				{
					rec = sr.ReadLine();
					if (rec == null) break;
					if (!rec.StartsWith(pathwayNbr)) continue;
					string[] sa = rec.Split('\t');
					tm.Label = sa[1].Trim() + " (KEGG: " + pathwayId + ")";
					break;
				}
				sr.Close();

				if (!String.IsNullOrEmpty(tm.Label)) break; // have it

				else if (download) // already downloaded but not found
				{
					tm.Label = pathwayId;
					break;
				}

				else
				{ // download again if not done yet
					download = true;
					continue;
				}
			}

// Get gif file

			if (Lex.Ne(pathwayPrefix, "map")) // if not a base level map then in organisms
				ftpFolder += @"organisms/"; 
			ftpFolder += pathwayPrefix + @"/";

			ftpPath = ftpFolder + pathwayId + ".gif";
			filePath = TargetMapDir + @"\" + pathwayId + ".gif";
			Download(ftpPath, filePath);

			Image img = Image.FromFile(filePath);
			tm.Bounds = new Rectangle(0, 0, img.Width, img.Height);
			tm.MarkBounds = true;

			filePath = TargetMapDir + @"\" + pathwayId + ".png"; // save as .png file that Spotfire can read
			img.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
			tm.ImageType = "png";
			tm.ImageFile = Path.GetFileName(filePath); // store just file name

// Get coordinates file

			if (Lex.Eq(pathwayPrefix, "map"))
			{
				ftpPath = ftpFolder + pathwayId + "_ec.coord";
				filePath = TargetMapDir + @"\" + pathwayId + "_ec.coord";
			}
			else
			{
				ftpPath = ftpFolder + pathwayId + "_gene.coord";
				filePath = TargetMapDir + @"\" + pathwayId + "_gene.coord";
			}
			Download(ftpPath, filePath);
			tm.CoordsFile = Path.GetFileName(filePath);

			TargetMapDict[tm.Name.ToUpper()] = tm; // add to list of maps
			TargetMapDictByLabel[tm.Label.ToUpper()] = tm;

			GetMapWithCoords(pathwayId); // read in coords to validate file format

// Write out the map file containing basic map data

			filePath = TargetMapDir + @"\" + pathwayId + ".map";
			StreamWriter sw = new StreamWriter(filePath);
			rec =
				tm.Name + "\t" +
				tm.Label + "\t" +
				tm.Source + "\t" +
				tm.Type + "\t" +
				tm.ImageType + "\t" +
				tm.ImageFile + "\t" +
				tm.Bounds.Left + "\t" +
				tm.Bounds.Top + "\t" +
				tm.Bounds.Right + "\t" +
				tm.Bounds.Bottom + "\t" +
				tm.MarkBounds + "\t" +
				tm.CoordsFile;

			sw.WriteLine(rec);
			sw.Close();

			return tm;
		}

/// <summary>
/// Do basic FTP download
/// </summary>
/// <param name="ftpPath"></param>
/// <param name="filePath"></param>

		private static void Download (
			string ftpPath,
			string filePath)
		{
			int t0 = TimeOfDay.Milliseconds();
			WebClient req = new WebClient();
			req.Credentials = new NetworkCredential("anonymous", "anonymous@server.com");
			byte[] ba = req.DownloadData(ftpPath);

			FileStream fs = new FileStream(filePath, FileMode.Create);
			fs.Write(ba, 0, ba.Length);
			fs.Close();

			//string fileString = System.Text.Encoding.UTF8.GetString(ba);
			//StreamWriter sw = new StreamWriter(filePath);
			//sw.Write(fileString);
			//sw.Close();

			t0 = TimeOfDay.Milliseconds() - t0;
			return;
		}

	} // end of TargetMapDao


}
