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
            ApplyNodes(ScanSnapshot());
            EnsureWatching();
        }

        public List<DirectoryNode> ScanSnapshot()
        {
            var nodes = new List<DirectoryNode>();

            if (!Directory.Exists(_workspaceRoot)) return nodes;

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
                var node = new DirectoryNode
                {
                    Name     = Path.GetFileName(dir),
                    FullPath = dir
                };

                foreach (var algorithm in BuildAlgorithmNodes(dir))
                    node.Algorithms.Add(algorithm);

                if (node.Algorithms.Count == 0) continue;

                nodes.Add(node);
            }

            return nodes;
        }

        public void ApplyNodes(IEnumerable<DirectoryNode> nodes)
        {
            Nodes.Clear();
            foreach (var node in nodes)
                Nodes.Add(node);
        }

        public void EnsureWatching()
        {
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

        private IEnumerable<AlgorithmNode> BuildAlgorithmNodes(string categoryDir)
        {
            var algorithms = new List<AlgorithmNode>();

            foreach (var file in Directory.GetFiles(categoryDir, "*.cpp", SearchOption.TopDirectoryOnly)
                         .OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
            {
                algorithms.Add(new AlgorithmNode
                {
                    Name     = Path.GetFileNameWithoutExtension(file),
                    FullPath = file,
                    Files    = new ObservableCollection<CppFile>
                    {
                        CreateCppFile(file, categoryDir)
                    }
                });
            }

            foreach (var dir in Directory.GetDirectories(categoryDir, "*", SearchOption.AllDirectories)
                         .Where(d => Directory.GetFiles(d, "*.cpp", SearchOption.TopDirectoryOnly).Length > 0)
                         .OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
            {
                var algorithm = new AlgorithmNode
                {
                    Name     = Path.GetRelativePath(categoryDir, dir).Replace('\\', '/'),
                    FullPath = dir
                };

                foreach (var file in Directory.GetFiles(dir, "*.cpp", SearchOption.TopDirectoryOnly)
                             .OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
                {
                    algorithm.Files.Add(CreateCppFile(file, categoryDir));
                }

                if (algorithm.Files.Count > 0)
                    algorithms.Add(algorithm);
            }

            return algorithms;
        }

        private static CppFile CreateCppFile(string filePath, string categoryDir)
        {
            return new CppFile
            {
                FilePath    = filePath,
                DisplayName = Path.GetFileName(filePath),
                Category    = Path.GetFileName(categoryDir)
            };
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
