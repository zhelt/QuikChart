using System;
using QuikSharp.DataStructures;

namespace QuikChart.StockMath {
    static class Formulas {

        public static DateTime ConvertQuikDateTime(QuikDateTime qdt)
        {
            return new DateTime(qdt.year, qdt.month, qdt.day, qdt.hour, qdt.min, qdt.sec, qdt.ms);
        }

        public static double CalculateRatio(double firstCount, double secondCount)
        {
            return Math.Round(firstCount / (firstCount + secondCount) * 100, 2);
        }

        public static double CalculateEnergy(double[,] trades)
        {
            double percentageDeltasSum = 0;
            double quantitiesSum = 0;
            for (int i = 0; i < trades.GetLength(1); i++)
            {
                percentageDeltasSum += trades[0, i];
                quantitiesSum += trades[1, i];
            }

            return percentageDeltasSum / trades.GetLength(1) * Math.Pow(quantitiesSum, 2);
        }

        public static double CalculateAcceleration(double time, double startSpeed, double endSpeed) {
            return (endSpeed - startSpeed) / time;
        }

        public static double CalculateForce(double acceleration, double mass) {
            return acceleration * mass;
        }
    }
}
