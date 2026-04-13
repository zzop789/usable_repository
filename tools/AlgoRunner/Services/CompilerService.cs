using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AlgoRunner.Services
{
    public enum CompilerType { GppMinGW, Msvc, None }

    public class CompilerInfo
    {
        public CompilerType Type    { get; set; }
        public string       Path    { get; set; } = "";
        public string       Version { get; set; } = "";

        public string DisplayName => Type switch
        {
            CompilerType.GppMinGW => string.IsNullOrWhiteSpace(Version) ? "g++" : $"g++ {Version}",
            CompilerType.Msvc     => string.IsNullOrWhiteSpace(Version) ? "cl.exe" : $"cl.exe {Version}",
            _                     => "未检测到编译器"
        };
    }

    public class CompilerService
    {
        // 常见 MinGW / MSYS2 安装位置
        private static readonly string[] KnownGppPaths =
        [
            @"C:\msys64\mingw64\bin\g++.exe",
            @"C:\msys64\ucrt64\bin\g++.exe",
            @"C:\msys64\clang64\bin\g++.exe",
            @"C:\mingw64\bin\g++.exe",
            @"C:\mingw32\bin\g++.exe",
            @"C:\TDM-GCC-64\bin\g++.exe",
        ];

        private static readonly string VsWherePath =
            @"C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe";

        public CompilerInfo  DetectedCompiler { get; private set; } = new() { Type = CompilerType.None };
        private CompilerInfo? _override;

        public CompilerInfo ActiveCompiler => _override ?? DetectedCompiler;

        // ── Detection ────────────────────────────────────────────────────────────

        public void Detect()
        {
            // 1. g++ from PATH
            var fromPath = TryWhich("g++");
            if (fromPath is not null)
            {
                DetectedCompiler = new CompilerInfo
                {
                    Type = CompilerType.GppMinGW,
                    Path = fromPath
                };
                return;
            }

            // 2. g++ at known locations
            foreach (var p in KnownGppPaths)
            {
                if (File.Exists(p))
                {
                    DetectedCompiler = new CompilerInfo
                    {
                        Type = CompilerType.GppMinGW,
                        Path = p
                    };
                    return;
                }
            }

            // 3. MSVC via vswhere
            var cl = FindClViaVsWhere();
            if (cl is not null)
            {
                DetectedCompiler = new CompilerInfo
                {
                    Type = CompilerType.Msvc,
                    Path = cl
                };
                return;
            }

            DetectedCompiler = new CompilerInfo { Type = CompilerType.None };
        }

        public void SetOverride(string compilerPath)
        {
            if (compilerPath.EndsWith("g++.exe", StringComparison.OrdinalIgnoreCase))
                _override = new CompilerInfo
                {
                    Type = CompilerType.GppMinGW,
                    Path = compilerPath
                };
            else if (compilerPath.EndsWith("cl.exe", StringComparison.OrdinalIgnoreCase))
                _override = new CompilerInfo
                {
                    Type = CompilerType.Msvc,
                    Path = compilerPath
                };
        }

        public void ClearOverride() => _override = null;

        // ── Compilation ──────────────────────────────────────────────────────────

        public async Task<(bool success, string output)> CompileAsync(
            string sourceFile,
            string outputDir,
            IProgress<string>? progress = null)
        {
            var compiler = ActiveCompiler;
            if (compiler.Type == CompilerType.None)
            {
                const string msg = "错误: 未找到 C++ 编译器。\n请在 GUI 右上角点击 [...] 手动指定编译器路径。";
                progress?.Report(msg);
                return (false, msg);
            }

            Directory.CreateDirectory(outputDir);
            var projectSources = ResolveProjectSources(sourceFile);
            var exePath        = GetOutputExePath(sourceFile, outputDir);

            if (projectSources.Count == 0)
            {
                var msg = "错误: 未找到可编译的 .cpp 源文件。";
                progress?.Report(msg);
                return (false, msg);
            }

            progress?.Report(projectSources.Count == 1
                ? $"构建模式: 单文件 ({Path.GetFileName(projectSources[0])})"
                : $"构建模式: 目录项目 ({projectSources.Count} 个源文件)");

            ProcessStartInfo psi;

            if (compiler.Type == CompilerType.GppMinGW)
            {
                var sourceArgs = string.Join(" ", projectSources.Select(s => $"\"{s}\""));
                psi = new ProcessStartInfo(compiler.Path)
                {
                    Arguments = $"-g -std=c++17 -Wall {sourceArgs} -o \"{exePath}\""
                };
            }
            else
            {
                // MSVC: wrap in a bat that calls vcvars64 first
                var clDir = System.IO.Path.GetDirectoryName(compiler.Path)!;
                // cl.exe: ...\VC\Tools\MSVC\<ver>\bin\Hostx64\x64\cl.exe
                // VC root: go up 6 levels from cl.exe directory, then Auxiliary\Build\vcvars64.bat
                var vcRoot = System.IO.Path.GetFullPath(
                    System.IO.Path.Combine(clDir, @"..\..\..\..\..\.."));
                var vcVars = System.IO.Path.Combine(vcRoot, @"Auxiliary\Build\vcvars64.bat");

                if (!File.Exists(vcVars))
                {
                    var msg = $"错误: 未找到 vcvars64.bat: {vcVars}";
                    progress?.Report(msg);
                    return (false, msg);
                }

                var tmpBat = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "algorunner_build.bat");
                var clSources = string.Join(" ", projectSources.Select(s => $"\"{s}\""));
                File.WriteAllText(tmpBat,
                    $"@echo off\r\n" +
                    $"call \"{vcVars}\" >nul 2>&1\r\n" +
                    $"cl.exe /nologo /Zi /std:c++17 /EHsc /utf-8 {clSources} /Fe:\"{exePath}\"\r\n" +
                    $"exit /b %errorlevel%\r\n",
                    Encoding.ASCII);

                psi = new ProcessStartInfo("cmd.exe") { Arguments = $"/c \"{tmpBat}\"" };
            }

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError  = true;
            psi.UseShellExecute        = false;
            psi.CreateNoWindow         = true;

            var sb   = new StringBuilder();
            using var proc = new Process { StartInfo = psi };

            proc.OutputDataReceived += (_, e) =>
            {
                if (e.Data is null) return;
                sb.AppendLine(e.Data);
                progress?.Report("[out] " + e.Data);
            };
            proc.ErrorDataReceived += (_, e) =>
            {
                if (e.Data is null) return;
                sb.AppendLine(e.Data);
                progress?.Report("[err] " + e.Data);
            };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            await proc.WaitForExitAsync();

            return (proc.ExitCode == 0, sb.ToString());
        }

        public string GetOutputExePath(string sourceFile, string outputDir) =>
            System.IO.Path.Combine(outputDir, GetOutputExeName(sourceFile));

        private static string GetOutputExeName(string sourceFile)
        {
            var sourceDir = Path.GetDirectoryName(sourceFile)!;
            var mainCpp   = Path.Combine(sourceDir, "main.cpp");

            // 目录下存在 main.cpp 时，按“一个算法一个文件夹”输出：<目录名>.exe
            if (File.Exists(mainCpp))
                return new DirectoryInfo(sourceDir).Name + ".exe";

            // 否则保持单文件模式：<文件名>.exe
            return Path.GetFileNameWithoutExtension(sourceFile) + ".exe";
        }

        private static List<string> ResolveProjectSources(string sourceFile)
        {
            var sourceDir = Path.GetDirectoryName(sourceFile)!;
            var mainCpp   = Path.Combine(sourceDir, "main.cpp");

            // 约定：目录含 main.cpp => 此目录为一个算法项目，编译该目录全部 .cpp
            if (File.Exists(mainCpp))
            {
                return Directory.GetFiles(sourceDir, "*.cpp", SearchOption.TopDirectoryOnly)
                    .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            // 兼容旧结构：单文件即项目
            return [sourceFile];
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static string? TryWhich(string exe)
        {
            try
            {
                // Use where.exe (always present on Windows)
                var psi = new ProcessStartInfo("where.exe", exe)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true
                };
                using var p = Process.Start(psi)!;
                var line = p.StandardOutput.ReadLine()?.Trim();
                p.WaitForExit();
                return (p.ExitCode == 0 && !string.IsNullOrWhiteSpace(line)) ? line : null;
            }
            catch { return null; }
        }

        private static string? FindClViaVsWhere()
        {
            if (!File.Exists(VsWherePath)) return null;
            try
            {
                var psi = new ProcessStartInfo(VsWherePath,
                    "-latest -requires Microsoft.VisualCpp.Tools.HostX64.TargetX64 " +
                    @"-find VC\Tools\MSVC\**\bin\HostX64\x64\cl.exe")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true
                };
                using var p = Process.Start(psi)!;
                var path = p.StandardOutput.ReadLine()?.Trim();
                p.WaitForExit();
                return string.IsNullOrWhiteSpace(path) ? null : path;
            }
            catch { return null; }
        }
    }
}
