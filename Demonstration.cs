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

using QuantConnect.Data;
using QuantConnect.Util;
using QuantConnect.Orders;
using QuantConnect.Algorithm;
using QuantConnect.DataSource;

namespace QuantConnect.DataLibrary.Tests
{
    /// <summary>
    /// Example algorithm using patent data as a source of alpha
    /// Demonstrates how to trade based on patent filing activity
    /// </summary>
    public class GrantedPatentsDataAlgorithm : QCAlgorithm
    {
        private Symbol _patentDataSymbol;
        private Symbol _equitySymbol;

        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            SetStartDate(1978, 1, 1);   // Apple's first patent was filed in 1977
            SetEndDate(1980, 12, 31);    
            _equitySymbol = AddEquity("AAPL").Symbol;
            _patentDataSymbol = AddData<GrantedPatentsData>(_equitySymbol).Symbol;
        }

        /// <summary>
        /// OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        /// </summary>
        /// <param name="slice">Slice object keyed by symbol containing the stock data</param>
        public override void OnData(Slice slice)
        {
            var data = slice.Get<GrantedPatentsData>();
            if (!data.IsNullOrEmpty())
            {
                var patentData = data[_patentDataSymbol];
                
                // Strategy: Buy when patents are filed (innovation signal)
                // Increase position when patent velocity increases
                if (patentData.PatentsFiled > 0)
                {
                    // Stronger signal if tech diversity is high (more varied innovation)
                    var weight = patentData.TechDiversity > 0.5m ? 1.0m : 0.5m;
                    
                    if (!Portfolio.Invested)
                    {
                        SetHoldings(_equitySymbol, (double)weight);
                        Debug($"Bought {_equitySymbol} - Patents Filed: {patentData.PatentsFiled}, Tech Diversity: {patentData.TechDiversity}");
                    }
                }
                // Hold position if cumulative patents growing
                else if (patentData.CumulativePatents > 0 && Portfolio[_equitySymbol].Invested)
                {
                    // Keep position
                }
            }
        }

        /// <summary>
        /// Order fill event handler. On an order fill update the resulting information is passed to this method.
        /// </summary>
        /// <param name="orderEvent">Order event details containing details of the events</param>
        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            if (orderEvent.Status.IsFill())
            {
                Debug($"Order Filled: {orderEvent.Symbol} - Quantity: {orderEvent.FillQuantity}");
            }
        }
    }
}
