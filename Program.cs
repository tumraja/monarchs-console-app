using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace monarchs_console_app
{
    public class Monarch: IComparable<Monarch>
    {
        [JsonPropertyName("id")]
        public int id { get; set; }
        
        [JsonPropertyName("nm")]
        public string name { get; set; }
        
        [JsonPropertyName("cty")]
        public string city { get; set; }
        
        [JsonPropertyName("hse")]
        public string house { get; set; }
        
        [JsonPropertyName("yrs")]
        public string years { get; set; }
        
        public int period { get; set; }

        public int CompareTo(Monarch monarch)
        {
            if (monarch == null)
                return 1;

            else
                return this.period.CompareTo(monarch.period);
        }
    }
    
    public class MonarchCollection
    {
        private List<Monarch> monarchs;

        public MonarchCollection(List<Monarch> monarchs)
        {
            this.monarchs = monarchs;
        }

        public int CountItems()
        {
            return this.monarchs.Count;
        }

        public string GetMostCommonName()
        {
            return this.monarchs.GroupBy(monarch => monarch.name.Split(' ')[0])
                .OrderByDescending(m => m.Count())
                .Select(m => m.Key).First();
        }

        public Monarch GetLongestRuledMonarch() {
            this.monarchs.Sort();
            return monarchs[monarchs.Count - 1];
        }
    }
    
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        static async Task Main(string[] args)
        {
            await ProcessMonarchs();
        }

        private static async Task ProcessMonarchs()
        {
            var streamTask = client.GetStreamAsync("https://gist.githubusercontent.com/christianpanton/10d65ccef9f29de3acd49d97ed423736/raw/b09563bc0c4b318132c7a738e679d4f984ef0048/kings");
            var monarchs = await JsonSerializer.DeserializeAsync<List<Monarch>>(await streamTask);

            foreach (var monarch in monarchs)
            {
                monarch.period = await CalculateNumberOfYears(monarch);
            }
            
            MonarchCollection collection = new  MonarchCollection(monarchs);
            Monarch longestMonarch = collection.GetLongestRuledMonarch();
            Console.WriteLine($"How many monarchs are there in the list?: {collection.CountItems()}");
            Console.WriteLine($"Which monarch ruled the longest (and for how long)?: {longestMonarch.name}, {(longestMonarch.period)} years");
            Console.WriteLine($"Which house ruled the longest (and for how long)?: {longestMonarch.house}, {(longestMonarch.period)} years");
            Console.WriteLine($"What was the most common first name?: {collection.GetMostCommonName()}");
        }
        
        private static async Task<int> CalculateNumberOfYears(Monarch monarch)
        {
            bool hasDash = monarch.years.Contains('-');

            if (hasDash)
            {
                var year = monarch.years.Split('-');
                if (String.IsNullOrEmpty(year[1]))
                {
                    return DateTime.Now.Year - Int32.Parse(year[0]);
                }
                else
                {
                    return Int32.Parse(year[1]) - Int32.Parse(year[0]);
                }
            }
            else
            {
                return 1;
            }
        }
    }
}
