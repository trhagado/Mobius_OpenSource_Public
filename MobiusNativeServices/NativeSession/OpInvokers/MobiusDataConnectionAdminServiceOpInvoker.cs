using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mobius.Services.Native.ServiceOpCodes;


namespace Mobius.Services.Native.OpInvokers
{
    public class MobiusDataConnectionAdminServiceOpInvoker : IInvokeServiceOps
    {
        #region IInvokeServiceOps Members

        object IInvokeServiceOps.InvokeServiceOperation(int opCode, object[] args)
        {
            MobiusDataConnectionAdminService op = (MobiusDataConnectionAdminService)opCode;
            switch (op)
            {
                case MobiusDataConnectionAdminService.DefineDataSource:
                    {
                        string arg = (string)args[0];
                        string result = UAL.DbConnectionMx.DefineDataSource(arg);
                        return result;
                    }
                case MobiusDataConnectionAdminService.AssociateSchema:
                    {
                        string arg = (string)args[0];
                        string result = UAL.DbConnectionMx.AssociateSchema(arg);
                        return result;
                    }
                case MobiusDataConnectionAdminService.LoadMetaData:
                    {
                        bool succeeded = true;
                        UAL.DataSourceMx.LoadMetadata();
                        return succeeded;
                    }
                case MobiusDataConnectionAdminService.GetOracleConnections:
                    {
                        string result = UAL.DbConnectionMx.GetActiveDatabaseConnections();
                        return result;
                    }
                case MobiusDataConnectionAdminService.TestConnections:
                    {
                        string result = UAL.DbConnectionMx.TestConnections();
                        return result;
                    }
                case MobiusDataConnectionAdminService.UpdateDatabaseLinks:
                    {
                        string singleInstance = (string)args[0];
                        string result = UAL.DbConnectionMx.UpdateDatabaseLinks(singleInstance);
                        return result;
                    }
                case MobiusDataConnectionAdminService.CheckForConnectionLeaks:
                    {
                        string result = UAL.DbConnectionMx.CheckForConnectionLeaks();
                        return result;
                    }
            }
            return null;
        }

        #endregion
    }
}
