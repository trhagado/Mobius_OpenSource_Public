using Mobius.Data;
using Mobius.ComOps;

using java.io;
using java.util;

using cdk = org.openscience.cdk;
using org.openscience.cdk;
using org.openscience.cdk.interfaces;
using org.openscience.cdk.fingerprint;
using org.openscience.cdk.tools.manipulator;
using org.openscience.cdk.graph;
using org.openscience.cdk.qsar.result;
using org.openscience.cdk.io;
using org.openscience.cdk.io.iterator;

using ambit2.smarts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.CdkMx
{
	public class CdkSmarts
	{
		public static void AmbitTest()
		{
			//string smirks = "[*:1][N:2](=[O:3])=[O:4]>>[*:1][N+:2](=[O:3])[O-:4]";
			//string smirks = "[N:1]1[C:2][C:3]=[C:4][C:5]1>>[N:1]1[C:2]=[C:3][C:4]=[C:5]1";
			string smirks = "[#16:1]>>[#16:1](=[O])";
			//string targetSmiles = "O=S(c2nc1ccc(OC)cc1n2)Cc3ncc(c(OC)c3C)C";
			string targetSmiles = "CC1=CN=C(C(=C1OC)C)CS(=O)C2=NC3=C(N2)C=C(C=C3)OC"; // Omeprazole
			string productSmiles = "";
			IAtomContainer target = new AtomContainer();

			IChemObjectBuilder icob = org.openscience.cdk.silent.SilentChemObjectBuilder.getInstance();
			SMIRKSManager smrkMan = new SMIRKSManager(icob);
			//SMIRKSManager smrkMan = SMIRKSManager.getDefaultSMIRKSManager();
			SMIRKSReaction reaction = smrkMan.parse(smirks);
			if (!smrkMan.getErrors().Equals(""))
			{
				throw (new Exception("Smirks Parser errors:\n" + smrkMan.getErrors()));
			}

			target = CdkUtil.SmilesToAtomContainer(targetSmiles);
			if (smrkMan.applyTransformation(target, reaction))
			{
				IAtomContainer product = target;
				productSmiles = CdkUtil.AtomContainerToSmiles(product);
				return; // all products inside the same atomcontainer (target), could be disconnected
			}

			else return;
			//Generate separate products for every possible reaction(used in Toxtree)


			//SMIRKSManager smrkMan = new SMIRKSManager();
			//SMIRKSReaction smr = smrkMan.parse(reaction.getSMIRKS());
			//IAtomContainer product = reactant; //(IAtomContainer) reactant.clone();
			//IAtomContainerSet rproducts = smrkMan.applyTransformationWithSingleCopyForEachPos(product, null, smr);
			//products returned in a separate atom sontainer set
		}

	}
}
