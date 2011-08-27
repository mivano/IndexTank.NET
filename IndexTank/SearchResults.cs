using System;
using System.Collections.Generic;

namespace IndexTank
{
    /// <summary>
    /// A set of paginated sorted search results. The product of performing a 'search' call.
    /// </summary>
    public class SearchResults
    {
        public long Matches { get; internal set; }
        public float SearchTime { get; internal set; }
        public List<Dictionary<string, Object>> Results { get; internal set; }
        public Dictionary<string, Dictionary<string, int>> Facets { get; internal set; }

        public SearchResults(Dictionary<string, Object> response)
        {
            Matches = (long)response["matches"];
            SearchTime = float.Parse((string)response["search_time"]);
            Results = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, Object>>>(response["results"].ToString());
            Facets = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(response["facets"].ToString());
        }

        public override string ToString()
        {
            return "Matches: " + Matches + "\nSearch Time: " + SearchTime + "\nResults: " + Results + "\nFacets: " + Facets;
        }
    }


}
