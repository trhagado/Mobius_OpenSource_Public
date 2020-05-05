using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Text;
using System.Threading.Tasks;

namespace Mobius.MolLib2
{

	/// <summary>
	/// RGroup Decomposition
	/// </summary>

	public class RGroupDecomp
	{
		public static object Grg = null; // the current decomposition

		public static bool Debug = false;

		public static void Initialize()
		{

			return;
		}

		public static void SetCoreStructure(Molecule mol, bool PromptUser)
		{
			throw new NotImplementedException();
		}

		public static void SetAlignStructuresToCore (bool b)
		{
			throw new NotImplementedException();
		}

		public static void SetAllowMultipleCoreMappings(bool b)
		{
			throw new NotImplementedException();
		}

		public static void SetIncludeHydrogenFragments(bool b)
		{
			throw new NotImplementedException();
		}

		public static int[] GetCoreRNumbers()
		{
			throw new NotImplementedException();
		}

		public static int GetNumLoadedSubstituents()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Get list of RNumbers and a count of instances for each
		/// </summary>
		/// <param name="mol"></param>
		/// <returns></returns>

		public static void GetCoreRGroupInfo(
			Molecule mol,
			out SortedDictionary<int, int> rgCounts,
			out SortedDictionary<int, int> rgBufferStartingPos)
		{
			Stopwatch sw = Stopwatch.StartNew();

			Util.GetCoreRGroupInfo(mol, out rgCounts, out rgBufferStartingPos);

			int msTime = (int)sw.ElapsedMilliseconds;
			if (Debug) DebugLog.Message("Time(ms): " + msTime);

			return;
		}

		public static int ProcessTargetMolecule(Molecule mol)
		{
			throw new NotImplementedException();
		}

		public static Molecule GetIthMappingFragment(
			int queryIndex, 
			int rGroupIndex)
		{
			throw new NotImplementedException();
		}

		public static Molecule GetAlignedTargetForMapping(
			int queryIndex)
		{
			throw new NotImplementedException();
		}


	}
}
