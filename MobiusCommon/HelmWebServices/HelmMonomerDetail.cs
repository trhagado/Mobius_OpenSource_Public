using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mobius.Helm
{

	public class HelmMonomerDetail
	{
		public string symbol { get; set; }
		public string name { get; set; }
		public string molfile { get; set; }
		public string author { get; set; }
		public int id { get; set; }
		public Rgroup[] rgroups { get; set; }
		public string polymerType { get; set; }
		public string createDate { get; set; }
		public string monomerType { get; set; }
		public string smiles { get; set; }
		public string naturalAnalog { get; set; }
	}

	public class Rgroup
	{
		public int id { get; set; }
		public string alternateId { get; set; }
		public string label { get; set; }
		public string capGroupName { get; set; }
		public string capGroupSMILES { get; set; }
	}

}