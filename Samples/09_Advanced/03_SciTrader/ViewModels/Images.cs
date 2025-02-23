using DevExpress.Xpf.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace SciTrader.ViewModels
{
	public static class Images
	{
		public static ImageSource About { get { return GetSvgImage(ImagePaths.About); } }
		public static ImageSource CloseSolution { get { return GetSvgImage(ImagePaths.CloseSolution); } }
		public static ImageSource Copy { get { return GetSvgImage(ImagePaths.Copy); } }
		public static ImageSource Cut { get { return GetSvgImage(ImagePaths.Cut); } }
		public static ImageSource DevExpressLogo { get { return GetImage(ImagePaths.DevExpressLogo); } }
		public static ImageSource Error { get { return GetSvgImage(ImagePaths.Error); } }
		public static ImageSource File { get { return GetSvgImage(ImagePaths.File); } }
		public static ImageSource FileCS { get { return GetSvgImage(ImagePaths.FileCS); } }
		public static ImageSource FileXaml { get { return GetSvgImage(ImagePaths.FileXaml); } }
		public static ImageSource Find { get { return GetSvgImage(ImagePaths.Find); } }
		public static ImageSource FindInFiles { get { return GetSvgImage(ImagePaths.FindInFiles); } }
		public static ImageSource FindInFilesWindow { get { return GetSvgImage(ImagePaths.FindInFilesWindow); } }
		public static ImageSource Info { get { return GetSvgImage(ImagePaths.Info); } }
		public static ImageSource LoadLayout { get { return GetSvgImage(ImagePaths.LoadLayout); } }
		public static ImageSource NewProject { get { return GetSvgImage(ImagePaths.NewProject); } }
		public static ImageSource OpenFile { get { return GetSvgImage(ImagePaths.OpenFile); } }
		public static ImageSource OpenSolution { get { return GetSvgImage(ImagePaths.OpenSolution); } }
		public static ImageSource Output { get { return GetSvgImage(ImagePaths.Output); } }
		public static ImageSource Paste { get { return GetSvgImage(ImagePaths.Paste); } }
		public static ImageSource PropertiesWindow { get { return GetSvgImage(ImagePaths.PropertiesWindow); } }
		public static ImageSource Redo { get { return GetSvgImage(ImagePaths.Redo); } }
		public static ImageSource References { get { return GetSvgImage(ImagePaths.References); } }
		public static ImageSource Refresh { get { return GetSvgImage(ImagePaths.Refresh); } }
		public static ImageSource Replace { get { return GetSvgImage(ImagePaths.Replace); } }
		public static ImageSource Run { get { return GetSvgImage(ImagePaths.Run); } }
		public static ImageSource Save { get { return GetSvgImage(ImagePaths.Save); } }
		public static ImageSource SaveAll { get { return GetSvgImage(ImagePaths.SaveAll); } }
		public static ImageSource SaveLayout { get { return GetSvgImage(ImagePaths.SaveLayout); } }
		public static ImageSource ShowAllFiles { get { return GetSvgImage(ImagePaths.ShowAllFiles); } }
		public static ImageSource SolutionExplorer { get { return GetSvgImage(ImagePaths.SolutionExplorer); } }
		public static ImageSource TaskList { get { return GetSvgImage(ImagePaths.TaskList); } }
		public static ImageSource Toolbox { get { return GetSvgImage(ImagePaths.Toolbox); } }
		public static ImageSource Undo { get { return GetSvgImage(ImagePaths.Undo); } }
		public static ImageSource Warning { get { return GetSvgImage(ImagePaths.Warning); } }

		public static ImageSource Connect { get { return GetSvgImage(ImagePaths.Connect); } }
		public static ImageSource Disconnect { get { return GetSvgImage(ImagePaths.Disconnect); } }
		public static ImageSource Settings { get { return GetSvgImage(ImagePaths.Settings); } }


		static ImageSource GetImage(string path)
		{
			return new BitmapImage(new Uri(path, UriKind.Relative));
		}

		static ImageSource GetSvgImage(string path)
		{
			var svgImageSource = new SvgImageSourceExtension() { Uri = new Uri(path) }.ProvideValue(null);
			return (ImageSource)svgImageSource;
		}
	}
}
