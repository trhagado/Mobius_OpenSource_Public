
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.Serialization;
using System.Windows.Forms;

using Mobius.Services.Util; 
using Mobius.Services.Types;
using Mobius.Services.Types.Internal;
using Mobius.ComOps;

namespace Mobius.Services.Native 
{
	static class Program 
	{
		/// <summary>
		/// The main entry point for the MobiusNativeServices application. 
		/// </summary>
		 
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			Form f = new Form(); // need to create a form so certain UI operations work properly (e.g. ISIS, LSW)

			NativeSessionHost.StartUp(args); // fire up the session

			Application.Run(); // Process UI messages until app exits

			return;
		}

	}
}
