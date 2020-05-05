using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mobius.Data
{

/// <summary>
/// Individual structure matching operations
/// </summary>

	public partial class StructureMatcher
	{
		MoleculeMx QueryMol = null; // most-recent query structure
		MoleculeMx TargetMol = null; // most-recent target molecule

		string FullStructureSearchType; // most-recent switches
		object QueryFingerprint; // fingerprint for current Molsim query

		static int[] EnkFilWeights; // weightings of 960 MDL keys

		static byte[] PreComputedNbBitsOn = // number of bits on in a byte indexed by the byte value
			 {0, 1, 1, 2, 1, 2,	2, 3, 1, 2, 2, 3, 2, 3, 3, 4,
				1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5, 
				1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4,	3, 4, 4, 5,
				2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
				1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
				2, 3,	3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
				2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 
				3, 4, 4, 5, 4, 5, 5, 6,	4, 5, 5, 6, 5, 6, 6, 7, 
				1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
				2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5,	5, 6,
				2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6, 
				3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
				2, 3, 3, 4,	3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
				3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 
				3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7, 
				4, 5, 5, 6, 5, 6, 6, 7, 5, 6, 6, 7, 6, 7, 7, 8 };

		/// <summary>
		/// Basic constructor
		/// </summary>

		public StructureMatcher()
		{
			return;
		}

		/// <summary>
		/// Calculate similarity score
		/// </summary>
		/// <param name="query"></param>
		/// <param name="target"></param>
		/// <returns></returns>

		public double Molsim(
			MoleculeMx query,
			MoleculeMx target,
			SimilaritySearchType type)
		{
			FingerprintType fpType = FingerprintType.Undefined;

			if (type == SimilaritySearchType.Normal)
				fpType = FingerprintType.MACCS;

			else if (type == SimilaritySearchType.ECFP4)
				fpType = FingerprintType.Circular;

			else throw new Exception("Similarity type not supported in this context: " + type);

			bool newQuery = (QueryMol == null || QueryMol.PrimaryValue != query.PrimaryValue);
			if (newQuery) // recalc query keys if query has changed
			{
				QueryMol = query.Clone();
				QueryFingerprint = query.BuildBitSetFingerprint(fpType);
			}

			object targetFingerprint = target.BuildBitSetFingerprint(fpType);

			double score = MoleculeMx.CalculateBitSetFingerprintSimilarity(QueryFingerprint, targetFingerprint);

			return score;
		}

		/// <summary>
		/// Compare two molecules for full equality 
		/// </summary>
		/// <param name="query"></param>
		/// <param name="target"></param>
		/// <returns></returns>

		public bool Equal(
			MoleculeMx query,
			MoleculeMx target)
		{
			string switches = "All";
			return FullStructureMatch(query, target, switches);
		}

		/// <summary>
		/// Do a similarity match
		/// </summary>
		/// <param name="query"></param>
		/// <param name="target"></param>
		/// <returns></returns>

		public bool FullStructureMatch(
			MoleculeMx query,
			MoleculeMx target,
			string switches)
		{
			bool newQuery = (QueryMol == null || QueryMol.PrimaryValue != query.PrimaryValue);
			if (newQuery)
			{
				QueryMol = query.Clone();

				SSSQueryMol = MolLibFactory.NewMolLib(query);
			}

			SSSTargetMol = MolLibFactory.NewMolLib(target);
			bool b = MolLibUtil.FullStructureMatch(SSSQueryMol, SSSTargetMol, switches);
			return b;
		}

		/// <summary>
		/// Calculate Tanimoto coefficient for a byte array
		/// a = count of bits set in first set only
		/// b = count of bits set in second set only
		/// c = count of common bits
		/// T = c / (a + b + c)
		/// </summary>
		/// <param name="keys1"></param>
		/// <param name="keys2"></param>
		/// <returns></returns>

		public static double CalculateTanimotoSimilarity(
			byte[] keys1,
			byte[] keys2)
		{
			int a = 0, b = 0, c = 0, d = 0;
			byte k1, k2, notK1, notK2;
			for (int bi = 0; bi < keys1.Length; bi++)
			{
				k1 = keys1[bi];
				notK1 = (byte)~k1;
				k2 = keys2[bi];
				notK2 = (byte)~k2;

				a += PreComputedNbBitsOn[k1 & notK2]; // on in first keyset only
				b += PreComputedNbBitsOn[notK1 & k2]; // on in second keyset only
				c += PreComputedNbBitsOn[k1 & k2]; // on in both sets
				d += PreComputedNbBitsOn[notK1 & notK2]; // off in both sets
			}

			if (d == keys1.Length * 8) return 1.0; // if all bits zero in both then say sim is 1.0

			double sim = c * 1.0 / (a + b + c);
			return sim;
		}

	} // class StructureMatcher

}
