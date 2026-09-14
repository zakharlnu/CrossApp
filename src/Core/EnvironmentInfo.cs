using System.Runtime.InteropServices;

namespace Core;

public static class EnvironmentInfo
{
    public static EnvironmentReport Collect()
    {
        #if NET10_0_OR_GREATER
            const string buildNote = "збірка під .NET 10.0";
        #else
            const string buildNote = "збірка під .NET 8.0";
        #endif
        return new(
            OsDescription: RuntimeInformation.OSDescription,
            FrameworkDescription: RuntimeInformation.FrameworkDescription,
            ProcessArchitecture: RuntimeInformation.ProcessArchitecture.ToString(),
            DetectedRid: DetectRid(),
            ReportedRid: RuntimeInformation.RuntimeIdentifier,
            BaseDirectory: AppContext.BaseDirectory,
            BuildNote: buildNote);
    }

    private static string DetectRid()
    {
        string os =
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" :
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" : "unknown";

        string arch = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "x64",
                Architecture.X86 => "x86",
                Architecture.Arm64 => "arm64",
                Architecture.Arm => "arm",
                _ => "unknown"
            };
        
        return $"{os}-{arch}";
    }
}