using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.Services.Native
{
    public interface IInvokeServiceOps
    {
        object InvokeServiceOperation(int opCode, object[] args);
    }
}
