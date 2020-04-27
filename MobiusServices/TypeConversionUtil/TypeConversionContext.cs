using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.Services.Util.TypeConversionUtil
{
    internal class TypeConversionContext
    {
        internal List<BreadCrumb> BreadCrumbs = new List<BreadCrumb>();
        internal Dictionary<object, object> KnownObjects = new Dictionary<object, object>();
    }
}
