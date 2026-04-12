using System.Diagnostics;
using System.IO;

namespace AlgoRunner.Services
{
    public class RunnerService
    {
        private Process? _current;

        /// <summary>
        /// 运行已编译的可执行文件，流式捕获输出。
        /// 30 秒超时后强制终止。
        /// 返回进程退出码。
        /// </summary>
        public async Task<int> RunAsync(
            string exePath,
            string? stdinData                               = null,
            IProgress<(string text, bool isError)>? progress = null,
            CancellationToken cancellationToken             = default)
        {
            if (!File.Exists(exePath))
                throw new FileNotFoundException($"可执行文件不存在: {exePath}");

            var psi = new ProcessStartInfo(exePath)
            {
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                RedirectStandardInput  = stdinData is not null,
                UseShellExecute        = false,
                CreateNoWindow         = true,
                WorkingDirectory       = Path.GetDirectoryName(exePath)!
            };

            _current = new Process { StartInfo = psi, EnableRaisingEvents = true };

            _current.OutputDataReceived += (_, e) =>
            {
                if (e.Data is not null) progress?.Report((e.Data, false));
            };
            _current.ErrorDataReceived += (_, e) =>
            {
                if (e.Data is not null) progress?.Report((e.Data, true));
            };

            _current.Start();
            _current.BeginOutputReadLine();
            _current.BeginErrorReadLine();

            if (stdinData is not null)
            {
                await _current.StandardInput.WriteAsync(stdinData);
                _current.StandardInput.Close();
            }

            // 30 秒超时
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(30));

            try
            {
                await _current.WaitForExitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                if (!_current.HasExited) _current.Kill(entireProcessTree: true);
                progress?.Report(("\n[程序运行超过 30 秒，已强制终止]", true));
            }

            var exitCode = _current.HasExited ? _current.ExitCode : -1;
            _current.Dispose();
            _current = null;
            return exitCode;
        }

        public void Kill()
        {
            if (_current is not null && !_current.HasExited)
                _current.Kill(entireProcessTree: true);
        }
    }
}
