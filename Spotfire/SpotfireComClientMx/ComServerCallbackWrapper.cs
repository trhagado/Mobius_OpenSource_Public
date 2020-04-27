using System.Runtime.InteropServices;

namespace Mobius.SpotfireClient.ComAutomation
{

	/// <summary>
	/// MobiusViewCallbackWrapper
	/// Based on Spotfire Com Automation Example: "ExampleViewCallbackWrapper.cs"
	/// </summary>

	[ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
		[Guid("dbac90b0-dd6c-4f80-bf14-cbf2f268b28d")] // new guid
	//[Guid("749BA76E-E9CC-4722-A6A9-D17C48A6EB70")] // original guid
	public class ComServerCallbackWrapper
    {
        // VERY IMPORTANT: 
        // If you copy-paste this code to create your own COM-visible object
        // you MUST replace the Guid above.

        private ComServerCallbackBase callback;


        internal ComServerCallbackWrapper(ComServerCallbackBase callback)
        {
            this.callback = callback;
        }


        public void OnStatusChanged(string status)
        {
            this.callback.OnStatusChanged(status);
        }
    }
}
