using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mobius.Data
{
/// <summary>
/// Allow calls to QueryEngineObj.GetAdditionalDataDelegate
/// </summary>
/// <param name="command"></param>
/// <returns></returns>

	public delegate object QeGetAdditionalDataDelegate(string command);
}
