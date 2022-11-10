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
    /// <summary>
    /// Indicates a filter function to use.
    /// </summary>
    public enum FilterBy
    {
        /// <summary>
        /// Filters by the current price of the tickers.
        /// </summary>
        CurrentPrice,

        /// <summary>
        /// Filters by the cumulative return.
        /// </summary>
        CumulativeReturn,

        /// <summary>
        /// Filters by the standard deviation of price.
        /// </summary>
        StdDevOfPrice,

        /// <summary>
        /// Filters by the standard deviation of return.
        /// </summary>
        StdDevOfReturn,

        /// <summary>
        /// Filters by the maximum drawdown.
        /// </summary>
        MaxDrawdown,

        /// <summary>
        /// Filters by the moving average of price.
        /// </summary>
        MovAvgOfPrice,

        /// <summary>
        /// Filters by the moving average of return.
        /// </summary>
        MovAvgOfReturn,

        /// <summary>
        /// Filters by the exponential moving average of price.
        /// </summary>
        ExpMovAvgOfPrice,

        /// <summary>
        /// Filters by the relatve strength index.
        /// </summary>
        RSI,
    }
}
