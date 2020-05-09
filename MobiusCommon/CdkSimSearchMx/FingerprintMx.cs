using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;
using Mobius.CdkMx;

using NCDK;
using NCDK.Aromaticities;
using NCDK.Config;
using NCDK.Default;
using NCDK.Depict;
using NCDK.Fingerprints;
using NCDK.Graphs;
using NCDK.Isomorphisms;
using NCDK.Isomorphisms.Matchers;
using NCDK.IO;
using NCDK.IO.Iterator;
using NCDK.Layout;
//using NCDK.Silent;
using NCDK.Smiles;
using NCDK.Tools;
using NCDK.Tools.Manipulator;

using Lucene.Net.Util;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.CdkSearchMx
{

/// <summary>
/// Mobius Fingerprint Class
/// </summary>

	public class FingerprintMx
	{

		/// <summary>
		/// Constructor
		/// </summary>
		public FingerprintMx()
		{
			return;
		}

		public BitSetFingerprint CdkFp = null;  // FingerprintMx is based on the CDK BitSetFingerprint

		public UniChemId Id = new UniChemId(); // associated compound ID

		public string CanonSmiles = ""; // canonical smiles of mol/fragment associated with this FP

		public int[] OnBits => (int[])CdkFp.GetSetBits(); //	the bits in the fingerprint that are set to true.

		public int Cardinality => CdkFp.Cardinality; // the number of bits set to true in the fingerprint.

		public long Size => CdkFp.Length; // the length of the binary fingerprint in bits

		public BitArray ToBitSet() 
		{ return CdkFp.AsBitSet();  }

		/// <summary>
		/// ToBoolArray - return as bool array
		/// </summary>
		/// <returns></returns>
		public bool[] ToBoolArray()
		{
			bool[] ba = new bool[Size];

			foreach (int fpBit in OnBits)
				ba[fpBit] = true;

			return ba;
		}

		public byte[] MinHashSignature { get; set; } // First set bit seen for each of the 100 reorderings of the original bit array

		public long[] LshHashBins { get; set; } // 100 MinHashSignature bytes packed in groups of 4 to provide 25 LSH hash bin values

		/// <summary>
		/// CopyToSoundFingerprint
		/// </summary>
		/// <returns></returns>
#if false
		public SoundFingerprinting.Data.Fingerprint CopyToSoundFingerprint()
		{
			SoundFingerprinting.Data.Fingerprint sfp = new SoundFingerprinting.Data.Fingerprint();
			sfp.Signature = ToBoolArray();
			return sfp;
		}
#endif

		/// <summary>
		/// ByteArrayToLongArray
		/// </summary>
		/// <param name="ba"></param>
		/// <returns></returns>

		public static long[] ByteArrayToLongrray(byte[] ba)
		{
			long[] la = new long[(ba.Length + 7) / 8];
			for (int i = 0; i < la.Length; i++)
			{
				la[i] = BitConverter.ToInt64(ba, i * 8);
			}

			return la;
		}

		/// <summary>
		/// LongArrayToByteArray
		/// </summary>
		/// <param name="la"></param>
		/// <returns></returns>
		public static byte[] LongArrayToByteArray(long[] la)
		{
			byte[] ba = new byte[la.Length * 8];
			for (int i = 0; i < la.Length; i++)
			{
				byte[] ba2 = BitConverter.GetBytes(la[i]);
				ba2.CopyTo(ba, i * 8);
			}

			return ba;
		}


		/// <summary>
		/// CalculateJaccardSimilarity with another FingerprintMx
		/// </summary>
		/// <param name="fp2"></param>
		/// <returns></returns>
		public double CalculateJaccardSimilarity(FingerprintMx fp2)
		{
			bool[] x = ToBoolArray();
			bool[] y = fp2.ToBoolArray();

			double sim = CalculateJaccardSimilarity(x, y);
			return sim;
		}

		/// <summary>
		/// CalculateJaccardSimilarity with another FingerprintMx
		/// </summary>
		/// <param name="y"></param>
		/// <returns></returns>
		public double CalculateJaccardSimilarity(bool[] y)
		{
			bool[] x = ToBoolArray();
			double sim = CalculateJaccardSimilarity(x, y);
			return sim;
		}

		/// <summary>
		///   Calculate similarity between 2 fingerprints.
		/// </summary>
		/// <param name = "x">Fingerprint x</param>
		/// <param name = "y">Fingerprint y</param>
		/// <returns>Jaccard similarity between array X and array Y</returns>
		/// <remarks>
		///   Similarity defined as  (A intersection B)/(A union B)
		///   for types of columns a (1,1), b(1,0), c(0,1) and d(0,0), it will be equal to
		///   Sim(x,y) = a/(a+b+c)
		///   +1 = 10
		///   -1 = 01
		///   0 = 00
		/// </remarks>
		public static double CalculateJaccardSimilarity(bool[] x, bool[] y)
		{
			int a = 0, b = 0;
			for (int i = 0, n = x.Length; i < n; i++)
			{
				if (x[i] && y[i])
				{
					// 1 1
					a++;
				}
				else if ((x[i] && !y[i]) || (!x[i] && y[i]))
				{
					// 1 0 || 0 1 
					b++;
				}
			}

			if (a + b == 0)
			{
				return 0;
			}

			double sim = (double)a / (a + b);
			return sim;
		}

	}

}
