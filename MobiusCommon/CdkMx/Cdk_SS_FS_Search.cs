
using Mobius.ComOps;
using Mobius.Data;

using NCDK;
using NCDK.Depict;
using NCDK.Isomorphisms;
using NCDK.Isomorphisms.Matchers;
using NCDK.Silent;

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mobius.CdkMx
{

/// <summary>
/// Substructure and full-structure search
/// </summary>

  public partial class CdkMol : INativeMol
  {

    CdkMol FSSQueryMolecule = null;

    /// <summary>
    /// Check if substructure query matches target molecule
    /// </summary>
    /// <param name="queryMol"></param>
    /// <param name="targetMol"></param>
    /// <returns></returns>

    public bool IsSSSMatch(
      INativeMol queryMol,
      INativeMol targetMol)
    {
      SetSSSQueryMolecule(queryMol as CdkMol);
      return IsSSSMatch(targetMol as CdkMol);
    }

    /// <summary>
    /// Prepare for SSS matching of supplied query molecule
    /// </summary>
    /// <param name="queryMol"></param>

    public void SetSSSQueryMolecule(
    INativeMol queryMol)
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
      INativeMol targetMol)
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
      INativeMol targetMol,
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

    public INativeMol HilightSSSMatchGMap(
      INativeMol targetMol,
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
      INativeMol query,
      INativeMol target,
      string FullStructureSearchType = null)
    {
      CdkMol q = query as CdkMol;
      CdkMol t = target as CdkMol;

      var fs = new UniversalIsomorphismTester();

      if (fs.IsIsomorph(q.NativeMol, t.NativeMol))
        return true;
      else return false;
    }

    /// <summary>
    /// Prepare for FSS matching of supplied query molecule
    /// </summary>
    /// <param name="queryMol"></param>

    public void SetFSSQueryMolecule(
      INativeMol queryMol,
      string FullStructureSearchType = null)
    {
      FSSQueryMolecule = queryMol as CdkMol;
      return;
    }

    /// <summary>
    /// Map current query against supplied target molecule
    /// </summary>
    /// <param name="targetMol"></param>
    /// <returns></returns>

    public bool IsFSSMatch(
      INativeMol targetMol)
    {
      return FullStructureMatch(FSSQueryMolecule, targetMol);
    }


  } // CdkMol
}
