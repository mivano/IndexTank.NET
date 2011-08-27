using System;
using System.Collections.Generic;
using System.Threading;
using IndexTank;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {

            var indexTankApiUrl = "<fill in your api key here>";
            var indexName = "textindextankapi";

            var client = new IndexTankClient(indexTankApiUrl);

            IndexTankClient.Index index = client.GetIndex(indexName);

            if (!index.Exists())
                index.Create();

            while (!index.Exists())
                Thread.Sleep(5000);

            index.AddDocument("doc1",   // Document identifier
                                new Dictionary<string, string>{  // Fields
                                    {"text", "This is a text to store in the index"}, 
                                    {"author", "Ronin"}
                                },
                                new Dictionary<int, float> {     // Variables
                                    { 0, 10 } 
                                },
                                new Dictionary<string, string> {  // Categories
                                    {"price", "cheap" }
                                });

            index.AddDocument("doc2",   // Document identifier
                              new Dictionary<string, string>{  // Fields
                                    {"text", "Another piece of text found normally in a store"}, 
                                    {"author", "Bach"}
                                },
                              new Dictionary<int, float> {     // Variables
                                    { 0, 12 } 
                                },
                              new Dictionary<string, string> {  // Categories
                                    {"price", "expensive" }
                                });

            var query = new Query("store")
                .WithScoringFunction(0)
                .WithStart(0)
                .WithLength(10)
                .WithSnippetFields(new[] { "text" })
                .WithFetchFields(new[] { "text", "author" });

            var results = index.Search(query);

            Console.WriteLine("Found {0} results in {1} ms.", results.Matches, results.SearchTime);
            Console.WriteLine("Results: ");
            foreach (var item in results.Results)
            {
                System.Console.WriteLine(" {0} ({1})", item["snippet_text"], item["query_relevance_score"]);
            }

            Console.WriteLine("Facets:");

            foreach (var item in results.Facets)
            {
                System.Console.WriteLine(" {0}", item.Key);
                foreach (var facetItems in item.Value)
                {
                    Console.WriteLine(" {0}:{1}", facetItems.Key, facetItems.Value);
                }
            }

            Console.WriteLine("");
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();
        }
    }
}
