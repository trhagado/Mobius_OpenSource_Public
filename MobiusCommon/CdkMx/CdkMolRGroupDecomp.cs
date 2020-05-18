using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Mobius.CdkMx
{
	/// <summary>
	/// RGroup Decomposition
	/// </summary>

	public partial class CdkMol : INativeMol
	{
		public static object Grg = null; // the current decomposition

		public static bool Debug = false;

		public static void Initialize()
		{

			return;
		}

		public static void SetCoreStructure(INativeMol mol, bool PromptUser)
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
			INativeMol mol,
			out SortedDictionary<int, int> rgCounts,
			out SortedDictionary<int, int> rgBufferStartingPos)
		{
			throw new NotImplementedException();
		}

		public static int ProcessTargetMolecule(INativeMol mol)
		{
			throw new NotImplementedException();
		}

		public static INativeMol GetIthMappingFragment(
			int queryIndex, 
			int rGroupIndex)
		{
			throw new NotImplementedException();
		}

		public static INativeMol GetAlignedTargetForMapping(
			int queryIndex)
		{
			throw new NotImplementedException();
		}


	}
}
