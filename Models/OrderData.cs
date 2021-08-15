

namespace QuikChart.Models {
    class OrderData
    {
        public string TimeRange { get; set; } = "Весь день";
        public double Total { get; set; } = 0;
        public double Bid { get; set; } = 0;
        public double Offer { get; set; } = 0;
        public double BidPercentage { get; set; } = 0;
        public double OfferPercentage { get; set; } = 0;
        public double BidEnergy { get; set; } = 0;
        public double OfferEnergy { get; set; } = 0;
    }
}
