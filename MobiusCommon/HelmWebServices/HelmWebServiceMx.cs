using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

using Mobius.ComOps;

namespace Mobius.Helm
{
	/// <summary>
	/// Mobius wrapper for HelmWebServiceClient built by NSwagStudio on Oct-5-2019
	/// from URL: http://[server]:8080/WebService/service/swagger.json
	/// This wrapper class provides additionaly method for the service that
	/// make it easier to use the service in the Mobius context.
	/// </summary>

	public class HelmWebServiceMx
	{
		public HelmWebService WSC = null; // the wrapped HelmWebServiceClient
		public HttpClient HttpClient = null; // associated HttpClient

		public static string HelmServiceBaseUrl = "http://[server]:8080/WebService/service";

		public HelmWebServiceMx()
		{
			HttpClientHandler handler = new HttpClientHandler { Credentials = System.Net.CredentialCache.DefaultNetworkCredentials };
			HttpClient = new HttpClient(handler);
			WSC = new HelmWebService(HelmServiceBaseUrl, HttpClient);
			return;
		}

		public string ConvertIntoPeptideAnalogSequence(string hELMNotation)
		{
			Task<string> task = WSC.ConvertIntoPEPTIDEAnalogSequencePostAsync(hELMNotation);
			return GetStringResult(task, "Sequence"); // PEPTIDE1{A.L.C}$$$$ ===> {"Sequence":"ALC"}
		}

		public string ConvertIntoRNAAnalogSequence(string hELMNotation) 
		{
			Task<string> task = WSC.ConvertIntoRNAAnalogSequencePostAsync(hELMNotation);
			return GetStringResult(task, "Sequence"); // RNA1{R(U)P.R(T)P.R(G)P.R(C)P.R(A)}$$$$ ===> {"Sequence":"UTGCA"}
		}

		/// <summary>
		/// Converts HELM Input into Fasta
		/// </summary>
		/// <param name="hELMNotation"></param>
		/// <returns></returns>

		public string GenerateFastaFromHelm(string hELMNotation)
		{
			Task<string> task = WSC.GenerateFastaPostAsync(hELMNotation);
			return GetStringResult(task, "FastaFile"); // PEPTIDE1{A.L.C}$$$$ ===> {"HELMNotation":"PEPTIDE1{A.L.C}$$$$","FastaFile":">PEPTIDE1\nALC\n"}
		}

		/// <summary>
		/// Reads peptide Fasta-Sequence(s) and generates HELMNotation
		/// </summary>
		/// <param name="peptide"></param>
		/// <returns></returns>

		public string GenerateHELMFromFastaPeptide(string peptide)
		{
			peptide = Lex.AdjustEndOfLineCharacters(peptide, "\n");
			Task<string> task = WSC.GenerateHELMInputPostPEPTIDEAsync(peptide);
			return GetStringResult(task, "HELMNotation"); // ">PEPTIDE1\nALC\n" ===> {"HELMNotation":"PEPTIDE1{A.L.C}$$$$" }
		}

		/// <summary>
		/// Reads rna Fasta-Sequence(s) and generates HELMNotation
		/// </summary>
		/// <param name="rna"></param>
		/// <returns></returns>

		public string GenerateHELMFromFastaRNA(string rna)
		{
			rna = Lex.AdjustEndOfLineCharacters(rna, "\n");
			Task<string> task = WSC.GenerateHELMInputPostRNAAsync(rna);
			return GetStringResult(task, "HELMNotation"); //  ===> RNA1{R(U)P.R(T)P.R(G)P.R(C)P.R(A)}$$$$
		}

		/// <summary>
		/// Reads peptide sequence and generates HELMNotation
		/// </summary>
		/// <param name="peptideSeq"></param>
		/// <returns></returns>

		public string GenerateHELMFromPeptideSequence(string peptideSeq)
		{
			Task task = WSC.GenerateHELMPeptideAsync(peptideSeq);
			return GetStringResult((task as Task<string>), "HELMNotation"); // ALC ===> { "HELMNotation": "PEPTIDE1{A.L.C}$$$$V2.0" }
		}

		/// <summary>
		/// Reads rna sequence and generates HELMNotation
		/// </summary>
		/// <param name="rna"></param>
		/// <returns></returns>

		public string GenerateHELMFromRnaSequence(string rna)
		{
			Task task = WSC.GenerateHELMRNAAsync(rna);
			return GetStringResult((task as Task<string>), "HELMNotation"); // UTGCA ===> { "HELMNotation": "RNA1{R(U)P.R(T)P.R(G)P.R(C)P.R(A)}$$$$V2.0" }
		}

		public string GenerateSMILESForHELM(string helm)
		{
			Task<string> task = WSC.GenerateSMILESForHELMPostAsync(helm, CancellationToken.None);
			return GetStringResult(task, "SMILES"); // PEPTIDE1{A.L.C}$$$$ ===> { "SMILES": "C[C@H](N[H])C(=O)N[C@@H](CC(C)C)C(=O)N[C@H](C(=O)O)CS[H]", "HELMNotation": "PEPTIDE1{A.L.C}$$$$"
		}
		
		public void ValidateHelm(string helm)
		{
			Task task = null;

			try
			{
				task = WSC.ValidateInputHELMPostAsync(helm);
				task.Wait();
				return;
			}

			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
		}

		/// <summary>
		/// Get a string result
		/// </summary>
		///// <param name="task"></param>
		/// <returns></returns>

		string GetStringResult(
			Task<string> task,
			string propertyName)
		{
			JProperty jp = null;
			JObject jo = null;

			try
			{
				task.Wait();

				string json = task.Result;
				jo = JObject.Parse(json); // parse json (e.g. {"Sequence":"ALC"} or {"HELMNotation":"PEPTIDE1{A.L.C}$$$$","FastaFile":">PEPTIDE1\nALC\n"}) to JObject

				if (jo.ContainsKey(propertyName))
				{
					string value = (string)jo.GetValue(propertyName, StringComparison.OrdinalIgnoreCase);
					return value;
				}

				else throw new Exception("Property not found: " + propertyName);
			}

			catch (Exception ex)
			{
				string msg = ex.Message;
				return ""; 
			}
		}

		/// <summary>
		/// Get a string result from an untyped Task
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>

		string GetStringResult(   // obsolete, remove
			Task task,
			string propertyName)
		{
			task.Wait();

			Task<string> t2 = task as Task<string>;
			string json = t2.Result;
			JObject jo = JObject.Parse(json); // parse json (e.g. "{\"Sequence\":\"ALC\"}") to JObject
			JProperty jp = (JProperty)jo.Last; // jo.Last is the JProperty we want (e.g. {"Sequence":"ALC"} or {"HELMNotation":"PEPTIDE1{A.L.C}$$$$","FastaFile":">PEPTIDE1\nALC\n"})
			string value = (string)jp.Value; // JValue  (e.g. "ALC");
			return value;
		}

	}
}
