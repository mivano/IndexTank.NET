using System;
using System.Collections.Generic;
using System.Linq;

namespace IndexTank
{
    public class Query
    {
        public class Range
        {
            public int Id { get; internal set; }
            public double Floor { get; internal set; }
            public double Ceil { get; internal set; }

            public Range(int id, double floor, double ceil)
            {
                this.Id = id;
                this.Floor = floor;
                this.Ceil = ceil;
            }

            public string FilterDocvar
            {
                get { return "filter_docvar" + Id; }
            }

            public string FilterFunction
            {
                get { return "filter_function" + Id; }
            }

            public string Value
            {
                get
                {
                    return (Floor == Double.NegativeInfinity ? "*"
                       : (Floor).ToString())
                       + ":"
                       + (Ceil == Double.PositiveInfinity ? "*"
                               : (Ceil).ToString());
                }
            }

        }

        protected int start =0;
        protected int length = 10;
        protected int scoringFunction;
        protected List<string> snippetFields;
        protected List<string> fetchFields;
        protected Dictionary<string, List<string>> categoryFilters;
        protected List<Range> functionFilters;
        protected List<Range> documentVariableFilters;
        protected Dictionary<int, float> queryVariables;
        protected string querystring;

        public static Query ForString(string query)
        {
            return new Query(query);
        }

        public Query(string query)
        {
            this.querystring = query;
        }

        public Query WithStart(int start)
        {
            this.start = start;
            return this;
        }

        public Query WithLength(int length)
        {
            this.length = length;
            return this;
        }

        public Query WithScoringFunction(int scoringFunction)
        {
            this.scoringFunction = scoringFunction;
            return this;
        }

        public Query WithSnippetFields(IEnumerable<string> snippetFields)
        {
            if (snippetFields == null)
            {
                throw new ArgumentNullException("snippetFields must be non-null");
            }

            if (this.snippetFields == null)
            {
                this.snippetFields = new List<string>();
            }

            this.snippetFields.AddRange(snippetFields);

            return this;
        }

        public Query WithSnippetFields(params string[] snippetFields)
        {
            return WithSnippetFields(new List<string>(snippetFields));
        }

        public Query WithFetchFields(IEnumerable<string> fetchFields)
        {
            if (fetchFields == null)
            {
                throw new ArgumentNullException("fetchFields must be non-null");
            }

            if (this.fetchFields == null)
            {
                this.fetchFields = new List<string>();
            }

            this.fetchFields.AddRange(fetchFields);

            return this;
        }

        public Query WithFetchFields(params string[] fetchFields)
        {
            return WithFetchFields(new List<string>(fetchFields));
        }

        public Query WithDocumentVariableFilter(int variableIndex, double floor, double ceil)
        {
            if (documentVariableFilters == null)
            {
                this.documentVariableFilters = new List<Range>();
            }

            documentVariableFilters.Add(new Range(variableIndex, floor, ceil));

            return this;
        }

        public Query WithFunctionFilter(int functionIndex, double floor, double ceil)
        {
            if (functionFilters == null)
            {
                this.functionFilters = new List<Range>();
            }

            functionFilters.Add(new Range(functionIndex, floor, ceil));

            return this;
        }

        public Query WithCategoryFilters(Dictionary<string, List<string>> categoryFilters)
        {
            if (categoryFilters == null)
            {
                throw new ArgumentNullException("categoryFilters must be non-null");
            }

            if (this.categoryFilters == null && categoryFilters.Any())
            {
                this.categoryFilters = new Dictionary<string, List<string>>();
            }
            if (categoryFilters.Any())
            {
                foreach (var item in categoryFilters)
                {
                    this.categoryFilters.Add(item.Key, item.Value);
                }

            }

            return this;
        }

        public Query WithQueryVariables(Dictionary<int, float> queryVariables)
        {
            if (queryVariables == null)
            {
                throw new ArgumentNullException("queryVariables must be non-null");
            }

            if (this.queryVariables == null && queryVariables.Any())
            {
                this.queryVariables = new Dictionary<int, float>();
            }

            if (queryVariables.Any())
            {
                foreach (var item in queryVariables)
                {
                    this.queryVariables.Add(item.Key, item.Value);
                }
            }

            return this;
        }

        public Query WithQueryVariable(int name, float value)
        {


            if (this.queryVariables == null)
            {
                this.queryVariables = new Dictionary<int, float>();
            }

            this.queryVariables.Add(name, value);

            return this;
        }

        public ParameterMap ToParameterMap()
        {
            ParameterMap parameters = new ParameterMap();

            parameters.Put("start", start.ToString());
            parameters.Put("len", length.ToString());
            parameters.Put("function", scoringFunction.ToString());

            if (snippetFields != null)
                parameters.Put("snippet", Join(snippetFields, ","));
            if (fetchFields != null)
                parameters.Put("fetch", Join(fetchFields, ","));
            if (categoryFilters != null)
                parameters.Put("category_filters", categoryFilters.ToJsonString());

            if (documentVariableFilters != null)
            {
                foreach (Range range in documentVariableFilters)
                {
                    string key = "filter_docvar" + range.Id;
                    string value = (range.Floor == Double.NegativeInfinity ? "*"
                            : (range.Floor).ToString())
                            + ":"
                            + (range.Ceil == Double.PositiveInfinity ? "*"
                                    : (range.Ceil).ToString());
                    string param = parameters.GetFirst(key);
                    if (param == null)
                    {
                        parameters.Add(key, value);
                    }
                    else
                    {
                        parameters.Add(key, param + "," + value);
                    }
                }
            }

            if (functionFilters != null)
            {
                foreach (Range range in functionFilters)
                {
                    string key = "filter_function" + range.Id;
                    string value = (range.Floor == Double.NegativeInfinity ? "*"
                            : (range.Floor).ToString())
                            + ":"
                            + (range.Ceil == Double.PositiveInfinity ? "*"
                                    : (range.Ceil).ToString());
                    string param = parameters.GetFirst(key);
                    if (param == null)
                    {
                        parameters.Add(key, value);
                    }
                    else
                    {
                        parameters.Add(key, param + "," + value);
                    }
                }
            }

            if (queryVariables != null)
            {
                foreach (var entry in queryVariables)
                {
                    parameters.Add("var" + entry.Key, entry.Value.ToString());
                }
            }

            parameters.Put("q", querystring);

            return parameters;
        }

        public static string Join(IEnumerable<string> s, string delimiter)
        {
            return String.Join(delimiter, s);
        }

    }

}
