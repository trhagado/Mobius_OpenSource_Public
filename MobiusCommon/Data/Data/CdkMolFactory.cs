using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Threading.Tasks;

namespace Mobius.Data

{
	/// <summary>
	/// CdkMolFactory used to create CdkMol instances
	/// 
	/// </summary>

	public class CdkMolFactory
	{
		/// <summary>
		/// Create basic CdkMol instance
		/// </summary>
		/// <returns></returns>
		/// 
		public static INativeMolMx NewCdkMol()
		{
			return I.NewCdkMol();
		}

		/// <summary>
		/// Create CdkMol instance from MoleculeMx
		/// </summary>
		/// <param name="molMx"></param>
		/// <returns></returns>

		public static INativeMolMx NewCdkMol(MoleculeMx molMx)
		{
			return I.NewCdkMol(molMx);
		}

		/// <summary>
		/// Construct from mol format and string
		/// </summary>
		/// <param name="molFormat"></param>
		/// <param name="molString"></param>
		/// <returns></returns>

		public static INativeMolMx NewCdkMol(
			MoleculeFormat molFormat,
			string molString)
		{
			return I.NewCdkMol(molFormat, molString);
		}


		/// <summary>
		/// Injected instance of CdkMolFactory
		/// </summary>

		public static INativeMolFactory I
		{
			get
			{
				if (i != null) return i;
				throw new NullReferenceException("CdkMolFactory instance not defined");
			}

			set => i = value;

		}
		static INativeMolFactory i = null;
	}
}
