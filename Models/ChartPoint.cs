using System;
using System.Collections.Generic;
using System.Text;

namespace QuikChart.Models {
    class ChartPoint {
        public long TradeId { get; set; }
        public DateTime Time { get; set; }
        public string Ticker { get; set; }
        public double Price { get; set; }
        public long Quantity { get; set; }
        public string Type { get; set; }
        public double Delta { get; set; }
        public double PercentageDelta { get; set; }
        public double BidPercentage { get; set; }
        public double OfferPercentage { get; set; }
    }
}
