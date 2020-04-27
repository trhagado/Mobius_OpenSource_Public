using Mobius.ComOps;
using Mobius.Data;
using Mobius.UAL;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Mobius.QueryEngineLibrary
{
	/// <summary>
	/// Proxy broker is a broker that runs somewhere other than in the current process.
	/// </summary>

	public class ProxyMetaBroker : GenericMetaBroker
	{

		// Example: Calls to AWS may need to come from a whitelisted machine other than the 
		// current machine. In this case another instance of the Mobius services is started
		// on one of the whitelisted machines and broker calls are passed through from
		// a proxy broker to the whitelisted broker for execution.
		//
		// (Not yet implemented)

	}
}
