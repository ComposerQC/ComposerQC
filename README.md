# ComposerQC
The QuantConnect backtesting companion for Invest Composer.

## Introduction
ComposerQC emulates the backtesting rules of Invest Composer, implementing an intuitive API for translating the visual design of a Composer symphony into C#.

## License
ComposerQC is licensed under the terms of the GNU General Public License v3.0.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

See the [LICENSE.txt](LICENSE.txt) file in this repository for more details.

## Features
This is alpha software. There will likely be bugs, and not all Composer features are currently implemented.

**It is highly recommended that you do NOT use use this version for live trading.** 

### Currently implemented features:
  - Full set of Composer indicators and periods.
  - Filtering by indicators.
  - Calendar strategies (daily/weekly/monthly/quarterly/yearly).
  - User-configurable execution time.

### Features not yet implemented:
  - Complex strategy nesting and weighting.
  - Weighting by market cap.
  - Weighting by inverse volatility.
  - Threshold strategies (use daily for now).

## Getting Started
Create a free account on [QuantConnect.com](https://www.quantconnect.com/), then head over to the [Releases](https://github.com/ComposerQC/ComposerQC/releases) section to clone the sample algorithm.
