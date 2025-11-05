# QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
# Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

from AlgorithmImports import *

### <summary>
### Example algorithm using patent data universe to select stocks with high innovation activity
### </summary>
class GrantedPatentsDataUniverseAlgorithm(QCAlgorithm):
    def Initialize(self):
        ''' Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized. '''

        # Data ADDED via universe selection is added with Daily resolution.
        self.UniverseSettings.Resolution = Resolution.Daily

        self.SetStartDate(2022, 2, 14)
        self.SetEndDate(2022, 2, 18)
        self.SetCash(100000)

        # Add universe based on patent filing activity
        universe = self.AddUniverse(GrantedPatentsDataUniverse, self.UniverseSelection)

        # Test historical universe data
        history = self.History(universe, TimeSpan(1, 0, 0, 0))
        if len(history) != 1:
            self.Log(f"Warning: Expected 1 day of historical data, got {len(history)}")

        for dataForDate in history:
            self.Log(f"Historical universe contains {len(dataForDate)} stocks")

    def UniverseSelection(self, data):
        ''' Selected the securities based on patent filing activity
        
        :param List of GrantedPatentsDataUniverse data: List of patent universe data
        :return: List of Symbol objects '''

        for datum in data:
            self.Log(f"{datum.Symbol} - Patents Filed: {datum.PatentsFiled}, Tech Diversity: {datum.TechDiversity}, 90d Patents: {datum.Patents90d}")
        
        # Selection criteria: Companies with patent activity and tech diversity
        # Filter for companies with:
        # 1. Recent patent filing activity (Patents90d > 0)
        # 2. High technology diversity (TechDiversity > 0.5) 
        # 3. Minimum cumulative patents (CumulativePatents > 10)
        selected = [d for d in data 
                    if d.Patents90d > 0 
                    and d.TechDiversity > 0.5
                    and d.CumulativePatents > 10]
        
        # Sort by tech diversity (most diverse first)
        selected.sort(key=lambda x: x.TechDiversity, reverse=True)
        
        return [d.Symbol for d in selected]

    def OnSecuritiesChanged(self, changes):
        ''' Event fired each time that we add/remove securities from the data feed
		
        :param SecurityChanges changes: Security additions/removals for this time step
        '''
        self.Log(f"Universe changed - Added: {len(changes.AddedSecurities)}, Removed: {len(changes.RemovedSecurities)}")
        
        # Equal weight the selected securities
        if len(changes.AddedSecurities) > 0:
            weight = 1.0 / len(changes.AddedSecurities)
            for security in changes.AddedSecurities:
                self.SetHoldings(security.Symbol, weight)
