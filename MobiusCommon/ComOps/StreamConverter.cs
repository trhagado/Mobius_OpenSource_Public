using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Mobius.ComOps
{
	public class StreamConverter
	{
		/// <summary>
		/// String to MemoryStream
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>

		public MemoryStream StringToMemoryStream(string s)
		{
			byte[] byteArray = Encoding.ASCII.GetBytes(s);
			MemoryStream stream = new MemoryStream(byteArray);
			return stream;
		}

		/// <summary>
		/// Stream to string
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>

		public string StreamToString(Stream stream)
		{
			StreamReader reader = new StreamReader(stream);
			string text = reader.ReadToEnd();
			return text;
		}
	}
}
