using Mobius.ComOps;
using Mobius.Data;

using NCDK;
using NCDK.Aromaticities;
using NCDK.Depict;
using NCDK.Isomorphisms;
using NCDK.Isomorphisms.Matchers;
using NCDK.Silent;
using NCDK.Smiles;
using NCDK.Tools.Manipulator;

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mobius.CdkMx
{

/// <summary>
/// Depiction/Rendering
/// </summary>

  public partial class CdkMol : ICdkMol
  {
    /// <summary>
    /// Get structure bitmap with optional caption
    /// </summary>
    /// <param name="pixWidth"></param>
    /// <param name="pixHeight"></param>
    /// <param name="desiredBondLength">Desired length in milliinches</param>
    /// <param name="cellStyle"></param>
    /// <param name="caption"></param>
    /// <returns></returns>

    public Bitmap GetFixedHeightMoleculeBitmap(
      int pixWidth,
      int pixHeight,
      DisplayPreferences dp,
      CellStyleMx cellStyle,
      string caption)
    {
      const int minWidth = 100, minHeight = 100;

      if (pixWidth < minWidth) pixWidth = minWidth;
      if (pixHeight < minHeight) pixHeight = minHeight;

      Bitmap bm = null;

      if (cellStyle == null)
      {
        Font font = new Font("Tahoma", 8.25f);
        cellStyle = new CellStyleMx(font, Color.Black, Color.Empty);
      }

      if (dp == null)
      {
        dp = new DisplayPreferences();
        //SetStandardDisplayPreferences(dp);
      }

      //if (Lex.Contains(qc.Criteria, "SSS")) // no H display if SS query
      //  dp.HydrogenDisplayMode = HydrogenDisplayMode.Off;
      if (String.IsNullOrEmpty(caption))
        bm = GetMoleculeBitmap(pixWidth, pixHeight, dp);

      else
      {
        bm = GetMoleculeBitmap(pixWidth, pixHeight + 20, dp); // get image with a bit of extra height for text

        Rectangle captionRect = new Rectangle(2, 0, pixWidth, pixHeight); // show the text part
        Graphics g = System.Drawing.Graphics.FromImage(bm);
        Brush brush = Brushes.Black;
        if (cellStyle.ForeColor == Color.Blue) brush = Brushes.Blue;
        g.DrawString(caption, cellStyle.Font, brush, captionRect);
      }

      return bm;
    }

    /// <summary>
    /// Render molecule into bitmap of specified size.
    /// </summary>
    /// <param name="bitmapWidth"></param>
    /// <param name="bitmapHeight"></param>
    /// <param name="dp"></param>
    /// <returns></returns>

    public Bitmap GetMoleculeBitmap(
      int bitmapWidth,
      int bitmapHeight,
      DisplayPreferences dp = null)
    {
      byte[] ba;
      FileStream fs;
      float top, bottom, left, right, height, width, strBottom, hCenter, drop, stdBndLen, scale, fontSize, bondThickness;
      int txtLen;
      Bitmap bm;

      UpdateNativeMolecule();

      if (NativeMol == null || NativeMol.Atoms.Count == 0) return null;

      //NativeMol.setProperty(CDKConstants.TITLE, "caffeine"); // title already set from input!

      DepictionGenerator dptgen = new DepictionGenerator();
      dptgen.Size = new System.Windows.Size(bitmapWidth, bitmapHeight);              // px (raster) or mm (vector)
      //dptgen.withMolTitle();
      //dptgen.withTitleColor(Color.DarkGray); // annotations are red by default
      Depiction d = dptgen.Depict(NativeMol);

      //string svg = d.ToSvgString();
      //bm = SvgUtil.GetBitmapFromSvgXml(svg, bitmapWidth);

      //System.Windows.Media.Imaging.RenderTargetBitmap rtBm = d.ToBitmap();

      string path = TempFile.GetTempFileName(ClientDirs.TempDir, "jpg", true); 
      d.WriteTo("jpg", path);
      bm = new Bitmap(path);

      return bm;
    }

    /// <summary>
    /// Convert to Metafile 
    /// </summary>
    /// <returns></returns>
    public Metafile GetMetafile(
      int width,
      int height)
    {
      throw new NotImplementedException();
    }
  }
}
