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

using System.Linq;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.DataSource;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Example algorithm using patent data universe to select stocks with high innovation activity
    /// </summary>
    public class GrantedPatentsDataUniverseAlgorithm : QCAlgorithm
    {
        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            // Data ADDED via universe selection is added with Daily resolution.
            UniverseSettings.Resolution = Resolution.Daily;

            SetStartDate(2022, 2, 14);
            SetEndDate(2022, 2, 18);
            SetCash(100000);

            // Add universe based on patent filing activity
            var universe = AddUniverse<GrantedPatentsDataUniverse>(data =>
            {
                foreach (GrantedPatentsDataUniverse datum in data)
                {
                    Log($"{datum.Symbol} - Patents Filed: {datum.PatentsFiled}, Tech Diversity: {datum.TechDiversity}, 90d Patents: {datum.Patents90d}");
                }

                // Selection criteria: Companies with patent activity and tech diversity
                // Filter for companies with:
                // 1. Recent patent filing activity (Patents90d > 0)
                // 2. High technology diversity (TechDiversity > 0.5) 
                // 3. Minimum cumulative patents (CumulativePatents > 10)
                return from GrantedPatentsDataUniverse d in data
                       where d.Patents90d > 0 
                       && d.TechDiversity > 0.5m
                       && d.CumulativePatents > 10
                       orderby d.TechDiversity descending
                       select d.Symbol;
            });

            // Test historical universe data
            var history = History(universe, 1).ToList();
            if (history.Count != 1)
            {
                Log($"Warning: Expected 1 day of historical data, got {history.Count}");
            }
            foreach (var dataForDate in history)
            {
                var universeData = dataForDate.ToList();
                Log($"Historical universe contains {universeData.Count} stocks");
            }
        }

        /// <summary>
        /// Event fired each time that we add/remove securities from the data feed
        /// </summary>
        /// <param name="changes">Security additions/removals for this time step</param>
        public override void OnSecuritiesChanged(SecurityChanges changes)
        {
            Log($"Universe changed - Added: {changes.AddedSecurities.Count}, Removed: {changes.RemovedSecurities.Count}");
            
            // Equal weight the selected securities
            if (changes.AddedSecurities.Count > 0)
            {
                var weight = 1.0m / changes.AddedSecurities.Count;
                foreach (var security in changes.AddedSecurities)
                {
                    SetHoldings(security.Symbol, (double)weight);
                }
            }
        }
    }
}