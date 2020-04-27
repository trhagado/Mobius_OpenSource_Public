using System;
using System.Text;

namespace Mobius.ComOps
{
	/// <summary>
	/// Summary description for XlEncrypt.
	/// </summary>
	public class EncryptMx
	{
		public EncryptMx()
		{
		}

/// <summary>
/// Get public & private keys for encryption   
/// </summary>
/// <param name="publicKey"></param>
/// <param name="privateKey"></param>

		public static void GetKeys (
			out string publicKey,
			out string privateKey)
	{
		publicKey = (TimeOfDay.Milliseconds() % 256).ToString();
		privateKey = publicKey;
	}


		/// <summary>
		/// Encrypt message 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="publicKey"></param>
		/// <returns></returns>

		public static string Encrypt (
			string message,
			string publicKey)
		{
			byte bKey = Byte.Parse(publicKey);

			StringBuilder sb = new StringBuilder();
			for (int i1=0; i1<message.Length; i1++)
			{
				char c = message[i1];
				byte b = (byte)c;
				b = (byte)(b ^ bKey);
				c = (char)b;
				sb.Append(c);
			}
			return sb.ToString();
		}

/// <summary>
/// Decrypt message
/// </summary>
/// <param name="message"></param>
/// <param name="privateKey"></param>
/// <returns></returns>

		public static string Decrypt (
			string message,
			string privateKey)
		{
			return Encrypt(message,privateKey);
		}


	}
}
