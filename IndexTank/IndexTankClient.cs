using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace IndexTank
{
    public class IndexTankClient : ApiCallBase, IIndexTankClient
    {
        public static string ApiUrl;
        public static string PrivatePass;

        public IndexTankClient(string apiUrl)
        {
            ApiUrl = AppendTrailingSlash(apiUrl);
            try
            {
                PrivatePass = new Uri(apiUrl).UserInfo;
            }
            catch (UriFormatException e)
            {
                throw new InvalidOperationException(
                    "Could not create an IndexTankClient instance, the privatepass could not be set.", e);
            }
        }

        #region IIndexTankClient Members

        public Index GetIndex(string indexName)
        {
            return new Index(GetIndexUrl(indexName));
        }


        public Index CreateIndex(string indexName)
        {
            return CreateIndex(indexName, null);
        }


        public Index CreateIndex(string indexName, IndexConfiguration conf)
        {
            Index index = GetIndex(indexName);
            index.Create(conf);
            return index;
        }


        public void DeleteIndex(string indexName)
        {
            GetIndex(indexName).Delete();
        }


        public List<Index> ListIndexes()
        {
            try
            {
                var responseMap = CallApi<Dictionary<string, object>>(GET_METHOD, GetIndexesUrl(), PrivatePass);

                return responseMap.Select(entry => new Index(GetIndexUrl(entry.Key), (Dictionary<string, object>)entry.Value)).ToList();
            }
            catch (HttpCodeException e)
            {
                throw new UnexpectedCodeException(e);
            }
        }

        #endregion

        private static string AppendTrailingSlash(string apiUrl)
        {
            if (!apiUrl.EndsWith("/"))
            {
                apiUrl += "/";
            }
            return apiUrl;
        }

        private string GetIndexUrl(string indexName)
        {
            return GetIndexesUrl() + EncodeIndexName(indexName);
        }

        private static string EncodeIndexName(string indexName)
        {
            Uri url;
            try
            {
                url = new Uri("http://none.com/" + indexName);
            }
            catch (UriFormatException e)
            {
                return indexName;
            }
            return url.AbsolutePath.Substring(1);
        }

        private string GetIndexesUrl()
        {
            string indexesUrl = ApiUrl + "v1/indexes/";
            return indexesUrl;
        }

        #region Nested type: Index

        /// <summary>
        /// Client to control a specific index.
        /// </summary>
        public class Index : IIndex
        {
            private readonly string indexUrl;
            private Dictionary<string, Object> metadata;

            internal Index(string indexUrl)
            {
                this.indexUrl = indexUrl;
            }

            internal Index(string indexUrl, Dictionary<string, Object> metadata)
            {
                this.indexUrl = indexUrl;
                this.metadata = metadata;
            }

            #region IIndex Members

            public SearchResults Search(string query)
            {
                return Search(Query.ForString(query));
            }


            public SearchResults Search(Query query)
            {
                ParameterMap parameters = query.ToParameterMap();

                try
                {
                    return
                        new SearchResults(CallApi<Dictionary<string, Object>>(GET_METHOD, indexUrl + SEARCH_URL, parameters, PrivatePass));
                }
                catch (HttpCodeException e)
                {
                    if (e.GetHttpCode() == 400)
                    {
                        throw new InvalidSyntaxException(e);
                    }

                    throw new UnexpectedCodeException(e);
                }
            }


            public void DeleteBySearch(string query)
            {
                DeleteBySearch(Query.ForString(query));
            }


            public void DeleteBySearch(Query query)
            {
                ParameterMap parameters = query.ToParameterMap();

                try
                {
                    CallApi<Dictionary<string, Object>>(DELETE_METHOD, indexUrl + SEARCH_URL, parameters, PrivatePass);
                }
                catch (HttpCodeException e)
                {
                    switch (e.GetHttpCode())
                    {
                        case 400:
                            throw new InvalidSyntaxException(e);
                        case 404:
                            throw new IndexDoesNotExistException(e);
                        default:
                            throw new UnexpectedCodeException(e);
                    }
                }
            }


            public void Create()
            {
                Create(null);
            }


            public void Create(IndexConfiguration conf)
            {
                if (Exists())
                    throw new IndexAlreadyExistsException("Index already exists");

                Dictionary<string, Object> data = null;
                if (conf != null)
                    data = conf.ToConfigurationMap();

                try
                {
                    CallApi<Dictionary<string, Object>>(PUT_METHOD, indexUrl, null, data, PrivatePass);
                }
                catch (HttpCodeException e)
                {
                    switch (e.GetHttpCode())
                    {
                        case 204:
                            throw new IndexAlreadyExistsException(e);
                        case 409:
                            throw new MaximumIndexesExceededException(e);
                        default:
                            throw new UnexpectedCodeException(e);
                    }
                }
            }


            public void Update(IndexConfiguration conf)
            {
                if (conf == null)
                    throw new ArgumentNullException("conf");

                Dictionary<string, Object> data = conf.ToConfigurationMap();

                if (data.Count == 0)
                    throw new ArgumentNullException("conf");

                if (!Exists())
                    throw new IndexDoesNotExistException("Index does not exist");

                try
                {
                    CallApi<Dictionary<string, Object>>(PUT_METHOD, indexUrl, null, data, PrivatePass);
                }
                catch (HttpCodeException e)
                {
                    if (e.GetHttpCode() == 204)
                    {
                        RefreshMetadata();
                        return;
                    }
                    throw new UnexpectedCodeException(e);
                }
            }


            public void Delete()
            {
                try
                {
                    CallApi<Dictionary<string, Object>>(DELETE_METHOD, indexUrl, PrivatePass);
                }
                catch (HttpCodeException e)
                {
                    if (e.GetHttpCode() == 404)
                    {
                        throw new IndexDoesNotExistException(e);
                    }
                    throw new UnexpectedCodeException(e);
                }
            }


            public BatchResults AddDocuments(IEnumerable<Document> documents)
            {
                var data = new List<Dictionary<string, Object>>();

                foreach (Document document in documents)
                {
                    Dictionary<string, Object> documentMap = document.ToDocumentMap();
                    data.Add(documentMap);
                }

                try
                {
                    var results = CallApi<List<Dictionary<string, Object>>>(PUT_METHOD, indexUrl + DOCS_URL, null, data,
                                                                            PrivatePass);

                    var addeds = new List<bool>(results.Count);
                    var errors = new List<string>(results.Count);
                    bool hasErrors = false;

                    for (int i = 0; i < results.Count; i++)
                    {
                        Dictionary<string, Object> result = results[i];
                        var added = (bool)result["added"];

                        addeds.Insert(i, added);

                        if (!added)
                        {
                            hasErrors = true;
                            errors.Insert(i, (string)result["error"]);
                        }
                    }

                    var documentsList = documents.ToList();

                    return new BatchResults(addeds, errors, documentsList, hasErrors);
                }
                catch (HttpCodeException e)
                {
                    switch (e.GetHttpCode())
                    {
                        case 400:
                            throw new InvalidSyntaxException(e);
                        case 404:
                            throw new IndexDoesNotExistException(e);
                        default:
                            throw new UnexpectedCodeException(e);
                    }
                }
            }


            public void AddDocument(string documentId, Dictionary<string, string> fields)
            {
                AddDocument(documentId, fields, null);
            }


            public void AddDocument(string documentId, Dictionary<string, string> fields,
                                    Dictionary<int, float> variables)
            {
                AddDocument(documentId, fields, variables, null);
            }


            public void AddDocument(string documentId, Dictionary<string, string> fields,
                                    Dictionary<int, float> variables, Dictionary<string, string> categories)
            {
                if (null == documentId)
                    throw new ArgumentNullException("documentId");

                if (Encoding.UTF8.GetBytes(documentId).Length > 1024)
                    throw new ArgumentOutOfRangeException(paramName: "documentId",
                                                          message: "documentId can not be longer than 1024 bytes when UTF-8 encoded.");

                var data = new Dictionary<string, Object>();
                data.Add("docid", documentId);
                data.Add("fields", fields);
                if (variables != null)
                {
                    data.Add("variables", variables);
                }

                if (categories != null)
                {
                    data.Add("categories", categories);
                }

                try
                {
                    CallApi<Dictionary<string, Object>>(PUT_METHOD, indexUrl + DOCS_URL, null, data, PrivatePass);
                }
                catch (HttpCodeException e)
                {
                    switch (e.GetHttpCode())
                    {
                        case 400:
                            throw new UnexpectedCodeException(e);
                        case 404:
                            throw new IndexDoesNotExistException(e);
                        default:
                            throw new UnexpectedCodeException(e);
                    }
                }
            }


            public void DeleteDocument(string documentId)
            {
                if (null == documentId)
                    throw new ArgumentNullException("documentId");

                var parameters = new ParameterMap();
                parameters.Add("docid", documentId);

                try
                {
                    CallApi<Dictionary<string, Object>>(DELETE_METHOD, indexUrl + DOCS_URL, parameters, PrivatePass);
                }
                catch (HttpCodeException e)
                {
                    if (e.GetHttpCode() == 404)
                    {
                        throw new IndexDoesNotExistException(e);
                    }
                    throw new UnexpectedCodeException(e);
                }
            }


            public BulkDeleteResults DeleteDocuments(IEnumerable<string> documentIds)
            {
                if (null == documentIds)
                    throw new ArgumentNullException("documentIds");

                var parameters = new ParameterMap();
                parameters.AddAll("docid", documentIds);

                try
                {
                    var results = CallApi<List<Dictionary<string, Object>>>(DELETE_METHOD, indexUrl + DOCS_URL,
                                                                            parameters, PrivatePass);

                    var deleted = new List<bool>(results.Count);
                    var errors = new List<string>(results.Count);
                    bool hasErrors = false;

                    for (int i = 0; i < results.Count; i++)
                    {
                        Dictionary<string, Object> result = results[i];
                        var wasDeleted = (bool)result["deleted"];

                        deleted.Insert(i, wasDeleted);

                        if (!wasDeleted)
                        {
                            hasErrors = true;
                            errors.Insert(i, (string)result["error"]);
                        }
                    }

                    var docidList = documentIds.ToList();

                    return new BulkDeleteResults(deleted, errors, docidList, hasErrors);
                }
                catch (HttpCodeException e)
                {
                    if (e.GetHttpCode() == 404)
                    {
                        throw new IndexDoesNotExistException(e);
                    }
                    throw new UnexpectedCodeException(e);
                }
            }


            public void UpdateVariables(string documentId, Dictionary<int, float> variables)
            {
                if (null == documentId)
                    throw new ArgumentNullException("documentId");

                var data = new Dictionary<string, Object>
                               {
                                   {"docid", documentId},
                                   {"variables", variables}
                               };

                try
                {
                    CallApi<Dictionary<string, Object>>(PUT_METHOD, indexUrl + VARIABLES_URL, null, data, PrivatePass);
                }
                catch (HttpCodeException e)
                {
                    if (e.GetHttpCode() == 404)
                    {
                        throw new IndexDoesNotExistException(e);
                    }
                    throw new UnexpectedCodeException(e);
                }
            }


            public void UpdateCategories(string documentId, Dictionary<string, string> variables)
            {
                var data = new Dictionary<string, Object>
                               {
                                   {"docid", documentId},
                                   {"categories", variables}
                               };

                try
                {
                    CallApi<Dictionary<string, Object>>(PUT_METHOD, indexUrl + CATEGORIES_URL, null, data, PrivatePass);
                }
                catch (HttpCodeException e)
                {
                    switch (e.GetHttpCode())
                    {
                        case 404:
                            throw new IndexDoesNotExistException(e);
                        default:
                            throw new UnexpectedCodeException(e);
                    }
                }
            }


            public void Promote(string documentId, string query)
            {
                if (null == documentId)
                    throw new ArgumentNullException("documentId");

                var data = new Dictionary<string, Object>
                               {
                                   {"docid", documentId}, 
                                   {"query", query}
                               };

                try
                {
                    CallApi<Dictionary<string, Object>>(PUT_METHOD, indexUrl + PROMOTE_URL, null, data, PrivatePass);
                }
                catch (HttpCodeException e)
                {
                    switch (e.GetHttpCode())
                    {
                        case 404:
                            throw new IndexDoesNotExistException(e);
                        default:
                            throw new UnexpectedCodeException(e);
                    }
                }
            }


            public void AddFunction(int functionIndex, string definition)
            {
                var data = new Dictionary<string, Object>
                               {
                                   {"definition", definition}
                               };

                try
                {
                    CallApi<Dictionary<string, Object>>(PUT_METHOD, indexUrl + FUNCTIONS_URL + "/" + functionIndex, null, data, PrivatePass);
                }
                catch (HttpCodeException e)
                {
                    switch (e.GetHttpCode())
                    {
                        case 404:
                            throw new IndexDoesNotExistException(e);
                        case 400:
                            throw new InvalidSyntaxException(e);
                        default:
                            throw new UnexpectedCodeException(e);
                    }
                }
            }


            public void DeleteFunction(int functionIndex)
            {
                try
                {
                    CallApi<Dictionary<string, Object>>(DELETE_METHOD, indexUrl + FUNCTIONS_URL + "/" + functionIndex,
                                                        PrivatePass);
                }
                catch (HttpCodeException e)
                {
                    switch (e.GetHttpCode())
                    {
                        case 404:
                            throw new IndexDoesNotExistException(e);
                        default:
                            throw new UnexpectedCodeException(e);
                    }
                }
            }


            public Dictionary<string, string> ListFunctions()
            {
                try
                {
                    var responseMap = CallApi<Dictionary<string, Object>>(GET_METHOD, indexUrl + FUNCTIONS_URL,
                                                                          PrivatePass);

                    return responseMap.ToDictionary(entry => entry.Key, entry => (string)entry.Value);
                }
                catch (HttpCodeException e)
                {
                    if (e.GetHttpCode() == 404)
                    {
                        throw new IndexDoesNotExistException(e);
                    }

                    throw new UnexpectedCodeException(e);
                }
            }


            public bool Exists()
            {
                try
                {
                    RefreshMetadata();
                    return true;
                }
                catch (IndexDoesNotExistException)
                {
                    return false;
                }
            }


            public bool HasStarted()
            {
                RefreshMetadata();

                return GetMetaData()["started"] != null && (bool)GetMetaData()["started"];
            }


            public string GetStatus()
            {
                return (string)GetMetaData()["status"];
            }


            public string GetCode()
            {
                return (string)GetMetaData()["code"];
            }


            public DateTime GetCreationTime()
            {
                try
                {
                    return DateTime.ParseExact(((string)GetMetaData()["creation_time"]), ISO8601_PARSER,
                                               CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                    return DateTime.MinValue;
                }
            }


            public bool IsPublicSearchEnabled()
            {
                return (bool)GetMetaData()["public_search"];
            }


            public void RefreshMetadata()
            {
                try
                {
                    metadata = CallApi<Dictionary<string, Object>>(GET_METHOD, indexUrl, PrivatePass);
                }
                catch (HttpCodeException e)
                {
                    if (e.GetHttpCode() == 404)
                    {
                        throw new IndexDoesNotExistException(e);
                    }
                    throw new UnexpectedCodeException(e);
                }
            }


            public Dictionary<string, Object> GetMetaData()
            {
                if (metadata == null)
                {
                    RefreshMetadata();
                }

                return metadata;
            }

            #endregion
        }

        #endregion
    }


    public class ParameterMap
    {
        private readonly Dictionary<string, List<string>> _innerMap;

        public ParameterMap()
        {
            _innerMap = new Dictionary<string, List<string>>();
        }

        public IEnumerable<string> KeySet()
        {
            return _innerMap.Keys;
        }

        public void AddAll(string key, IEnumerable<string> newvalues)
        {
            var values = _innerMap[key];
            if (values == null)
            {
                values = new List<string>();
                _innerMap.Add(key, values);
            }

            values.AddRange(newvalues);
        }

        public bool IsEmpty()
        {
            return _innerMap.Count == 0;
        }

        public void Add(string key, string value)
        {

            List<string> values;
            if (_innerMap.TryGetValue(key, out values))
            {
                values.Add(value);
            }
            else
            {
                Put(key, value);
            }
        }

        public void Put(string key, string value)
        {
            var values = new List<string> { value };
            _innerMap.Add(key, values);
        }

        public string GetFirst(string key)
        {
            if (_innerMap.ContainsKey(key))
            {
                var values = _innerMap[key];
                if (values != null && values.Count > 0)
                    return values[0];
            }
            return null;
        }

        public IEnumerable<string> Get(string key)
        {
            return _innerMap[key];
        }
    }
}