using System.Runtime.InteropServices;

// Mobius CallbackWrapper
// Based on Spotfire Com Automation Example: "CallbackWrapper.cs"

namespace Mobius.SpotfireClient.ComAutomation
{
	/// <summary>
	/// ComCallbackWrapper
	/// </summary>
	
	[ComVisible(true)]
  [ClassInterface(ClassInterfaceType.AutoDispatch)]
	[Guid("a3d06aaa-4da8-455c-9636-ace2fd584e9b")] // new guid
	//[Guid("7FD852DB-C744-4956-8EAC-5A8826334752")] // original guid
	public class ComCallbackWrapper
    {
        // VERY IMPORTANT: 
        // If you copy-paste this code to create your own COM-visible object
        // you MUST replace the Guid above.

        private ComCallbackBase callback;

        internal ComCallbackWrapper(ComCallbackBase callback)
        {
            this.callback = callback;
        }

        public void Exited([MarshalAs(UnmanagedType.IUnknown)] object context)
        {
            this.callback.Exited(new ExitContextWrapper(context));
        }

        public void LoadViews([MarshalAs(UnmanagedType.IUnknown)] object context)
        {
            this.callback.LoadViews(new LoadViewsContextWrapper(context));
        }

        public void Started([MarshalAs(UnmanagedType.IUnknown)] object context)
        {
            this.callback.Started(new StartContextWrapper(context));
        }

    }
}
