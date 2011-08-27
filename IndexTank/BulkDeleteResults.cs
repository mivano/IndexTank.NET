using System.Collections.Generic;

namespace IndexTank
{
    /// <summary>
    /// Aggregation of the outcome of deleting every document in the batch.
    /// </summary>

    public  class BulkDeleteResults : AbstractBatchResults<string>
    {

        public BulkDeleteResults(List<bool> results, List<string> errors, List<string> docids, bool hasErrors)
            : base(results, errors, docids, hasErrors)
        {

        }

        public string GetDocid(int position)
        {
            return this.GetElement(position);
        }

        public IEnumerable<string> GetFailedDocids()
        {
            return this.GetFailedElements();
        }
    }
}
