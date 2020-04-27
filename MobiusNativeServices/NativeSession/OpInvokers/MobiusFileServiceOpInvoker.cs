using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mobius.Services.Native.ServiceOpCodes;


namespace Mobius.Services.Native.OpInvokers
{
    public class MobiusFileServiceOpInvoker : IInvokeServiceOps
    {
        #region IInvokeServiceOps Members

        object IInvokeServiceOps.InvokeServiceOperation(int opCode, object[] args)
        {
            MobiusFileService op = (MobiusFileService)opCode;
            switch (op)
            {
                case MobiusFileService.CopyToServer:
                    {
                        byte[] fileContents = (byte[])args[0];
                        string serverFileName = (string)args[1];
                        UAL.ServerFile.CopyToServer(serverFileName, fileContents);
                        return true;
                    }
                case MobiusFileService.CopyToClient:
                    {
                        string serverFileName = (string)args[0];
                        byte[] serverFileContents = UAL.ServerFile.CopyToClient(serverFileName);
                        return serverFileContents;
                    }
                case MobiusFileService.GetLastWriteTime:
                    {
                        string serverFileName = (string)args[0];
                        DateTime lastWriteTime = UAL.ServerFile.GetLastWriteTime(serverFileName);
                        return lastWriteTime;
                    }
                case MobiusFileService.GetTempFileName:
                    {
                        string extension = (string)args[0];
                        bool deleteOnExit = (bool)args[1];
                        string tempFileName = Mobius.UAL.ServerFile.GetTempFileName(extension, deleteOnExit);
                        return tempFileName;
                    }
								case MobiusFileService.CanWriteFileFromServiceAccount:
										{
											string path = (string)args[0];
											bool result = Mobius.UAL.ServerFile.CanWriteFileFromServiceAccount(path);
											return result;
										}
						}
            return null;
        }

        #endregion
    }
}
