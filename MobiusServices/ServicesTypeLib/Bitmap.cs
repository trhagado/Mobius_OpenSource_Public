using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;
using System.IO;
using System.Drawing.Imaging;


namespace Mobius.Services.Types
{
	[DataContract(Namespace = "http://server/MobiusServices/v1.0")]
	[Serializable]
	public class Bitmap
	{
		//create our own class to serialize this...
		//for the sake of compression and standards-compliance, use PNG as the internal format...
		//Makes the class name a misnomer, but the name can be changed...  Later...
		[DataMember]
		public byte[] Contents;

		public Bitmap()
		{
		}

		public Bitmap(System.Drawing.Bitmap bitmap)
		{
			if (bitmap != null)
			{
				MemoryStream memoryStream = new MemoryStream();
				System.Drawing.Bitmap bm2 = new System.Drawing.Bitmap(bitmap); // workaround for 'A generic error occurred in GDI+' exception
				bm2.Save(memoryStream, ImageFormat.Png);

				Contents = new byte[memoryStream.Length];

				memoryStream.Position = 0;
				int offset = 0;
				int totalBytesRead = 0;
				do
				{
					int readLen = memoryStream.Read(Contents, offset, Contents.Length - totalBytesRead);
					offset += readLen;
					totalBytesRead += readLen;
				} while (totalBytesRead < Contents.Length);
			}
		}

		public static explicit operator System.Drawing.Bitmap(Bitmap serviceBitmap)
		{
			System.Drawing.Bitmap bitmap = null;
			if (serviceBitmap != null && serviceBitmap.Contents != null)
			{
				MemoryStream memoryStream = new MemoryStream(serviceBitmap.Contents);
				bitmap = System.Drawing.Bitmap.FromStream(memoryStream) as System.Drawing.Bitmap;
			}
			return bitmap;
		}
	}
}
