using System;
using System.Collections.Generic;
using System.Text;

namespace QuikChart.Models {
    class QcTrade {
        public long TradeId { get; set; }
        public DateTime Time { get; set; }
        public string Ticker { get; set; }
        public double Price { get; set; }
        public long Quantity { get; set; }
        public string Type { get; set; }
        public double Delta { get; set; }
        public double PercentageDelta { get; set; }

        public QcTrade(long tradeId, DateTime time, string ticker, double price, long quantity, string type, double delta, double percentageDelta) {
            this.TradeId = tradeId;
            this.Time = time;
            this.Ticker = ticker;
            this.Price = price;
            this.Quantity = quantity;
            this.Type = type;
            this.Delta = delta;
            this.PercentageDelta = percentageDelta;
        }

    }
}
