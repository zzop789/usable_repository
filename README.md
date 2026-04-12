# Base C++ Learning Framework

这个仓库是一个“算法文件即项目/目录项目”双模式学习框架：
- 单文件模式：每个 `.cpp` 可独立编译、运行、调试
- 目录项目模式：目录中存在 `main.cpp` 时，自动将该目录所有 `.cpp` 一起编译链接
- 用 C# WPF 提供 GUI 管理器（AlgoRunner）
- 自动扫描工作区算法文件，支持一键打开 VS Code 调试

---

## 1. 项目目标

适合 C++ STL / 算法刷题与知识点实验场景：
- 不强制 CMake，不强制单一大工程
- 每个算法文件可以单独验证与断点调试
- 用目录做主题分类（排序、搜索、图、DP、数据结构等）

---

## 2. 目录结构

```text
base_cpp_learning/
├── sorting/
│   ├── bubble_sort/
│   │   └── main.cpp
│   └── merge_sort/
│       └── main.cpp
├── searching/
│   └── binary_search/
│       └── main.cpp
├── stl/
│   ├── map_demo/
│   │   └── main.cpp
│   └── vector_demo/
│       └── main.cpp
├── graph/
│   ├── bfs/
│   │   └── main.cpp
│   └── dfs/
│       └── main.cpp
├── dynamic_programming/
│   ├── fibonacci_dp/
│   │   └── main.cpp
│   ├── knapsack/
│   │   └── main.cpp
│   └── demo_multi/
│       ├── main.cpp
│       ├── solver.cpp
│       └── input.txt
├── data_structures/
│   ├── linked_list/
│   │   └── main.cpp
│   └── stack_queue/
│       └── main.cpp
├── .vscode/
│   ├── tasks.json
│   ├── launch.json
│   ├── build-current.cmd
│   └── c_cpp_properties.json
├── bin/
├── tools/
│   └── AlgoRunner/
│       ├── AlgoRunner.csproj
│       ├── MainWindow.xaml
│       ├── MainWindow.xaml.cs
│       ├── Models/
│       └── Services/
└── README.md
```

---

## 3. 环境要求

### 必需
- Windows
- .NET SDK（当前工程使用 net9.0-windows）
- VS Code + C/C++ 扩展（ms-vscode.cpptools）

### C++ 编译器（二选一）
- MinGW / MSYS2 的 `g++`
- 或 Visual Studio 的 `cl.exe`（MSVC）

说明：
- 当前任务与 GUI 都支持自动编译器选择：优先 `g++`，缺失时回退 `cl.exe`。

---

## 4. 快速开始

### 4.1 启动 GUI（推荐）
在仓库根目录执行：

```powershell
cd tools/AlgoRunner
dotnet run
```

启动后你可以：
- 左侧选择任意 `.cpp`
- 点击“编译并运行”查看输出
- 点击“VS Code 调试”自动准备调试配置并跳转 VS Code
- 新增/删除 `.cpp` 会自动刷新文件树
- 运行时会自动读取输入文件（见第 6.3 节）

### 4.2 直接在 VS Code 中运行
1. 打开任意 `.cpp`
2. 执行 Build Task（当前是 `build-current`）
3. F5 调试（使用 `.vscode/launch.json`）

---

## 5. GUI 架构说明（AlgoRunner）

### 5.1 核心模块
- `WorkspaceScanner`
  - 扫描工作区 `.cpp`
  - 监听文件变化（FileSystemWatcher）
- `CompilerService`
  - 自动检测 `g++` / `cl.exe`
  - 负责编译并输出日志
- `RunnerService`
  - 启动可执行文件
  - 流式读取 stdout/stderr，带超时保护
- `VsCodeService`
  - 写入/更新 `.vscode/launch.json`
  - 用 `code --goto` 打开目标文件

### 5.2 交互流程
1. 选择文件
2. 编译（带调试符号）
3. 运行或进入 VS Code 调试
4. 输出日志显示在右侧面板

---

## 6. 如何新增算法项目（新结构）

1. 在主题目录下新建一个“算法目录”（例如 `sorting/quick_sort/`）。
2. 在该目录放 `main.cpp`（必须有）。
3. 若拆分实现，可继续添加 `*.cpp / *.h`（如 `partition.cpp`, `partition.h`）。
4. 需要固定输入时，在该目录新增 `input.txt`。
5. 保存后 GUI 会自动识别；可直接点击“编译并运行”或“VS Code 调试”。

推荐模板：

```text
<category>/<algorithm_name>/
├── main.cpp
├── xxx.cpp        (可选)
├── xxx.h          (可选)
└── input.txt      (可选)
```

### 6.1 单文件模式（兼容旧结构）
- 直接创建 `xxx.cpp`，工具只编译当前文件。

### 6.2 目录项目模式（推荐）
约定规则：
- 目录里存在 `main.cpp` 时，视为“一个算法项目”。
- 工具会自动编译该目录下全部 `.cpp` 并链接。
- 输出可执行文件命名为 `<算法目录名>.exe`（避免多个 `main.cpp` 互相覆盖）。

示例：

```text
graph/
└── dijkstra_multi/
  ├── main.cpp
  ├── dijkstra.cpp
  └── dijkstra.h
```

### 6.3 统一输入文件（免命令行手输）
运行按钮会自动把 input 文件内容喂给程序 stdin，优先级如下：
1. 算法目录下的 `input.txt`
2. 工作区根目录的 `input.txt`

例如：

```text
dynamic_programming/
└── lis_project/
  ├── main.cpp
  ├── lis.cpp
  └── input.txt   <- 该项目专用输入

base_cpp_learning/
└── input.txt       <- 全局默认输入
```

### 6.4 旧结构迁移到新结构
旧结构（平铺）：

```text
sorting/
├── bubble_sort.cpp
└── merge_sort.cpp
```

新结构（目录项目）：

```text
sorting/
├── bubble_sort/
│   └── main.cpp
└── merge_sort/
  └── main.cpp
```

迁移规则：
1. 每个算法 `.cpp` 新建一个同名目录。
2. 将原 `.cpp` 移入该目录并重命名为 `main.cpp`。
3. 若算法拆分多文件，把辅助实现放同目录（如 `solver.cpp`、`solver.h`）。
4. 需要固定输入则在同目录加 `input.txt`。

---

## 7. 常见问题（FAQ）

### Q1: `#include <iostream>` 报 includePath 错误
已通过 `.vscode/c_cpp_properties.json` 配置了可用工具链和系统头路径。
如果仍报错：
1. 执行 VS Code 命令：`C/C++: Reset IntelliSense Database`
2. 重新打开工作区
3. 检查 `compilerPath` 对应文件是否真实存在

### Q2: Build Task 失败，提示 `g++` 未找到
已完成处理：`tasks.json` 现在会自动检测编译器。
- 若检测到 `g++`：使用 `g++` 构建
- 若未检测到 `g++`：自动回退到 `cl.exe`（通过 `vswhere` + `vcvars64.bat`）
- 若当前目录含 `main.cpp`：会自动编译该目录全部 `.cpp`

如果依然失败：
1. 确认已安装 Visual Studio C++ 工具集（MSVC）
2. 确认 `vswhere.exe` 存在：`C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe`
3. 重新打开 VS Code 后再执行 Build Task

### Q3: GUI 能检测到 cl.exe，但 VS Code 任务仍失败
该问题已解决：
- GUI 与 VS Code 两侧都支持 `g++/cl.exe` 兜底
- 在缺失 `g++` 的机器上，可直接使用 Build Task 完成编译

---

## 8. 推荐学习节奏

1. 先跑通现有示例（sorting/searching/stl）
2. 每个主题新增 3~5 个算法文件
3. 每次写完先“仅编译”，再“编译并运行”
4. 对边界条件打断点（空数组、单元素、大输入）

---

## 9. 后续可扩展点

- 算法标签与难度管理（在 GUI 中按标签过滤）
- 运行输入模板（stdin 预设）
- 基准测试模式（多组输入统计耗时）
- 一键生成新算法文件模板

如果你愿意，我可以直接继续做第一个扩展：
“在 GUI 里新增【新建算法文件】按钮 + 模板生成器”。
