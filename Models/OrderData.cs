

namespace QuikChart.Models {
    public struct OrderData
    {
        public string TimeRange { get; set; }
        public double Total { get; set; }
        public double Bid { get; set; }
        public double Offer { get; set; }
        public double BidPercentage { get; set; }
        public double OfferPercentage { get; set; }
        public double BidEnergy { get; set; }
        public double OfferEnergy { get; set; }
    }
}
