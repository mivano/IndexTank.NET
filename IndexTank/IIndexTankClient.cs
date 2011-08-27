using System.Collections.Generic;

namespace IndexTank
{
    public interface IIndexTankClient
    {
        IndexTankClient.Index GetIndex(string indexName);

        IndexTankClient.Index CreateIndex(string indexName);

        IndexTankClient.Index CreateIndex(string indexName, IndexConfiguration conf);

        void DeleteIndex(string indexName);

        List<IndexTankClient.Index> ListIndexes();

    }
}