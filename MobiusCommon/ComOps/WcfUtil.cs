using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;

namespace Mobius.ComOps
{
	public class WcfUtil
	{

		public static string BaseAddress = null; // base address to connect to

/// <summary>
/// Get standard NetTcpBinding
/// </summary>

		public static NetTcpBinding TcpBinding
		{
			get
			{
				NetTcpBinding b = new NetTcpBinding();

				b.MaxBufferPoolSize = 2147483647;
				b.MaxReceivedMessageSize = 2147483647;
				b.MaxBufferSize = 2147483647;
				b.ReaderQuotas.MaxStringContentLength = 2147483647;
				b.ReaderQuotas.MaxDepth = 2147483647;
				b.ReaderQuotas.MaxBytesPerRead = 2147483647;
				b.ReaderQuotas.MaxNameTableCharCount = 2147483647;
				b.ReaderQuotas.MaxArrayLength = 2147483647;
				b.OpenTimeout = new TimeSpan(23, 59, 0);
				b.SendTimeout = new TimeSpan(23, 59, 0);
				b.ReceiveTimeout = new TimeSpan(23, 59, 0);

				b.ReliableSession.Ordered = true;
				b.ReliableSession.InactivityTimeout = new TimeSpan(23, 59, 0);
				b.ReliableSession.Enabled = false;

				b.Security.Mode = SecurityMode.None;
				b.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
				b.Security.Message.ClientCredentialType = MessageCredentialType.None;
				//			b.Security.Message.EstablishSecurityContext = false; // only for WSHttpBinding

				return b;
			}
		}

/// <summary>
/// Build an endpoint address from the BaseAddress and the service name
/// </summary>
/// <param name="serviceName"></param>
/// <returns></returns>

		public static EndpointAddress Epa(
			string serviceName)
		{
			return Epa(BaseAddress, serviceName);
		}

		/// <summary>
		/// Build an endpoint address from the BaseAddress and the service name
		/// </summary>
		/// <param name="serviceName"></param>
		/// <returns></returns>

		public static EndpointAddress Epa(
			string baseAddress,
			string serviceName)
		{
			string uriString = baseAddress + "/" + serviceName;
			EndpointAddress epa = new EndpointAddress(uriString);
			return epa;
		}

	}

	/// <summary>
	/// Additional endpoint to be configured
	/// </summary>

	public class EndpointConfig
	{
		public Type ServiceType;
		public Type ContractType;
		public string BindingType;
		public int Port;

		public EndpointConfig(
			Type serviceType,
			Type contractType)
		{
			ServiceType = serviceType;
			ContractType = contractType;
			return;
		}

		public EndpointConfig(
			Type serviceType,
			Type contractType,
			string bindingType,
			int port)
		{
			ServiceType = serviceType;
			ContractType = contractType;
			BindingType = bindingType;
			Port = port;
			return;
		}
	}
}
