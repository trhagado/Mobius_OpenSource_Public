using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mobius.Services.Types;
using Mobius.Services.Native.ServiceOpCodes;

namespace Mobius.Services.Native.OpInvokers
{
	public class MobiusUalUtilServiceOpInvoker : IInvokeServiceOps
	{
		object IInvokeServiceOps.InvokeServiceOperation(int opCode, object[] args)
		{
			MobiusUalUtilService op = (MobiusUalUtilService)opCode;
			switch (op)
			{

				case MobiusUalUtilService.GetServicesIniFile: 
					{
						string iniFileContent = UAL.UalUtil.GetServicesIniFile();
						return iniFileContent;
					}

				case MobiusUalUtilService.SelectOracleBlob:
					{
						string table = (string)args[0];
						string matchCol = (string)args[1];
						string typeCol = (string)args[2];
						string contentCol = (string)args[3];
						string matchVal = (string)args[4];

						string typeVal;
						byte[] ba;
						UAL.UalUtil.SelectOracleBlob(table, matchCol, typeCol, contentCol, matchVal, out typeVal, out ba);

						TypedBlob blob = new TypedBlob();
						blob.Type = typeVal;
						blob.Data = ba;

						return blob;
					}

				case MobiusUalUtilService.SelectOracleClob:
					{
						string table = (string)args[0];
						string matchCol = (string)args[1];
						string typeCol = (string)args[2];
						string contentCol = (string)args[3];
						string matchVal = (string)args[4];

						string typeVal, clob;
						UAL.UalUtil.SelectOracleClob(table, matchCol, typeCol, contentCol, matchVal, out typeVal, out clob);

						TypedClob tClob = new TypedClob();
						tClob.Type = typeVal;
						tClob.Data = clob;

						return tClob;
					}
			}
			return null;
		}

	}
}
