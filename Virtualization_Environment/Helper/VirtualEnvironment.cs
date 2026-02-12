using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Virtualization_Environment.Enums;
using Virtualization_Environment.Structs;
using static Virtualization_Environment.Api.Kernel32Wrapper;

namespace Virtualization_Environment.Helper
{
    public class VirtualEnvironment
    {
        public static void RunInSandbox(string exePath, string workingDir)
        {
            try
            {
                // Tạo Job Object mới
                IntPtr job = CreateJobObject(IntPtr.Zero, Application.ProductName);
                if (job == IntPtr.Zero)
                    throw new Exception("Failed to create Job Object");

                // Cấu hình giới hạn cho Job Object
                JOBOBJECT_EXTENDED_LIMIT_INFORMATION extendedLimits = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
                {
                    BasicLimitInformation = new JOBOBJECT_BASIC_LIMIT_INFORMATION
                    {
                        LimitFlags = JobObjectLimitFlags.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE,
                            //| JobObjectLimitFlags.JOB_OBJECT_LIMIT_PROCESS_MEMORY
                            //| JobObjectLimitFlags.JOB_OBJECT_LIMIT_ACTIVE_PROCESS
                            //| JobObjectLimitFlags.JOB_OBJECT_LIMIT_AFFINITY, // Thêm JOB_OBJECT_LIMIT_AFFINITY

                        
                        ActiveProcessLimit = 1 // Giới hạn 1 tiến trình đang hoạt động
                    },
                    ProcessMemoryLimit = (UIntPtr)(100 * 1024 * 1024), // Giới hạn bộ nhớ 100MB
                };

                // Lấy kích thước chính xác của cấu trúc
                int length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
                IntPtr limitPtr = Marshal.AllocHGlobal(length);
                Marshal.StructureToPtr(extendedLimits, limitPtr, false);

                // Thiết lập Job Object với giới hạn đã cấu hình
                if (!SetInformationJobObject(job, JobObjectInfoType.ExtendedLimitInformation, limitPtr, (uint)length))
                {
                    int error = Marshal.GetLastWin32Error();
                    Debug.WriteLine($"Structure size: {length}");
                    Marshal.FreeHGlobal(limitPtr);
                    throw new Exception($"Failed to set Job Object limits. Error code: {error}");
                }

                Marshal.FreeHGlobal(limitPtr);

                // Khởi tạo thông tin tiến trình
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = false,
                    WorkingDirectory = workingDir,
                   
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    // CreateNoWindow = true // Bạn có thể đặt false nếu không muốn hiển thị cửa sổ
                };

             
                // Khởi động tiến trình
                Process process = Process.Start(processStartInfo);
                SymbolicLink(process.StandardOutput.ReadToEnd(), workingDir);

                if (process == null)
                    throw new Exception("Failed to start process");

                // Gán tiến trình vào Job Object
                if (!AssignProcessToJobObject(job, process.Handle))
                    throw new Exception("Failed to assign process to Job Object");

                // Chờ tiến trình kết thúc
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        public static void SymbolicLink(string linkDirectory, string targetDirectory)
        {
            if (!CreateSymbolicLink(linkDirectory, targetDirectory, 0))
                throw new Exception($"Failed to create symbolic link. Error code: {Marshal.GetLastWin32Error()}");
        }

    }
}
