using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using QuikChart.Models;
using QuikSharp.DataStructures;

namespace QuikChart.StockMath {
    static class Formulas {

        public static double CalculateTotal(List<QcTrade> data)
        {
            return (from el in data select el.Quantity).Sum();
        }

        public static double CalculateBid(List<QcTrade> data)
        {
            return data.Where(x => x.Type == "B").Select(x => x.Quantity).Sum();
        }

        public static double CalculateOffer(List<QcTrade> data) {
            return data.Where(x => x.Type == "S").Select(x => x.Quantity).Sum();
        }

        public static double CalculateBidPercentage(List<QcTrade> data)
        {
            return Math.Round((CalculateBid(data: data) / CalculateTotal(data: data)) * 100, 1);
        }
        public static double CalculateOfferPercentage(List<QcTrade> data)
        {
            return Math.Round((CalculateOffer(data: data) / CalculateTotal(data: data)) * 100, 1);
        }

        public static DateTime ConvertQuikDateTime(QuikDateTime qdt)
        {
            return new DateTime(qdt.year, qdt.month, qdt.day, qdt.hour, qdt.min, qdt.sec, qdt.ms);
        }

        public static double CalculatePercentageDelta(double currentPrice, double lastPrice)
        {
            return (currentPrice - lastPrice) / lastPrice * 100;
        }

        public static double CalculateBidEnergy(List<QcTrade> data, int count = 10) {
            return data.Count(x => x.Type == "B") >= count ? Math.Round(data.Where(x => x.Type == "B").Select(x => x.PercentageDelta).TakeLast(count).Average() *
                                                                        data.Where(x => x.Type == "B").Select(x => x.Quantity).TakeLast(10).Sum(), 1) : 0;
        }

        public static double CalculateOfferEnergy(List<QcTrade> data, int count = 10) {
            return data.Count(x => x.Type == "B") >= count ? Math.Round(data.Where(x => x.Type == "S").Select(x => x.PercentageDelta).TakeLast(count).Average() *
                                                                        data.Where(x => x.Type == "S").Select(x => x.Quantity).TakeLast(10).Sum(), 1) : 0;
        }



        public static double CalculateTotal(ObservableCollection<QcTrade> data) {
            return (from el in data select el.Quantity).Sum();
        }

        public static double CalculateBid(ObservableCollection<QcTrade> data) {
            return data.Where(x => x.Type == "B").Select(x => x.Quantity).Sum();
        }

        public static double CalculateOffer(ObservableCollection<QcTrade> data) {
            return data.Where(x => x.Type == "S").Select(x => x.Quantity).Sum();
        }

        public static double CalculateBidPercentage(ObservableCollection<QcTrade> data) {
            return Math.Round((CalculateBid(data: data) / CalculateTotal(data: data)) * 100, 1);
        }

        public static double CalculateOfferPercentage(ObservableCollection<QcTrade> data)
        {
            return Math.Round((CalculateOffer(data: data) / CalculateTotal(data: data)) * 100, 1);
        }

        public static double CalculateBidEnergy(ObservableCollection<QcTrade> data, int count = 10) {
            return data.Count(x => x.Type == "B") >= count ? Math.Round(data.Where(x => x.Type == "B").Select(x => x.PercentageDelta).TakeLast(count).Average() *
                                                                        data.Where(x => x.Type == "B").Select(x => x.Quantity).TakeLast(10).Sum(), 1) : 0;
        }

        public static double CalculateOfferEnergy(ObservableCollection<QcTrade> data, int count = 10) {
            return data.Count(x => x.Type == "B") >= count ? Math.Round(data.Where(x => x.Type == "S").Select(x => x.PercentageDelta).TakeLast(count).Average() *
                                                                        data.Where(x => x.Type == "S").Select(x => x.Quantity).TakeLast(10).Sum(), 1) : 0;
        }
    }
}
