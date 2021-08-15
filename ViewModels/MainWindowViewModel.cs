using System;
using QuikChart.Models;
using QuikChart.ViewModels.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using MoreLinq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using QuikChart.Infrastructure.Commands;
using QuikChart.StockMath;
using QuikChart.Views.Windows;
using QuikSharp;
using QuikSharp.DataStructures;

namespace QuikChart.ViewModels {
    class MainWindowViewModel : ViewModel
    {
        private readonly Dispatcher _currentDispatcher = Dispatcher.CurrentDispatcher;

        const int DataLimit = 10;

        private double lastPrice = 0;

        private static Quik _quik;

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

        #region TradeDataPoints

        public ObservableCollection<QcTrade> TradeDataPoints { get; set; } = new ObservableCollection<QcTrade>();

        #endregion

        #region BidEnergyDataPoints

        public ObservableCollection<Energy> BidEnergyDataPoints { get; } = new ObservableCollection<Energy>();

        #endregion

        #region OfferEnergyDataPoints


        public ObservableCollection<Energy> OfferEnergyDataPoints { get; } = new ObservableCollection<Energy>();

        #endregion

        #region RatioDataPoints

        public ObservableCollection<TradeRatio> RatioDataPoints { get; } = new ObservableCollection<TradeRatio>();

        #endregion

        #region OrderDataGrid

        public ObservableCollection<OrderData> OrderDataGrid { get; set; } = new ObservableCollection<OrderData>
        (
            Enumerable.Repeat(new OrderData(), 7)
        );

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

        private async void OnLoadTextDataCommandExecuted(object p) {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = ".txt | *.txt"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                DateTime start = DateTime.Now;

                double lastFilePrice = 0;
                string[] lines = await File.ReadAllLinesAsync(openFileDialog.FileName);

                foreach (string line in lines)
                {
                    string[] data = line.Split(",");
                    await Task.Run(async () =>
                    {
                        await _currentDispatcher.InvokeAsync((Action) (() =>
                        {
                            double currentPrice = double.Parse(data[5].Replace(".", ","));
                            DateTime currentFileTime = DateTime.Parse(data[1]);
                            TradeDataPoints.Add(
                                new QcTrade(
                                    long.Parse(data.First()),
                                    currentFileTime, 
                                    data[4],
                                    currentPrice,
                                    long.Parse(data[7]),
                                    data.Last(),
                                    currentPrice - lastFilePrice,
                                    Formulas.CalculatePercentageDelta(currentPrice, lastFilePrice))
                            );


                            Minimum = DateTimeAxis.ToDouble(currentFileTime.AddMinutes(-5));
                            Maximum = DateTimeAxis.ToDouble(currentFileTime.AddMinutes(1));

                            double bidEnergy = Formulas.CalculateBidEnergy(TradeDataPoints);
                            double offerEnergy = Formulas.CalculateOfferEnergy(TradeDataPoints);

                            PlotTitle = $"{currentFileTime} {currentPrice} {bidEnergy} {offerEnergy}";

                            RatioDataPoints.Add(new TradeRatio
                            {
                                BidPercentage = Formulas.CalculateBidPercentage(TradeDataPoints),
                                OfferPercentage = Formulas.CalculateOfferPercentage(TradeDataPoints),
                                Time = currentFileTime
                            });
                            BidEnergyDataPoints.Add(new Energy { Time = currentFileTime, Value = bidEnergy}); 
                            OfferEnergyDataPoints.Add(new Energy { Time = currentFileTime, Value = offerEnergy});

                            DateTime currentTime = DateTime.Now;

                            #region Temporal Lists

                            List<QcTrade> eightHoursTemp =
                                TradeDataPoints.Where(x => (currentTime - x.Time).Hours <= 8).ToList();
                            List<QcTrade> sixHoursTemp =
                                TradeDataPoints.Where(x => (currentTime - x.Time).Hours <= 6).ToList();
                            List<QcTrade> hourTemp =
                                TradeDataPoints.Where(x => (currentTime - x.Time).Hours <= 1).ToList();
                            List<QcTrade> tenMinutesTemp =
                                TradeDataPoints.Where(x => (currentTime - x.Time).Minutes <= 10 && x.Time.Hour == currentTime.Hour).ToList();
                            List<QcTrade> fiveMinutesTemp =
                                TradeDataPoints.Where(x => (currentTime - x.Time).Minutes <= 5 && x.Time.Hour == currentTime.Hour).ToList();
                            List<QcTrade> minuteTemp =
                                TradeDataPoints.Where(x => (currentTime - x.Time).Minutes <= 1 && x.Time.Hour == currentTime.Hour).ToList();

                            #endregion

                            #region OrderDataGrid

                            OrderDataGrid[0] = new OrderData
                            {
                                TimeRange = "Весь день",
                                Total = Formulas.CalculateTotal(TradeDataPoints),
                                Bid = Formulas.CalculateBid(TradeDataPoints),
                                Offer = Formulas.CalculateOffer(TradeDataPoints),
                                BidPercentage = Formulas.CalculateBidPercentage(TradeDataPoints),
                                OfferPercentage = Formulas.CalculateOfferPercentage(TradeDataPoints),
                                BidEnergy = Formulas.CalculateBidEnergy(TradeDataPoints),
                                OfferEnergy = Formulas.CalculateOfferEnergy(TradeDataPoints)
                            };

                            OrderDataGrid[1] = new OrderData
                            {
                                TimeRange = "8 часов",
                                Total = Formulas.CalculateTotal(eightHoursTemp),
                                Bid = Formulas.CalculateBid(eightHoursTemp),
                                Offer = Formulas.CalculateOffer(eightHoursTemp),
                                BidPercentage = Formulas.CalculateBidPercentage(eightHoursTemp),
                                OfferPercentage = Formulas.CalculateOfferPercentage(eightHoursTemp),
                                BidEnergy = Formulas.CalculateBidEnergy(eightHoursTemp),
                                OfferEnergy = Formulas.CalculateOfferEnergy(eightHoursTemp)
                            };

                            OrderDataGrid[2] = new OrderData
                            {
                                TimeRange = "6 часов",
                                Total = Formulas.CalculateTotal(sixHoursTemp),
                                Bid = Formulas.CalculateBid(sixHoursTemp),
                                Offer = Formulas.CalculateOffer(sixHoursTemp),
                                BidPercentage = Formulas.CalculateBidPercentage(sixHoursTemp),
                                OfferPercentage = Formulas.CalculateOfferPercentage(sixHoursTemp),
                                BidEnergy = Formulas.CalculateBidEnergy(sixHoursTemp),
                                OfferEnergy = Formulas.CalculateOfferEnergy(sixHoursTemp)
                            };

                            OrderDataGrid[3] = new OrderData
                            {
                                TimeRange = "1 час",
                                Total = Formulas.CalculateTotal(hourTemp),
                                Bid = Formulas.CalculateBid(hourTemp),
                                Offer = Formulas.CalculateOffer(hourTemp),
                                BidPercentage = Formulas.CalculateBidPercentage(hourTemp),
                                OfferPercentage = Formulas.CalculateOfferPercentage(hourTemp),
                                BidEnergy = Formulas.CalculateBidEnergy(hourTemp),
                                OfferEnergy = Formulas.CalculateOfferEnergy(hourTemp)
                            };

                            OrderDataGrid[4] = new OrderData
                            {
                                TimeRange = "10 минут",
                                Total = Formulas.CalculateTotal(tenMinutesTemp),
                                Bid = Formulas.CalculateBid(tenMinutesTemp),
                                Offer = Formulas.CalculateOffer(tenMinutesTemp),
                                BidPercentage = Formulas.CalculateBidPercentage(tenMinutesTemp),
                                OfferPercentage = Formulas.CalculateOfferPercentage(tenMinutesTemp),
                                BidEnergy = Formulas.CalculateBidEnergy(tenMinutesTemp),
                                OfferEnergy = Formulas.CalculateOfferEnergy(tenMinutesTemp)
                            };

                            OrderDataGrid[5] = new OrderData
                            {
                                TimeRange = "5 минут",
                                Total = Formulas.CalculateTotal(fiveMinutesTemp),
                                Bid = Formulas.CalculateBid(fiveMinutesTemp),
                                Offer = Formulas.CalculateOffer(fiveMinutesTemp),
                                BidPercentage = Formulas.CalculateBidPercentage(fiveMinutesTemp),
                                OfferPercentage = Formulas.CalculateOfferPercentage(fiveMinutesTemp),
                                BidEnergy = Formulas.CalculateBidEnergy(fiveMinutesTemp),
                                OfferEnergy = Formulas.CalculateOfferEnergy(fiveMinutesTemp)
                            };

                            OrderDataGrid[6] = new OrderData
                            {
                                TimeRange = "1 минута",
                                Total = Formulas.CalculateTotal(minuteTemp),
                                Bid = Formulas.CalculateBid(minuteTemp),
                                Offer = Formulas.CalculateOffer(minuteTemp),
                                BidPercentage = Formulas.CalculateBidPercentage(minuteTemp),
                                OfferPercentage = Formulas.CalculateOfferPercentage(minuteTemp),
                                BidEnergy = Formulas.CalculateBidEnergy(minuteTemp),
                                OfferEnergy = Formulas.CalculateOfferEnergy(minuteTemp)
                            };

                            #endregion

                            lastFilePrice = double.Parse(data[5].Replace(".", ","));
                        }));
                    });
                }

                await DistinctTrades();
            }
        }
        private bool CanLoadTextDataCommandExecuted(object p) => true;

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
            TradeDataPoints = new ObservableCollection<QcTrade>(TradeDataPoints.DistinctBy(trade => trade.TradeId));
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

        private async void Event_OnAllTrade(AllTrade trade)
        {

            Title = trade.TradeNum.ToString();
            

            string tradeType = "None";

            if ((trade.Flags & AllTradeFlags.Sell) != 0)
            {
                tradeType = "S";
            }
            else if ((trade.Flags & AllTradeFlags.Buy) != 0)
            {
                tradeType = "B";
            }

            await Task.Run((Action) (() =>
            {
                _currentDispatcher.InvokeAsync((Action)(() =>
                {

                    DateTime currentTradeTime = Formulas.ConvertQuikDateTime(trade.Datetime);

                    TradeDataPoints.Add(new QcTrade(
                        trade.TradeNum,
                        currentTradeTime, 
                        trade.SecCode, trade.Price,
                        trade.Qty, tradeType,
                        trade.Price - lastPrice,
                        Formulas.CalculatePercentageDelta(trade.Price, lastPrice)));
                    RatioDataPoints.Add(new TradeRatio
                    {
                        BidPercentage = Formulas.CalculateBidPercentage(TradeDataPoints),
                        OfferPercentage = Formulas.CalculateOfferPercentage(TradeDataPoints),
                        Time = Formulas.ConvertQuikDateTime(trade.Datetime)
                    });
                    BidEnergyDataPoints.Add(new Energy { Time = Formulas.ConvertQuikDateTime(trade.Datetime), Value = Formulas.CalculateBidEnergy(TradeDataPoints) });
                    OfferEnergyDataPoints.Add(new Energy { Time = Formulas.ConvertQuikDateTime(trade.Datetime), Value = Formulas.CalculateOfferEnergy(TradeDataPoints) });

                    Minimum = DateTimeAxis.ToDouble(currentTradeTime.AddMinutes(-5));
                    Maximum = DateTimeAxis.ToDouble(currentTradeTime.AddMinutes(1));

                    double bidEnergy = Formulas.CalculateBidEnergy(TradeDataPoints);
                    double offerEnergy = Formulas.CalculateOfferEnergy(TradeDataPoints);

                    PlotTitle = $"{currentTradeTime} {trade.Price} {bidEnergy} {offerEnergy}";

                    RatioDataPoints.Add(new TradeRatio
                    {
                        BidPercentage = Formulas.CalculateBidPercentage(TradeDataPoints),
                        OfferPercentage = Formulas.CalculateOfferPercentage(TradeDataPoints),
                        Time = currentTradeTime
                    });
                    BidEnergyDataPoints.Add(new Energy { Time = currentTradeTime, Value = bidEnergy });
                    OfferEnergyDataPoints.Add(new Energy { Time = currentTradeTime, Value = offerEnergy });

                    DateTime currentTime = DateTime.Now;

                    #region Temporal Lists

                    List<QcTrade> eightHoursTemp =
                        TradeDataPoints.Where(x => (currentTime - x.Time).Hours <= 8).ToList();
                    List<QcTrade> sixHoursTemp =
                        TradeDataPoints.Where(x => (currentTime - x.Time).Hours <= 6).ToList();
                    List<QcTrade> hourTemp =
                        TradeDataPoints.Where(x => (currentTime - x.Time).Hours <= 1).ToList();
                    List<QcTrade> tenMinutesTemp =
                        TradeDataPoints.Where(x => (currentTime - x.Time).Minutes <= 10 && x.Time.Hour == currentTime.Hour).ToList();
                    List<QcTrade> fiveMinutesTemp =
                        TradeDataPoints.Where(x => (currentTime - x.Time).Minutes <= 5 && x.Time.Hour == currentTime.Hour).ToList();
                    List<QcTrade> minuteTemp =
                        TradeDataPoints.Where(x => (currentTime - x.Time).Minutes <= 1 && x.Time.Hour == currentTime.Hour).ToList();

                    #endregion

                    #region OrderDataGrid

                    OrderDataGrid[0] = new OrderData
                    {
                        TimeRange = "Весь день",
                        Total = Formulas.CalculateTotal(TradeDataPoints),
                        Bid = Formulas.CalculateBid(TradeDataPoints),
                        Offer = Formulas.CalculateOffer(TradeDataPoints),
                        BidPercentage = Formulas.CalculateBidPercentage(TradeDataPoints),
                        OfferPercentage = Formulas.CalculateOfferPercentage(TradeDataPoints),
                        BidEnergy = Formulas.CalculateBidEnergy(TradeDataPoints),
                        OfferEnergy = Formulas.CalculateOfferEnergy(TradeDataPoints)
                    };

                    OrderDataGrid[1] = new OrderData
                    {
                        TimeRange = "8 часов",
                        Total = Formulas.CalculateTotal(eightHoursTemp),
                        Bid = Formulas.CalculateBid(eightHoursTemp),
                        Offer = Formulas.CalculateOffer(eightHoursTemp),
                        BidPercentage = Formulas.CalculateBidPercentage(eightHoursTemp),
                        OfferPercentage = Formulas.CalculateOfferPercentage(eightHoursTemp),
                        BidEnergy = Formulas.CalculateBidEnergy(eightHoursTemp),
                        OfferEnergy = Formulas.CalculateOfferEnergy(eightHoursTemp)
                    };

                    OrderDataGrid[2] = new OrderData
                    {
                        TimeRange = "6 часов",
                        Total = Formulas.CalculateTotal(sixHoursTemp),
                        Bid = Formulas.CalculateBid(sixHoursTemp),
                        Offer = Formulas.CalculateOffer(sixHoursTemp),
                        BidPercentage = Formulas.CalculateBidPercentage(sixHoursTemp),
                        OfferPercentage = Formulas.CalculateOfferPercentage(sixHoursTemp),
                        BidEnergy = Formulas.CalculateBidEnergy(sixHoursTemp),
                        OfferEnergy = Formulas.CalculateOfferEnergy(sixHoursTemp)
                    };

                    OrderDataGrid[3] = new OrderData
                    {
                        TimeRange = "1 час",
                        Total = Formulas.CalculateTotal(hourTemp),
                        Bid = Formulas.CalculateBid(hourTemp),
                        Offer = Formulas.CalculateOffer(hourTemp),
                        BidPercentage = Formulas.CalculateBidPercentage(hourTemp),
                        OfferPercentage = Formulas.CalculateOfferPercentage(hourTemp),
                        BidEnergy = Formulas.CalculateBidEnergy(hourTemp),
                        OfferEnergy = Formulas.CalculateOfferEnergy(hourTemp)
                    };

                    OrderDataGrid[4] = new OrderData
                    {
                        TimeRange = "10 минут",
                        Total = Formulas.CalculateTotal(tenMinutesTemp),
                        Bid = Formulas.CalculateBid(tenMinutesTemp),
                        Offer = Formulas.CalculateOffer(tenMinutesTemp),
                        BidPercentage = Formulas.CalculateBidPercentage(tenMinutesTemp),
                        OfferPercentage = Formulas.CalculateOfferPercentage(tenMinutesTemp),
                        BidEnergy = Formulas.CalculateBidEnergy(tenMinutesTemp),
                        OfferEnergy = Formulas.CalculateOfferEnergy(tenMinutesTemp)
                    };

                    OrderDataGrid[5] = new OrderData
                    {
                        TimeRange = "5 минут",
                        Total = Formulas.CalculateTotal(fiveMinutesTemp),
                        Bid = Formulas.CalculateBid(fiveMinutesTemp),
                        Offer = Formulas.CalculateOffer(fiveMinutesTemp),
                        BidPercentage = Formulas.CalculateBidPercentage(fiveMinutesTemp),
                        OfferPercentage = Formulas.CalculateOfferPercentage(fiveMinutesTemp),
                        BidEnergy = Formulas.CalculateBidEnergy(fiveMinutesTemp),
                        OfferEnergy = Formulas.CalculateOfferEnergy(fiveMinutesTemp)
                    };

                    OrderDataGrid[6] = new OrderData
                    {
                        TimeRange = "1 минута",
                        Total = Formulas.CalculateTotal(minuteTemp),
                        Bid = Formulas.CalculateBid(minuteTemp),
                        Offer = Formulas.CalculateOffer(minuteTemp),
                        BidPercentage = Formulas.CalculateBidPercentage(minuteTemp),
                        OfferPercentage = Formulas.CalculateOfferPercentage(minuteTemp),
                        BidEnergy = Formulas.CalculateBidEnergy(minuteTemp),
                        OfferEnergy = Formulas.CalculateOfferEnergy(minuteTemp)
                    };

                    #endregion

                    lastPrice = trade.Price;
                }));
            }));
        }

        public MainWindowViewModel()
        {
            ShowSettingsWindowCommand =
                new LambdaCommand(OnShowSettingsWindowCommandExecuted, CanShowSettingsWindowCommandExecuted);
            RunCommand = 
                new LambdaCommand(OnRunCommandExecuted, CanRunCommandExecuted);
            LoadTextDataCommand =
                new LambdaCommand(OnLoadTextDataCommandExecuted, CanLoadTextDataCommandExecuted);
        }
    }
}