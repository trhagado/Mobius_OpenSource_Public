namespace Mobius.SpotfireClient.ComAutomation
{
	/// <summary>
	/// 
	/// LoadViewsContextWrapper
	/// </summary>

	public class LoadViewsContextWrapper : ComWrapper
	{
		public LoadViewsContextWrapper(object implementation)
				: base(implementation)
		{
		}

		public object CreateObject(string assemblyQualifiedTypename)
		{
			return ComHelper.InvokeMethod(this.Wrapped, "CreateObject", assemblyQualifiedTypename);
		}

		public object CreateObjectFrom(string typename, string codebase)
		{
			return ComHelper.InvokeMethod(this.Wrapped, "CreateObjectFrom", typename, codebase);
		}

	}
}
