using DevExpress.Utils;
using DevExpress.Xpf.DemoBase.Helpers.TextColorizer;
using DevExpress.Xpf.DemoBase.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace SciTrader.ViewModels
{
    public class DocumentViewModel : PanelWorkspaceViewModel
    {
        public SciStockChartViewModel ChartViewModel { get; set; }
        //public TestViewModel ChartViewModel { get; set; }
        public DocumentViewModel()
        {
            IsClosed = false;
            ChartViewModel = new SciStockChartViewModel();
        }
        public DocumentViewModel(string displayName, string text) : this()
        {
            ChartViewModel = new SciStockChartViewModel();
            DisplayName = displayName;
            CodeLanguageText = new CodeLanguageText(CodeLanguage.CS, text);
        }

        public CodeLanguage ModelCodeLanguage { get; private set; }
        public CodeLanguageText CodeLanguageText { get; private set; }
        public string Description { get; protected set; }
        public string FilePath { get; protected set; }
        public string Footer { get; protected set; }
        protected override string WorkspaceName { get { return "DocumentHost"; } }

        public bool OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Visual C# Files (*.cs)|*.cs|XAML Files (*.xaml)|*.xaml";
            openFileDialog.FilterIndex = 1;
            bool? dialogResult = openFileDialog.ShowDialog();
            bool dialogResultOK = dialogResult.HasValue && dialogResult.Value;
            if (dialogResultOK)
            {
                DisplayName = openFileDialog.SafeFileName;
                FilePath = openFileDialog.FileName;
                SetCodeLanguageProperties(Path.GetExtension(openFileDialog.SafeFileName));
                Stream fileStream = File.OpenRead(openFileDialog.FileName);
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    CodeLanguageText = new CodeLanguageText(ModelCodeLanguage, reader.ReadToEnd());
                }
                fileStream.Close();
            }
            return dialogResultOK;
        }
        public override void OpenItemByPath(string path)
        {
            DisplayName = Path.GetFileName(path);
            FilePath = path;
            SetCodeLanguageProperties(Path.GetExtension(path));
            CodeLanguageText = new CodeLanguageText(ModelCodeLanguage, () => { return GetCodeTextByPath(path); });
            IsActive = true;
        }
        string GenerateClassText(string className)
        {
            string text = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VisualStudioDocking {{
    class {0} {{
    }}
}}";
            return string.Format(text, className);
        }
        CodeLanguage GetCodeLanguage(string fileExtension)
        {
            switch (fileExtension)
            {
                case ".cs": return CodeLanguage.CS;
                case ".vb": return CodeLanguage.VB;
                case ".xaml": return CodeLanguage.XAML;
                default: return CodeLanguage.Plain;
            }
        }
        string GetCodeTextByPath(string path)
        {
            Assembly assembly = typeof(DocumentViewModel).Assembly;
            using (Stream stream = AssemblyHelper.GetResourceStream(assembly, path, true))
            {
                if (stream == null)
                    return GenerateClassText(Path.GetFileNameWithoutExtension(path));
                using (StreamReader reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }
        string GetDescription(CodeLanguage codeLanguage)
        {
            switch (codeLanguage)
            {
                case CodeLanguage.CS: return "Visual C# Source file";
                case CodeLanguage.VB: return "Visual Basic Source file";
                case CodeLanguage.XAML: return "Windows Markup File";
                default: return "Other file";
            }
        }
        void SetCodeLanguageProperties(string fileExtension)
        {
            ModelCodeLanguage = GetCodeLanguage(fileExtension);
            Description = GetDescription(ModelCodeLanguage);
            Footer = DisplayName;
            Glyph = ModelCodeLanguage.Equals(CodeLanguage.XAML) ? Images.FileXaml : ModelCodeLanguage.Equals(CodeLanguage.CS) ? Images.FileCS : null;
        }
    }
}
