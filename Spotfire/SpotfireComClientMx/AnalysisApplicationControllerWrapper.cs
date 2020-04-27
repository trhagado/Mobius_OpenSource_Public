// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalysisApplicationControllerWrapper.cs" company="Spotfire AB">
//   Copyright © 2009 - 2011 Spotfire AB, 
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

    internal class AnalysisApplicationControllerWrapper
    {
        #region Constants and Fields

        private object controllerInstance;

        #endregion

        #region Constructors and Destructors

        public AnalysisApplicationControllerWrapper()
        {
            this.controllerInstance = Activator.CreateInstance(Type.GetTypeFromProgID("Spotfire.Dxp"));
        }

        #endregion

        #region Public Methods

        public void Exit()
        {
            ComHelper.InvokeMethod(this.controllerInstance, "Exit");
        }

        public void Start(ComCallbackBase callback)
        {
            ComHelper.InvokeMethod(this.controllerInstance, "Start", new ComCallbackWrapper(callback));
        }

        public void StartWithCommandLine(ComCallbackBase callback, string commandLine)
        {
            ComHelper.InvokeMethod(this.controllerInstance, "StartWithCommandLine", new ComCallbackWrapper(callback), commandLine);
        }

        #endregion
    }
}
