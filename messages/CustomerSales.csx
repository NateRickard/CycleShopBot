using Newtonsoft.Json;

public class CustomerSales
{
    [JsonProperty("CUSTOMER")]
    public string Customer { get; set; }

    [JsonProperty("TOTALSALES")]
    public decimal TotalSales { get; set; }
}