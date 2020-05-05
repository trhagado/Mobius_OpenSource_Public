
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

  public partial class CdkMol : IMolLib
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

    /// <summary>
    /// Check if substructure query matches target molecule
    /// </summary>
    /// <param name="queryMol"></param>
    /// <param name="targetMol"></param>
    /// <returns></returns>

    public bool IsSSSMatch(
      IMolLib queryMol,
      IMolLib targetMol)
    {
      SetSSSQueryMolecule(queryMol as CdkMol);
      return IsSSSMatch(targetMol as CdkMol);
    }

    /// <summary>
    /// Prepare for SSS matching of supplied query molecule
    /// </summary>
    /// <param name="queryMol"></param>

    public void SetSSSQueryMolecule(
    IMolLib queryMol)
    {
      try
      {
        throw new NotImplementedException();
      }
      catch (Exception ex)
      {
        throw new Exception(ex.Message, ex);
      }
    }

    /// <summary>
    /// Map current query against supplied target molecule
    /// </summary>
    /// <param name="targetMol"></param>
    /// <returns></returns>

    public bool IsSSSMatch(
      IMolLib targetMol)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Get mapping of current query against supplied target
    /// </summary>
    /// <param name="targetMol"></param>
    /// <param name="queryIndex"></param>
    /// <param name="mappedAtoms"></param>
    /// <param name="mappedBonds"></param>
    /// <returns></returns>

    public bool GetSSSMapping(
      IMolLib targetMol,
      out int queryIndex,
      out int[] mappedAtoms,
      out int[] mappedBonds)
    {
      queryIndex = -1;
      mappedAtoms = mappedBonds = null;

      try
      {
        throw new NotImplementedException();
      }
      catch (Exception ex)
      {
        return false; // just say false if exception encountered
      }
    }

    /// <summary>
    /// GetNextSGMapping
    /// </summary>
    /// <param name="queryIndex"></param>
    /// <param name="mappedAtoms"></param>
    /// <param name="mappedBonds"></param>
    /// <returns></returns>

    public bool GetNextSSSMapping(
      out int queryIndex,
      out int[] mappedAtoms,
      out int[] mappedBonds)
    {
      queryIndex = -1;
      mappedAtoms = mappedBonds = null;

      try
      {
        throw new NotImplementedException();
      }
      catch (Exception ex)
      {
        return false; // just say false if exception encountered
      }
    }

    /// <summary>
    /// Map and hilight a substructure match
    /// </summary>
    /// <param name="molfile"></param>
    /// <returns></returns>

    public string HilightSSSMatch(string molfile)
    {
      // This seems to be slow for some reason when running in the VisualStudio debugger

      int queryIndex;
      int[] mappedAtoms, mappedBonds;

      try
      {

        CdkMol m = new CdkMol(MoleculeFormat.Molfile, molfile);
        if (GetSSSMapping(m, out queryIndex, out mappedAtoms, out mappedBonds))
        {
          throw new NotImplementedException();
        }

        else return molfile;
      }

      catch (Exception ex)
      {
        return molfile; // just return the input if exception encountered
      }
    }

    /// <summary>
    /// Hilight a target molecule using mapped atoms and bonds
    /// </summary>
    /// <param name="targetMol"></param>
    /// <param name="mappedAtoms"></param>
    /// <param name="mappedBonds"></param>
    /// <returns></returns>

    public IMolLib HilightSSSMatchGMap(
      IMolLib targetMol,
      int[] mappedAtoms,
      int[] mappedBonds)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Map current query against structure & return oriented match
    /// </summary>
    /// <param name="target molfile"></param>
    /// <returns></returns>

    public string OrientToMatchingSubstructure(
      string targetMolfile)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Set substructure search option
    /// </summary>
    /// <param name="option"></param>

    public void SetSSSOption(
      //SGMap.SearchOption option, 
      bool value)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Perform a full structure search
    /// </summary>
    /// <param name="query"></param>
    /// <param name="target"></param>
    /// <param name="switches"></param>
    /// <returns></returns>

    public bool FullStructureMatch(
      IMolLib query,
      IMolLib target,
      string FullStructureSearchType = null)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Prepare for FSS matching of supplied query molecule
    /// </summary>
    /// <param name="queryMol"></param>

    public void SetFSSQueryMolecule(
      IMolLib queryMol,
      string FullStructureSearchType = null)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Map current query against supplied target molecule
    /// </summary>
    /// <param name="targetMol"></param>
    /// <returns></returns>

    public bool IsFSSMatch(
      IMolLib targetMol)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// GetMolFormulaDotDisconnect
    /// </summary>
    /// <param name="molfile"></param>
    /// <returns></returns>

    public string GetMolFormulaDotDisconnect(
      string molfile)
    {
      IMolLib mol = new CdkMol(MoleculeFormat.Molfile, molfile);
      string mf = GetMolFormulaDotDisconnect(mol as CdkMol);
      return mf;
    }

    /// <summary>
    /// GetMolFormulaDotDisconnect
    /// </summary>
    /// <param name="mol"></param>
    /// <returns></returns>

    public String GetMolFormulaDotDisconnect(
      IMolLib mol)
    {
      string mf = GetMolFormula(mol as CdkMol, includeSpaces: false, separateFragments: true);
      return mf;
    }

    /// <summary>
    /// GetFolFormula
    /// </summary>
    /// <param name="mol"></param>
    /// <param name="includeSpaces"></param>
    /// <param name="separateFragments"></param>
    /// <param name="use2Hand3HforHydrogenIsotopes"></param>
    /// <param name="ignoreIsotopes"></param>
    /// <returns></returns>

    public String GetMolFormula(
      IMolLib mol,
      bool includeSpaces = false,
      bool separateFragments = false,
      bool use2Hand3HforHydrogenIsotopes = false,
      bool ignoreIsotopes = false)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Get mol weight for a molfile
    /// </summary>
    /// <param name="molfile"></param>
    /// <returns></returns>

    public double GetMolWeight(
      string molfile)
    {
      IMolLib mol = new CdkMol(MoleculeFormat.Molfile, molfile);
      double mw = GetMolWeight(mol as CdkMol);
      return mw;
    }

    /// <summary>
    /// GetMolWeight
    /// </summary>
    /// <param name="mol"></param>
    /// <returns></returns>

    public double GetMolWeight(
      IMolLib mol)
    {
      throw new NotImplementedException();
    }


    /// <summary>
    /// Get list of the Rgroups and the instance count and first buffer position for each one
    /// </summary>
    /// <param name="mol"></param>
    /// <param name="rgCounts"></param>
    /// <param name="rgBufferStartingPos"></param>

    public void GetCoreRGroupInfo(
      IMolLib mol,
      out SortedDictionary<int, int> rgCounts,
      out SortedDictionary<int, int> rgBufferStartingPos)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Scan the atoms in a fragment to identify the rgroup that the fragment has been mapped to
    /// </summary>
    /// <param name="fragment"></param>
    /// <returns></returns>

    public int GetFragmentRGroupAssignment(
      IMolLib fragment)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Remove RGroup attachment point atoms
    /// </summary>
    /// <param name="molfile"></param>
    /// <returns></returns>

    public string RemoveRGroupAttachmentPointAtoms(string molfile)
    {
      throw new NotImplementedException();

    }

  } // CdkMol
}
