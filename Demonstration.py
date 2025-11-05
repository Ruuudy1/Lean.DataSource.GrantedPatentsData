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
### Example algorithm using patent data as a source of alpha
### Demonstrates how to trade based on patent filing activity
### </summary>
class GrantedPatentsDataAlgorithm(QCAlgorithm):
    def Initialize(self):
        ''' Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.'''
        
        self.SetStartDate(2010, 1, 1)   # Apple's first patent was filed in 1977
        self.SetEndDate(2025, 12, 31)
        self.equity_symbol = self.AddEquity("AAPL", Resolution.Daily).Symbol
        self.patent_data_symbol = self.AddData(GrantedPatentsData, self.equity_symbol).Symbol

    def OnData(self, slice):
        ''' OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.

        :param Slice slice: Slice object keyed by symbol containing the stock data
        '''
        data = slice.Get(GrantedPatentsData)
        if data:
            patent_data = data[self.patent_data_symbol]
            
            # Strategy: Buy when patents are filed (innovation signal)
            # Increase position when patent velocity increases
            if patent_data.PatentsFiled > 0:
                # Stronger signal if tech diversity is high (more varied innovation)
                weight = 1.0 if patent_data.TechDiversity > 0.5 else 0.5
                
                if not self.Portfolio.Invested:
                    self.SetHoldings(self.equity_symbol, weight)
                    self.Debug(f"Bought {self.equity_symbol} - Patents Filed: {patent_data.PatentsFiled}, Tech Diversity: {patent_data.TechDiversity}")
            
            # Hold position if cumulative patents growing
            elif patent_data.CumulativePatents > 0 and self.Portfolio[self.equity_symbol].Invested:
                # Keep position
                pass

    def OnOrderEvent(self, orderEvent):
        ''' Order fill event handler. On an order fill update the resulting information is passed to this method.

        :param OrderEvent orderEvent: Order event details containing details of the events
        '''
        if orderEvent.Status == OrderStatus.Filled:
            self.Debug(f'Order Filled: {orderEvent.Symbol} - Quantity: {orderEvent.FillQuantity}')

