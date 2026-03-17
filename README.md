# SourceCollector

SourceCollector 是一个 Windows 桌面工具，用于根据文件扩展名筛选并复制文件，同时生成带重命名映射的目录树。支持图形界面和命令行两种模式，适用于代码备份、资源整理、项目归档等场景。

## 功能特性

- **双模式操作**：GUI 界面直观易用，命令行模式适合自动化脚本。
- **灵活的扩展名筛选**：
  - 预定义多种文件类型（如 C/C++ 源码、图片、视频等）。
  - 自定义包含后缀（`-extend_SN`）和排除后缀（`-reduce_SN`）。
  - 支持 `-all` 匹配所有文件，`-no-<type>` 排除某类文件。
- **屏蔽名称**：通过竖线分隔的列表跳过特定文件或文件夹（如 `bin|obj`）。
- **忽略隐藏文件**：可选跳过系统隐藏属性的文件/文件夹。
- **仅文件夹模式**：`-D` 选项只生成不含文件的目录树，便于查看项目结构。
- **自动重命名**：当输出目录中存在同名文件时，自动添加 `(1)`、`(2)` 等后缀避免覆盖。
- **目录树生成**：以树形结构展示文件（复制后文件名变化时会标注原名→新名）。

## 使用方法

### 图形界面模式

直接双击运行 `SourceCollector.exe`（不带任何参数），即可打开主窗口：

1. **目标目录**：要扫描的源文件夹。
2. **输出目录**：文件复制和目录树保存的位置。
3. **屏蔽名称**：需要跳过的文件/文件夹名，多个用 `|` 分隔（需勾选“启用屏蔽”）。
4. **忽视隐藏**：勾选后忽略隐藏属性的项目。
5. **目标后缀**：
   - 可输入多个后缀，用 `|` 分隔，如 `.cs|.xaml`。
   - 以 `\` 开头表示排除，如 `\.txt` 排除所有 `.txt` 文件。
   - 输入 `.*` 表示匹配所有文件。
6. **操作选项**：
   - **目标文件**：复制匹配的文件到输出目录。
   - **目录文件**：生成目录树（保存为 `目录.txt`）。
   - **仅文件夹**：勾选后忽略文件，只生成目录结构。
7. 点击“确认”开始处理，日志区域显示进度。

### 命令行模式

```
SourceCollector.exe <目标目录> <输出目录> [选项...]
```

#### 常用选项

| 选项 | 说明 |
|------|------|
| `-c` | 包含 C/C++ 源码（.c, .h, .cpp 等） |
| `-cc` | 包含 C# 源码（.cs, .xaml） |
| `-I` | 包含图片文件（.jpg, .png, .gif 等） |
| `-V` | 包含视频文件（.mp4, .avi, .mkv 等） |
| `-M` | 包含 3D 模型文件（.obj, .fbx, .stl 等） |
| `-txt` | 包含文本/文档文件（.txt, .rtf, .csv 等） |
| `-excel` | 包含 Excel 文件（.xls, .xlsx 等） |
| `-word` | 包含 Word 文件（.doc, .docx 等） |
| `-pdf` | 包含 PDF 文件 |
| `-py` | 包含 Python 文件（.py） |
| `-java` | 包含 Java 文件（.java, .jar） |
| `-php` | 包含 PHP 文件 |
| `-html` | 包含 HTML 文件 |
| `-css` | 包含 CSS 文件 |
| `-js` | 包含 JavaScript 文件（未预定义，但可通过 `-extend_SN ".js"` 实现） |
| `-cfg` | 包含配置文件（.ini, .json, .yml 等） |
| `-dll` | 包含动态链接库（.dll, .ocx） |
| `-exe` | 包含可执行文件（.exe, .msi, .bat） |
| `-zip` | 包含压缩文件（.zip, .rar, .7z 等） |
| `-font` | 包含字体文件（.ttf, .otf） |
| `-sql` | 包含数据库文件（.sql, .db） |
| `-script` | 包含脚本文件（.sh, .bash） |
| `-vb` | 包含 Visual Basic 文件（.vb, .vbs） |
| `-all` | 匹配所有文件（忽略扩展名） |
| `-no-<type>` | 排除某类文件，如 `-no-V` 排除所有视频文件 |
| `-extend_SN ".ext1\|.ext2"` | 自定义包含后缀（可多个，用竖线分隔） |
| `-reduce_SN ".ext1\|.ext2"` | 自定义排除后缀（优先于包含） |
| `-skip "名称1\|名称2"` | 跳过指定名称的文件/文件夹（支持通配符？此处为精确匹配） |
| `-no_hidden` | 忽略隐藏文件和文件夹 |
| `-copy_file` | 仅复制文件（不生成目录树） |
| `-index_file` | 仅生成目录树（不复制文件） |
| `-D` | 仅文件夹模式（只生成目录树，不复制任何文件） |
| `-help` | 显示帮助信息 |

> **注意**：
> - 默认情况下，如果不指定 `-copy_file` 或 `-index_file`，两者都会执行（既复制又生成目录树）。
> - 如果同时使用 `-D`，则自动忽略文件复制，只生成目录树。

#### 示例

```bash
# 复制所有 C# 和 XAML 文件，并生成目录树，跳过 bin 和 obj 文件夹，忽略隐藏文件
SourceCollector.exe D:\MyProject D:\Backup -cc -skip "bin|obj" -no_hidden

# 只生成目录树（不含文件）
SourceCollector.exe D:\MyProject D:\Backup -D

# 复制所有文件，但排除视频和图片
SourceCollector.exe D:\MyProject D:\Backup -all -no-V -no-I -copy_file

# 自定义包含 .js 和 .ts 文件，同时包含图片文件，排除 .txt 文件
SourceCollector.exe D:\MyProject D:\Backup -extend_SN ".js|.ts" -I -reduce_SN ".txt"
```

## 编译与构建

### 环境要求
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)（或兼容版本，如 .NET 6/7/8）
- Visual Studio 2022（或更高版本）或任何支持 .NET 的编辑器

### 从源码构建
1. 克隆仓库：
   ```bash
   git clone https://github.com/yourname/SourceCollector.git
   ```
2. 进入项目目录并构建：
   ```bash
   cd SourceCollector
   dotnet build -c Release
   ```
3. 编译后的输出位于 `bin\Release\net10.0-windows` 目录。

## 发布独立可执行文件

为了在没有安装 .NET 运行时的电脑上运行，你需要将程序发布为**独立（self-contained）** 单文件。

### 使用 Visual Studio 2022 发布
1. 右键项目 → “发布”。
2. 选择“文件夹”目标，指定输出目录。
3. 点击“显示所有设置”：
   - **配置**：Release
   - **目标框架**：`net10.0-windows`
   - **部署模式**：独立
   - **目标运行时**：`win-x64`（或 `win-x86`）
   - **文件发布选项**：勾选“生成单个文件”
4. 点击“发布”。

### 使用命令行发布
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
发布后的文件位于 `bin\Release\net10.0-windows\win-x64\publish\`。

将生成的 `SourceCollector.exe` 复制到目标电脑即可运行。

## 许可证

本项目采用 MIT 许可证。详情请查看 [LICENSE](LICENSE) 文件。
