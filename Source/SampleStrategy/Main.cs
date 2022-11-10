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

namespace QuantConnect.Algorithm.CSharp
{
    using System;
    using ComposerQC;
    using ComposerQC.Model;

    /**
     * The ComposerQCAlgorithm class handles all aspects related to strategy and trade execution.
     */
    public class ComposerQCSampleAlgorithm : ComposerQCAlgorithm
    {
        /// <summary>
        /// This is where we set up the strategy and hand it over for execution.
        /// </summary>
        public override IStrategy SetupStrategy()
        {
            /**
             * Uncomment the line below if you would like to start your backtest
             * at a date LATER than the strategy would.
             */
            //BacktestStartDate = new DateTime(2022, 1, 1);

            /**
             * Uncomment the line below if you would like to end your backtest
             * earlier than the current day.
             */
            //BacktestEndDate = new DateTime(2022, 11, 8);

            // This is where we create and return the strategy for execution.
            return new SampleStrategy(this);
        }
    }
}
