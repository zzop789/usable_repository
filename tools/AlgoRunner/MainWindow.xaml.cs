using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using AlgoRunner.Models;
using AlgoRunner.Services;
using Microsoft.Win32;
using System.Diagnostics;

namespace AlgoRunner
{
    public partial class MainWindow : Window
    {
        // ── Constants ────────────────────────────────────────────────────────────
        private static readonly string WorkspaceRoot =
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\.."));
        private static readonly string BinDir = Path.Combine(WorkspaceRoot, "bin");

        // ── Services ─────────────────────────────────────────────────────────────
        private readonly WorkspaceScanner _scanner;
        private readonly CompilerService  _compiler;
        private readonly RunnerService    _runner;
        private readonly VsCodeService    _vsCode;

        // ── State ────────────────────────────────────────────────────────────────
        private CppFile?                  _selectedFile;
        private CancellationTokenSource?  _runCts;

        // ── Constructor ──────────────────────────────────────────────────────────
        public MainWindow()
        {
            InitializeComponent();

            _compiler = new CompilerService();
            _scanner  = new WorkspaceScanner(WorkspaceRoot);
            _runner   = new RunnerService();
            _vsCode   = new VsCodeService(WorkspaceRoot, _compiler);

            Loaded += OnLoaded;
            Closed += (_, _) => _scanner.Dispose();
        }

        // ── Startup ──────────────────────────────────────────────────────────────
        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var startupTimer = Stopwatch.StartNew();

            // Initialize output document
            OutputBox.Document.PagePadding = new Thickness(0);
            OutputBox.Document.PageWidth   = 8000;

            StatusLabel.Text = "启动中...";
            CompilerLabel.Text = "检测中...";
            FileCountLabel.Text = "扫描中...";
            _scanner.WorkspaceChanged += OnWorkspaceChanged;

            AppendLine($"[AlgoRunner 已启动]  工作区: {WorkspaceRoot}", Colors.DimGray);
            AppendLine("正在后台检测编译器并扫描项目...", Colors.DimGray);
            AppendLine("", Colors.White);

            var detectCompilerTask = Task.Run(() =>
            {
                _compiler.Detect();
                return _compiler.ActiveCompiler;
            });

            var scanWorkspaceTask = Task.Run(() => _scanner.ScanSnapshot());

            var detectedCompiler = await detectCompilerTask;
            UpdateCompilerLabel();

            var nodes = await scanWorkspaceTask;
            _scanner.ApplyNodes(nodes);
            _scanner.EnsureWatching();
            RefreshTreeView();

            AppendLine($"编译器: {detectedCompiler.DisplayName}", Colors.DimGray);
            if (detectedCompiler.Type == CompilerType.None)
                AppendLine("  → 未检测到 g++ 或 cl.exe，请点击右上角 [···] 手动指定",
                    Color.FromRgb(0xFF, 0xA5, 0x00));
            AppendLine($"启动完成，用时: {startupTimer.ElapsedMilliseconds} ms", Colors.DimGray);
            AppendLine("", Colors.White);
        }

        // ── File tree ────────────────────────────────────────────────────────────
        private void OnWorkspaceChanged()
        {
            _ = Dispatcher.InvokeAsync(async () =>
            {
                StatusLabel.Text = "刷新项目树...";
                var nodes = await Task.Run(() => _scanner.ScanSnapshot());
                _scanner.ApplyNodes(nodes);
                RefreshTreeView();
            });
        }

        private void RefreshTreeView()
        {
            var search = SearchBox.Text.Trim().ToLowerInvariant();

            IEnumerable<DirectoryNode> source = string.IsNullOrEmpty(search)
                ? _scanner.Nodes.Select(CloneDirectoryNode)
                : _scanner.Nodes
                    .Select(n => FilterDirectoryNode(n, search))
                    .OfType<DirectoryNode>();

            FileTree.ItemsSource = source.ToList();

            var total = _scanner.Nodes.Sum(n => n.Algorithms.Sum(a => a.Files.Count));
            FileCountLabel.Text = string.IsNullOrEmpty(search)
                ? $"共 {total} 个文件  "
                : $"匹配 {source.Sum(n => n.Algorithms.Sum(a => a.Files.Count))}/{total}  ";

            StatusLabel.Text = "就绪";
        }

        private static DirectoryNode CloneDirectoryNode(DirectoryNode source)
        {
            return new DirectoryNode
            {
                Name       = source.Name,
                FullPath   = source.FullPath,
                Algorithms = new ObservableCollection<AlgorithmNode>(
                    source.Algorithms.Select(CloneAlgorithmNode))
            };
        }

        private static DirectoryNode? FilterDirectoryNode(DirectoryNode source, string search)
        {
            var algorithms = source.Algorithms
                .Select(a => FilterAlgorithmNode(a, search))
                .Where(a => a is not null)
                .Cast<AlgorithmNode>()
                .ToList();

            if (algorithms.Count == 0) return null;

            return new DirectoryNode
            {
                Name       = source.Name,
                FullPath   = source.FullPath,
                Algorithms = new ObservableCollection<AlgorithmNode>(algorithms)
            };
        }

        private static AlgorithmNode CloneAlgorithmNode(AlgorithmNode source)
        {
            return new AlgorithmNode
            {
                Name     = source.Name,
                FullPath = source.FullPath,
                Files    = new ObservableCollection<CppFile>(source.Files.Select(CloneCppFile))
            };
        }

        private static AlgorithmNode? FilterAlgorithmNode(AlgorithmNode source, string search)
        {
            var matchesAlgorithm = source.Name.Contains(search, StringComparison.OrdinalIgnoreCase);
            var files = source.Files
                .Where(f => matchesAlgorithm || FileMatchesSearch(f, search))
                .Select(CloneCppFile)
                .ToList();

            if (files.Count == 0) return null;

            return new AlgorithmNode
            {
                Name     = source.Name,
                FullPath = source.FullPath,
                Files    = new ObservableCollection<CppFile>(files)
            };
        }

        private static bool FileMatchesSearch(CppFile file, string search)
        {
            return file.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase)
                || file.FilePath.Contains(search, StringComparison.OrdinalIgnoreCase);
        }

        private static CppFile CloneCppFile(CppFile source)
        {
            return new CppFile
            {
                FilePath    = source.FilePath,
                DisplayName = source.DisplayName,
                Category    = source.Category
            };
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Toggle placeholder visibility
            SearchPlaceholder.Visibility =
                string.IsNullOrEmpty(SearchBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            RefreshTreeView();
        }

        private void FileTree_SelectedItemChanged(object sender,
            RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is not CppFile file) return;

            _selectedFile = file;
            SelectedFileLabel.Text =
                Path.GetRelativePath(WorkspaceRoot, file.FilePath).Replace('\\', '/');

            SetActionButtonsEnabled(true);
        }

        // ── Button handlers ──────────────────────────────────────────────────────
        private async void RunBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFile is null) return;
            SetActionButtonsEnabled(false);
            StopBtn.IsEnabled = true;

            AppendLine($"\n[{Ts()}] ▶ 编译并运行: {Path.GetFileName(_selectedFile.FilePath)}", Colors.Yellow);
            StatusLabel.Text = "正在编译...";

            var compileProgress = new Progress<string>(msg => Dispatcher.Invoke(() =>
                AppendLine(msg.Replace("[out] ", "").Replace("[err] ", ""),
                    msg.StartsWith("[err]") ? Colors.OrangeRed : Colors.LightGray)));

            var (success, _) = await _compiler.CompileAsync(_selectedFile.FilePath, BinDir, compileProgress);

            if (!success)
            {
                AppendLine("[编译失败]", Colors.OrangeRed);
                StatusLabel.Text = "编译失败";
                SetActionButtonsEnabled(true);
                StopBtn.IsEnabled = false;
                return;
            }

            AppendLine("[编译成功] 运行中...\n", Colors.LightGreen);
            StatusLabel.Text = "运行中...";

            _runCts = new CancellationTokenSource();
            var runProgress = new Progress<(string text, bool isError)>(info =>
                Dispatcher.Invoke(() => AppendLine(info.text, info.isError ? Colors.OrangeRed : Colors.White)));

            try
            {
                var exePath  = _compiler.GetOutputExePath(_selectedFile.FilePath, BinDir);
                var stdin    = TryLoadUnifiedInput(_selectedFile.FilePath);
                var exitCode = await _runner.RunAsync(exePath, stdin, runProgress, _runCts.Token);

                AppendLine($"\n[{Ts()}] 进程退出，退出码: {exitCode}",
                    exitCode == 0 ? Colors.DimGray : Colors.OrangeRed);
                StatusLabel.Text = $"完成 (退出码 {exitCode})";
            }
            catch (Exception ex)
            {
                AppendLine($"[运行错误] {ex.Message}", Colors.OrangeRed);
                StatusLabel.Text = "运行出错";
            }
            finally
            {
                _runCts?.Dispose();
                _runCts = null;
                SetActionButtonsEnabled(true);
                StopBtn.IsEnabled = false;
            }
        }

        private async void CompileBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFile is null) return;
            SetActionButtonsEnabled(false);

            AppendLine($"\n[{Ts()}] 🔨 仅编译: {Path.GetFileName(_selectedFile.FilePath)}", Colors.Yellow);
            StatusLabel.Text = "正在编译...";

            var progress = new Progress<string>(msg => Dispatcher.Invoke(() =>
                AppendLine(msg.Replace("[out] ", "").Replace("[err] ", ""),
                    msg.StartsWith("[err]") ? Colors.OrangeRed : Colors.LightGray)));

            var (success, _) = await _compiler.CompileAsync(_selectedFile.FilePath, BinDir, progress);

            AppendLine(success ? "[编译成功]" : "[编译失败]",
                success ? Colors.LightGreen : Colors.OrangeRed);
            StatusLabel.Text = success ? "编译成功" : "编译失败";
            SetActionButtonsEnabled(true);
        }

        private async void DebugBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFile is null) return;
            SetActionButtonsEnabled(false);

            AppendLine($"\n[{Ts()}] 🐛 准备 VS Code 调试会话...", Colors.Yellow);
            StatusLabel.Text = "准备调试...";

            var progress = new Progress<string>(msg =>
                Dispatcher.Invoke(() => AppendLine(msg, Colors.LightGray)));

            var ok = await _vsCode.OpenDebugSessionAsync(_selectedFile.FilePath, BinDir, progress);

            if (ok)
            {
                AppendLine("[✓ launch.json 已更新，VS Code 已打开]", Colors.LightGreen);
                AppendLine("  在 VS Code 中按 F5 开始调试，可设置任意断点", Colors.DimGray);
                StatusLabel.Text = "已打开 VS Code 调试";
            }
            else
            {
                AppendLine("[✗ 调试准备失败，请检查编译错误]", Colors.OrangeRed);
                StatusLabel.Text = "调试准备失败";
            }

            SetActionButtonsEnabled(true);
        }

        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFile is null) return;
            _vsCode.OpenFile(_selectedFile.FilePath);
            AppendLine($"[打开] {Path.GetFileName(_selectedFile.FilePath)}", Colors.DimGray);
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            _runCts?.Cancel();
            _runner.Kill();
            AppendLine("[用户停止程序]", Colors.OrangeRed);
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            OutputBox.Document.Blocks.Clear();
        }

        private void BrowseCompilerBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title  = "选择编译器（g++.exe 或 cl.exe）",
                Filter = "编译器 (g++.exe;cl.exe)|g++.exe;cl.exe|所有可执行文件 (*.exe)|*.exe"
            };
            if (dlg.ShowDialog() != true) return;

            _compiler.SetOverride(dlg.FileName);
            UpdateCompilerLabel();
            AppendLine($"[编译器已更换] {dlg.FileName}", Colors.DimGray);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────
        private void SetActionButtonsEnabled(bool enabled)
        {
            RunBtn.IsEnabled     = enabled && _selectedFile is not null;
            CompileBtn.IsEnabled = enabled && _selectedFile is not null;
            DebugBtn.IsEnabled   = enabled && _selectedFile is not null;
            OpenBtn.IsEnabled    = enabled && _selectedFile is not null;
        }

        private void UpdateCompilerLabel()
        {
            CompilerLabel.Text       = _compiler.ActiveCompiler.DisplayName;
            CompilerLabel.Foreground = _compiler.ActiveCompiler.Type == CompilerType.None
                ? Brushes.OrangeRed
                : new SolidColorBrush(Color.FromRgb(0x4E, 0xC9, 0xB0));
        }

        private static string Ts() => DateTime.Now.ToString("HH:mm:ss");

        private string? TryLoadUnifiedInput(string sourceFile)
        {
            // 优先级：算法目录 input.txt > 工作区根 input.txt
            var dirInput  = Path.Combine(Path.GetDirectoryName(sourceFile)!, "input.txt");
            var rootInput = Path.Combine(WorkspaceRoot, "input.txt");

            string? chosen = null;
            if (File.Exists(dirInput)) chosen = dirInput;
            else if (File.Exists(rootInput)) chosen = rootInput;

            if (chosen is null) return null;

            var text = File.ReadAllText(chosen);
            AppendLine($"[stdin] 已加载输入文件: {Path.GetRelativePath(WorkspaceRoot, chosen)}", Colors.DimGray);
            return text;
        }

        private void AppendLine(string text, Color color)
        {
            var run  = new Run(text) { Foreground = new SolidColorBrush(color) };
            var para = new Paragraph(run) { Margin = new Thickness(0), LineHeight = 18 };
            OutputBox.Document.Blocks.Add(para);
            OutputScroll.ScrollToEnd();
        }
    }
}
