using System.Collections.Generic;

namespace IndexTank
{
    /// <summary>
    /// Aggregation of the outcome of indexing every document in the batch.
    /// </summary>
    public  class BatchResults : AbstractBatchResults<Document>
    {

        public BatchResults(List<bool> results, List<string> errors, List<Document> documents, bool hasErrors)
            : base(results, errors, documents, hasErrors)
        {

        }

        public Document GetDocument(int position)
        {
            return this.GetElement(position);
        }

        public IEnumerable<Document> GetFailedDocuments()
        {
            return this.GetFailedElements();
        }
    }
}
