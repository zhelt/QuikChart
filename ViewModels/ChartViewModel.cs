using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using QuikChart.Infrastructure.Commands;
using QuikChart.ViewModels.Base;

namespace QuikChart.ViewModels
{
    class ChartViewModel : ViewModel
    {

        public PlotModel DataPlot { get; set; }
        private double _xValue = 1;

        public LinearAxis XAxis { get; set; }
        public LinearAxis YAxis { get; set; }

        private Dispatcher disp;

        public ChartViewModel()
        {
             XAxis = new OxyPlot.Axes.LinearAxis();
             XAxis.Title = "X";
             XAxis.Position = AxisPosition.Bottom;

            //Define Y-Axis
            YAxis = new OxyPlot.Axes.LinearAxis();
            YAxis.Title = "Y";
            YAxis.Position = AxisPosition.Left;


            disp = Dispatcher.CurrentDispatcher;
            DataPlot = new PlotModel();
            DataPlot.Series.Add(new LineSeries());
            DataPlot.Axes.Add(XAxis);
            DataPlot.Axes.Add(YAxis);

            TestCommand = new LambdaCommand(OnTestCommandExecuted, CanTestCommandExecuted);
            //var dispatcherTimer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 1)};
            //dispatcherTimer.Tick += dispatcherTimer_Tick;
            //dispatcherTimer.Start();
        }

        #region TestCommand


        public ICommand TestCommand { get; }

        private void OnTestCommandExecuted(object p)
        {
            AddDot();
        }
        private bool CanTestCommandExecuted(object p) => true;

        #endregion

        public async void AddDot()
        {
            await Task.Run((Action)(() =>
            {
                disp.Invoke(() =>
                {
                    (DataPlot.Series[0] as LineSeries).Points.Add(new DataPoint(_xValue, Math.Sqrt(_xValue)));
                    XAxis.Zoom(_xValue-10, _xValue + 10);
                    YAxis.Zoom(0, 20);

                    DataPlot.InvalidatePlot(true);
                    _xValue++;
                });
            }));
        }

    }
}
