### Requesting Data
To add Granted Patents Data from USPTO PatentsView to your algorithm, use the AddData method to request the data. As with all datasets, you should save a reference to your symbol for easy use later in your algorithm. For detailed documentation on using custom data, see [Importing Custom Data](https://www.quantconnect.com/docs/algorithm-reference/importing-custom-data).

Python:
```python
class GrantedPatentsDataAlgorithm(QCAlgorithm):
    def Initialize(self):
        self.SetStartDate(2020, 1, 1)
        self.equity_symbol = self.AddEquity("AAPL", Resolution.Daily).Symbol
        self.patent_data_symbol = self.AddData(GrantedPatentsData, self.equity_symbol).Symbol
```

C#:
```csharp
public class GrantedPatentsDataAlgorithm : QCAlgorithm
{
    private Symbol _equitySymbol;
    private Symbol _patentDataSymbol;
    
    public override void Initialize()
    {
        SetStartDate(2020, 1, 1);
        _equitySymbol = AddEquity("AAPL", Resolution.Daily).Symbol;
        _patentDataSymbol = AddData<GrantedPatentsData>(_equitySymbol).Symbol;
    }
}
```

### Accessing Data
Data can be accessed via Slice events. Slice delivers unique events to your algorithm as they happen. We recommend saving the symbol object when you add the data for easy access to slice later. Data is available in daily resolution. You can see an example of the slice accessor in the code below.

Python:
```python
def OnData(self, slice):
    data = slice.Get(GrantedPatentsData)
    if data:
        patent_data = data[self.patent_data_symbol]
        
        # Access patent metrics
        if patent_data.PatentsFiled > 0:
            self.Log(f"Patents filed today: {patent_data.PatentsFiled}")
            self.Log(f"90-day patent count: {patent_data.Patents90d}")
            self.Log(f"Tech diversity: {patent_data.TechDiversity}")
            self.Log(f"Total patents: {patent_data.CumulativePatents}")
```

C#:
```csharp
public override void OnData(Slice slice)
{
    var data = slice.Get<GrantedPatentsData>();
    if (data.ContainsKey(_patentDataSymbol))
    {
        var patentData = data[_patentDataSymbol];
        
        // Access patent metrics
        if (patentData.PatentsFiled > 0)
        {
            Log($"Patents filed today: {patentData.PatentsFiled}");
            Log($"90-day patent count: {patentData.Patents90d}");
            Log($"Tech diversity: {patentData.TechDiversity}");
            Log($"Total patents: {patentData.CumulativePatents}");
        }
    }
}
```


### Historical Data
You can request historical custom data in your algorithm using the custom data Symbol object. To learn more about historical data requests, please visit the [Historical Data](https://www.quantconnect.com/docs/algorithm-reference/historical-data) documentation. If there is no custom data in the period you request, the history result will be empty. The following example gets the historical data for Granted Patents Data innovation metrics by using the History API.

Python:
```python
def Initialize(self):
    self.SetStartDate(2020, 1, 1)
    self.equity_symbol = self.AddEquity("AAPL", Resolution.Daily).Symbol
    self.patent_data_symbol = self.AddData(GrantedPatentsData, self.equity_symbol).Symbol
    
    # Request 365 days of patent history
    history = self.History(GrantedPatentsData, self.patent_data_symbol, 365, Resolution.Daily)
    
    for data in history:
        self.Log(f"Date: {data.Time}, Patents Filed: {data.PatentsFiled}, " +
                 f"Tech Diversity: {data.TechDiversity}")
```

C#:
```csharp
public override void Initialize()
{
    SetStartDate(2020, 1, 1);
    _equitySymbol = AddEquity("AAPL", Resolution.Daily).Symbol;
    _patentDataSymbol = AddData<GrantedPatentsData>(_equitySymbol).Symbol;
    
    // Request 365 days of patent history
    var history = History<GrantedPatentsData>(_patentDataSymbol, 365, Resolution.Daily);
    
    foreach (var data in history)
    {
        Log($"Date: {data.Time}, Patents Filed: {data.PatentsFiled}, " +
            $"Tech Diversity: {data.TechDiversity}");
    }
}
```

### Universe Selection
You can use patent data for universe selection to filter stocks based on innovation metrics. The GrantedPatentsDataUniverse class provides patent metrics for multiple stocks on each date.

Python:
```python
class PatentUniverseAlgorithm(QCAlgorithm):
    def Initialize(self):
        self.SetStartDate(2020, 1, 1)
        self.UniverseSettings.Resolution = Resolution.Daily
        self.AddUniverse(GrantedPatentsDataUniverse, "GrantedPatentsDataUniverse", self.PatentSelection)
    
    def PatentSelection(self, data):
        # Select stocks with strong innovation signals
        return [x.Symbol for x in data 
                if x.Patents90d > 0 
                and x.TechDiversity > 0.5 
                and x.CumulativePatents > 10]
```

C#:
```csharp
public class PatentUniverseAlgorithm : QCAlgorithm
{
    public override void Initialize()
    {
        SetStartDate(2020, 1, 1);
        UniverseSettings.Resolution = Resolution.Daily;
        AddUniverse<GrantedPatentsDataUniverse>("GrantedPatentsDataUniverse", PatentSelection);
    }
    
    private IEnumerable<Symbol> PatentSelection(IEnumerable<GrantedPatentsDataUniverse> data)
    {
        // Select stocks with strong innovation signals
        return from x in data
               where x.Patents90d > 0 
                     && x.TechDiversity > 0.5m 
                     && x.CumulativePatents > 10
               select x.Symbol;
    }
}
```