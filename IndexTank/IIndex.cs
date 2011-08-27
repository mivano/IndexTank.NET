using System;
using System.Collections.Generic;

namespace IndexTank
{
    public interface IIndex
    {

        SearchResults Search(string query);

        SearchResults Search(Query query);

        void DeleteBySearch(string query);

        void DeleteBySearch(Query query);

        /// <summary>
        /// Creates this index.
        /// </summary>
        /// <param name="conf">The conf.</param>
        void Create(IndexConfiguration conf);

        /// <summary>
        /// Creates this index.
        /// 
        /// this method is equivalent to {@link Index#create(false)} 
        /// 
        /// @throws IndexAlreadyExistsException
        ///             If it already existed
        /// @throws MaximumIndexesExceededException
        ///             If the account has reached the limit
        /// </summary>
        void Create();


        /// <summary>
        /// Update this index.
        /// 
        /// @throws IndexDoesNotExistException*
        ///          if the index does not exist
        /// </summary>
        void Update(IndexConfiguration conf);

        /// <summary>
        /// Delete this index
        /// 
        /// @throws IndexDoesNotExistException*
        ///             If this index does not exists
        /// </summary>
        void Delete();

        /// <summary>
        /// Indexes a batch of documents
        /// 
        /// @param documents
        ///            an iterable of {@link IndexTankClient.Document}s
        /// @return a {@link IndexTankClient.BatchResults} with the results
        ///         information
        /// @throws IOException
        /// @throws IndexDoesNotExistException
        ///             if the index name used to build the Index object does not
        ///             match any index in the account
        /// @throws UnexpectedCodeException
        ///             if an error occurs serverside. This represents a temporary
        ///             error and it SHOULD BE HANDLED if a retry policy is
        ///             implemented.
        /// </summary>
        BatchResults AddDocuments(IEnumerable<Document> documents);

        void AddDocument(string documentId, Dictionary<string, string> fields);

        /// <summary>
        /// Indexes a document for the given docid and fields.
        /// 
        /// @param documentId
        ///            unique document identifier. Can't be longer than 1024 bytes
        ///            when UTF-8 encoded. Never {@code null}.
        /// @param fields
        ///            map with the document fields
        /// @param variables
        ///            map int -&gt; float with values for variables that can
        ///            later be used in scoring functions during searches.
        /// @throws IOException
        /// @throws IndexDoesNotExistException
        ///             if the index name used to build the Index object does not
        ///             match any index in the account
        /// @throws UnexpectedCodeException
        ///             if an error occurs serverside. This represents a temporary
        ///             error and it SHOULD BE HANDLED if a retry policy is
        ///             implemented.
        /// </summary>
        void AddDocument(string documentId, Dictionary<string, string> fields, Dictionary<int, float> variables);

        /// <summary>
        /// Indexes a document for the given docid and fields.
        /// 
        /// @param documentId
        ///            unique document identifier. Can't be longer than 1024 bytes
        ///            when UTF-8 encoded. Never {@code null}.
        /// @param fields
        ///            map with the document fields
        /// @param variables
        ///            map int -&gt; float with values for variables that can
        ///            later be used in scoring functions during searches.
        /// @param categories
        ///            map string -&gt; string with values for the faceting
        ///            categories for this document.
        /// @throws IOException
        /// @throws IndexDoesNotExistException
        ///             if the index name used to build the Index object does not
        ///             match any index in the account
        /// @throws UnexpectedCodeException
        ///             if an error occurs serverside. This represents a temporary
        ///             error and it SHOULD BE HANDLED if a retry policy is
        ///             implemented.
        /// </summary>
        void AddDocument(string documentId, Dictionary<string, string> fields, Dictionary<int, float> variables, Dictionary<string, string> categories);

        /// <summary>
        /// Deletes the given docid from the index if it existed. Otherwise, does
        /// nothing.
        /// 
        /// @param documentId
        ///            unique document identifier. Never {@code null}.
        /// @throws IOException
        /// @throws IndexDoesNotExistException
        /// @throws UnexpectedCodeException
        /// </summary>
        void DeleteDocument(string documentId);

        /// <summary>
        /// Deletes the given docids from the index if they existed. Otherwise, does
        /// nothing.
        /// 
        /// @param documentIds
        ///            a iterable with unique document identifiers. Never {@code null}.
        /// @throws IOException
        /// @throws IndexDoesNotExistException
        /// @throws UnexpectedCodeException
        /// </summary>
        BulkDeleteResults DeleteDocuments(IEnumerable<string> documentIds);

        /// <summary>
        /// Updates the variables of the document for the given docid.
        /// 
        /// @param documentId
        ///            unique document identifier. Never {@code null}.
        /// @param variables
        ///            map int -&gt; float with values for variables that can
        ///            later be used in scoring functions during searches.
        /// @throws IOException
        /// @throws IndexDoesNotExistException
        /// @throws UnexpectedCodeException
        /// </summary>
        void UpdateVariables(string documentId, Dictionary<int, float> variables);

        /// <summary>
        /// Updates the categories (for faceting purposes) of the document for the
        /// given docid.
        /// 
        /// @param documentId
        ///            unique document identifier. Never {@code null}.
        /// @param categories
        ///            map string -&gt; string with the values of this document for
        ///            each category. A blank value equals to removing the category
        ///            for the document.
        /// @throws IOException
        /// @throws IndexDoesNotExistException
        /// @throws UnexpectedCodeException
        /// </summary>
        void UpdateCategories(string documentId, Dictionary<string, string> variables);

        void Promote(string documentId, string query);

        void AddFunction(int functionIndex, string definition);

        void DeleteFunction(int functionIndex);

        Dictionary<string, string> ListFunctions();

        /// <summary>
        /// Returns whether an index for the name of this instance exists. If it
        /// doesn't, it can be created byAlready calling {@link #create()}.
        /// 
        /// @return true if the index exists
        /// </summary>
        bool Exists();

        /// <summary>
        /// Returns whether this index is responsive. Newly created indexes can take
        /// a little while to get started.
        /// 
        /// If this method returns False most methods in this class will raise an
        /// HttpException with a status of 503.
        /// 
        /// @return true if the Index has already started
        /// @throws IndexDoesNotExistException
        /// @throws IOException
        /// </summary>
        bool HasStarted();

        string GetStatus();

        string GetCode();

        DateTime GetCreationTime();

        bool IsPublicSearchEnabled();

        void RefreshMetadata();

        Dictionary<string, Object> GetMetaData();

    }
}
