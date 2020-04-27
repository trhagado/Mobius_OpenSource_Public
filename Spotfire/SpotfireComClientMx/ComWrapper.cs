using System;

namespace Mobius.SpotfireClient.ComAutomation
{

	/// <summary>
	/// ComWrapper
	/// 
	/// Wraps a COM object and provides support for calling late-bound methods on that object.
	/// Inherit this class and add methods that call <see cref="M:InvokeMethod"/> to invoke 
	/// late-bound methods on the wrapped COM object.
	/// 
	/// Based on Spotfire Com Automation Example: "ComWrapper.cs"
	/// </summary>
	public class ComWrapper : IDisposable
	{
		private object implementation;

		public ComWrapper(object implementation)
		{
			if (implementation == null)
			{
				throw new ArgumentNullException("implementation");
			}

			if (!implementation.GetType().IsCOMObject)
			{
				throw new InvalidCastException("Wrapper only supports COM-objects.");
			}

			this.implementation = implementation;
		}

		/// <summary>
		/// Gets the wrapped object.
		/// </summary>
		protected object Wrapped
		{
			get
			{
				if (this.IsDisposed())
				{
					throw new ObjectDisposedException(this.GetType().FullName);
				}

				return this.implementation;
			}
		}

		public virtual void Dispose()
		{
			if (!this.IsDisposed())
			{
				ComHelper.Release(this.implementation);
				this.implementation = null;
			}
		}

		/// <summary>
		/// Invoked the method with the specified methodName on the wrapped COM object.
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="args"></param>
		/// <returns></returns>

		public object InvokeMethod(string methodName, params object[] args)
		{
			return ComHelper.InvokeMethod(this.Wrapped, methodName, args);
		}

		protected bool IsDisposed()
		{
			return this.implementation == null;
		}

	}
}