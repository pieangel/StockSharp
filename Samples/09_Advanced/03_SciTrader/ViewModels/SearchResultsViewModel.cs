using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciTrader.ViewModels
{
	public class SearchResultsViewModel : PanelWorkspaceViewModel
	{
		public SearchResultsViewModel()
		{
			DisplayName = "Search Results";
			Glyph = Images.FindInFilesWindow;
			Text = @"Matching lines: 0    Matching files: 0    Total files searched: 61";
		}

		public string Text { get; private set; }
		protected override string WorkspaceName { get { return "BottomHost"; } }
	}
}
