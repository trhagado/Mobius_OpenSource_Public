using Mobius.ComOps;
using Mobius.Data;

using java.io;

using cdk = org.openscience.cdk;
using org.openscience.cdk;
using org.openscience.cdk.inchi;
using org.openscience.cdk.interfaces;
using org.openscience.cdk.fingerprint;
using org.openscience.cdk.smiles;
using org.openscience.cdk.tools.manipulator;
using org.openscience.cdk.graph;
using org.openscience.cdk.qsar.result;
using org.openscience.cdk.io;
using org.openscience.cdk.io.iterator;
using org.openscience.cdk.tools;
using org.openscience.cdk.layout;
using org.openscience.cdk.config;
using org.openscience.cdk.config.isotopes;

using net.sf.jniinchi; // low level IUPAC interface, needed for access to some enumerations

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mobius.CdkMx
{
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
      //StructureConverter sc;
      float top, bottom, left, right, height, width, strBottom, hCenter, drop, stdBndLen, scale, fontSize, bondThickness;
      int txtLen;
      Bitmap bm;

      string molfile = this.MolfileString;

      throw new NotImplementedException();
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
