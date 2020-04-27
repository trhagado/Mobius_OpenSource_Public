using java.util;
using java.util.zip;
// Note: The .Net J# module vjsnativ.dll must be copied to the bin directory that the app is running in for .Net 4.0+ apps

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Mobius.ComOps
{
	public delegate Enumeration EnumerationMethod();
	public delegate bool FilterEntryMethod(ZipEntry e);

/// <summary>
/// ZipFile utilities 
/// </summary>

	public class ZipUtils
	{

		/// <summary>
		/// Extract the first file from a Zip file in the form of a byte array
		/// </summary>
		/// <param name="ba"></param>
		/// <param name="outputFile"></param>

		public static void ExtractFirstFile(
			byte[] ba,
			string outputFile)
		{
			string tempZipFile = TempFile.GetTempFileName(); // temp file for zip file
			FileStream fs = new FileStream(tempZipFile, FileMode.Create);
			fs.Write(ba, 0, ba.Length);
			fs.Close();

			ZipFile currentFile = new ZipFile(tempZipFile);
			foreach (ZipEntry entry in new EnumerationAdapter(new EnumerationMethod(currentFile.entries)))
			{ // extract first file from the .zip file
				if (entry.isDirectory()) continue;
				java.io.InputStream s = currentFile.getInputStream(entry);
				try
				{
					java.io.FileOutputStream dest = new java.io.FileOutputStream(outputFile);
					try
					{
						ZipUtils.CopyStream(s, dest);
					}
					finally
					{
						dest.close();
					}
				}
				finally
				{
					s.close();
					FileUtil.DeleteFile(tempZipFile);
				}

				break;
			}

		}

		/// <summary>
		/// CopyStream
		/// </summary>

		public static void CopyStream(java.io.InputStream from, java.io.OutputStream to)
		{
			sbyte[] buffer = new sbyte[8192];
			int got;
			while ((got = from.read(buffer, 0, buffer.Length)) > 0)
				to.write(buffer, 0, got);
		}

		/// <summary>
		/// Extract the content of a zip file passed as a Base64 string
		/// </summary>
		/// <param name="b64String"></param>
		/// <param name="path"></param>
		/// <param name="filter"></param>

		public static void ExtractZipFileFromBase64String(
			string b64String,
			string path,
			FilterEntryMethod filter = null)
		{
			string tempZipFile = TempFile.GetTempFileName("zip"); // temp file for zip file
			FileUtil.WriteFileFromBase64String(b64String, tempZipFile);

			ZipFile zf = new ZipFile(tempZipFile);

			ExtractZipFile(zf, path, filter);

			FileUtil.DeleteFile(tempZipFile);
			return;
		}

		/// <summary>
		/// Extract files from a ZipFile to a file folder
		/// </summary>
		/// <param name="file"></param>
		/// <param name="path"></param>
		/// <param name="filter"></param>

		public static void ExtractZipFile(
			ZipFile file, 
			string path, 
			FilterEntryMethod filter = null)
		{
			foreach (ZipEntry entry in new EnumerationAdapter(new EnumerationMethod(file.entries)))
			{
				if (!entry.isDirectory())
				{
					if ((filter == null || filter(entry)))
					{
						java.io.InputStream s = file.getInputStream(entry);
						try
						{
							string fname = System.IO.Path.GetFileName(entry.getName());
							string newpath = System.IO.Path.Combine(path, System.IO.Path.GetDirectoryName(entry.getName()));

							System.IO.Directory.CreateDirectory(newpath);

							java.io.FileOutputStream dest = new java.io.FileOutputStream(System.IO.Path.Combine(newpath, fname));
							try
							{
								CopyStream(s, dest);
							}
							finally
							{
								dest.close();
							}
						}
						finally
						{
							s.close();
						}
					}
				}
			}
		}

		public static ZipFile CreateEmptyZipFile(string fileName)
		{
			new ZipOutputStream(new java.io.FileOutputStream(fileName)).close();
			return new ZipFile(fileName);
		}

		public static ZipFile UpdateZipFile(ZipFile file, FilterEntryMethod filter, string[] newFiles)
		{
			string prev = file.getName();
			string tmp = System.IO.Path.GetTempFileName();
			ZipOutputStream to = new ZipOutputStream(new java.io.FileOutputStream(tmp));
			try
			{
				CopyEntries(file, to, filter);
				// add entries here
				if (newFiles != null)
				{
					foreach (string f in newFiles)
					{
						ZipEntry z = new ZipEntry(f.Remove(0, System.IO.Path.GetPathRoot(f).Length));
						z.setMethod(ZipEntry.DEFLATED);
						to.putNextEntry(z);
						try
						{
							java.io.FileInputStream s = new java.io.FileInputStream(f);
							try
							{
								CopyStream(s, to);
							}
							finally
							{
								s.close();
							}
						}
						finally
						{
							to.closeEntry();
						}
					}
				}
			}
			finally
			{
				to.close();
			}
			file.close();

			// now replace the old file with the new one
			System.IO.File.Copy(tmp, prev, true);
			System.IO.File.Delete(tmp);

			return new ZipFile(prev);
		}

		public static void CopyEntries(ZipFile from, ZipOutputStream to)
		{
			CopyEntries(from, to, null);
		}

		public static void CopyEntries(ZipFile from, ZipOutputStream to, FilterEntryMethod filter)
		{
			foreach (ZipEntry entry in new EnumerationAdapter(new EnumerationMethod(from.entries)))
			{
				if (filter == null || filter(entry))
				{
					java.io.InputStream s = from.getInputStream(entry);
					try
					{
						to.putNextEntry(entry);
						try
						{
							CopyStream(s, to);
						}
						finally
						{
							to.closeEntry();
						}
					}
					finally
					{
						s.close();
					}
				}
			}
		}
	}

	/// <summary>
	/// Wraps java enumerators 
	/// </summary>

	public class EnumerationAdapter : IEnumerable
	{
		private class EnumerationWrapper : IEnumerator
		{
			private EnumerationMethod m_Method;
			private Enumeration m_Wrapped;
			private object m_Current;

			public EnumerationWrapper(EnumerationMethod method)
			{
				m_Method = method;
			}

			// IEnumerator
			public object Current
			{
				get { return m_Current; }
			}

			public void Reset()
			{
				m_Wrapped = m_Method();
				if (m_Wrapped == null)
					throw new InvalidOperationException();
			}

			public bool MoveNext()
			{
				if (m_Wrapped == null)
					Reset();
				bool Result = m_Wrapped.hasMoreElements();
				if (Result)
					m_Current = m_Wrapped.nextElement();
				return Result;
			}
		}

		private EnumerationMethod m_Method;

		public EnumerationAdapter(EnumerationMethod method)
		{
			if (method == null)
				throw new ArgumentException();
			m_Method = method;
		}

		// IEnumerable
		public IEnumerator GetEnumerator()
		{
			return new EnumerationWrapper(m_Method);
		}
	}

}
