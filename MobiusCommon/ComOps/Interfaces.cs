using System;
using System.Collections.Generic;
using System.Text;

namespace Mobius.ComOps
{

// General use simple delegates

	public delegate void SimpleDelegate(); // simple delegate with no parameters and no return value

	public delegate void ObjectParmDelegate(object arg); // simple delegate with a single object parameter

	public delegate object ObjectObjectDelegate(object arg); // simple delegate with a single object parameter and object return value


	/// <summary>
	/// Allow calls into SessionManager
	/// </summary>

	public interface ISessionManager
	{
		void DisplayStartupMessage(string msg);
	}

	/// <summary>
	/// Allow calls to ServerFile methods from both the client & services
	/// </summary>

	public interface IServerFile
	{
		string ReadAll(string fileName);
	}

}
