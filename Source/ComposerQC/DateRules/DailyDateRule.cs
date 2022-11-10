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
    using System.Linq;
    using QuantConnect;

    /// <summary>
    /// Date rule that triggers strategy evaluation every trading day.
    /// </summary>
    public class DailyDateRule : ComposerQCDateRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DailyDateRule"/> class.
        /// </summary>
        /// <param name="algorithm">Executing algorithm.</param>
        public DailyDateRule(ComposerQCAlgorithm algorithm)
            : base(algorithm)
        {
        }

        /// <inheritdoc/>
        public override string Name => "ComposerQC.DailyDateRule";

        /// <inheritdoc/>
        public override IEnumerable<DateTime> GetDates(DateTime start, DateTime end) =>
            this.Algorithm.TradingCalendar
                .GetDaysByType(TradingDayType.BusinessDay, start, end)
                .Select(x => x.Date)
                .ToList();
    }
}
