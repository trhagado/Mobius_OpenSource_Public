using Mobius.ComOps;
using Mobius.Data; 

using System;
using System.Collections.Generic;
using System.IO;

using System.Net;
using System.Drawing;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mobius.QueryEngineLibrary
{

	/// <summary>
	/// AssayCurveService - Get a concentration response curve for a multipoint assay result
	/// </summary>

	public class AssayCurveService
	{
		Exception GetImageException = null;
		Bitmap GetImageBitmap = null;

		public static string ServiceUrl = @"<url>";
		public static bool UseSeparateThread = true;

/// <summary>
/// Call service to get CRC image 
/// </summary>
/// <param name="parms"></param>
/// <returns></returns>

		public Bitmap GetImage(AssayGetImageParms parms)
		{
			GetImageException = null;
			GetImageBitmap = null;

			if (UseSeparateThread)
			{
				Thread thread = new Thread(GetImageMethod); // run on separate thread to prevent the "big red X" issue
				thread.Name = "GetImage";
				thread.Start(parms);
				thread.Join(); // wait for thread to exit while continuing to pump messages
			}

			else GetImageMethod(parms);

			if (GetImageException == null) return GetImageBitmap;
			else throw new Exception(GetImageException.Message, GetImageException);
		}

		/// <summary>
		/// GetImageMethod
		/// </summary>
		/// <param name="parmObj"></param>

		void GetImageMethod(object args)
		{
			int ci;

			AssayGetImageParms parms = args as AssayGetImageParms;
			if (parms == null) throw new Exception("Null parameters");

			throw new NotImplementedException(); // todo
		}

		private static Color NextColor(ref int ci)
		{
			Color c = Palette[ci];
			ci++;
			if (ci >= Palette.Length)
				ci = 0;

			return c;
		}

		private static Color[] Palette = new Color[] { 
			Color.FromArgb(0, 150 ,0), // green
 		  Color.FromArgb(0, 0, 255), // blue
			Color.FromArgb(247, 150, 70), // orange
			Color.FromArgb(75, 172, 198), // cyan
			Color.FromArgb(255, 0, 0), // red
			Color.FromArgb(0, 0, 0) // black

			//Color.FromArgb(192, 80, 77), // red 
			//Color.FromArgb(155, 187, 89), // green
			//Color.FromArgb(79, 129, 189), // blue
			//Color.FromArgb(189, 73, 50),  // brown 
		};

		private static int InitialColor = 0;

	}

	/// <summary>
	/// Parameters for GetImage method
	/// </summary>

	public class AssayGetImageParms
	{
		public string SourceName = "";
		public string ResultIdsString = "";
		public int Width = -1;
		public int Height = -1;
		public string XAxisLabel = null;
		public string YAxisLabel = null;
		public bool ShowAssayNameInLegend = false;
		public bool ShowBookpageInLegend = false;
		public bool ShowBookpageInXAxis = false;
		public bool ShowCorpIdInLegend = false;
		public bool ShowRunDateInLegend = false;
	}
}
