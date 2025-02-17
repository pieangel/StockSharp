using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.ViewportManagers;
using SciChart.Examples.ExternalDependencies.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SciChart.Charting.Common.Helpers;
using SciChart.Examples.ExternalDependencies.Common;

namespace SciTrader.ViewModels
{
    public class BindToDataSeriesSetViewModel : BaseViewModel
    {
        private IUniformXyDataSeries<double> _dataSeries0;
        private readonly RandomWalkGenerator _dataSource;

        public BindToDataSeriesSetViewModel()
        {
            ViewportManager = new DefaultViewportManager();

            // Create a DataSeriesSet
            _dataSeries0 = new UniformXyDataSeries<double>();

            // Create a single data-series
            _dataSource = new RandomWalkGenerator();

            // Append data to series
            _dataSeries0.Append(_dataSource.GetRandomWalkYData(1000));
        }

        // Databound to via SciChartSurface.DataSet in the view
        public IUniformXyDataSeries<double> ChartData
        {
            get => _dataSeries0;
            set
            {
                _dataSeries0 = value;
                OnPropertyChanged("ChartData");
            }
        }

        public IViewportManager ViewportManager { get; set; }

        public ICommand AppendDataCommand => new ActionCommand(AppendData);

        // Called when the AppendDataCommand is invoked via button click on the view
        private void AppendData()
        {
            _dataSeries0.Append(_dataSource.GetRandomWalkYData(50));

            ViewportManager.ZoomExtents();
        }
    }
}
