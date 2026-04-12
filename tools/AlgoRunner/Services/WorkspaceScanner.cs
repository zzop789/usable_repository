using System.Collections.ObjectModel;
using System.IO;
using AlgoRunner.Models;

namespace AlgoRunner.Services
{
    public class WorkspaceScanner : IDisposable
    {
        private readonly string _workspaceRoot;
        private FileSystemWatcher? _watcher;

        public ObservableCollection<DirectoryNode> Nodes { get; } = new();

        /// <summary>工作区 .cpp 文件发生变化时触发（在线程池线程上）</summary>
        public event Action? WorkspaceChanged;

        public WorkspaceScanner(string workspaceRoot)
        {
            _workspaceRoot = workspaceRoot;
        }

        /// <summary>扫描工作区，填充 Nodes，并启动 FileSystemWatcher。必须在调用前 Dispose 旧 watcher。</summary>
        public void Scan()
        {
            Nodes.Clear();

            if (!Directory.Exists(_workspaceRoot)) return;

            // 遍历一级子目录，跳过 tools\ 和隐藏目录（如 .vscode）
            var dirs = Directory.GetDirectories(_workspaceRoot)
                .Where(d =>
                {
                    var name = Path.GetFileName(d);
                    return !name.Equals("tools", StringComparison.OrdinalIgnoreCase)
                        && !name.Equals("bin", StringComparison.OrdinalIgnoreCase)
                        && !name.StartsWith('.');
                })
                .OrderBy(d => Path.GetFileName(d));

            foreach (var dir in dirs)
            {
                var files = Directory.GetFiles(dir, "*.cpp", SearchOption.AllDirectories)
                    .Select(f => new CppFile
                    {
                        FilePath    = f,
                        DisplayName = Path.GetFileName(f),
                        Category    = Path.GetFileName(dir)
                    })
                    .OrderBy(f => f.DisplayName)
                    .ToList();

                if (files.Count == 0) continue;

                var node = new DirectoryNode
                {
                    Name     = Path.GetFileName(dir),
                    FullPath = dir
                };
                foreach (var f in files) node.Files.Add(f);
                Nodes.Add(node);
            }

            SetupWatcher();
        }

        private void SetupWatcher()
        {
            _watcher?.Dispose();

            if (!Directory.Exists(_workspaceRoot)) return;

            _watcher = new FileSystemWatcher(_workspaceRoot)
            {
                Filter                = "*.cpp",
                IncludeSubdirectories = true,
                NotifyFilter          = NotifyFilters.FileName | NotifyFilters.DirectoryName,
                EnableRaisingEvents   = true
            };

            _watcher.Created += OnFsEvent;
            _watcher.Deleted += OnFsEvent;
            _watcher.Renamed += OnFsEvent;
        }

        private void OnFsEvent(object sender, FileSystemEventArgs e)
        {
            // 忽略 tools\ 目录下的变化
            if (e.FullPath.Contains(Path.Combine(_workspaceRoot, "tools"),
                StringComparison.OrdinalIgnoreCase)) return;

            WorkspaceChanged?.Invoke();
        }

        public void Dispose()
        {
            _watcher?.Dispose();
            _watcher = null;
        }
    }
}
