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
using org.openscience.cdk.depict;
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

      if (NativeMol == null || NativeMol.getAtomCount() == 0) return null;

      //NativeMol.setProperty(CDKConstants.TITLE, "caffeine"); // title already set from input!

      sun.awt.Win32FontManager w32Font = new sun.awt.Win32FontManager(); // need this for class to be found

      Environment.SetEnvironmentVariable("CLASSPATH", ".", EnvironmentVariableTarget.Process);
      Environment.SetEnvironmentVariable("JAVA_HOME", @"C:\Mobius_OpenSource\MobiusCommon\CdkMx\bin\Debug", EnvironmentVariableTarget.Process);
      Environment.SetEnvironmentVariable("Path", @"%JAVA_HOME%", EnvironmentVariableTarget.Process);
      /* Need following?
       * CLASSPATH=.; / JAVA_HOME=C:\Program Files\Java\jdk1.6.0_21 / Path=%JAVA_HOME%\bin;
       */

      //DecimalFormatSymbols.getInstance(Local.ENGLISH);

      //Locale.forLanguageTag("en-US")

      DepictionGenerator dptgen = new DepictionGenerator();
      dptgen.withSize(bitmapWidth, bitmapHeight);              // px (raster) or mm (vector)
      //dptgen.withMolTitle();
      //dptgen.withTitleColor(Color.DarkGray); // annotations are red by default
      Depiction d = dptgen.depict(NativeMol);

      /* 

      public const string SVG_FMT = "svg";
      public const string PS_FMT = "ps";
      public const string EPS_FMT = "eps";
      public const string PDF_FMT = "pdf";
      public const string JPG_FMT = "jpg";
      public const string PNG_FMT = "png";
      public const string GIF_FMT = "gif";

      public const string UNITS_MM = "mm";
      public const string UNITS_PX = "px";
      */
      java.util.List formats = d.listFormats();
      java.io.File file = new java.io.File(@"c:\downloads\test.jpg");
      bool b = file.canWrite();
      string s = d.toSvgStr();
      d.writeTo("jpg", file);
      //d.writeTo("svg", @"c:\downloads\test.svg");

      //string svg = d.toSvgStr();
      //bm = SvgUtil.GetBitmapFromSvgXml(svg, bitmapWidth);
      bm = new Bitmap(@"c:\downloads\test.jpg");
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
