using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Security.Principal;
using System.Diagnostics;
using System.Threading;

namespace Mobius.SpotfireClient
{
	public class PipeClient
	{
		static NamedPipeClientStream pipeClient = null;
		static StreamString ss = null;

		public static void Test()
		{
			if (pipeClient == null)
			{
				pipeClient =
						new NamedPipeClientStream(".", "MobiusSpotfirePipe",
								PipeDirection.InOut, PipeOptions.None,
								TokenImpersonationLevel.Impersonation);

				pipeClient.Connect();

				ss = new StreamString(pipeClient);
			}

			ss.WriteString("Command...");
			string commandResult = ss.ReadString();

			//pipeClient.Close();
			return;
		}
	}
}
