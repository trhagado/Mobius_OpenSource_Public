using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Mobius.ComOps
{

	/// <summary>
/// Bitmap utilities 
/// </summary>

	public class BitmapUtil
	{

/// <summary>
/// Remove the white space surrounding a bitmap & return the cropped image 
/// </summary>
/// <param name="bmp"></param>
/// <returns></returns>

		public static Bitmap RemoveWhiteSpace(Bitmap bmp)
		{
			int leftOffset, rightOffset, topOffset, bottomOffset;

			try
			{
				Point start, end;
				int padding = 2;

				GetBitmapLimits(bmp, out start, out end);

				leftOffset = start.X - padding;
				if (leftOffset < 0) leftOffset = 0;

				rightOffset = bmp.Width - (end.X + 1) - padding;
				if (rightOffset < 0) rightOffset = 0;

				topOffset = start.Y - padding;
				if (topOffset < 0) topOffset = 0;

				bottomOffset = bmp.Height - (end.Y + 1) - padding;
				if (bottomOffset < 0) bottomOffset = 0;

				Bitmap newBmp = new Bitmap(bmp.Width - leftOffset - rightOffset, bmp.Height - topOffset - bottomOffset);

				// Get a graphics object for the new bitmap, and draw the original bitmap onto it, offsetting it do remove the whitespace
				Graphics g = Graphics.FromImage(newBmp);
				g.DrawImage(bmp, 0 - leftOffset, 0 - topOffset);

				//bmp.Save(@"c:\\download\before.png", ImageFormat.Png);
				//newBmp.Save(@"c:\\download\after.png", ImageFormat.Png);

				return newBmp;
			}
			catch (Exception ex)
			{ return bmp; }
		}

/// <summary>
/// Get the limits of the bitmap
/// </summary>
/// <param name="bmp"></param>
/// <param name="start"></param>
/// <param name="end"></param>

		public static void GetBitmapLimits(Bitmap bmp, out Point start, out Point end)
		{
			start = Point.Empty;
			end = Point.Empty;
			Color color = Color.White;

			int bitmapWidth = bmp.Width;
			int bitmapHeight = bmp.Height;
			
			BitmapData data = bmp.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			try
			{
				unsafe // unsafe code, must be compiled with unsafe switch
				{
					byte* pData0 = (byte*)data.Scan0;
					for (int y = 0; y < bitmapHeight; y++)
					{
						for (int x = 0; x < bitmapWidth; x++)
						{
							byte* pData = pData0 + (y * data.Stride) + (x * 4);

							byte xyBlue = pData[0];
							byte xyGreen = pData[1];
							byte xyRed = pData[2];
							byte xyAlpha = pData[3];

							if (color.A != xyAlpha
											|| color.B != xyBlue
											|| color.R != xyRed
											|| color.G != xyGreen)
							{
								//ignore transparent pixels
								if (xyAlpha == 0)
									continue;
								if (start.IsEmpty)
								{
									start = new Point(x, y);
								}
								else if (start.X > x)
								{
									start.X = x;
								}
								else if (start.Y > y)
								{
									start.Y = y;
								}

								if (end.IsEmpty)
								{
									end = new Point(x, y);
								}
								else if (end.X < x)
								{
									end.X = x;
								}
								else if (end.Y < y)
								{
									end.Y = y;
								}
							}
						}
					}
				}
			}
			finally
			{
				bmp.UnlockBits(data);
			}

			return;
		}

/// <summary>
/// Create a bitmap from text string
/// </summary>
/// <param name="sImageText"></param>
/// <returns></returns>

		public static Bitmap CreateBitmapFromTextString(string sImageText)
   {
       Bitmap objBmpImage = new Bitmap(1, 1);
    
       int intWidth = 0;
       int intHeight = 0;
    
       // Create the Font object for the image text drawing.
       Font objFont = new Font("Arial", 20, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
    
       // Create a graphics object to measure the text's width and height.
       Graphics objGraphics = Graphics.FromImage(objBmpImage);
       
       // This is where the bitmap size is determined.
       intWidth = (int)objGraphics.MeasureString(sImageText, objFont).Width;
       intHeight = (int)objGraphics.MeasureString(sImageText, objFont).Height;
    
       // Create the bmpImage again with the correct size for the text and font.
       objBmpImage = new Bitmap(objBmpImage, new Size(intWidth, intHeight));
    
       // Add the colors to the new bitmap.
       objGraphics = Graphics.FromImage(objBmpImage);
    
       // Set Background color
       objGraphics.Clear(Color.White);
       objGraphics.SmoothingMode = SmoothingMode.AntiAlias;
       objGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
       objGraphics.DrawString(sImageText, objFont, new SolidBrush(Color.FromArgb(102, 102, 102)), 0, 0);
       objGraphics.Flush();
    
       return (objBmpImage);
   }

		/// <summary>
		/// Scale bitmap to desired width
		/// </summary>
		/// <param name="bmp"></param>
		/// <param name="desiredWidth"></param>
		/// <returns></returns>

		public static Bitmap ScaleBitmap(
			Bitmap bmp,
			int desiredWidth)
		{
			if (desiredWidth == bmp.Width) return bmp; // return as is if correct width

			if (desiredWidth > bmp.Width) desiredWidth = bmp.Width;
			float scale = (float)desiredWidth / bmp.Width;
			bmp = new Bitmap(bmp, (int)(bmp.Width * scale), (int)(bmp.Height * scale));

			return bmp;
		}

		/// <summary>
		/// Update a Bitmap reference, disposing of the old bitmap if defined to save memory
		/// </summary>
		/// <param name="bitmapRef"></param>
		/// <param name="newBitmap"></param>

		public static void UpdateBitmap(
			ref Bitmap bitmapRef,
			Bitmap newBitmap)
		{
			Bitmap oldBitmap = bitmapRef;
			bitmapRef = newBitmap;
			//Release resources from old bitmap
			if (oldBitmap != null)
				((IDisposable)oldBitmap).Dispose();

			return;
		}

		/// <summary>
		/// Dispose of an image if is defined to save memory
		/// </summary>
		/// <param name="image"></param>

		public static void DisposeOfBitmap(
			Image image)
		{
			if (image != null)
				((IDisposable)image).Dispose();

			return;
		}

	}
}
