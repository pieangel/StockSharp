using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;

using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.POCO;

namespace SciTrader.ViewModels
{
	public class Solution : BindableBase
	{
		string[] codePaths = new string[] {
			"MainWindow.xaml",
			"MainWindow.xaml.cs",
			"Resources.xaml",
			"BarTemplateSelector.cs",
			"MainViewModel.cs"
		};
		int newItemsCount;

		protected Solution()
		{
			SolutionItem root = SolutionItem.Create(this, "WPFApplication", ImagePaths.SolutionExplorer);
			SolutionItem properties = SolutionItem.Create(this, "Properties", ImagePaths.PropertiesWindow);
			SolutionItem references = SolutionItem.Create(this, "References", ImagePaths.References);
			root.Items.Add(properties);
			root.Items.Add(references);
			var files = GetCodeFiles();
			foreach (SolutionItem file in files)
			{
				root.Items.Add(file);
			}
			LastOpenedItem = files.FirstOrDefault();
			Items = new ObservableCollection<SolutionItem> { root };
		}

		public ObservableCollection<SolutionItem> Items { get; private set; }
		public virtual SolutionItem LastOpenedItem { get; set; }

		public static Solution Create()
		{
			return ViewModelSource.Create(() => new Solution());
		}
		public string AddNewItemToRoot()
		{
			newItemsCount++;
			string newItemName = string.Format("Class{0}.cs", newItemsCount);
			var solutionItem = SolutionItem.CreateFile(this, newItemName);
			(Items[0] as SolutionItem).Items.Add(solutionItem);
			return newItemName;
		}
		List<SolutionItem> GetCodeFiles()
		{
			var result = new List<SolutionItem>();
			var subFiles = new List<SolutionItem>();
			foreach (var codePath in codePaths)
				if (codePath.EndsWith(".xaml.cs") || codePath.EndsWith(".xaml.vb"))
					subFiles.Add(SolutionItem.CreateFile(this, codePath));
				else
					result.Add(SolutionItem.CreateFile(this, codePath));
			foreach (var subFile in subFiles)
			{
				var xamlFile = result.FirstOrDefault(x => x.FilePath == subFile.FilePath.Replace(".xaml.cs", ".xaml").Replace(".xaml.vb", ".xaml"));
				if (xamlFile == null)
					result.Add(subFile);
				else
					xamlFile.Items.Add(subFile);
			}
			return result;
		}
	}

	public class SolutionExplorerViewModel : PanelWorkspaceViewModel
	{
		public SolutionExplorerViewModel()
		{
			DisplayName = "Solution Explorer";
			Glyph = Images.SolutionExplorer;
			PropertiesWindow = Images.PropertiesWindow;
			ShowAllFiles = Images.ShowAllFiles;
			Refresh = Images.Refresh;
		}

		public event EventHandler<SolutionItemOpeningEventArgs> ItemOpening;

		public ImageSource PropertiesWindow { get; set; }
		public ImageSource Refresh { get; set; }
		public ImageSource ShowAllFiles { get; set; }
		public Solution Solution { get; set; }
		protected override string WorkspaceName { get { return "RightHost"; } }

		public void OpenItem(SolutionItem item)
		{
			if (item != null && item.IsFile && ItemOpening != null)
				ItemOpening.Invoke(this, new SolutionItemOpeningEventArgs(item));
		}
	}

	public class SolutionItem
	{
		readonly Solution solution;

		protected SolutionItem(Solution solution)
		{
			this.solution = solution;
			Items = new ObservableCollection<SolutionItem>();
		}

		public string DisplayName { get; private set; }
		public string FilePath { get; private set; }
		public string GlyphPath { get; private set; }
		public bool IsFile { get { return FilePath != null; } }
		public ObservableCollection<SolutionItem> Items { get; private set; }

		public static SolutionItem Create(Solution solution, string displayName, string glyph)
		{
			var solutionItem = ViewModelSource.Create(() => new SolutionItem(solution));
			solutionItem.Do(x =>
			{
				x.DisplayName = displayName;
				x.GlyphPath = glyph;
			});
			return solutionItem;
		}
		public static SolutionItem CreateFile(Solution solution, string path)
		{
			var solutionItem = ViewModelSource.Create(() => new SolutionItem(solution));
			solutionItem.Do(x =>
			{
				x.DisplayName = Path.GetFileName(path);
				x.GlyphPath = Path.GetExtension(path) == ".cs" ? ImagePaths.FileCS : ImagePaths.FileXaml;
				x.FilePath = path;
			});
			return solutionItem;
		}
	}

	public class SolutionItemOpeningEventArgs : EventArgs
	{
		public SolutionItemOpeningEventArgs(SolutionItem solutionItem)
		{
			SolutionItem = solutionItem;
		}

		public SolutionItem SolutionItem { get; set; }
	}
}
