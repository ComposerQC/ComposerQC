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
    using ComposerQC.Model;

    /// <summary>
    /// Base class for strategies that follow a calendar strategy.
    /// </summary>
    public abstract class CalendarStrategy : StrategyBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalendarStrategy"/> class.
        /// </summary>
        /// <param name="algorithm">Executing algorithm.</param>
        public CalendarStrategy(ComposerQCAlgorithm algorithm)
            : base(algorithm)
        {
        }
    }
}
