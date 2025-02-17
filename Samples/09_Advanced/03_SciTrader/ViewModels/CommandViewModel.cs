using DevExpress.Xpf.Bars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SciTrader.ViewModels
{
	public class CommandViewModel : ViewModel
	{
		public CommandViewModel() { }
		public CommandViewModel(string displayName, List<CommandViewModel> subCommands)
			: this(displayName, null, null, subCommands)
		{
		}
		public CommandViewModel(string displayName, ICommand command = null)
			: this(displayName, null, command, null)
		{
		}
		public CommandViewModel(WorkspaceViewModel owner, ICommand command)
			: this(string.Empty, owner, command)
		{
		}
		private CommandViewModel(string displayName, WorkspaceViewModel owner = null, ICommand command = null, List<CommandViewModel> subCommands = null)
		{
			IsEnabled = true;
			Owner = owner;
			if (Owner != null)
			{
				DisplayName = Owner.DisplayName;
				Glyph = Owner.Glyph;
			}
			else DisplayName = displayName;
			Command = command;
			Commands = subCommands;
		}

		public ICommand Command { get; private set; }
		public List<CommandViewModel> Commands { get; set; }
		public BarItemDisplayMode DisplayMode { get; set; }
		public bool IsComboBox { get; set; }
		public bool IsEnabled { get; set; }
		public bool IsSeparator { get; set; }
		public bool IsSubItem { get; set; }
		public KeyGesture KeyGesture { get; set; }
		public WorkspaceViewModel Owner { get; private set; }
	}
}
