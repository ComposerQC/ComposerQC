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

namespace QuantConnect
{
    using System.Collections.Generic;
    using ComposerQC;
    using ComposerQC.DateRules;
    using QuantConnect.Algorithm.Framework.Portfolio;

    /**
     * This is a sample strategy that picks the top performer of two leveraged funds
     * when SPY is above its 200 day moving average of price, and holds a 60/40 VTI/BND
     * portfolio when it is not.
     *
     * This is not intended to be a profitable strategy, but is instead intended to
     * illustrate how to write strategies in ComposerQC.
     */
    public class SampleStrategy : CalendarStrategy
    {
        // Set each of the periods your strategy's indicators use here.
        // These must be set ahead of time for the algorithm to warm up history properly.
        public override IEnumerable<int> Periods => new[] { 90, 200 };

        // Set all of the tickers your strategy uses here.
        // This includes tickers you invest in, as well as use for an indicator.
        public override IEnumerable<string> Tickers => new[] { "SPY", "SSO", "QLD", "VTI", "BND" };

        /**
         * This is the class constructor, and acts as an initializer for your strategy.
         *
         * You will set your strategy's earliest possible backtesting date here,
         * and the frequency with which it should be evaluated.
         */
        public SampleStrategy(ComposerQCAlgorithm algorithm) : base(algorithm)
        {
            /**
             * This sets the interval by which the strategy is to be evaluated.
             *
             * You may use Daily, Weekly, Quarterly, Monthly, or Yearly strategies
             * by changing the date rule you assign here.
             */
            this.EvaluationDateRule = new DailyDateRule(algorithm);

            // Set the earliest your strategy can be backtested in Composer here.
            this.SetBacktestStartDate(2007, 4, 10);
        }

        /**
         * This is where you define your strategy. You may use tickers and indicator periods
         * you have specified above. The strategy is expected to return a list of PortfolioTarget
         * objects that the algorithm will invest in.
         */
        public override List<PortfolioTarget> Evaluate()
        {
            // The SymbolData dictionary is indexed by ticker, and provides access to all of your indicators.
            if (SymbolData["SPY"].CurrentPrice() > SymbolData["SPY"].MovAvgOfPrice(200))
            {
                /**
                 * Here we use the Filter() function on an array of tickers to select
                 * the top performer over a 30 day period.
                 *
                 * The Filter() function returns an array of tickers that match the
                 * criteria we indicated, which we assign to the "tickers" variable.
                 */
                var ticker = Filter(new[] { "SSO", "QLD" },
                                    FilterBy.CumulativeReturn, 30,
                                    Select.Top, 1);

                /**
                 * We only have one ticker filtered above, so we give it 100% weight.
                 * If we filtered more than one ticker above, we would equally weight them.
                 */
                return EqualWeight(ticker);
            }
            else
            {
                // Hold a static allocation of 60% VTI/40% BND.
                var targets = new List<PortfolioTarget>
                {
                    new PortfolioTarget("VTI", 0.6m), // Percentage target expressed as a decimal.
                    new PortfolioTarget("BND", 0.4m)  // In C#, the "m" indicates a decimal data type.
                };

                return targets;
            }
        }
    }
}
