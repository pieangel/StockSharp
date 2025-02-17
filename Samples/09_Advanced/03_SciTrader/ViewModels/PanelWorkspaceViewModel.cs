using DevExpress.Xpf.Docking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciTrader.ViewModels
{
	abstract public class PanelWorkspaceViewModel : WorkspaceViewModel, IMVVMDockingProperties
	{
		string _targetName;

		protected PanelWorkspaceViewModel()
		{
			_targetName = WorkspaceName;
		}

		abstract protected string WorkspaceName { get; }
		string IMVVMDockingProperties.TargetName
		{
			get { return _targetName; }
			set { _targetName = value; }
		}

		public virtual void OpenItemByPath(string path) { }
	}
}
