using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SourceCollector
{
    public partial class MainForm : Form
    {
        // 控件字段
        private TextBox txtTarget = null!;
        private TextBox txtOutput = null!;
        private TextBox txtSkip = null!;
        private CheckBox chkEnableSkip = null!;
        private CheckBox chkIgnoreHidden = null!;
        private TextBox txtExtensions = null!;
        private Button btnTargetBrowse = null!;
        private Button btnOutputBrowse = null!;
        private Button btnProcess = null!;
        private RichTextBox txtLog = null!;
        private ToolTip toolTip = new ToolTip();

        // 新增复选框
        private CheckBox chkCopyFiles = null!;      // 目标文件
        private CheckBox chkGenerateIndex = null!;  // 目录文件
        private CheckBox chkOnlyDirectories = null!; // 仅文件夹
        private CheckBox chkConvertToTxt = null!;    // 新增：转txt文件

        // 水印文本
        private const string ExtWatermark = "如 .cs|.xaml，\\开头表示排除";

        public MainForm()
        {
            InitializeComponent();
            LoadHistory();
            SetupWatermark();
        }

        private void InitializeComponent()
        {
            this.Text = "文件复制与目录树生成工具";
            this.Size = new Size(804, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Microsoft YaHei", 9);

            int labelX = 12, controlX = 120, buttonX = 700, rowHeight = 35;
            int textBoxWidth = 570;

            // 目标目录行
            Label lblTarget = new Label { Text = "目标目录：", Location = new Point(labelX, 15), Size = new Size(100, 30) };
            txtTarget = new TextBox { Location = new Point(controlX, 12), Size = new Size(textBoxWidth, 25) };
            btnTargetBrowse = new Button { Text = "浏览...", Location = new Point(buttonX, 10), Size = new Size(75, 30) };
            btnTargetBrowse.Click += BtnTargetBrowse_Click;

            // 输出目录行
            Label lblOutput = new Label { Text = "输出目录：", Location = new Point(labelX, 15 + rowHeight), Size = new Size(100, 30) };
            txtOutput = new TextBox { Location = new Point(controlX, 12 + rowHeight), Size = new Size(textBoxWidth, 25) };
            btnOutputBrowse = new Button { Text = "浏览...", Location = new Point(buttonX, 10 + rowHeight), Size = new Size(75, 30) };
            btnOutputBrowse.Click += BtnOutputBrowse_Click;

            // 屏蔽文件行
            Label lblSkip = new Label { Text = "屏蔽名称：", Location = new Point(labelX, 15 + rowHeight * 2), Size = new Size(100, 30) };
            toolTip.SetToolTip(lblSkip, "多个屏蔽名请用竖线 | 分隔");
            // 修改：缩短文本框宽度，为新增复选框腾出空间
            txtSkip = new TextBox { Location = new Point(controlX, 12 + rowHeight * 2), Size = new Size(textBoxWidth - 160, 25) };
            chkEnableSkip = new CheckBox { Text = "启用屏蔽", Location = new Point(buttonX - 160, 10 + rowHeight * 2), Size = new Size(80, 30), Checked = false };
            chkIgnoreHidden = new CheckBox { Text = "忽视隐藏", Location = new Point(buttonX - 80, 10 + rowHeight * 2), Size = new Size(80, 30), Checked = true };
            // 新增：转txt文件复选框
            chkConvertToTxt = new CheckBox { Text = "转txt文件", Location = new Point(buttonX, 10 + rowHeight * 2), Size = new Size(80, 30), Checked = false };
            toolTip.SetToolTip(chkConvertToTxt, "将复制出的文件后缀名统一转为 .txt");

            // 目标后缀行
            Label lblExt = new Label { Text = "目标后缀：", Location = new Point(labelX, 15 + rowHeight * 3), Size = new Size(100, 30) };
            toolTip.SetToolTip(lblExt, "多个后缀用竖线分隔，\\开头表示排除（如\\.txt），输入 .* 表示所有文件");
            txtExtensions = new TextBox { Location = new Point(controlX, 12 + rowHeight * 3), Size = new Size(textBoxWidth - 240, 25) };
            chkCopyFiles = new CheckBox { Text = "目标文件", Location = new Point(buttonX - 240, 10 + rowHeight * 3), Size = new Size(80, 30), Checked = true };
            chkGenerateIndex = new CheckBox { Text = "目录文件", Location = new Point(buttonX - 160, 10 + rowHeight * 3), Size = new Size(80, 30), Checked = true };
            chkOnlyDirectories = new CheckBox { Text = "仅文件夹", Location = new Point(buttonX - 80, 10 + rowHeight * 3), Size = new Size(80, 30), Checked = false };
            chkOnlyDirectories.CheckedChanged += ChkOnlyDirectories_CheckedChanged;

            // 确认按钮
            btnProcess = new Button { Text = "确认", Location = new Point(buttonX, 10 + rowHeight * 3), Size = new Size(75, 30) };
            btnProcess.Click += BtnProcess_Click;

            // 日志区域
            txtLog = new RichTextBox
            {
                Location = new Point(12, 12 + rowHeight * 4 + 10),
                Size = new Size(this.ClientSize.Width - 24, this.ClientSize.Height - (12 + rowHeight * 4 + 24)),
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // 修改：添加 chkConvertToTxt 到控件集合
            this.Controls.AddRange(new Control[] {
                lblTarget, txtTarget, btnTargetBrowse,
                lblOutput, txtOutput, btnOutputBrowse,
                lblSkip, txtSkip, chkEnableSkip, chkIgnoreHidden, chkConvertToTxt, // 新增
                lblExt, txtExtensions, chkCopyFiles, chkGenerateIndex, chkOnlyDirectories, btnProcess,
                txtLog
            });
        }

        private void ChkOnlyDirectories_CheckedChanged(object? sender, EventArgs e)
        {
            if (chkOnlyDirectories.Checked)
            {
                chkCopyFiles.Checked = false;
                chkCopyFiles.Enabled = false;
                chkGenerateIndex.Checked = true;
                chkGenerateIndex.Enabled = false;
                txtExtensions.Enabled = false;
                txtExtensions.Text = ""; // 清空后缀，因为无效
                chkConvertToTxt.Enabled = false; // 新增：仅文件夹模式下禁用转txt
            }
            else
            {
                chkCopyFiles.Enabled = true;
                chkGenerateIndex.Enabled = true;
                txtExtensions.Enabled = true;
                chkConvertToTxt.Enabled = true; // 新增：恢复启用
                SetWatermarkIfNeeded(); // 恢复水印
            }
        }

        // 设置水印
        private void SetupWatermark()
        {
            txtExtensions.Enter += TxtExtensions_Enter;
            txtExtensions.Leave += TxtExtensions_Leave;
            SetWatermarkIfNeeded();
        }

        private void TxtExtensions_Enter(object? sender, EventArgs e)
        {
            if (txtExtensions.Text == ExtWatermark)
            {
                txtExtensions.Text = "";
                txtExtensions.ForeColor = SystemColors.WindowText;
            }
        }

        private void TxtExtensions_Leave(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtExtensions.Text))
            {
                txtExtensions.Text = ExtWatermark;
                txtExtensions.ForeColor = SystemColors.GrayText;
            }
        }

        private void SetWatermarkIfNeeded()
        {
            if (string.IsNullOrEmpty(txtExtensions.Text) && !chkOnlyDirectories.Checked)
            {
                txtExtensions.Text = ExtWatermark;
                txtExtensions.ForeColor = SystemColors.GrayText;
            }
            else
            {
                txtExtensions.ForeColor = SystemColors.WindowText;
            }
        }

        private void BtnTargetBrowse_Click(object? sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
                txtTarget.Text = dlg.SelectedPath;
        }

        private void BtnOutputBrowse_Click(object? sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
                txtOutput.Text = dlg.SelectedPath;
        }

        private async void BtnProcess_Click(object? sender, EventArgs e)
        {
            string targetDir = txtTarget.Text.Trim();
            string outputDir = txtOutput.Text.Trim();
            string extInput = txtExtensions.Text.Trim();
            string skipInput = txtSkip.Text.Trim();
            bool enableSkip = chkEnableSkip.Checked;
            bool ignoreHidden = chkIgnoreHidden.Checked;
            bool copyFiles = chkCopyFiles.Checked;
            bool generateIndex = chkGenerateIndex.Checked;
            bool onlyDirectories = chkOnlyDirectories.Checked;
            bool convertToTxt = chkConvertToTxt.Checked; // 新增

            // 验证
            if (string.IsNullOrEmpty(targetDir) || !Directory.Exists(targetDir))
            {
                MessageBox.Show("目标目录不存在或为空。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(outputDir))
            {
                MessageBox.Show("请指定输出目录。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 解析后缀名（包含和排除）
            HashSet<string> includeExts = new(StringComparer.OrdinalIgnoreCase);
            HashSet<string> excludeExts = new(StringComparer.OrdinalIgnoreCase);
            bool matchAll = false;

            if (!onlyDirectories)
            {
                foreach (string item in extInput.Split('|', StringSplitOptions.RemoveEmptyEntries))
                {
                    string trimmed = item.Trim();
                    if (string.IsNullOrEmpty(trimmed)) continue;

                    bool isExclude = trimmed.StartsWith("\\");
                    string ext = isExclude ? trimmed.Substring(1) : trimmed;

                    if (ext == ".*")
                    {
                        if (isExclude)
                        {
                            MessageBox.Show("排除项不能包含 .*", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        matchAll = true;
                        includeExts.Clear();
                        break;
                    }

                    if (!ext.StartsWith(".")) ext = "." + ext;

                    if (isExclude)
                        excludeExts.Add(ext);
                    else
                        includeExts.Add(ext);
                }

                // 如果没有包含项且不是 matchAll，则报错
                if (!matchAll && includeExts.Count == 0 && excludeExts.Count > 0)
                {
                    MessageBox.Show("必须至少指定一个包含后缀，排除不能作为唯一条件。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!matchAll && includeExts.Count == 0 && excludeExts.Count == 0)
                {
                    MessageBox.Show("请至少指定一个后缀名，或输入 .* 表示所有文件。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                // 仅文件夹模式：忽略后缀
                matchAll = false;
                includeExts.Clear();
                excludeExts.Clear();
            }

            // 解析屏蔽名称
            HashSet<string> skipNames = new(StringComparer.OrdinalIgnoreCase);
            if (enableSkip)
            {
                foreach (string name in skipInput.Split('|', StringSplitOptions.RemoveEmptyEntries))
                {
                    string trimmed = name.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                        skipNames.Add(trimmed);
                }
            }

            try
            {
                Directory.CreateDirectory(outputDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法创建输出目录：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnProcess.Enabled = false;
            txtLog.Clear();
            AppendLog("开始处理...");

            // 后台执行
            await Task.Run(() =>
            {
                try
                {
                    // 修改：传递 convertToTxt 参数
                    ProcessDirectory(targetDir, outputDir, includeExts, excludeExts, skipNames,
                                     ignoreHidden, copyFiles, generateIndex, matchAll, onlyDirectories, convertToTxt);
                }
                catch (Exception ex)
                {
                    AppendLog($"处理过程中出现错误：{ex.Message}");
                }
            });

            AppendLog("处理完成。");
            btnProcess.Enabled = true;

            // 保存历史（新增 convertToTxt）
            SaveHistory(targetDir, outputDir, extInput, skipInput, enableSkip, ignoreHidden, copyFiles, generateIndex, convertToTxt);
        }

        // 修改：添加 convertToTxt 参数
        private void ProcessDirectory(string targetDir, string outputDir,
            HashSet<string> includeExts, HashSet<string> excludeExts,
            HashSet<string> skipNames, bool ignoreHidden,
            bool copyFiles, bool generateIndex, bool matchAll, bool onlyDirectories,
            bool convertToTxt)
        {
            var root = new DirectoryInfo(targetDir);
            string rootName = root.Name + "/";

            // 第一次遍历：收集文件列表（如果需要复制）
            List<string>? filesToCopy = copyFiles ? new List<string>() : null;
            if (copyFiles && !onlyDirectories)
            {
                BuildTree(root, "", true, null, filesToCopy, includeExts, excludeExts, skipNames,
                          ignoreHidden, matchAll, onlyDirectories, null, AppendLog);
            }

            // 复制文件并记录重命名映射
            Dictionary<string, string>? renameMap = copyFiles ? new Dictionary<string, string>() : null;
            int copied = 0, failed = 0;
            if (copyFiles && filesToCopy != null && renameMap != null)
            {
                foreach (string src in filesToCopy)
                {
                    string dest;
                    // 新增：若 convertToTxt 为 true，构造虚拟路径强制使用 .txt 扩展名
                    if (convertToTxt)
                    {
                        string virtualSrc = Path.Combine(Path.GetDirectoryName(src) ?? "",
                                                          Path.GetFileNameWithoutExtension(src) + ".txt");
                        dest = GetUniqueDestinationPath(outputDir, virtualSrc);
                    }
                    else
                    {
                        dest = GetUniqueDestinationPath(outputDir, src);
                    }
                    string finalName = Path.GetFileName(dest);
                    renameMap[src] = finalName;
                    try
                    {
                        File.Copy(src, dest, overwrite: false);
                        copied++;
                    }
                    catch (Exception ex)
                    {
                        AppendLog($"复制失败：{src} -> {dest}，原因：{ex.Message}");
                        failed++;
                    }
                }
            }

            // 第二次遍历：生成目录树（如果需要）
            StringBuilder? treeBuilder = generateIndex ? new StringBuilder() : null;
            if (generateIndex)
            {
                treeBuilder?.AppendLine(rootName);
                BuildTree(root, "", true, treeBuilder, null, includeExts, excludeExts, skipNames,
                          ignoreHidden, matchAll, onlyDirectories, renameMap, AppendLog);
            }

            // 写入目录树
            if (generateIndex && treeBuilder != null)
            {
                string treeFile = Path.Combine(outputDir, "目录.txt");
                try
                {
                    File.WriteAllText(treeFile, treeBuilder.ToString(), new UTF8Encoding(false));
                    AppendLog($"目录树已保存至：{treeFile}");
                }
                catch (Exception ex)
                {
                    AppendLog($"写入目录树失败：{ex.Message}");
                }
            }

            if (copyFiles)
                AppendLog($"共复制 {copied} 个文件，失败 {failed} 个。");
        }

        // 递归构建树（UI版）
        private void BuildTree(DirectoryInfo dir, string prefix, bool isLast,
            StringBuilder? sb, List<string>? files,
            HashSet<string> includeExts, HashSet<string> excludeExts,
            HashSet<string> skipNames, bool ignoreHidden, bool matchAll, bool onlyDirectories,
            Dictionary<string, string>? renameMap, Action<string> log)
        {
            // 获取子目录
            List<DirectoryInfo> subDirs = new();
            try
            {
                subDirs = dir.GetDirectories()
                    .Where(d =>
                    {
                        try
                        {
                            if (ignoreHidden && d.Attributes.HasFlag(FileAttributes.Hidden))
                                return false;
                            if (skipNames.Contains(d.Name))
                                return false;
                            return true;
                        }
                        catch { return false; }
                    })
                    .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch (UnauthorizedAccessException)
            {
                log($"无法访问目录：{dir.FullName}，已跳过。");
                return;
            }
            catch (Exception ex)
            {
                log($"读取目录 {dir.FullName} 出错：{ex.Message}，已跳过。");
                return;
            }

            // 获取文件（除非仅文件夹模式）
            List<FileInfo> fileInfos = new();
            if (!onlyDirectories)
            {
                try
                {
                    fileInfos = dir.GetFiles()
                        .Where(f =>
                        {
                            try
                            {
                                if (ignoreHidden && f.Attributes.HasFlag(FileAttributes.Hidden))
                                    return false;
                                if (skipNames.Contains(f.Name))
                                    return false;
                                string ext = f.Extension.ToLowerInvariant();

                                // 排除优先
                                if (excludeExts.Contains(ext))
                                    return false;

                                if (matchAll)
                                    return true;
                                return includeExts.Contains(ext);
                            }
                            catch { return false; }
                        })
                        .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }
                catch (Exception ex)
                {
                    log($"读取目录 {dir.FullName} 的文件出错：{ex.Message}，跳过该目录下的文件。");
                }
            }

            // 合并所有项目（目录在前，文件在后）
            var allItems = new List<FileSystemInfo>();
            allItems.AddRange(subDirs);
            allItems.AddRange(fileInfos);

            for (int i = 0; i < allItems.Count; i++)
            {
                var item = allItems[i];
                bool last = (i == allItems.Count - 1);
                string itemName = item.Name;

                // 修改：如果是文件且存在重命名映射，则显示原名（现用文件名:新名）
                if (item is FileInfo file && renameMap != null && renameMap.TryGetValue(file.FullName, out string? newName))
                {
                    itemName = $"{item.Name} (现用文件名:{newName})";
                }

                sb?.AppendLine(prefix + (last ? "└── " : "├── ") + itemName);

                if (item is DirectoryInfo subDir)
                {
                    string newPrefix = prefix + (last ? "    " : "│   ");
                    BuildTree(subDir, newPrefix, last, sb, files,
                        includeExts, excludeExts, skipNames, ignoreHidden, matchAll, onlyDirectories,
                        renameMap, log);
                }
                else if (item is FileInfo file2)
                {
                    files?.Add(file2.FullName);
                }
            }
        }

        // 获取唯一的目标路径（自动重命名）
        private string GetUniqueDestinationPath(string destDir, string sourceFile)
        {
            string baseName = Path.GetFileName(sourceFile);
            string basePath = Path.Combine(destDir, baseName);
            if (!File.Exists(basePath))
                return basePath;

            string nameWithoutExt = Path.GetFileNameWithoutExtension(baseName);
            string ext = Path.GetExtension(baseName);
            int counter = 1;
            while (true)
            {
                string newName = $"{nameWithoutExt} ({counter}){ext}";
                string newPath = Path.Combine(destDir, newName);
                if (!File.Exists(newPath))
                    return newPath;
                counter++;
                // 防止死循环
                if (counter > 1000)
                    return basePath; // 放弃，返回原路径，让复制时可能覆盖（但会抛出异常）
            }
        }

        private void AppendLog(string message)
        {
            if (txtLog.InvokeRequired)
                txtLog.Invoke(new Action<string>(AppendLog), message);
            else
                txtLog.AppendText(message + Environment.NewLine);
        }

        // 从 history.ini 加载配置（新增 ConvertToTxt）
        private void LoadHistory()
        {
            string historyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "history.ini");
            if (!File.Exists(historyPath))
            {
                chkIgnoreHidden.Checked = true;
                chkCopyFiles.Checked = true;
                chkGenerateIndex.Checked = true;
                chkOnlyDirectories.Checked = false;
                chkConvertToTxt.Checked = false; // 新增默认
                return;
            }

            try
            {
                var lines = File.ReadAllLines(historyPath, Encoding.UTF8);
                foreach (string line in lines)
                {
                    if (line.StartsWith("Target="))
                        txtTarget.Text = line.Substring(7);
                    else if (line.StartsWith("Output="))
                        txtOutput.Text = line.Substring(7);
                    else if (line.StartsWith("Extensions="))
                    {
                        string val = line.Substring(11);
                        txtExtensions.Text = val;
                    }
                    else if (line.StartsWith("SkipPatterns="))
                        txtSkip.Text = line.Substring(13);
                    else if (line.StartsWith("EnableSkip="))
                        chkEnableSkip.Checked = line.Substring(11).Trim() == "1";
                    else if (line.StartsWith("IgnoreHidden="))
                        chkIgnoreHidden.Checked = line.Substring(13).Trim() == "1";
                    else if (line.StartsWith("CopyFiles="))
                        chkCopyFiles.Checked = line.Substring(10).Trim() == "1";
                    else if (line.StartsWith("GenerateIndex="))
                        chkGenerateIndex.Checked = line.Substring(14).Trim() == "1";
                    else if (line.StartsWith("ConvertToTxt=")) // 新增
                        chkConvertToTxt.Checked = line.Substring(12).Trim() == "1";
                }
                SetWatermarkIfNeeded();
            }
            catch
            {
                chkIgnoreHidden.Checked = true;
                chkCopyFiles.Checked = true;
                chkGenerateIndex.Checked = true;
                chkOnlyDirectories.Checked = false;
                chkConvertToTxt.Checked = false; // 新增
            }
        }

        // 保存当前配置到 history.ini（新增 ConvertToTxt）
        private void SaveHistory(string target, string output, string extensions, string skipPatterns,
            bool enableSkip, bool ignoreHidden, bool copyFiles, bool generateIndex, bool convertToTxt)
        {
            string historyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "history.ini");
            try
            {
                using var writer = new StreamWriter(historyPath, false, Encoding.UTF8);
                writer.WriteLine($"[General]");
                writer.WriteLine($"Target={target}");
                writer.WriteLine($"Output={output}");
                writer.WriteLine($"Extensions={extensions}");
                writer.WriteLine($"SkipPatterns={skipPatterns}");
                writer.WriteLine($"EnableSkip={(enableSkip ? 1 : 0)}");
                writer.WriteLine($"IgnoreHidden={(ignoreHidden ? 1 : 0)}");
                writer.WriteLine($"CopyFiles={(copyFiles ? 1 : 0)}");
                writer.WriteLine($"GenerateIndex={(generateIndex ? 1 : 0)}");
                writer.WriteLine($"ConvertToTxt={(convertToTxt ? 1 : 0)}"); // 新增
            }
            catch { /* 忽略写入错误 */ }
        }
    }
}