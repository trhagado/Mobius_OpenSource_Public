﻿using Mobius.ComOps;
using Mobius.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobius.CdkMx
{
  public class CdkMolFactory : INativeMolFactory
  {

		/// <summary>
		/// Basic constructor
		/// </summary>

		public INativeMolMx NewCdkMol()
		{
			return new CdkMol();
		}

		/// <summary>
		/// Construct and assign parent IMoleculeMx 
		/// </summary>
		/// <param name="parent"></param>

		public INativeMolMx NewCdkMol (MoleculeMx parent)
		{
			return new CdkMol(parent);
		}

		/// <summary>
		/// Construct from mol format and string
		/// </summary>
		/// <param name="molFormat"></param>
		/// <param name="molString"></param>
		/// <returns></returns>

		public INativeMolMx NewCdkMol(
			MoleculeFormat molFormat,
			string molString)
		{
			return new CdkMol(molFormat, molString);
		}
	}
}
