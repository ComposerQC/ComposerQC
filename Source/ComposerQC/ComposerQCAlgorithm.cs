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

namespace ComposerQC
{
    using System;
    using System.Linq;
    using ComposerQC.Model;
    using QuantConnect;
    using QuantConnect.Algorithm;
    using QuantConnect.Data;
    using QuantConnect.Parameters;

    /// <summary>
    /// ComposerQC trading algorithm base class.
    /// </summary>
    public abstract class ComposerQCAlgorithm : QCAlgorithm
    {
        /// <summary>
        /// Number of bars per day at the chosen data resolution.
        /// </summary>
        public const int BarsPerDay = 390;

        /// <summary>
        /// Symbol used for benchmarking.
        /// </summary>
        public const string BenchmarkTicker = "SPY";

        /// <summary>
        /// Data resolution for fetching history and subscriptions.
        /// </summary>
        public const Resolution DataResolution = Resolution.Minute;

        /// <summary>
        /// ComposerQC version string.
        /// </summary>
        public const string ComposerQCVersion = "1.0.0-alpha.1";

        /// <summary>
        /// Offset from the execution time, in minutes, to consolidate trade bar data.
        /// </summary>
        public const int ConsolidationTimeOffset = 1;

        // Performance plotting fields
        private decimal lastBenchmarkValue;
        private decimal benchmarkPerformance;
        private Symbol benchmarkSymbol = default!;

        /// <summary>
        /// Time of day for trade execution to occur.
        /// </summary>
        [Parameter("ExecutionTime")]
        public TimeSpan ExecutionTime { get; set; }

        /// <summary>
        /// Optional end date for backtesting.
        /// </summary>
        public DateTime? BacktestEndDate { get; set; }

        /// <summary>
        /// If set, start testing on this date instead of the strategy's start date.
        /// </summary>
        public DateTime? BacktestStartDate { get; set; }

        /// <summary>
        /// Time of day for trade bar consolidation to occur.
        /// </summary>
        public TimeSpan ConsolidationTime { get; private set; }

        /// <summary>
        /// Gets the strategy this algorithm should use.
        /// </summary>
        public IStrategy Strategy { get; private set; } = default!;

        /// <summary>
        /// Initializes the algorithm.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the consolidation time is not set.</exception>
        public override void Initialize()
        {
            this.Log($"ComposerQC version {ComposerQCVersion} initializing.");

            if (this.ExecutionTime == default)
            {
                throw new InvalidOperationException($"{nameof(this.ExecutionTime)} is not set.");
            }

            this.ConsolidationTime = this.ExecutionTime.Add(TimeSpan.FromMinutes(-ConsolidationTimeOffset));
            this.Strategy = this.SetupStrategy();
            this.SetStartDate(this.BacktestStartDate ?? this.Strategy.BacktestStartDate);
            this.SetCash(10000);

            if (this.BacktestEndDate != null)
            {
                this.SetEndDate((DateTime)this.BacktestEndDate);
            }

            foreach (var ticker in this.Strategy.Tickers)
            {
                var equity = this.AddEquity(ticker, DataResolution, Market.USA);
                this.Strategy.AddSymbolData(equity);
            }

            // Always add the benchmark
            this.benchmarkSymbol = this.AddEquity(BenchmarkTicker, DataResolution, Market.USA).Symbol;
            this.SetBenchmark(this.benchmarkSymbol);
            this.benchmarkPerformance = this.Portfolio.TotalPortfolioValue;

            // Warm up the indicators
            this.SetWarmUp(this.Strategy.Periods.Max() * BarsPerDay, DataResolution);
        }

        /// <inheritdoc/>
        public override void OnWarmupFinished()
        {
            // Schedule strategy evaluation
            _ = this.Schedule.On(
                this.Strategy?.EvaluationDateRule,
                this.TimeRules.At(this.ExecutionTime),
                this.ExecuteStrategy);

            // Schedule plotting benchmark graph
            if (this.LiveMode)
            {
                _ = this.Schedule.On(
                    this.DateRules.EveryDay(),
                    this.TimeRules.BeforeMarketClose(this.benchmarkSymbol),
                    this.PlotBenchmark);
            }
            else
            {
                _ = this.Schedule.On(
                    this.DateRules.Every(DayOfWeek.Friday),
                    this.TimeRules.BeforeMarketClose(this.benchmarkSymbol),
                    this.PlotBenchmark);
            }

            // Initial execution for algorithm start
            if (!this.Portfolio.Invested)
            {
                _ = this.Schedule.On(
                    this.DateRules.On(this.Time.Year, this.Time.Month, this.Time.Day),
                    this.TimeRules.At(this.ExecutionTime),
                    this.ExecuteStrategy);
            }
        }

        /// <inheritdoc/>
        public override void OnData(Slice slice)
        {
            var benchmark = this.Securities[BenchmarkTicker].Close;

            if (this.lastBenchmarkValue != default)
            {
                this.benchmarkPerformance *= benchmark / this.lastBenchmarkValue;
            }

            this.lastBenchmarkValue = benchmark;
        }

        /// <inheritdoc/>
        public override void OnEndOfAlgorithm() =>
            this.PlotBenchmark();

        /// <summary>
        /// Creates a strategy for the algorithm to execute.
        /// </summary>
        /// <returns>Created <see cref="IStrategy"/>.</returns>
        public abstract IStrategy SetupStrategy();

        /// <summary>
        /// Executes the strategy defined in this algorithm.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when a strategy is not created by <see cref="SetupStrategy"/>.</exception>
        private void ExecuteStrategy()
        {
            if (this.Strategy == null)
            {
                throw new InvalidOperationException("No strategy provided.");
            }

            var targets = this.Strategy.Evaluate();
            var targetSymbols = targets.Select(x => x.Symbol).ToList();

            var investedSymbols = this.Portfolio.Values
                .Where(x => x.Invested)
                .Select(x => x.Symbol)
                .ToList();

            var liquidateExistingHoldings = investedSymbols.Any(x => !targetSymbols.Contains(x));

            this.SetHoldings(targets, liquidateExistingHoldings);
        }

        /// <summary>
        /// Plots the benchmark on the strategy equity graph.
        /// </summary>
        private void PlotBenchmark() =>
            this.Plot("Strategy Equity", "Benchmark", this.benchmarkPerformance);
    }
}
