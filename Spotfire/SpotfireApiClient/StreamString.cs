using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace Mobius.SpotfireClient
{
	/// <summary>
	/// Defines the data protocol for reading and writing strings on our stream 
	/// </summary>

	public class StreamString
	{
		private Stream pipeStream;
		private UnicodeEncoding StreamEncoding;

		public StreamString(Stream ioStream)
		{
			pipeStream = ioStream;
			StreamEncoding = new UnicodeEncoding();
		}

		public string ReadString()
		{
			int len = 0;

			len = pipeStream.ReadByte();
			if (len == -1) throw new Exception("End of stream");
			len = len * 256;
			len += pipeStream.ReadByte();
			byte[] inBuffer = new byte[len];
			pipeStream.Read(inBuffer, 0, len);

			return StreamEncoding.GetString(inBuffer);
		}

		public int WriteString(string outString)
		{
			byte[] outBuffer = StreamEncoding.GetBytes(outString);
			int len = outBuffer.Length;
			if (len > UInt16.MaxValue)
			{
				len = (int)UInt16.MaxValue;
			}
			pipeStream.WriteByte((byte)(len / 256));
			pipeStream.WriteByte((byte)(len & 255));
			pipeStream.Write(outBuffer, 0, len);
			pipeStream.Flush();

			return outBuffer.Length + 2;
		}
	}

	// Contains the method executed in the context of the impersonated user 
	public class ReadFileToStream
	{
		private string fn;
		private StreamString ss;

		public ReadFileToStream(StreamString str, string filename)
		{
			fn = filename;
			ss = str;
		}

		public void Start()
		{
			string contents = File.ReadAllText(fn);
			ss.WriteString(contents);
		}
	}

}
