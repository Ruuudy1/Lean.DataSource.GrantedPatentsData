/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/

using System;
using NodaTime;
using ProtoBuf;
using System.IO;
using QuantConnect.Data;
using System.Collections.Generic;
using System.Globalization;
using QuantConnect.Data.UniverseSelection;

namespace QuantConnect.DataSource
{
    /// <summary>
    /// GrantedPatentsDataUniverse - Universe selection based on patent filing activity
    /// Provides daily universe of stocks with significant patent activity
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class GrantedPatentsDataUniverse : BaseDataCollection
    {
        /// <summary>
        /// Time passed between the date of the data and the time the data became available to us
        /// </summary>
        private readonly static TimeSpan _period = TimeSpan.FromDays(1);

        /// <summary>
        /// Number of patents filed by this company on this date
        /// </summary>
        public decimal PatentsFiled { get; set; }

        /// <summary>
        /// Cumulative number of patents filed by this company
        /// </summary>
        public decimal CumulativePatents { get; set; }

        /// <summary>
        /// Rolling 30-day patent count
        /// </summary>
        public decimal Patents30d { get; set; }

        /// <summary>
        /// Rolling 90-day patent count
        /// </summary>
        public decimal Patents90d { get; set; }

        /// <summary>
        /// Rolling 365-day patent count
        /// </summary>
        public decimal Patents365d { get; set; }

        /// <summary>
        /// Number of unique IPC classification codes
        /// </summary>
        public decimal UniqueIpcCodes { get; set; }

        /// <summary>
        /// Number of unique IPC sections (high-level tech categories)
        /// </summary>
        public decimal UniqueSections { get; set; }

        /// <summary>
        /// Technology diversity metric (0-1 scale)
        /// </summary>
        public decimal TechDiversity { get; set; }

        /// <summary>
        /// Number of unique geographic locations
        /// </summary>
        public decimal UniqueLocations { get; set; }

        /// <summary>
        /// Time the data became available
        /// </summary>
        public override DateTime EndTime => Time + _period;

        /// <summary>
        /// Return the URL string source of the file. This will be converted to a stream
        /// </summary>
        /// <param name="config">Configuration object</param>
        /// <param name="date">Date of this source file</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>String URL of source file.</returns>
        public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
        {
            return new SubscriptionDataSource(
                Path.Combine(
                    Globals.DataFolder,
                    "alternative",
                    "grantedpatentsdata",
                    "universe",
                    $"{date.ToStringInvariant(DateFormat.EightCharacter)}.csv"
                ),
                SubscriptionTransportMedium.LocalFile,
                FileFormat.FoldingCollection
            );
        }

        /// <summary>
        /// Parses the data from the line provided and loads it into LEAN
        /// </summary>
        /// <param name="config">Subscription configuration</param>
        /// <param name="line">Line of data</param>
        /// <param name="date">Date</param>
        /// <param name="isLiveMode">Is live mode</param>
        /// <returns>New instance</returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
        {
            // Expected format: SecurityIdentifier,Ticker,PatentsFiled,CumulativePatents,Patents30d,Patents90d,Patents365d,UniqueIpcCodes,UniqueSections,TechDiversity,UniqueLocations
            var csv = line.Split(',');

            var patentsFiled = decimal.Parse(csv[2], NumberStyles.Any, CultureInfo.InvariantCulture);

            return new GrantedPatentsDataUniverse
            {
                Symbol = new Symbol(SecurityIdentifier.Parse(csv[0]), csv[1]),
                Time = date - _period,
                Value = patentsFiled, // Use patents filed as primary value
                
                PatentsFiled = patentsFiled,
                CumulativePatents = decimal.Parse(csv[3], NumberStyles.Any, CultureInfo.InvariantCulture),
                Patents30d = decimal.Parse(csv[4], NumberStyles.Any, CultureInfo.InvariantCulture),
                Patents90d = decimal.Parse(csv[5], NumberStyles.Any, CultureInfo.InvariantCulture),
                Patents365d = decimal.Parse(csv[6], NumberStyles.Any, CultureInfo.InvariantCulture),
                UniqueIpcCodes = decimal.Parse(csv[7], NumberStyles.Any, CultureInfo.InvariantCulture),
                UniqueSections = decimal.Parse(csv[8], NumberStyles.Any, CultureInfo.InvariantCulture),
                TechDiversity = decimal.Parse(csv[9], NumberStyles.Any, CultureInfo.InvariantCulture),
                UniqueLocations = decimal.Parse(csv[10], NumberStyles.Any, CultureInfo.InvariantCulture)
            };
        }

        /// <summary>
        /// Indicates whether the data source is tied to an underlying symbol and requires that corporate events be applied to it as well, such as renames and delistings
        /// </summary>
        /// <returns>true</returns>
        public override bool RequiresMapping()
        {
            return true;
        }

        /// <summary>
        /// Indicates whether the data is sparse.
        /// If true, we disable logging for missing files
        /// </summary>
        /// <returns>true</returns>
        public override bool IsSparseData()
        {
            return true;
        }

        /// <summary>
        /// Converts the instance to string
        /// </summary>
        public override string ToString()
        {
            return $"{Symbol} - {Value}";
        }

        /// <summary>
        /// Gets the default resolution for this data and security type
        /// </summary>
        public override Resolution DefaultResolution()
        {
            return Resolution.Daily;
        }

        /// <summary>
        /// Gets the supported resolution for this data and security type
        /// </summary>
        public override List<Resolution> SupportedResolutions()
        {
            return DailyResolution;
        }

        /// <summary>
        /// Specifies the data time zone for this data type. This is useful for custom data types
        /// </summary>
        /// <returns>The <see cref="T:NodaTime.DateTimeZone" /> of this data type</returns>
        public override DateTimeZone DataTimeZone()
        {
            return DateTimeZone.Utc;
        }

        /// <summary>
        /// Clones this instance
        /// </summary>
        public override BaseData Clone()
        {
            return new GrantedPatentsDataUniverse
            {
                Symbol = Symbol,
                Time = Time,
                Data = Data,
                Value = Value,

                PatentsFiled = PatentsFiled,
                CumulativePatents = CumulativePatents,
                Patents30d = Patents30d,
                Patents90d = Patents90d,
                Patents365d = Patents365d,
                UniqueIpcCodes = UniqueIpcCodes,
                UniqueSections = UniqueSections,
                TechDiversity = TechDiversity,
                UniqueLocations = UniqueLocations
            };
        }
    }
}