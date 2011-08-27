using System;
using System.Collections.Generic;

namespace IndexTank
{
    /// <summary>
    /// A document to be added to the Index 
    /// </summary>
    public class Document
    {
        /**
         * unique identifier
         */
        private string id;

        /**
         * fields
         */
        private Dictionary<string, string> fields;

        /**
         * scoring variables
         */
        private Dictionary<int, float> variables;

        /**
         * faceting categories
         */
        private Dictionary<string, string> categories;

        public Document(string id, Dictionary<string, string> fields, Dictionary<int, float> variables, Dictionary<string, string> categories)
        {
            if (id == null)
                throw new ArgumentNullException("Id cannot be null");

            if (System.Text.Encoding.UTF8.GetBytes(id).Length > 1024)
                throw new ArgumentNullException("documentId can not be longer than 1024 bytes when UTF-8 encoded.");

            this.id = id;
            this.fields = fields;
            this.variables = variables;
            this.categories = categories;
        }


        public Dictionary<string, Object> ToDocumentMap()
        {
            Dictionary<string, Object> documentMap = new Dictionary<string, Object>();
            documentMap.Add("docid", id);
            documentMap.Add("fields", fields);
            if (variables != null)
            {
                documentMap.Add("variables", variables);
            }
            if (categories != null)
            {
                documentMap.Add("categories", categories);
            }
            return documentMap;
        }


    }
}
