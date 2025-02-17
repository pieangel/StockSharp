using DevExpress.Mvvm.POCO;
using System.Collections.Generic;
using SciTrader.Data;

namespace SciTrader.ViewModels {
    public class OpenOrdersViewModel : PanelWorkspaceViewModel
    {
        public static OpenOrdersViewModel Create(List<OpenOrderData> orders) {
            return ViewModelSource.Create(() => new OpenOrdersViewModel(orders));
        }

        readonly List<OpenOrderData> ordersDataSource;

        public List<OpenOrderData> OrdersDataSource { get { return ordersDataSource; } }

        public OpenOrdersViewModel(List<OpenOrderData> orders) {
            ordersDataSource = orders;
        }

        public OpenOrdersViewModel()
        {
            DisplayName = "Open Orders";
            Glyph = Images.Output;
        }

        protected override string WorkspaceName { get { return "Toolbox"; } }
    }
}
