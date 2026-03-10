using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SourceCollector
{
	internal static class Program
	{
		// 预定义类型及其扩展名映射（不区分大小写）
		private static readonly Dictionary<string, string[]> TypeExtensions = new(StringComparer.OrdinalIgnoreCase)
		{
			["-A"] = new[] { ".mp3", ".mp2", ".mpga", ".m4a", ".m4b", ".m4p", ".m4r", ".aac", ".ac3", ".eac3", ".dts", ".mlp", ".wav", ".wave", ".bwf", ".aiff", ".aif", ".aifc", ".au", ".snd", ".flac", ".alac", ".ape", ".mac", ".wv", ".wavpack", ".shn", ".tta", ".tak", ".ofr", ".ofs", ".spx", ".ogg", ".oga", ".mogg", ".opus", ".ra", ".ram", ".rm", ".rma", ".wma", ".asf", ".wm", ".wmx", ".wvx", ".mid", ".midi", ".rmi", ".kar", ".mod", ".s3m", ".xm", ".it", ".mtm", ".umx", ".amf", ".ams", ".dsm", ".far", ".mdl", ".med", ".okt", ".ptm", ".stm", ".ult", ".uni", ".mt2", ".psm", ".mptm", ".flp", ".als", ".aup", ".aup3", ".cpr", ".npr", ".sesx", ".ptx", ".ptf", ".ptx", ".sng", ".vox", ".voc", ".8svx", ".sln", ".sam", ".smpl", ".sf2", ".sfz", ".gig", ".dls", ".bank", ".w64", ".rf64" },
			["-I"] = new[] { ".jpg", ".jpeg", ".jpe", ".jif", ".jfif", ".png", ".apng", ".gif", ".bmp", ".dib", ".tiff", ".tif", ".webp", ".avif", ".heic", ".heif", ".bpg", ".exr", ".hdr", ".pic", ".tga", ".pcx", ".pbm", ".pgm", ".ppm", ".pnm", ".pfm", ".dds", ".ico", ".cur", ".svg", ".svgz", ".ai", ".eps", ".epi", ".ps", ".psb", ".pdd", ".psd", ".xcf", ".cr2", ".cr3", ".crw", ".nef", ".nrw", ".arw", ".dng", ".orf", ".rw2", ".sr2", ".srw", ".raf", ".x3f", ".pef", ".3fr", ".fff", ".iiq", ".eip", ".mef", ".mos", ".kdc", ".dcr", ".kc2", ".mrw", ".bay", ".cs1", ".cap", ".iiq", ".eip", ".fff", ".mef", ".mos", ".nrw", ".orf", ".pef", ".rw2", ".srw", ".x3f", ".jng", ".mng", ".wmf", ".emf", ".cgm", ".cdr", ".cmx", ".dxf", ".dwg", ".hpgl", ".plt", ".cgm" },
			["-M"] = new[] { ".obj", ".fbx", ".stl", ".3ds", ".max", ".blend", ".dae", ".gltf", ".glb", ".usd", ".usda", ".usdc", ".usdz", ".ma", ".mb", ".c4d", ".lwo", ".lws", ".dxf", ".dwg", ".step", ".stp", ".iges", ".igs", ".x3d", ".x3dv", ".vrml", ".wrl", ".b3d", ".md2", ".md3", ".md5", ".smd", ".vta", ".psk", ".psa", ".abc", ".ply", ".pdb", ".off", ".mesh", ".kmz", ".scn", ".zpr", ".sldprt", ".sldasm", ".3mf", ".amf", ".wings", ".bvh", ".mot" },
			["-V"] = new[] { ".mp4", ".m4v", ".mp4v", ".mpv4", ".mpeg", ".mpg", ".mpe", ".m1v", ".m2v", ".mp2v", ".m2p", ".m2t", ".m2ts", ".mts", ".mt2s", ".ts", ".tp", ".trp", ".vob", ".ifo", ".bup", ".evo", ".mkv", ".mk3d", ".mka", ".mks", ".webm", ".avi", ".divx", ".xvid", ".mov", ".qt", ".3gp", ".3g2", ".asf", ".wmv", ".wmx", ".wvx", ".flv", ".f4v", ".f4p", ".f4a", ".f4b", ".swf", ".rm", ".rmvb", ".ra", ".ram", ".rv", ".rma", ".ogv", ".ogm", ".ogx", ".mxf", ".gxf", ".lxf", ".dv", ".dvr-ms", ".wtv", ".mpc", ".mpcpl", ".mpls", ".bdmv", ".m2ts", ".mts", ".m2t", ".m2ts", ".iso", ".img", ".bin", ".dat", ".fli", ".flc", ".flic", ".mjpg", ".mjpeg", ".mj2", ".mjp", ".avchd", ".mod", ".tod", ".mpeg4", ".h264", ".h265", ".hevc", ".264", ".265", ".vc1", ".vp8", ".vp9", ".av1" },
			["-c"] = new[] { ".c", ".h", ".cpp", ".hpp", ".cxx", ".hxx", ".cc", ".hh", ".inl" },
			["-cc"] = new[] { ".cs", ".xaml" },
			["-css"] = new[] { ".css", ".scss", ".sass", ".less", ".styl" },
			["-cfg"] = new[] { ".ini", ".json", ".cfg", ".conf", ".properties", ".toml", ".yaml" },
			["-dll"] = new[] { ".dll", ".ocx", ".sys" },
			["-excel"] = new[] { ".xls", ".xlsx", ".xlsm", ".xlsb", ".xlt", ".xltx", ".xltm", ".xla", ".xlam", ".xlw", ".csv" },
			["-exe"] = new[] { ".exe", ".msi", ".bat", ".cmd" },
			["-font"] = new[] { ".ttf", ".otf", ".woff", ".woff2", ".eot" },
			["-html"] = new[] { ".html", ".htm", ".xhtml", ".shtml", ".dhtml" },
			["-java"] = new[] { ".java", ".jar" },
			["-office"] = new[] { ".doc", ".docx", ".docm", ".dot", ".dotx", ".dotm", ".rtf", ".xls", ".xlsx", ".xlsm", ".xlsb", ".xlt", ".xltx", ".xltm", ".xla", ".xlam", ".xlw", ".csv", ".ppt", ".pptx", ".pptm", ".pot", ".potx", ".potm", ".pps", ".ppsx", ".ppsm", ".sldx", ".sldm", ".bas", ".cls", ".frm" },
			["-pdf"] = new[] { ".pdf" },
			["-php"] = new[] { ".php", ".php3", ".php4", ".php5", ".php7", ".php8", ".phtml", ".phpt", ".phps", ".inc", ".module", ".theme", ".engine", ".install", ".profile", ".phar", ".hh", ".hack", ".ctp", ".tpl", ".twig" },
			["-powerpoint"] = new[] { ".ppt", ".pptx", ".pptm", ".pot", ".potx", ".potm", ".pps", ".ppsx", ".ppsm", ".sldx", ".sldm" },
			["-py"] = new[] { ".py", ".pyc", ".pyo", ".pyd", ".pyw", ".pyi" },
			["-script"] = new[] { ".sh", ".bash", ".zsh", ".fish" },
			["-sql"] = new[] { ".sql", ".db", ".sqlite", ".mdb", ".accdb" },
			["-txt"] = new[] { ".rtf", ".csv", ".tsv", ".tab", ".diz", ".readme", ".1", ".2", ".3", ".4", ".5", ".6", ".7", ".8", ".9", ".man", ".ms", ".mm", ".me", ".pod", ".asciidoc", ".adoc", ".wiki", ".mediawiki", ".creole", ".textile", ".rdoc", ".rd", ".rmd", ".qmd", ".diff", ".patch", ".changes", ".news", ".changelog", ".copying", ".license", ".tex", ".ltx", ".cls", ".sty", ".bst", ".bib", ".asc", ".brf", ".eml", ".mbox", ".dtd", ".ent", ".tld", ".csl", ".xml", ".xhtml", ".htm", ".html" },
			["-vb"] = new[] { ".vb", ".vbs", ".vba" },
			["-word"] = new[] { ".doc", ".docx", ".docm", ".dot", ".dotx", ".dotm", ".rtf" },
			["-json"] = new[] { ".json" },
			["-yaml"] = new[] { ".yml", ".yaml" },
			["-xml"] = new[] { ".xml", ".xsd", ".xslt", ".xsl", ".dtd", ".xhtml" },
			["-yml"] = new[] { ".yml", ".yaml" },
			["-zip"] = new[] { ".tgz", ".tbz2", ".txz", ".tzst", ".tbr", ".tlz", ".tlzma", ".zst", ".zstd", ".br", ".brotli", ".lz", ".lzma", ".lzo", ".lz4", ".lzop", ".sz", ".rz", ".lrz", ".z", ".cab", ".arj", ".ace", ".lzh", ".lha", ".zoo", ".arc", ".rk", ".pak", ".pkg", ".dmg", ".img", ".bin", ".cue", ".nrg", ".mdf", ".mds", ".ccd", ".sub", ".daa", ".uif", ".wim", ".swm", ".esd", ".vhd", ".vhdx", ".vmdk", ".qcow2", ".vdi", ".hdd", ".fvd", ".ova", ".ovf", ".deb", ".rpm", ".snap", ".flatpak", ".flatpakref", ".appimage", ".crx", ".xpi", ".vsix", ".msu", ".appx", ".appxbundle", ".msix", ".jar", ".war", ".ear", ".sar", ".apk", ".ipa", ".xapk", ".obb", ".aab" }
		};

		// 控制选项（无参数）
		private static readonly string[] ControlOptions = { "-no_hidden", "-help" };
		// 带参数选项
		private const string SkipOption = "-skip";
		private const string CustomExtOption = "-ex_sn";

		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				// 无参数：启动 UI
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new MainForm());
				return;
			}

			// 处理 -help
			if (args.Contains("-help", StringComparer.OrdinalIgnoreCase))
			{
				ShowHelp();
				return;
			}

			// 解析参数：至少需要目标路径、输出路径和一个后缀选项
			if (args.Length < 3)
			{
				Console.WriteLine("错误：至少需要指定目标目录、输出目录和一个后缀选项。");
				ShowHelp();
				return;
			}

			string targetDir = args[0];
			string outputDir = args[1];

			if (!Directory.Exists(targetDir))
			{
				Console.WriteLine($"错误：目标目录不存在 - {targetDir}");
				return;
			}

			// 解析选项
			bool noHidden = false;
			string skipPatterns = string.Empty;          // 取第一个 -skip 的值
			HashSet<string> extensions = new(StringComparer.OrdinalIgnoreCase);
			bool hasExtensionOption = false;
			bool matchAll = false;                        // -all 选项
			bool copyFiles = false;                        // -copy_file
			bool generateIndex = false;                    // -index_file

			for (int i = 2; i < args.Length; i++)
			{
				string arg = args[i];
				if (arg.Equals("-no_hidden", StringComparison.OrdinalIgnoreCase))
				{
					noHidden = true;
				}
				else if (arg.Equals(SkipOption, StringComparison.OrdinalIgnoreCase))
				{
					// 只取第一个有效值
					if (string.IsNullOrEmpty(skipPatterns) && i + 1 < args.Length && !args[i + 1].StartsWith("-"))
					{
						skipPatterns = args[++i];
					}
					// 忽略后续 -skip 或无参数的情况
				}
				else if (arg.Equals(CustomExtOption, StringComparison.OrdinalIgnoreCase))
				{
					if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
					{
						string extList = args[++i];
						foreach (string ext in extList.Split('|', StringSplitOptions.RemoveEmptyEntries))
						{
							string trimmed = ext.Trim();
							if (!string.IsNullOrEmpty(trimmed))
							{
								if (!trimmed.StartsWith(".")) trimmed = "." + trimmed;
								extensions.Add(trimmed);
								hasExtensionOption = true;
							}
						}
					}
				}
				else if (arg.Equals("-all", StringComparison.OrdinalIgnoreCase))
				{
					matchAll = true;
					hasExtensionOption = true;
				}
				else if (arg.Equals("-copy_file", StringComparison.OrdinalIgnoreCase))
				{
					copyFiles = true;
				}
				else if (arg.Equals("-index_file", StringComparison.OrdinalIgnoreCase))
				{
					generateIndex = true;
				}
				else if (TypeExtensions.ContainsKey(arg))
				{
					// 预定义类型
					foreach (string ext in TypeExtensions[arg])
						extensions.Add(ext);
					hasExtensionOption = true;
				}
				else if (arg.StartsWith("-"))
				{
					Console.WriteLine($"警告：忽略未知选项 {arg}");
				}
				else
				{
					Console.WriteLine($"警告：意外的参数 {arg}，请检查命令格式。");
				}
			}

			if (!hasExtensionOption)
			{
				Console.WriteLine("错误：必须至少指定一个后缀选项（预定义类型、-ex_sn 或 -all）。");
				return;
			}

			// 默认行为：如果两个操作选项都未指定，则两者都做
			if (!copyFiles && !generateIndex)
			{
				copyFiles = true;
				generateIndex = true;
			}

			// 准备屏蔽列表（从 skipPatterns 解析）
			HashSet<string> skipNames = new(StringComparer.OrdinalIgnoreCase);
			if (!string.IsNullOrEmpty(skipPatterns))
			{
				foreach (string name in skipPatterns.Split('|', StringSplitOptions.RemoveEmptyEntries))
				{
					string trimmed = name.Trim();
					if (!string.IsNullOrEmpty(trimmed))
						skipNames.Add(trimmed);
				}
			}

			// 执行复制（命令行模式，无界面日志用 Console）
			try
			{
				ProcessCommandLine(targetDir, outputDir, extensions, skipNames, noHidden, copyFiles, generateIndex, matchAll);
				// 保存本次配置到历史文件，供下次 UI 启动时加载
				SaveHistory(targetDir, outputDir, extensions, skipNames, noHidden, copyFiles, generateIndex, matchAll);
				Console.WriteLine("操作完成。");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"处理失败：{ex.Message}");
			}
		}

		private static void ProcessCommandLine(string targetDir, string outputDir, HashSet<string> extensions,
			HashSet<string> skipNames, bool noHidden, bool copyFiles, bool generateIndex, bool matchAll)
		{
			Directory.CreateDirectory(outputDir);
			var root = new DirectoryInfo(targetDir);
			List<string>? filesToCopy = copyFiles ? new List<string>() : null;
			StringBuilder? treeBuilder = generateIndex ? new StringBuilder() : null;
			treeBuilder?.AppendLine(root.Name + "/");

			// 递归遍历
			BuildTree(root, "", true, treeBuilder, filesToCopy, extensions, skipNames, noHidden, matchAll, Console.WriteLine);

			// 复制文件
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
						Console.WriteLine($"复制失败：{src} -> {dest}，原因：{ex.Message}");
						failed++;
					}
				}
			}

			// 写入目录树
			if (generateIndex && treeBuilder != null)
			{
				string treeFile = Path.Combine(outputDir, "目录.txt");
				File.WriteAllText(treeFile, treeBuilder.ToString(), Encoding.UTF8);
				Console.WriteLine($"目录树已保存：{treeFile}");
			}

			if (copyFiles)
				Console.WriteLine($"共复制 {copied} 个文件，失败 {failed} 个。");
		}

		// 与 UI 共用的 BuildTree 方法（通过 Action<string> 输出日志）
		private static void BuildTree(DirectoryInfo dir, string prefix, bool isLast, StringBuilder? sb,
			List<string>? files, HashSet<string> extensions, HashSet<string> skipNames, bool noHidden, bool matchAll,
			Action<string> log)
		{
			// 获取子目录（排除隐藏和屏蔽名称）
			List<DirectoryInfo> subDirs = new();
			try
			{
				subDirs = dir.GetDirectories()
					.Where(d =>
					{
						try
						{
							if (noHidden && d.Attributes.HasFlag(FileAttributes.Hidden))
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

			// 获取文件（排除隐藏和屏蔽名称，后缀匹配）
			List<FileInfo> fileInfos = new();
			try
			{
				fileInfos = dir.GetFiles()
					.Where(f =>
					{
						try
						{
							if (noHidden && f.Attributes.HasFlag(FileAttributes.Hidden))
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
					BuildTree(subDir, newPrefix, last, sb, files, extensions, skipNames, noHidden, matchAll, log);
				}
				else if (item is FileInfo file)
				{
					files?.Add(file.FullName);
				}
			}
		}

		private static void ShowHelp()
		{
			Console.WriteLine("用法：");
			Console.WriteLine("  SourceCollector.exe <目标目录> <输出目录> [选项...]");
			Console.WriteLine("  SourceCollector.exe -help");
			Console.WriteLine();
			Console.WriteLine("选项（按ASCII升序）：");

			// 收集所有选项并排序
			var allOptions = new List<string>();
			allOptions.AddRange(TypeExtensions.Keys);
			allOptions.AddRange(ControlOptions);
			allOptions.Add(SkipOption);
			allOptions.Add(CustomExtOption);
			allOptions.Add("-all");
			allOptions.Add("-copy_file");
			allOptions.Add("-index_file");
			allOptions.Sort(StringComparer.Ordinal);

			foreach (string opt in allOptions)
			{
				if (TypeExtensions.TryGetValue(opt, out var exts))
				{
					Console.WriteLine($"  {opt,-10} 包含扩展名：{string.Join(" ", exts)}");
				}
				else if (opt == SkipOption)
				{
					Console.WriteLine($"  {SkipOption,-10} 后跟 \"名称1|名称2|...\"，跳过匹配的文件/文件夹（仅第一个有效）");
				}
				else if (opt == CustomExtOption)
				{
					Console.WriteLine($"  {CustomExtOption,-10} 后跟 \".ext1|.ext2|...\"，自定义后缀（可与预定义类型并用）");
				}
				else if (opt == "-no_hidden")
				{
					Console.WriteLine($"  -no_hidden  忽略隐藏文件和文件夹");
				}
				else if (opt == "-help")
				{
					Console.WriteLine($"  -help        显示此帮助");
				}
				else if (opt == "-all")
				{
					Console.WriteLine($"  -all         匹配所有文件（忽略扩展名）");
				}
				else if (opt == "-copy_file")
				{
					Console.WriteLine($"  -copy_file   复制匹配的文件（默认与 -index_file 同时启用）");
				}
				else if (opt == "-index_file")
				{
					Console.WriteLine($"  -index_file  生成目录树（默认与 -copy_file 同时启用）");
				}
			}

			Console.WriteLine();
			Console.WriteLine("示例：");
			Console.WriteLine("  SourceCollector.exe D:\\Src D:\\Out -c -cc -no_hidden -skip \"bin|obj\"");
			Console.WriteLine("  SourceCollector.exe D:\\Src D:\\Out -ex_sn \".cs|.xaml\" -I");
			Console.WriteLine("  SourceCollector.exe D:\\Src D:\\Out -all -copy_file         # 只复制所有文件，不生成目录树");
		}

		private static void SaveHistory(string target, string output, HashSet<string> extensions,
			HashSet<string> skipNames, bool noHidden, bool copyFiles, bool generateIndex, bool matchAll)
		{
			string historyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "history.ini");
			try
			{
				using var writer = new StreamWriter(historyPath, false, Encoding.UTF8);
				writer.WriteLine($"[General]");
				writer.WriteLine($"Target={target}");
				writer.WriteLine($"Output={output}");
				string extString = matchAll ? ".*" : string.Join("|", extensions);
				writer.WriteLine($"Extensions={extString}");
				writer.WriteLine($"SkipPatterns={string.Join("|", skipNames)}");
				writer.WriteLine($"EnableSkip={(skipNames.Count > 0 ? 1 : 0)}");
				writer.WriteLine($"IgnoreHidden={(noHidden ? 1 : 0)}");
				writer.WriteLine($"CopyFiles={(copyFiles ? 1 : 0)}");
				writer.WriteLine($"GenerateIndex={(generateIndex ? 1 : 0)}");
			}
			catch { /* 忽略写入错误 */ }
		}
	}
}