# Granted Patents Data - Strategy Examples

## Example 1: Innovation Momentum Strategy

Track companies with accelerating patent activity as a signal of R&D investment and future growth.

```python
class InnovationMomentumAlgorithm(QCAlgorithm):
    def Initialize(self):
        self.SetStartDate(2020, 1, 1)
        self.SetCash(100000)
        
        # Track patent data for tech stocks
        self.stocks = ["AAPL", "MSFT", "GOOGL", "NVDA", "TSLA"]
        self.patent_symbols = {}
        
        for ticker in self.stocks:
            equity = self.AddEquity(ticker, Resolution.Daily)
            self.patent_symbols[equity.Symbol] = self.AddData(GrantedPatentsData, equity.Symbol).Symbol
    
    def OnData(self, slice):
        for equity_symbol, patent_symbol in self.patent_symbols.items():
            if patent_symbol in slice.Keys:
                patent_data = slice[patent_symbol]
                
                # Buy when 90-day patent count exceeds 365-day average
                if patent_data.Patents90d > 0 and patent_data.Patents365d > 0:
                    momentum = patent_data.Patents90d / (patent_data.Patents365d / 4.0)
                    
                    if momentum > 1.5 and not self.Portfolio[equity_symbol].Invested:
                        self.SetHoldings(equity_symbol, 0.15)
                        self.Log(f"Buying {equity_symbol}: 90d={patent_data.Patents90d}, momentum={momentum:.2f}")
```

---

## Example 2: Technology Diversity Filter

Select companies with diverse patent portfolios across multiple technology areas.

```csharp
public class TechDiversityAlgorithm : QCAlgorithm
{
    private Dictionary<Symbol, Symbol> _patentSymbols = new Dictionary<Symbol, Symbol>();
    
    public override void Initialize()
    {
        SetStartDate(2020, 1, 1);
        SetCash(100000);
        
        // Universe selection based on patent diversity
        UniverseSettings.Resolution = Resolution.Daily;
        AddUniverse<GrantedPatentsDataUniverse>("PatentUniverse", PatentDiversitySelection);
    }
    
    private IEnumerable<Symbol> PatentDiversitySelection(IEnumerable<GrantedPatentsDataUniverse> data)
    {
        // Select top 10 companies by technology diversity
        return (from x in data
                where x.TechDiversity > 0.6m 
                      && x.CumulativePatents > 100
                      && x.UniqueSections >= 3
                orderby x.TechDiversity descending
                select x.Symbol)
               .Take(10);
    }
    
    public override void OnData(Slice slice)
    {
        // Equal weight the diversified innovators
        foreach (var security in ActiveSecurities.Values)
        {
            if (!Portfolio[security.Symbol].Invested)
            {
                SetHoldings(security.Symbol, 1.0m / 10);
            }
        }
    }
}
```

---

## Example 3: Patent Velocity Signal

Combine patent filing rates with price action for timing entries.

```python
class PatentVelocityAlgorithm(QCAlgorithm):
    def Initialize(self):
        self.SetStartDate(2020, 1, 1)
        self.SetCash(100000)
        
        self.equity_symbol = self.AddEquity("AAPL", Resolution.Daily).Symbol
        self.patent_symbol = self.AddData(GrantedPatentsData, self.equity_symbol).Symbol
        
        # Track patent velocity
        self.patent_history = []
        
    def OnData(self, slice):
        if self.patent_symbol not in slice.Keys:
            return
            
        patent_data = slice[self.patent_symbol]
        
        # Store patent counts
        self.patent_history.append({
            'time': self.Time,
            'patents_90d': patent_data.Patents90d,
            'tech_diversity': patent_data.TechDiversity
        })
        
        # Keep only last 12 months
        self.patent_history = [p for p in self.patent_history 
                                if (self.Time - p['time']).days <= 365]
        
        if len(self.patent_history) < 4:
            return
        
        # Calculate velocity (rate of change)
        recent_avg = sum(p['patents_90d'] for p in self.patent_history[-3:]) / 3
        older_avg = sum(p['patents_90d'] for p in self.patent_history[:3]) / 3
        
        if older_avg > 0:
            velocity = (recent_avg - older_avg) / older_avg
            
            # Buy on accelerating innovation + price pullback
            if velocity > 0.2 and not self.Portfolio[self.equity_symbol].Invested:
                self.SetHoldings(self.equity_symbol, 1.0)
                self.Log(f"Buying on patent acceleration: velocity={velocity:.2%}")
            
            # Sell if innovation stalls
            elif velocity < -0.2 and self.Portfolio[self.equity_symbol].Invested:
                self.Liquidate(self.equity_symbol)
                self.Log(f"Selling on patent deceleration: velocity={velocity:.2%}")
```

---

## Example 4: Cross-Sectional Patent Ranking

Rank stocks within sector by patent metrics and go long-short.

```csharp
public class PatentRankingAlgorithm : QCAlgorithm
{
    private Dictionary<Symbol, Symbol> _patentSymbols;
    
    public override void Initialize()
    {
        SetStartDate(2020, 1, 1);
        SetCash(100000);
        
        // Add technology sector stocks
        var techStocks = new[] { "AAPL", "MSFT", "GOOGL", "META", "NVDA", 
                                  "AMD", "INTC", "QCOM", "AVGO", "TXN" };
        
        _patentSymbols = new Dictionary<Symbol, Symbol>();
        foreach (var ticker in techStocks)
        {
            var equity = AddEquity(ticker, Resolution.Daily).Symbol;
            _patentSymbols[equity] = AddData<GrantedPatentsData>(equity).Symbol;
        }
        
        Schedule.On(DateRules.MonthStart(), TimeRules.At(10, 0), Rebalance);
    }
    
    private void Rebalance()
    {
        var patentScores = new Dictionary<Symbol, decimal>();
        
        // Calculate composite patent score
        foreach (var kvp in _patentSymbols)
        {
            var equitySymbol = kvp.Key;
            var patentSymbol = kvp.Value;
            
            var history = History<GrantedPatentsData>(patentSymbol, 1, Resolution.Daily);
            var latest = history.LastOrDefault();
            
            if (latest != null && latest.CumulativePatents > 0)
            {
                // Composite score: 50% diversity + 30% recent activity + 20% total patents
                var diversityScore = latest.TechDiversity;
                var activityScore = latest.Patents90d / Math.Max(latest.CumulativePatents, 1);
                var scaleScore = Math.Min(latest.CumulativePatents / 1000m, 1);
                
                patentScores[equitySymbol] = 
                    0.5m * diversityScore + 
                    0.3m * activityScore + 
                    0.2m * scaleScore;
            }
        }
        
        // Rank and allocate
        var ranked = patentScores.OrderByDescending(x => x.Value).ToList();
        
        // Long top 5, short bottom 5
        for (int i = 0; i < ranked.Count; i++)
        {
            var symbol = ranked[i].Key;
            
            if (i < 5)
                SetHoldings(symbol, 0.2m);  // Long 20% each
            else
                SetHoldings(symbol, -0.1m); // Short 10% each
        }
    }
}
```

---

## Example 5: Geographic Diversification Premium

Favor companies with geographically distributed R&D.

```python
class GlobalInnovationAlgorithm(QCAlgorithm):
    def Initialize(self):
        self.SetStartDate(2020, 1, 1)
        self.SetCash(100000)
        
        self.UniverseSettings.Resolution = Resolution.Daily
        self.AddUniverse(GrantedPatentsDataUniverse, "PatentUniverse", self.SelectGlobalInnovators)
    
    def SelectGlobalInnovators(self, data):
        # Select companies with globally distributed R&D
        global_innovators = [x for x in data 
                              if x.UniqueLocations >= 5 
                              and x.TechDiversity > 0.5
                              and x.CumulativePatents > 50]
        
        # Weight by location count (more global = higher weight)
        ranked = sorted(global_innovators, 
                       key=lambda x: x.UniqueLocations, 
                       reverse=True)[:20]
        
        return [x.Symbol for x in ranked]
    
    def OnData(self, slice):
        # Equal weight portfolio
        targets = 1.0 / len(self.ActiveSecurities)
        
        for symbol in self.ActiveSecurities.Keys:
            if not self.Portfolio[symbol].Invested:
                self.SetHoldings(symbol, targets)
```

---

## Additional Resources

- **Full Documentation**: See IMPLEMENTATION_SUMMARY.md for complete technical details
- **Data Source**: See DATA_SOURCE_DETAILS.md for USPTO PatentsView information  
- **QuantConnect Examples**: https://github.com/QuantConnect?q=Lean.DataSource&type=&language=&sort=
