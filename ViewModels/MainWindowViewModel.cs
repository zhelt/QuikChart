using System;
using System.Collections.Generic;
using QuikChart.Models;
using QuikChart.ViewModels.Base;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using OxyPlot.Axes;
using QuikChart.Infrastructure.Commands;
using QuikChart.StockMath;
using QuikChart.Views.Windows;
using QuikSharp;
using QuikSharp.DataStructures;

namespace QuikChart.ViewModels {
    class MainWindowViewModel : ViewModel
    {
        private readonly Dispatcher _currentDispatcher = Dispatcher.CurrentDispatcher;

        private double lastPrice = 0;

        private static Quik _quik;

        public ObservableCollection<OrderData> OrderDatas { get; set; } = new ObservableCollection<OrderData>()
        {
            new OrderData { TimeRange = "День", Total = 1000000, Bid = 500000, Offer = 500000, BidEnergy = 0.0001, OfferEnergy = 0.0001, BidPercentage = 50.5, OfferPercentage = 49.5 }, new OrderData { TimeRange = "12ч" }, new OrderData { TimeRange = "8ч" },
            new OrderData { TimeRange = "6ч" }, new OrderData { TimeRange = "3ч" }, new OrderData { TimeRange = "1ч" },
            new OrderData { TimeRange = "30мин" }, new OrderData { TimeRange = "15мин" }, new OrderData { TimeRange = "10мин" },
            new OrderData { TimeRange = "5мин" }, new OrderData { TimeRange = "1 мин" },
        };

        #region Trades Count

        public int BidCount = 0;
        public int OfferCount = 0;

        #endregion

        #region Trades

        public static int EnergyRange { get; set; } = 30;

        public double[,] BidEnergyComponents = new double[2, EnergyRange];
        public double[,] OfferEnergyComponents = new double[2, EnergyRange];

        #endregion

        #region Plot Title

        private string _plotTitle = "Цена";

        public string PlotTitle {
            get => _plotTitle;
            set => Set(ref _plotTitle, value);
        }

        #endregion

        #region MinMax

        private double _minimum;

        private double _maximum;

        public double Minimum {
            get => _minimum;
            set => Set(ref _minimum, value);
        }

        public double Maximum
        {
            get => _maximum;
            set => Set(ref _maximum, value);
        }

        #endregion

        #region ProgramTitle

        private string _title = "Quik Chart";

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        #endregion

        #region ChartPoints

        private ObservableCollection<QcTrade> _chartPoints = new ObservableCollection<QcTrade>();
        public ObservableCollection<QcTrade> ChartPoints { get => _chartPoints; set => Set(ref _chartPoints, value); }

        #endregion

        #region RunCommand

        public ICommand RunCommand { get; }

        private void OnRunCommandExecuted(object p)
        {
            Init();
            new Thread(() =>
            {
                Run();
            }).Start();
        }
        private bool CanRunCommandExecuted(object p) => true;

        #endregion

        #region LoadTextDataCommand

        public ICommand LoadTextDataCommand { get; }

        private async void OnLoadTextDataCommandExecuted(object p)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                    Filter = ".txt | *.txt"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                double lastFilePrice = 0;
                string[] lines = await File.ReadAllLinesAsync(openFileDialog.FileName);

                foreach (string line in lines)
                {
                    string[] data = line.Split(",");
                    await Task.Run(async () =>
                    {
                        await _currentDispatcher.InvokeAsync((Action) (() =>
                        {
                            if (data[^1] == "B")
                            {

                            }
                            else
                            {

                            }
                        }));
                    });
                }
            }
        }
        
        private bool CanLoadTextDataCommandExecuted(object p) => true;

        #endregion

        #region DistinctTradesCommand

        public ICommand DistinctTradesCommand { get; }

        private async void OnDistinctTradesCommandExecuted(object p)
        {
            await DistinctTrades();
        }

        private bool CanDistinctTradesCommandExecuted(object p) => true;

        #endregion

        #region ShowSettingsWindowCommand

        public ICommand ShowSettingsWindowCommand { get; }

        private void OnShowSettingsWindowCommandExecuted(object p)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Show();
        }
        private bool CanShowSettingsWindowCommandExecuted(object p) => true;

        #endregion

        public Task DistinctTrades()
        {
            return Task.CompletedTask;
        }

        public void Init()
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                _quik = new Quik(Quik.DefaultPort, new InMemoryStorage());
            }
            catch
            {
                Title = "Не подключено";
            }

            if (_quik != null)
            {
                Title = "Экземпляр Quik создан";
                try
                {
                    Title = _quik.Service.IsConnected().Result.ToString();
                }
                catch
                {
                    Title = "Соединение с сервером не установлено";
                }
            }
        }

        public void Run()
        {
            if (_quik != null)
            {
                _quik.Events.OnAllTrade += Event_OnAllTrade;
                Title = "Подписка на OnAllTrade удалась!";
            }
        }

        private async void Event_OnAllTrade(AllTrade trade) {

            Title = trade.TradeNum.ToString();


            string tradeType = "None";

            if ((trade.Flags & AllTradeFlags.Sell) != 0)
            {
                tradeType = "S";
                OfferCount += (int)trade.Qty;
            }
            else if ((trade.Flags & AllTradeFlags.Buy) != 0)
            {
                tradeType = "B";
                BidCount += (int)trade.Qty;
            }

            double bidPercentage = Formulas.CalculateRatio(BidCount, OfferCount);
            double offerPercentage = Formulas.CalculateRatio(BidCount, OfferCount);


            await Task.Run((Action)(() =>
            {
                _currentDispatcher.InvokeAsync((Action)(() =>
                {
                    DateTime currentTradeTime = Formulas.ConvertQuikDateTime(trade.Datetime);

                    ChartPoints.Add(new QcTrade
                        {
                            TradeId = trade.TradeNum,
                            Time = currentTradeTime,
                            Ticker = trade.SecCode,
                            Price = trade.Price,
                            Quantity = trade.Qty, 
                            Type = tradeType,
                            BidPercentage = bidPercentage,
                            OfferPercentage = offerPercentage
                        }
                    );

                    Minimum = DateTimeAxis.ToDouble(currentTradeTime.AddMinutes(-5));
                    Maximum = DateTimeAxis.ToDouble(currentTradeTime.AddMinutes(1));


                    PlotTitle = $"{currentTradeTime} {trade.Price} {bidPercentage} {offerPercentage}";
                }));
            }));
        }

        public MainWindowViewModel()
        {
            DistinctTradesCommand =
                new LambdaCommand(OnDistinctTradesCommandExecuted, CanDistinctTradesCommandExecuted);
            ShowSettingsWindowCommand =
                new LambdaCommand(OnShowSettingsWindowCommandExecuted, CanShowSettingsWindowCommandExecuted);
            RunCommand = 
                new LambdaCommand(OnRunCommandExecuted, CanRunCommandExecuted);
            LoadTextDataCommand =
                new LambdaCommand(OnLoadTextDataCommandExecuted, CanLoadTextDataCommandExecuted);
        }
    }
}