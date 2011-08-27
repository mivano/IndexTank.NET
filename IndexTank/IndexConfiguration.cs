using System;
using System.Collections.Generic;

namespace IndexTank
{
    public class IndexConfiguration
    {
        protected Boolean publicSearch;

        public IndexConfiguration EnablePublicSearch(Boolean publicSearchEnabled)
        {
            this.publicSearch = publicSearchEnabled;
            return this;
        }

        public Dictionary<string, Object> ToConfigurationMap()
        {
            var conf = new Dictionary<string, Object>();
            conf.Add("public_search", this.publicSearch);
            return conf;
        }
    }
}