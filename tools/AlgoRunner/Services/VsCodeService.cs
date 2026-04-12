using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AlgoRunner.Services
{
    public class VsCodeService
    {
        private readonly string        _workspaceRoot;
        private readonly CompilerService _compiler;

        public VsCodeService(string workspaceRoot, CompilerService compiler)
        {
            _workspaceRoot = workspaceRoot;
            _compiler      = compiler;
        }

        /// <summary>
        /// 完成"VS Code 调试"全流程：
        ///  1. 用 CompilerService 编译（含调试符号）
        ///  2. 写入 .vscode/launch.json
        ///  3. 调用 code.cmd 打开工作区并定位到源文件
        /// </summary>
        public async Task<bool> OpenDebugSessionAsync(
            string sourceFile,
            string binDir,
            IProgress<string>? progress = null)
        {
            // Step 1 – compile
            progress?.Report("正在编译（调试模式）...");
            var (success, _) = await _compiler.CompileAsync(sourceFile, binDir,
                new Progress<string>(msg => progress?.Report(msg)));

            if (!success)
            {
                progress?.Report("编译失败，无法启动调试会话。");
                return false;
            }

            // Step 2 – write launch.json
            progress?.Report("正在更新 .vscode/launch.json...");
            WriteLaunchJson(sourceFile, binDir);

            // Step 3 – open VS Code
            progress?.Report("正在打开 VS Code...");
            OpenInVsCode(sourceFile);

            return true;
        }

        /// <summary>仅在 VS Code 中打开文件（不编译）</summary>
        public void OpenFile(string sourceFile) => OpenInVsCode(sourceFile);

        // ── Private ──────────────────────────────────────────────────────────────

        private void WriteLaunchJson(string sourceFile, string binDir)
        {
            var vscodeDir = Path.Combine(_workspaceRoot, ".vscode");
            Directory.CreateDirectory(vscodeDir);

            var exePath  = _compiler.GetOutputExePath(sourceFile, binDir);
            var exeRel   = "${workspaceFolder}/bin/" + Path.GetFileName(exePath);
            var isGpp    = _compiler.ActiveCompiler.Type == CompilerType.GppMinGW;
            var fileName = Path.GetFileName(sourceFile);

            object config;

            if (isGpp)
            {
                // gdb path lives next to g++
                var gdbPath = Path.Combine(
                    Path.GetDirectoryName(_compiler.ActiveCompiler.Path)!, "gdb.exe");

                config = new
                {
                    version        = "0.2.0",
                    configurations = new[]
                    {
                        new
                        {
                            name             = $"Debug: {fileName}",
                            type             = "cppdbg",
                            request          = "launch",
                            program          = exeRel,
                            args             = Array.Empty<string>(),
                            stopAtEntry      = false,
                            cwd              = "${workspaceFolder}/bin",
                            environment      = Array.Empty<object>(),
                            externalConsole  = false,
                            MIMode           = "gdb",
                            miDebuggerPath   = gdbPath,
                            setupCommands    = new[]
                            {
                                new
                                {
                                    description    = "Enable pretty-printing for gdb",
                                    text           = "-enable-pretty-printing",
                                    ignoreFailures = true
                                }
                            }
                        }
                    }
                };
            }
            else
            {
                config = new
                {
                    version        = "0.2.0",
                    configurations = new[]
                    {
                        new
                        {
                            name            = $"Debug: {fileName}",
                            type            = "cppvsdbg",
                            request         = "launch",
                            program         = exeRel,
                            args            = Array.Empty<string>(),
                            stopAtEntry     = false,
                            cwd             = "${workspaceFolder}/bin",
                            environment     = Array.Empty<object>(),
                            console         = "integratedTerminal"
                        }
                    }
                };
            }

            var json = JsonSerializer.Serialize(config,
                new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(Path.Combine(vscodeDir, "launch.json"), json);
        }

        private void OpenInVsCode(string filePath)
        {
            // Try both code.cmd (user install) and code (system install)
            foreach (var codeExe in new[] { "code.cmd", "code.exe", "code" })
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName        = "cmd.exe",
                        Arguments       = $"/c \"{codeExe}\" --goto \"{filePath}\"",
                        UseShellExecute = false,
                        CreateNoWindow  = true
                    };
                    Process.Start(psi);
                    return;
                }
                catch { /* try next */ }
            }
        }
    }
}
