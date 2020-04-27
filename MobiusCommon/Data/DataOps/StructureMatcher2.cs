using Mobius.ComOps;
using Mobius.MolLib2;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace Mobius.Data
{

	/// <summary>
	/// Individual structure matching operations
	/// </summary>

	public partial class StructureMatcher
	{
		static MolLib2.Molecule FSSQueryMol = null;
		static MolLib2.Molecule FSSTargetMol = null;

		static MolLib2.Molecule SSSQueryMol = null;
		static MolLib2.Molecule SSSTargetMol = null;

		/// <summary>
		/// Perform a full structure search of query molecule against target molecule
		/// </summary>
		/// <param name="query"></param>
		/// <param name="target"></param>
		/// <returns></returns>

		public bool IsFSSMatch(
			MoleculeMx query,
			MoleculeMx target)
		{
			if (query.IsBiopolymerFormat || target.IsBiopolymerFormat)
			{
				if (!query.IsBiopolymerFormat || !target.IsBiopolymerFormat)
					return false;

				if (Lex.Eq(query.PrimaryValue, target.PrimaryValue))
					return true;

				else return false;
			}

			SetFSSQueryMolecule(query);
			bool b = IsFSSMatch(target);
			return b;
		}

		/// <summary>
		/// Set full structure search query molecule
		/// </summary>
		/// <param name="cs"></param>

		public void SetFSSQueryMolecule(
			MoleculeMx cs)
		{
			if (cs.IsBiopolymerFormat) // fix later
			{
				QueryMol = cs;
				return;
			}

			QueryMol = cs.Clone();
			FSSQueryMol = MolLib2.Util.ChimeStringToMolecule(cs.GetChimeString());
			MolLib2.Util.SetFSSQueryMolecule(FSSQueryMol);

			return;
		}

		/// <summary>
		/// Do a SSS using the predefined query molecule and supplied target molecule
		/// </summary>
		/// <returns></returns>

		public bool IsFSSMatch(MoleculeMx cs)
		{
			if (MoleculeMx.IsUndefined(QueryMol) || MoleculeMx.IsUndefined(cs))
				return false;

			if (cs.IsBiopolymerFormat)
			{
				return IsFSSMatch(QueryMol, cs);
			}

			TargetMol = cs;
			FSSTargetMol = MolLib2.Util.ChimeStringToMolecule(cs.GetChimeString());
			bool b = MolLib2.Util.IsFSSMatch(FSSTargetMol);
			return b;
		}

		/// <summary>
		/// Perform a substructure search
		/// </summary>
		/// <param name="query"></param>
		/// <param name="target"></param>
		/// <returns></returns>

		public bool IsSSSMatch(
			MoleculeMx query,
			MoleculeMx target)
		{
			if (query.IsBiopolymerFormat || target.IsBiopolymerFormat) // fix later
				return false;

			QueryMol = query;
			SSSQueryMol = MolLib2.Util.ChimeStringToMolecule(query.GetChimeString());
			SSSTargetMol = MolLib2.Util.ChimeStringToMolecule(target.GetChimeString());
			bool b = MolLib2.Util.IsSSSMatch(SSSQueryMol, SSSTargetMol);
			return b;
		}

		/// <summary>
		/// Set substructure search query molecule
		/// </summary>
		/// <param name="cs"></param>

		public void SetSSSQueryMolecule(
			MoleculeMx cs)
		{
			QueryMol = cs.Clone();
			SSSQueryMol = MolLib2.Util.ChimeStringToMolecule(cs.GetChimeString());
			MolLib2.Util.SetSSSQueryMolecule(SSSQueryMol);

			return;
		}

		/// <summary>
		/// Do a SSS using the predefined query molecule and supplied target molecule
		/// </summary>
		/// <returns></returns>

		public bool IsSSSMatch(MoleculeMx cs)
		{
			if (QueryMol == null || cs.IsBiopolymerFormat) // fix later
				return false;

			TargetMol = cs;
			SSSTargetMol = MolLib2.Util.ChimeStringToMolecule(cs.GetChimeString());
			bool b = MolLib2.Util.IsSSSMatch(SSSTargetMol);
			return b;
		}

		/// <summary>
		/// Map query against structure & return highlighted match
		/// </summary>
		/// <param name="cs"></param>
		/// <returns></returns>

		public MoleculeMx HighlightMatchingSubstructure(
			MoleculeMx cs)
		{
			if (cs.IsBiopolymerFormat) // fix later
				return cs;

			MoleculeMx cs2 = null;
			PerformanceTimer pt = PT.Start("HighlightMatchingSubstructure");

			string molfile = MolLib2.Util.HilightSSSMatch(cs.GetMolfileString());
			cs2 = new MoleculeMx(MoleculeFormat.Molfile, molfile);

			pt.Update();
			return cs2;
		}

		/// <summary>
		/// Align supplied structure to query structure
		/// </summary>
		/// <param name="skid"></param>
		/// <returns></returns>

		public MoleculeMx AlignToMatchingSubstructure(
			MoleculeMx target)
		{
			if (target.IsBiopolymerFormat) // fix later
				return target;

			MoleculeMx cs2 = null;
			PerformanceTimer pt = PT.Start("AlignToMatchingSubstructure");
			Stopwatch sw = Stopwatch.StartNew();

			string molfile = MolLib2.Util.OrientToMatchingSubstructure(target.GetMolfileString());
			cs2 = new MoleculeMx(MoleculeFormat.Molfile, molfile);

			pt.Update();
			return cs2;
		}

	} // class StructureMatcher
}
