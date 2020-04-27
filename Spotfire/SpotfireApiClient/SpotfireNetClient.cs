using Mobius.ComOps;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace Mobius.SpotfireClient
{
	/// <summary>
	/// Methods to send and receive messages over the network to/from the SpotfireNetServices
	/// </summary>

	public class SpotfireNetClient
	{
		public static IServerMessage ISM = null; // instance of class to call to interact with client

		public static void SendMessage(string msg)
		{
			ISM.SendToClient(msg);
		}

		/// <summary>
		/// Open hosts for services to requests can be received
		/// </summary>

		public static int OpenServiceHosts(
			int port)
		{
			return ISM.OpenServiceHosts(port);
		}

		/// <summary>
		/// Send binary message to client
		/// </summary>
		/// <param name="msg"></param>

		public static void SendToClientBinary(byte[] msg)
		{
			ISM.SendToClientBinary(msg);
		}

		/// <summary>
		/// Return true if a message has been received and is available in the queue
		/// </summary>
		/// <returns></returns>

		public static bool ReceivedMessageIsAvailable()
		{
			return ISM.ReceivedMessageIsAvailable();
		}

		/// <summary>
		/// Receive message from client
		/// </summary>
		/// <param name="msg"></param>

		public static string ReceiveMessage()
		{
			return ISM.ReceiveFromClient();
		}

		/// <summary>
		/// Receive binary message from client
		/// </summary>
		/// <param name="msg"></param>

		public static byte[] ReceiveFromClientBinary()
		{
			return ISM.ReceiveFromClientBinary();
		}
	}

	/// <summary>
	/// Interface to methods to send and receive messages from graphics client
	/// </summary>

	public interface IServerMessage
	{

		/// <summary>
		/// Open hosts for services to requests can be received
		/// </summary>

		int OpenServiceHosts(int port);

		/// <summary>
		/// Send message to client
		/// </summary>
		/// <param name="msg"></param>

		void SendToClient(string msg);

		/// <summary>
		/// Send binary message to client
		/// </summary>
		/// <param name="msg"></param>

		void SendToClientBinary(byte[] msg);

		/// <summary>
		/// Return true if a message has been received and is available in the queue
		/// </summary>
		/// <returns></returns>

		bool ReceivedMessageIsAvailable();

		/// <summary>
		/// Receive message from client
		/// </summary>
		/// <param name="msg"></param>

		string ReceiveFromClient();

		/// <summary>
		/// Receive binary message from client
		/// </summary>
		/// <param name="msg"></param>

		byte[] ReceiveFromClientBinary();
	}
}
