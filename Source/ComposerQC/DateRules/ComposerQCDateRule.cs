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

namespace ComposerQC.DateRules
{
    using System;
    using System.Collections.Generic;
    using QuantConnect.Scheduling;

    /// <summary>
    /// Base class for ComposerQC date rules.
    /// </summary>
    public abstract class ComposerQCDateRule : IDateRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComposerQCDateRule"/> class.
        /// </summary>
        /// <param name="algorithm">Executing algorithm</param>
        public ComposerQCDateRule(ComposerQCAlgorithm algorithm) =>
            this.Algorithm = algorithm;

        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the executing algorithm.
        /// </summary>
        protected ComposerQCAlgorithm Algorithm { get; }

        /// <inheritdoc/>
        public abstract IEnumerable<DateTime> GetDates(DateTime start, DateTime end);
    }
}
