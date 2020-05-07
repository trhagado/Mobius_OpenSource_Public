using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobius.CdkMx
{
  public class CdkMolFactory : ICdkMolFactory
  {

		/// <summary>
		/// Basic constructor
		/// </summary>

		public ICdkMol NewCdkMol()
		{
			return new CdkMol();
		}

		/// <summary>
		/// Construct and assign parent IMoleculeMx 
		/// </summary>
		/// <param name="parent"></param>

		public ICdkMol NewCdkMol (MoleculeMx parent)
		{
			return new CdkMol(parent);
		}

		/// <summary>
		/// Construct from mol format and string
		/// </summary>
		/// <param name="molFormat"></param>
		/// <param name="molString"></param>
		/// <returns></returns>

		public ICdkMol NewCdkMol(
			MoleculeFormat molFormat,
			string molString)
		{
			return new CdkMol(molFormat, molString);
		}
	}
}
