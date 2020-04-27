using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Mobius.ComOps
{
	public class MemoryInfo
	{
		[DllImport("KERNEL32.DLL")]
		private static extern int OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);
		[DllImport("KERNEL32.DLL")]
		private static extern int CloseHandle(int handle);

		[StructLayout(LayoutKind.Sequential)]
		private class PROCESS_MEMORY_COUNTERS
		{
			public int cb;
			public int PageFaultCount;
			public int PeakWorkingSetSize;
			public int WorkingSetSize;
			public int QuotaPeakPagedPoolUsage;
			public int QuotaPagedPoolUsage;
			public int QuotaPeakNonPagedPoolUsage;
			public int QuotaNonPagedPoolUsage;
			public int PagefileUsage;
			public int PeakPagefileUsage;
		}

		[DllImport("psapi.dll")]
		private static extern int GetProcessMemoryInfo(int hProcess, [Out] PROCESS_MEMORY_COUNTERS counters, int size);

		public static long GetMemoryUsageForProcess(long pid)
		{
			long mem = 0;
			int pHandle = OpenProcess(0x0400 | 0x0010, 0, (uint)pid);
			try
			{
				var pmc = new PROCESS_MEMORY_COUNTERS();
				if (GetProcessMemoryInfo(pHandle, pmc, 40) != 0)
					mem = pmc.WorkingSetSize;
			}
			finally
			{
				CloseHandle(pHandle);
			}
			return mem;
		}

		public static string ToString(long numOfBytes)
		{
			double bytes = numOfBytes;
			if (bytes < 1024)
				return bytes.ToString();

			bytes /= 1024;
			if (bytes < 1024)
			{
				return bytes.ToString("#.# KB");
			}
			bytes /= 1024;
			if (bytes < 1024)
			{
				return bytes.ToString("#.# MB");
			}
			bytes /= 1024;
			return bytes.ToString("#.# GB");
		}

		public static string GetFormattedMemoryUsageForProcess(long pid)
		{
			return ToString(GetMemoryUsageForProcess(pid));
		}
	}
}
