﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Mobius.ComOps
{

/// <summary>
/// String compression/Decompression
/// </summary>

	public class GZip
	{

		/// <summary>
		/// Compress string into byte array
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>

		public static byte[] Compress(string str)
		{
			var bytes = Encoding.UTF8.GetBytes(str);

			using (var msi = new MemoryStream(bytes))
			using (var mso = new MemoryStream())
			{
				using (var gs = new GZipStream(mso, CompressionMode.Compress))
				{
					msi.CopyTo(gs); // .Net 4.0
					//CopyTo(msi, gs);
				}

				return mso.ToArray();
			}
		}

		/// <summary>
		/// Try to decompress byte array into string
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="s"></param>
		/// <returns></returns>

		public static bool TryDecompress(
			byte[] bytes,
			out string s)
		{
			s = null;
			try
			{
				s = Decompress(bytes);
				return true;
			}

			catch (Exception ex) { return false; }
		}

/// <summary>
/// Decompress byte array into string
/// </summary>
/// <param name="bytes"></param>
/// <returns></returns>

		public static string Decompress(byte[] bytes)
		{
			using (var msi = new MemoryStream(bytes))
			using (var mso = new MemoryStream())
			{
				using (var gs = new GZipStream(msi, CompressionMode.Decompress))
				{
					gs.CopyTo(mso); // .Net 4.0
					//CopyTo(gs, mso);
				}

				return Encoding.UTF8.GetString(mso.ToArray());
			}
		}

        //public static void CopyTo(Stream src, Stream dest)
        //{
        //    byte[] bytes = new byte[4096];

        //    int cnt;

        //    while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
        //    {
        //        dest.Write(bytes, 0, cnt);
        //    }
        //}

		public static void Test()
		{
			byte[] r1 = Compress("StringStringStringStringStringStringStringStringStringStringStringStringStringString");
			string r2 = Decompress(r1);
		}

	}
}