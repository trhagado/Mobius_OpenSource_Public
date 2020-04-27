// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComHelper.cs" company="Spotfire AB">
//   Copyright © 2007 - 2011 Spotfire AB, 
// Första Långgatan 26, SE-413 28 Göteborg, Sweden. 
// All rights reserved. 
// This software is the confidential and proprietary information 
// of Spotfire AB ("Confidential Information"). You shall not 
// disclose such Confidential Information and may not use it in any way, 
// absent an express written license agreement between you and Spotfire AB 
// or TIBCO Software Inc. that authorizes such use.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Mobius.SpotfireClient.ComAutomation
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Help class to perform late-bound method invokation on COM objects.
    /// </summary>
    internal static class ComHelper
    {
        #region Public Methods

        public static void DisposeAndRelease(object instance)
        {
            IDisposable disposable = instance as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }

            Release(instance);
        }

        public static object InvokeMethod(object instance, string name, params object[] args)
        {
            return InvokeMethod(instance, name, BindingFlags.InvokeMethod | BindingFlags.OptionalParamBinding, args);
        }

        public static object InvokeMethod(object instance, string name, BindingFlags attrs, object[] args)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance", "Value cannot be null.");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            try
            {
                return instance.GetType().InvokeMember(name, attrs, null, instance, args, CultureInfo.InvariantCulture);
            }
            catch (COMException comError)
            {
                if (comError.ErrorCode == unchecked((int)0x80020006))
                {
                    if ((attrs & BindingFlags.GetProperty) == BindingFlags.GetProperty)
                    {
                        throw new MissingFieldException(
                            string.Format("Failed to invoke \"{0}\" via Automation.", name), comError);
                    }
                    else
                    {
                        throw new MissingMemberException(
                            string.Format("Failed to invoke \"{0}\" via Automation.", name), comError);
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        public static bool IsNull(object instance)
        {
            return instance == null || Convert.IsDBNull(instance);
        }

        public static void Release(object instance)
        {
            if (instance == null)
            {
                return;
            }

            if (instance.GetType().IsCOMObject)
            {
                Marshal.ReleaseComObject(instance);
            }
        }

        #endregion
    }
}
