using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows;

using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;



namespace SciTrader.ViewModels
{
	public abstract class ViewModel : IDisposable
	{
		public string BindableName { get { return GetBindableName(DisplayName); } }
		public virtual string DisplayName { get; set; }
		public virtual ImageSource Glyph { get; set; }

		string GetBindableName(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name), "Value cannot be null.");
			}
			return "_" + Regex.Replace(name, @"\W", "");
		}

		#region IDisposable Members
		public virtual void Dispose()
		{
			OnDispose();
		}
		protected virtual void OnDispose() { }
#if DEBUG
		~ViewModel()
		{
			string msg = string.Format("{0} ({1}) ({2}) Finalized", GetType().Name, DisplayName, GetHashCode());
			System.Diagnostics.Debug.WriteLine(msg);
		}
#endif
		#endregion
	}
}
