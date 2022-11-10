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
    using QuantConnect.Algorithm.Framework.Portfolio;
    using QuantConnect.Scheduling;
    using QuantConnect.Securities.Equity;

    /// <summary>
    /// Base class for Composer strategies.
    /// </summary>
    public abstract class StrategyBase : IStrategy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StrategyBase"/> class.
        /// </summary>
        /// <param name="algorithm">Executing algorithm.</param>
        public StrategyBase(ComposerQCAlgorithm algorithm) =>
            this.Algorithm = algorithm;

        /// <inheritdoc/>
        public DateTime BacktestStartDate { get; private set; }

        /// <inheritdoc/>
        public IDateRule EvaluationDateRule { get; protected set; } = default!;

        /// <inheritdoc/>
        public abstract IEnumerable<int> Periods { get; }

        /// <inheritdoc/>
        public abstract IEnumerable<string> Tickers { get; }

        /// <summary>
        /// Gets the algorithm this strategy is executing in.
        /// </summary>
        protected ComposerQCAlgorithm Algorithm { get; }

        /// <summary>
        /// Gets a dictionary of all symbol data this strategy has, keyed by ticker.
        /// </summary>
        protected Dictionary<string, SymbolData> SymbolData { get; } = new();

        /// <inheritdoc/>
        public abstract List<PortfolioTarget> Evaluate();

        /// <inheritdoc/>
        public void AddSymbolData(Equity equity)
        {
            var symbolData = new SymbolData(this.Algorithm, equity.Symbol, this.Periods);
            this.SymbolData.Add(equity.Symbol.Value, symbolData);
        }

        /// <summary>
        /// Equally weights a list of tickers.
        /// </summary>
        /// <param name="tickers">List of tickers to equally weight.</param>
        /// <param name="startWeight">Starting weight to balance equities within.</param>
        /// <returns>Equally weighted <see cref="List{T}"/> of <see cref="PortfolioTarget"/>s.</returns>
        protected static List<PortfolioTarget> EqualWeight(IEnumerable<string> tickers, decimal startWeight = 1m)
        {
            var targets = new List<PortfolioTarget>();

            foreach (var ticker in tickers)
            {
                var symbol = Symbol.Create(ticker, SecurityType.Equity, Market.USA);
                targets.Add(new PortfolioTarget(symbol, startWeight / tickers.Count()));
            }

            return targets;
        }

        /// <summary>
        /// Filters a list of <paramref name="tickers"/> according to an indicated
        /// <paramref name="filterFunction"/> and <paramref name="selectFunction"/>.
        /// </summary>
        /// <param name="tickers">Tickers to filter.</param>
        /// <param name="filterFunction">Filter function to apply.</param>
        /// <param name="window">Number of periods for the <paramref name="filterFunction"/> indicator.</param>
        /// <param name="selectFunction">Selection function to apply.</param>
        /// <param name="count">Number of tickers to return from the <paramref name="selectFunction"/>.</param>
        /// <returns>A <see cref="List{T}"/> of filtered equities.</returns>
        /// <exception cref="ArgumentException">Thrown when an unknown filter function is provided.</exception>
        protected List<string> Filter(IEnumerable<string> tickers, FilterBy filterFunction, int window, Select selectFunction, int count)
        {
            var selectedSymbols = this.SymbolData
                .Where(x => tickers.Contains(x.Key))
                .ToList();

            var projectedSymbols = filterFunction switch
            {
                FilterBy.CumulativeReturn =>
                    selectedSymbols.ToDictionary(x => x.Key, x => x.Value.CumulativeReturn(window)),

                FilterBy.CurrentPrice =>
                    selectedSymbols.ToDictionary(x => x.Key, x => x.Value.CurrentPrice()),

                FilterBy.StdDevOfPrice =>
                    selectedSymbols.ToDictionary(x => x.Key, x => x.Value.StdDevOfPrice(window)),

                FilterBy.StdDevOfReturn =>
                    selectedSymbols.ToDictionary(x => x.Key, x => x.Value.StdDevOfReturn(window)),

                FilterBy.MaxDrawdown =>
                    selectedSymbols.ToDictionary(x => x.Key, x => x.Value.MaxDrawdown(window)),

                FilterBy.MovAvgOfPrice =>
                    selectedSymbols.ToDictionary(x => x.Key, x => x.Value.MovAvgOfPrice(window)),

                FilterBy.MovAvgOfReturn =>
                    selectedSymbols.ToDictionary(x => x.Key, x => x.Value.MovAvgOfReturn(window)),

                FilterBy.ExpMovAvgOfPrice =>
                    selectedSymbols.ToDictionary(x => x.Key, x => x.Value.ExpMovAvgOfPrice(window)),

                FilterBy.RSI =>
                    selectedSymbols.ToDictionary(x => x.Key, x => x.Value.RSI(window)),

                _ => throw new ArgumentException($"Unrecognized filter function: {filterFunction}"),
            };

            var returnTickers = selectFunction switch
            {
                Select.Bottom =>
                    projectedSymbols
                        .OrderByDescending(x => x.Value)
                        .TakeLast(count)
                        .Select(x => x.Key)
                        .ToList(),

                Select.Top =>
                    projectedSymbols
                        .OrderByDescending(x => x.Value)
                        .Take(count)
                        .Select(x => x.Key)
                        .ToList(),

                _ => throw new ArgumentException($"Unrecognized select function: {selectFunction}"),
            };

            return returnTickers;
        }

        /// <summary>
        /// Sets the earliest backtest date this strategy can use.
        /// </summary>
        /// <param name="year">Year to start backtest on.</param>
        /// <param name="month">Month to start backtest on.</param>
        /// <param name="day">Day to start backtest on.</param>
        protected void SetBacktestStartDate(int year, int month, int day) =>
            this.BacktestStartDate = new DateTime(year, month, day);
    }
}
