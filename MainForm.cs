using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        // 水印文本
        private const string ExtWatermark = "如 .cs|.xaml";

        public MainForm()
        {
            InitializeComponent();
            LoadHistory();
            SetupWatermark();
        }

        private void InitializeComponent()
        {
            this.Text = "文件复制与目录树生成工具";
            this.Size = new Size(804, 536);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Microsoft YaHei", 9);

            int labelX = 12, controlX = 120, buttonX = 700, rowHeight = 35;
            int textBoxWidth = 570;          // 目标/输出输入框宽度

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
            txtSkip = new TextBox { Location = new Point(controlX, 12 + rowHeight * 2), Size = new Size(textBoxWidth - 80, 25) };
            chkEnableSkip = new CheckBox { Text = "启用屏蔽", Location = new Point(buttonX - 80, 10 + rowHeight * 2), Size = new Size(80, 30), Checked = false };
            chkIgnoreHidden = new CheckBox { Text = "忽视隐藏", Location = new Point(buttonX, 10 + rowHeight * 2), Size = new Size(90, 30), Checked = true };

            // 目标后缀行
            Label lblExt = new Label { Text = "目标后缀：", Location = new Point(labelX, 15 + rowHeight * 3), Size = new Size(100, 30) };
            toolTip.SetToolTip(lblExt, "多个后缀名请用竖线 | 分隔，输入 .* 表示所有文件");
            txtExtensions = new TextBox { Location = new Point(controlX, 12 + rowHeight * 3), Size = new Size(textBoxWidth - 160, 25) };
            chkCopyFiles = new CheckBox { Text = "目标文件", Location = new Point(buttonX - 160, 10 + rowHeight * 3), Size = new Size(80, 30), Checked = true };
            chkGenerateIndex = new CheckBox { Text = "目录文件", Location = new Point(buttonX - 80, 10 + rowHeight * 3), Size = new Size(80, 30), Checked = true };

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

            this.Controls.AddRange(new Control[] {
                lblTarget, txtTarget, btnTargetBrowse,
                lblOutput, txtOutput, btnOutputBrowse,
                lblSkip, txtSkip, chkEnableSkip, chkIgnoreHidden,
                lblExt, txtExtensions, chkCopyFiles, chkGenerateIndex, btnProcess,
                txtLog
            });
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
            if (string.IsNullOrEmpty(txtExtensions.Text))
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

            // 解析后缀名
            HashSet<string> extensions = new(StringComparer.OrdinalIgnoreCase);
            bool matchAll = false;
            foreach (string ext in extInput.Split('|', StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmed = ext.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                if (trimmed == ".*")
                {
                    matchAll = true;
                    // 如果出现 .*，则忽略其他后缀，直接清空集合
                    extensions.Clear();
                    break;
                }
                if (!trimmed.StartsWith(".")) trimmed = "." + trimmed;
                extensions.Add(trimmed.ToLowerInvariant());
            }

            // 如果没有指定任何有效后缀且不是 .*，则报错
            if (!matchAll && extensions.Count == 0)
            {
                MessageBox.Show("请至少指定一个后缀名，或输入 .* 表示所有文件。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
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
                    ProcessDirectory(targetDir, outputDir, extensions, skipNames, ignoreHidden, copyFiles, generateIndex, matchAll);
                }
                catch (Exception ex)
                {
                    AppendLog($"处理过程中出现错误：{ex.Message}");
                }
            });

            AppendLog("处理完成。");
            btnProcess.Enabled = true;

            // 保存历史
            SaveHistory(targetDir, outputDir, extInput, skipInput, enableSkip, ignoreHidden, copyFiles, generateIndex);
        }

        // 处理目录
        private void ProcessDirectory(string targetDir, string outputDir, HashSet<string> extensions,
            HashSet<string> skipNames, bool ignoreHidden, bool copyFiles, bool generateIndex, bool matchAll)
        {
            var root = new DirectoryInfo(targetDir);
            string rootName = root.Name + "/";
            StringBuilder? treeBuilder = generateIndex ? new StringBuilder() : null;
            treeBuilder?.AppendLine(rootName);

            List<string>? filesToCopy = copyFiles ? new List<string>() : null;

            BuildTree(root, "", true, treeBuilder, filesToCopy, extensions, skipNames, ignoreHidden, matchAll, AppendLog);

            int copied = 0, failed = 0;
            if (copyFiles && filesToCopy != null)
            {
                foreach (string src in filesToCopy)
                {
                    string dest = Path.Combine(outputDir, Path.GetFileName(src));
                    try
                    {
                        File.Copy(src, dest, true);
                        copied++;
                    }
                    catch (Exception ex)
                    {
                        AppendLog($"复制失败：{src} -> {dest}，原因：{ex.Message}");
                        failed++;
                    }
                }
            }

            if (generateIndex && treeBuilder != null)
            {
                string treeFile = Path.Combine(outputDir, "目录.txt");
                try
                {
                    File.WriteAllText(treeFile, treeBuilder.ToString(), Encoding.UTF8);
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

        // 递归构建树
        private void BuildTree(DirectoryInfo dir, string prefix, bool isLast, StringBuilder? sb,
            List<string>? files, HashSet<string> extensions, HashSet<string> skipNames, bool ignoreHidden, bool matchAll,
            Action<string> log)
        {
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

            List<FileInfo> fileInfos = new();
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
                            if (matchAll)
                                return true;
                            string ext = f.Extension.ToLowerInvariant();
                            return extensions.Contains(ext);
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

            var allItems = new List<FileSystemInfo>();
            allItems.AddRange(subDirs);
            allItems.AddRange(fileInfos);

            for (int i = 0; i < allItems.Count; i++)
            {
                var item = allItems[i];
                bool last = (i == allItems.Count - 1);
                sb?.AppendLine(prefix + (last ? "└── " : "├── ") + item.Name);

                if (item is DirectoryInfo subDir)
                {
                    string newPrefix = prefix + (last ? "    " : "│   ");
                    BuildTree(subDir, newPrefix, last, sb, files, extensions, skipNames, ignoreHidden, matchAll, log);
                }
                else if (item is FileInfo file)
                {
                    files?.Add(file.FullName);
                }
            }
        }

        private void AppendLog(string message)
        {
            if (txtLog.InvokeRequired)
                txtLog.Invoke(new Action<string>(AppendLog), message);
            else
                txtLog.AppendText(message + Environment.NewLine);
        }

        // 从 history.ini 加载配置
        private void LoadHistory()
        {
            string historyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "history.ini");
            if (!File.Exists(historyPath))
            {
                // 默认：忽视隐藏选中，其他为空，两个复选框默认勾选
                chkIgnoreHidden.Checked = true;
                chkCopyFiles.Checked = true;
                chkGenerateIndex.Checked = true;
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
                        chkCopyFiles.Checked = line.Substring(9).Trim() == "1";
                    else if (line.StartsWith("GenerateIndex="))
                        chkGenerateIndex.Checked = line.Substring(13).Trim() == "1";
                }
                SetWatermarkIfNeeded();
            }
            catch
            {
                // 加载失败时使用默认
                chkIgnoreHidden.Checked = true;
                chkCopyFiles.Checked = true;
                chkGenerateIndex.Checked = true;
            }
        }

        // 保存当前配置到 history.ini
        private void SaveHistory(string target, string output, string extensions, string skipPatterns,
            bool enableSkip, bool ignoreHidden, bool copyFiles, bool generateIndex)
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
            }
            catch { /* 忽略写入错误 */ }
        }
    }
}