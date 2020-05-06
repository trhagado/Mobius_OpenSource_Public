using Mobius.ComOps;
using Mobius.Data;

using cdk = org.openscience.cdk;
using org.openscience.cdk;
using org.openscience.cdk.inchi;
using org.openscience.cdk.interfaces;
using org.openscience.cdk.fingerprint;
using org.openscience.cdk.smiles;
using org.openscience.cdk.tools.manipulator;
using org.openscience.cdk.aromaticity;
using org.openscience.cdk.graph;
using org.openscience.cdk.qsar.result;
using org.openscience.cdk.io;
using org.openscience.cdk.io.iterator;
using org.openscience.cdk.tools;

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

	public partial class CdkMol : ICdkMol
	{
		public static object Grg = null; // the current decomposition

		public static bool Debug = false;

		public static void Initialize()
		{

			return;
		}

		public static void SetCoreStructure(ICdkMol mol, bool PromptUser)
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
			ICdkMol mol,
			out SortedDictionary<int, int> rgCounts,
			out SortedDictionary<int, int> rgBufferStartingPos)
		{
			throw new NotImplementedException();
		}

		public static int ProcessTargetMolecule(ICdkMol mol)
		{
			throw new NotImplementedException();
		}

		public static ICdkMol GetIthMappingFragment(
			int queryIndex, 
			int rGroupIndex)
		{
			throw new NotImplementedException();
		}

		public static ICdkMol GetAlignedTargetForMapping(
			int queryIndex)
		{
			throw new NotImplementedException();
		}


	}
}
