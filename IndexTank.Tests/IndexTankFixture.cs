using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IndexTank.Tests
{
    [TestClass]
    public class IndexTankFixture
    {
        private const String INDEX_NAME = "Index";
        private const String APIURL = "http://:0GoaPt9n+HmLx3@djtun.api.indextank.com";

        [TestCleanup]
        public void Cleanup()
        {
            IndexTankClient client = new IndexTankClient(APIURL);
            IndexTank.IndexTankClient.Index index = client.GetIndex(INDEX_NAME);

            if (index.Exists())
                index.Delete();
        }

        [TestMethod]
        public void CanCreateIndex()
        {
            IndexTankClient client = new IndexTankClient(APIURL);
            IndexTank.IndexTankClient.Index index = client.GetIndex(INDEX_NAME);

            if (index.Exists())
                index.Delete();

            index.Create();

            Assert.IsTrue(index.Exists());

            index.Delete();

            Assert.IsFalse(index.Exists());
        }

        [TestMethod]
        public void CanAddDocumentToIndex()
        {
            IndexTankClient client = new IndexTankClient(APIURL);
            IndexTank.IndexTankClient.Index index = client.GetIndex(INDEX_NAME);

            if (!index.Exists())
                index.Create();

            Assert.IsTrue(index.Exists());

            while (index.HasStarted() == false)
                System.Threading.Thread.Sleep(1000);

            var fields = new Dictionary<string, string>{
                             {"text", "This is a test document"}, 
                             {"category", "test material"}
            };

            index.AddDocument("DOC1", fields);

        }

        [TestMethod]
        public void CanAddDocumentToIndexAndSearch()
        {
            IndexTankClient client = new IndexTankClient(APIURL);
            IndexTank.IndexTankClient.Index index = client.GetIndex(INDEX_NAME);

            if (!index.Exists())
                index.Create();

            Assert.IsTrue(index.Exists());

            while (index.HasStarted() == false)
                System.Threading.Thread.Sleep(1000);

            var fields = new Dictionary<string, string>{
                             {"text", "This is a test document"}, 
                             {"category", "test material"}
            };

            index.AddDocument("DOC1", fields);

             fields = new Dictionary<string, string>{
                             {"text", "This is another document"}, 
                             {"category", "test material"}
            };

            index.AddDocument("DOC2", fields);

            fields = new Dictionary<string, string>{
                             {"text", "Nothing better then localhost"}, 
                             {"category", "test material"},
                             {"price", "4,00"}
            };

            index.AddDocument("DOC3", fields);

            // Sleep to they can get processed
            System.Threading.Thread.Sleep(3000);

            var query = new Query("text:document");
            query.WithFetchFields(new[] {"text", "category", "price"});

            var results = index.Search(query);

            Assert.IsNotNull(results);

        }
    }
}
