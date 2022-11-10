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
    using QuantConnect.Algorithm.Framework.Portfolio;
    using QuantConnect.Scheduling;
    using QuantConnect.Securities.Equity;

    /// <summary>
    /// Strategy that <see cref="ComposerQCAlgorithm"/> can execute.
    /// </summary>
    public interface IStrategy
    {
        /// <summary>
        /// Gets the earliest backtesting date a strategy can use.
        /// </summary>
        DateTime BacktestStartDate { get; }

        /// <summary>
        /// Gets the date rule for strategy evaluation.
        /// </summary>
        IDateRule EvaluationDateRule { get; }

        /// <summary>
        /// Gets the list of all indicator periods this strategy uses.
        /// </summary>
        IEnumerable<int> Periods { get; }

        /// <summary>
        /// Gets the list of all tickers this strategy uses.
        /// </summary>
        IEnumerable<string> Tickers { get; }

        /// <summary>
        /// Adds the specified <paramref name="equity"/> to the strategy's symbol data.
        /// </summary>
        /// <remarks>
        /// Used by <see cref="ComposerQCAlgorithm"/>. Do not invoke directly.
        /// </remarks>
        /// <param name="equity">Equity to add.</param>
        void AddSymbolData(Equity equity);

        /// <summary>
        /// Evaluates the strategy and returns a list of portfolio targets.
        /// </summary>
        /// <returns><see cref="List{T}"/> of <see cref="PortfolioTarget"/>s.</returns>
        List<PortfolioTarget> Evaluate();
    }
}
