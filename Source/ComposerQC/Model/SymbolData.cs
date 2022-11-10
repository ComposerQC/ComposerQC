// ComposerQC - backtesting companion for Invest Composer.
// Copyright (C) 2022 SolarianKnight.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

namespace ComposerQC.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using QuantConnect;
    using QuantConnect.Data.Consolidators;
    using QuantConnect.Data.Market;
    using QuantConnect.Indicators;

    /// <summary>
    /// Contains indicators and subscription logic for the symbols a strategy uses.
    /// </summary>
    public class SymbolData : IDisposable
    {
        private readonly ComposerQCAlgorithm algorithm;
        private readonly TradeBarConsolidator consolidator;
        private readonly RollingWindow<decimal> priceHistory;
        private readonly Symbol symbol;

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolData"/> class.
        /// </summary>
        /// <param name="algorithm">Executing algorithm.</param>
        /// <param name="symbol"><see cref="Symbol"/> to add data for.</param>
        /// <param name="periods">All periods to generate indicators for.</param>
        internal SymbolData(ComposerQCAlgorithm algorithm, Symbol symbol, IEnumerable<int> periods)
        {
            this.algorithm = algorithm;
            this.symbol = symbol;
            this.consolidator = new TradeBarConsolidator(this.CalculateConsolidationDateTime);
            this.priceHistory = new RollingWindow<decimal>(periods.Max());

            // Create the indicators
            this.CreateIndicators(periods);

            // Attach subscription for price feed
            this.consolidator.DataConsolidated += this.OnDataConsolidated;
            this.algorithm.SubscriptionManager.AddConsolidator(symbol, this.consolidator);
        }

        private Dictionary<int, ExponentialMovingAverage> ExpMovAvgOfPriceIndicators { get; } = new();

        private Dictionary<int, SimpleMovingAverage> MovAvgOfPriceIndicators { get; } = new();

        private Dictionary<int, SimpleMovingAverage> MovAvgOfReturnIndicators { get; } = new();

        private List<RateOfChange> ROCIndicators { get; } = new();

        private Dictionary<int, RelativeStrengthIndex> RSIIndicators { get; } = new();

        private Dictionary<int, StandardDeviation> StdDevOfPriceIndicators { get; } = new();

        private Dictionary<int, StandardDeviation> StdDevOfReturnIndicators { get; } = new();

        /// <summary>
        /// Retrieves the current price of the symbol.
        /// </summary>
        /// <returns>Current price of this symbol.</returns>
        public decimal CurrentPrice() => this.algorithm.Securities[this.symbol].Price;

        /// <summary>
        /// Calculates the cumulative return of the symbol.
        /// </summary>
        /// <param name="periods">Number of periods to calculate the cumulative returns over.</param>
        /// <returns>Cumulative return of this symbol over the given <paramref name="periods"/>.</returns>
        public decimal CumulativeReturn(int periods)
        {
            var returns = this.priceHistory[0] / this.priceHistory[1];

            for (var i = 1; i < periods - 2; i++)
            {
                returns *= this.priceHistory[i] / this.priceHistory[i + 1];
            }

            return returns - 1;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.algorithm.SubscriptionManager.RemoveConsolidator(this.symbol, this.consolidator);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Retrieves the exponential moving average of price for the symbol over the specified <paramref name="periods"/>.
        /// </summary>
        /// <param name="periods">Number of periods to calculate the moving average for.</param>
        /// <returns>Exponential moving average of price for the desired timeframe.</returns>
        public decimal ExpMovAvgOfPrice(int periods) => this.ExpMovAvgOfPriceIndicators[periods].Current.Value;

        /// <summary>
        /// Retrieves the simple moving average of price for the symbol over the specified <paramref name="periods"/>.
        /// </summary>
        /// <param name="periods">Number of periods to calculate the moving average of price for.</param>
        /// <returns>Moving average of price for the desired timeframe.</returns>
        public decimal MovAvgOfPrice(int periods) => this.MovAvgOfPriceIndicators[periods].Current.Value;

        /// <summary>
        /// Retrieves the simple moving average of return for the symbol over the specified <paramref name="periods"/>.
        /// </summary>
        /// <param name="periods">Number of periods to calculate the moving average of returns for.</param>
        /// <returns>Moving average of return for the desired timeframe.</returns>
        public decimal MovAvgOfReturn(int periods) => this.MovAvgOfReturnIndicators[periods].Current.Value;

        /// <summary>
        /// Retrieves the relative strength index for the symbol over the specified <paramref name="periods"/>.
        /// </summary>
        /// <param name="periods">Number of periods to calculate the RSI for.</param>
        /// <returns>Relative strength index for the desired timeframe.</returns>
        public decimal RSI(int periods) => this.RSIIndicators[periods].Current.Value;

        /// <summary>
        /// Retrieves the standard deviation of price for the symbol over the specified <paramref name="periods"/>.
        /// </summary>
        /// <param name="periods">Number of periods to calculate the standard deviation of price for.</param>
        /// <returns>Standard deviation of price for the desired timeframe.</returns>
        public decimal StdDevOfPrice(int periods) => this.StdDevOfPriceIndicators[periods].Current.Value;

        /// <summary>
        /// Retrieves the standard deviation of return for the symbol over the specified <paramref name="periods"/>.
        /// </summary>
        /// <param name="periods">Number of periods to calculate the standard deviation of return for.</param>
        /// <returns>Standard deviation of return for the desired timeframe.</returns>
        public decimal StdDevOfReturn(int periods) => this.StdDevOfReturnIndicators[periods].Current.Value;

        /// <summary>
        /// Calculates the maximum drawdown for the symbol over the specified <paramref name="periods"/>.
        /// </summary>
        /// <param name="periods">Number of periods to calculate the drawdown for.</param>
        /// <returns>Maximum drawdown for the desired timeframe.</returns>
        public decimal MaxDrawdown(int periods)
        {
            var peak = this.priceHistory.Take(periods).Max();
            var trough = this.priceHistory.Take(periods).Min();

            return (trough - peak) / peak;
        }

        /// <summary>
        /// Sets up and registers indicators for the symbol.
        /// </summary>
        /// <param name="periods">All periods to create indicators for.</param>
        private void CreateIndicators(IEnumerable<int> periods)
        {
            foreach (var period in periods)
            {
                this.ExpMovAvgOfPriceIndicators[period] = new ExponentialMovingAverage(period);
                this.algorithm.RegisterIndicator(this.symbol, this.ExpMovAvgOfPriceIndicators[period], this.consolidator);

                this.MovAvgOfPriceIndicators[period] = new SimpleMovingAverage(period);
                this.algorithm.RegisterIndicator(this.symbol, this.MovAvgOfPriceIndicators[period], this.consolidator);

                this.RSIIndicators[period] = new RelativeStrengthIndex(period);
                this.algorithm.RegisterIndicator(this.symbol, this.RSIIndicators[period], this.consolidator);

                this.StdDevOfPriceIndicators[period] = new StandardDeviation(period);
                this.algorithm.RegisterIndicator(this.symbol, this.StdDevOfPriceIndicators[period], this.consolidator);

                var movAvgReturnROC = new RateOfChange(period);
                this.ROCIndicators.Add(movAvgReturnROC);
                this.MovAvgOfReturnIndicators[period] = new SimpleMovingAverage(period).Of(movAvgReturnROC, false);
                this.algorithm.RegisterIndicator(this.symbol, movAvgReturnROC, this.consolidator);

                var stdDevReturnROC = new RateOfChange(period);
                this.ROCIndicators.Add(stdDevReturnROC);
                this.StdDevOfReturnIndicators[period] = new StandardDeviation(period).Of(stdDevReturnROC, false);
                this.algorithm.RegisterIndicator(this.symbol, stdDevReturnROC, this.consolidator);
            }
        }

        /// <summary>
        /// Computes the next bar consolidation date and time from the given <paramref name="input"/>.
        /// </summary>
        /// <param name="input"><see cref="DateTime"/> to compute the next consolidation time from.</param>
        /// <returns>A <see cref="CalendarInfo"/> indicating the next consolidation time.</returns>
        private CalendarInfo CalculateConsolidationDateTime(DateTime input)
        {
            var period = TimeSpan.FromDays(1);
            var start = input.Date + this.algorithm.ConsolidationTime;

            if (start > input)
            {
                start -= period;
            }

            return new CalendarInfo(start, period);
        }

#pragma warning disable CS8632 // Annotation for nullable reference types
        /// <summary>
        /// <see cref="TradeBarConsolidator"/>.DataConsolidated event handler.
        /// </summary>
        /// <param name="sender">Object sending event.</param>
        /// <param name="data">Consolidated <see cref="TradeBar"/> data.</param>
        private void OnDataConsolidated(object? sender, TradeBar data) =>
            this.priceHistory.Add(data.Close);
#pragma warning restore CS8632 // Annotation for nullable reference types
    }
}
