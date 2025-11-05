### Meta
- **Dataset name**: Granted Patents Data
- **Vendor name**: USPTO (United States Patent and Trademark Office)
- **Vendor Website**: https://patentsview.org/


### Introduction

Granted Patents Data from USPTO PatentsView tracks innovation metrics for publicly traded companies through patent filing activity. The data covers patent filings with engineered features including filing counts, rolling windows, and technology diversity metrics, starting in 1976, and is delivered on a daily frequency. This dataset is created by aggregating and processing raw USPTO patent data from the PatentsView database with engineered features designed for quantitative trading strategies.

### About the Provider
The USPTO PatentsView database is maintained by the United States Patent and Trademark Office, providing comprehensive data on granted patents, inventors, assignees, and technology classifications. PatentsView provides access to detailed patent data for researchers, data scientists, and analysts interested in innovation trends and intellectual property analytics.

### Getting Started
Python:
```python
from AlgorithmImports import *

class GrantedPatentsDataAlgorithm(QCAlgorithm):
    def Initialize(self):
        self.SetStartDate(2020, 1, 1)
        self.equity_symbol = self.AddEquity("AAPL", Resolution.Daily).Symbol
        self.patent_data_symbol = self.AddData(GrantedPatentsData, self.equity_symbol).Symbol
```

C#:
```csharp
namespace QuantConnect.Algorithm.CSharp
{
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
}
```

### Data Summary
- **Start Date**: 1976 (varies by company)
- **Asset Coverage**: US public equities with patent activity
- **Resolution**: Daily
- **Data Density**: Sparse (patents are not filed every day)
- **Timezone**: UTC


### Example Applications

The USPTO Granted Patents Data enables researchers to accurately design strategies harnessing innovation metrics and intellectual property trends. Examples include:

- **Innovation Momentum**: Identify companies with increasing patent filing velocity (Patents30d, Patents90d) as signals of R&D acceleration
- **Technology Diversification**: Trade companies expanding into new technology areas (high TechDiversity, increasing UniqueIpcCodes)
- **Patent Portfolio Quality**: Weight positions by cumulative patent holdings and geographic diversity (CumulativePatents, UniqueLocations)
- **Sector Innovation Leaders**: Build universe filters selecting top patent filers within technology sectors
- **Long-term Innovation Alpha**: Correlate patent activity with long-term fundamental performance

### Data Point Attributes

- **PatentsFiled**: Number of patents filed on this date (integer)
- **CumulativePatents**: Total patents filed by company to date (integer)
- **Patents30d**: Rolling 30-day patent count (integer)
- **Patents90d**: Rolling 90-day patent count (integer)
- **Patents365d**: Rolling 365-day patent count (integer)
- **UniqueIpcCodes**: Count of unique IPC technology classifications (integer)
- **UniqueSections**: Count of unique IPC top-level sections (integer, 1-9)
- **TechDiversity**: Technology diversity score 0-1, higher = more varied (decimal)
- **UniqueLocations**: Count of distinct geographic locations in patent filings (integer)
