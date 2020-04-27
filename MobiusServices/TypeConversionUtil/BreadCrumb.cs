using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;


namespace Mobius.Services.Util.TypeConversionUtil
{
	internal class BreadCrumb
	{
		internal string InProgressConversionMethodName;
		internal object ObjectBeingConverted;

		internal BreadCrumb(string methodName, object objectBeingConverted)
		{
			InProgressConversionMethodName = methodName;
			ObjectBeingConverted = objectBeingConverted;
		}
	}

	internal static class BreadCrumbCollectionExtensions
	{
		internal static bool ContainsEquivalentBreadCrumb(
				this List<BreadCrumb> breadCrumbList,
				BreadCrumb breadCrumb)
		{
			bool isPresent = false;
			foreach (BreadCrumb listBreadCrumb in breadCrumbList)
			{
				if (listBreadCrumb.IsSameAs(breadCrumb))
				{
					isPresent = true;
					break;
				}
			}
			return isPresent;
		}

		internal static bool IsSameAs(
				this BreadCrumb breadCrumbA,
				BreadCrumb breadCrumbB)
		{
			bool result = (breadCrumbA.InProgressConversionMethodName == breadCrumbB.InProgressConversionMethodName) &&
										(breadCrumbA.ObjectBeingConverted == breadCrumbB.ObjectBeingConverted);
			return result;
		}
	}
}
