using System;
using System.Runtime.InteropServices;
using System.Text;


namespace SysInfoWin
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll")]
        static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint GetLogicalDriveStrings(uint nBufferLength, [Out] StringBuilder lpBuffer);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
            out ulong lpFreeBytesAvailable,
            out ulong lpTotalNumberOfBytes,
            out ulong lpTotalNumberOfFreeBytes);

        [DllImport("ntdll.dll")]
        static extern int RtlGetVersion(ref OSVERSIONINFOEX versionInfo);

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_INFO
        {
            public ushort ProcessorArchitecture;  
            public ushort wReserved;               
            public uint dwPageSize;                
            public IntPtr lpMinimumApplicationAddress; 
            public IntPtr lpMaximumApplicationAddress;  
            public IntPtr ActiveProcessorMask;        
            public uint NumberOfProcessors;          
            public uint dwProcessorType;       
            public uint dwAllocationGranularity;   
            public ushort wProcessorLevel;        
            public ushort wProcessorRevision;    
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORYSTATUSEX
        {
            public uint dwLength;    
            public uint dwMemoryLoad; 
            public ulong ullTotalPhys;  
            public ulong ullAvailPhys; 
            public ulong ullTotalPageFile;  
            public ulong ullAvailPageFile;   
            public ulong ullTotalVirtual;   
            public ulong ullAvailVirtual;     
            public ulong ullAvailExtendedVirtual; 
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct OSVERSIONINFOEX
        {
            public uint OSVersionInfoSize;  
            public uint dwMajorVersion; 
            public uint dwMinorVersion; 
            public uint dwBuildNumber; 
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]  
            public string szCSDVersion;  
        }

     
        static int CountProcessorsFromMask(IntPtr mask)
        {
            int count = 0;
            long longMask = mask.ToInt64();

            while (longMask != 0)
            {
               
                if ((longMask & 1) == 1)
                    count++;
          
                longMask /= 2;

            }

            return count;
        }

        static void Main()
        {
            try
            {
                GetOSVersion();
                GetComputerAndUserInfo();
                GetArchitectureAndProcessors();
                GetMemoryInfo();
                GetDriveInfo();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void GetOSVersion()
        {
            try
            {
                OSVERSIONINFOEX osVersion = new OSVERSIONINFOEX();
              
                osVersion.OSVersionInfoSize = (uint)Marshal.SizeOf(typeof(OSVERSIONINFOEX));

            
                if (RtlGetVersion(ref osVersion) == 0 && osVersion.dwMajorVersion >= 10)
                {
                    Console.WriteLine("OS: Windows 10 or Greater");
                }
                else
                {
                    Console.WriteLine("OS: Windows (Version detection failed)");
                }
            }
            catch
            {
                Console.WriteLine("OS: Windows (Version detection failed)");
            }
        }

        static void GetComputerAndUserInfo()
        {
            Console.WriteLine($"Computer Name: {Environment.MachineName}");
            Console.WriteLine($"User: {Environment.UserName}");
        }



        static void GetArchitectureAndProcessors()
        {
            try
            {
                SYSTEM_INFO sysInfo = new SYSTEM_INFO();
         
                GetNativeSystemInfo(ref sysInfo);

                string architecture;
                switch (sysInfo.ProcessorArchitecture)
                {
                    case 0:
                        architecture = "x86";
                        break;
                    case 9:
                        architecture = "x64 (AMD64)";
                        break;
                    case 5:
                        architecture = "ARM";
                        break;
                    case 12:
                        architecture = "ARM64";
                        break;
                    default:
                        architecture = "Unknown";
                        break;
                }

             
                int processorCount = CountProcessorsFromMask(sysInfo.ActiveProcessorMask);

                Console.WriteLine($"Architecture: {architecture}");
                Console.WriteLine($"Processors: {processorCount}");
            }
            catch
            {
                Console.WriteLine("Architecture: Unknown");
                Console.WriteLine("Processors: Unknown");
            }
        }

    
        static void GetMemoryInfo()
        {
            try
            {
                MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
           
                memStatus.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));

                if (GlobalMemoryStatusEx(ref memStatus))
                {
                   
      
                    ulong totalRAM = memStatus.ullTotalPhys / (1024 * 1024);
                    ulong usedRAM = totalRAM - (memStatus.ullAvailPhys / (1024 * 1024));

                    
                    ulong totalVirtual = memStatus.ullTotalVirtual / (1024 * 1024);


                    ulong totalPageFile = memStatus.ullTotalPageFile / (1024 * 1024);
                    ulong usedPageFile = totalPageFile - (memStatus.ullAvailPageFile / (1024 * 1024));

                    Console.WriteLine($"RAM: {usedRAM}MB / {totalRAM}MB");
                    Console.WriteLine($"Virtual Memory: {totalVirtual}MB");
                    Console.WriteLine($"Memory Load: {memStatus.dwMemoryLoad}%");
                    Console.WriteLine($"Pagefile: {usedPageFile}MB / {totalPageFile}MB");
                }
                else
                {
                    Console.WriteLine("Memory Info: Failed to retrieve memory status");
                }
            }
            catch
            {
                Console.WriteLine("Memory Info: Failed to retrieve memory status");
            }
        }

    
        static void GetDriveInfo()
        {
            try
            {
                Console.WriteLine("Drives:");

        
                uint bufferLength = 256;
            
                StringBuilder driveBuffer = new StringBuilder((int)bufferLength);
                // Получение списка дисков, функция возвращает строку с дисками в формате "C:\<null>D:\<null>E:\<null><null>"
                uint result = GetLogicalDriveStrings(bufferLength, driveBuffer);

                if (result > 0)
                {
                    foreach (string drive in driveBuffer.ToString().Split('\0'))
                    {
                        if (string.IsNullOrEmpty(drive))
                        { 
                            continue; 
                        }

                        try
                        {
                            if (GetDiskFreeSpaceEx(drive, out ulong freeBytes, out ulong totalBytes, out _))
                            {
                                // 1073741824 = (1024*1024*1024) байтов в гб
                                double freeGB = Math.Round(freeBytes / 1073741824.0, 0);
                                double totalGB = Math.Round(totalBytes / 1073741824.0, 0);
                                Console.WriteLine($"  - {drive}  (NTFS): {freeGB} GB free / {totalGB} GB total");
                            }
                        }
                        catch { }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Drives: Failed to get drive info");
            }
        }
    }
}
