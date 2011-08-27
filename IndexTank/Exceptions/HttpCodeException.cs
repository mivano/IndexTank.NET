using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace IndexTank
{
    public  class HttpCodeException : Exception
    {
        /**
         * 400 = Invalid syntax<br>
         * 401 = Auth failed<br>
         * 404 = Index doesn't exist<br>
         * 204 = Index already exists<br>
         * 409 = Max number of indexes reached<br>
         */
        protected int httpCode;

        public HttpCodeException(int httpCode, string message) :
            base(message)
        {
            this.httpCode = httpCode;
        }

        public int GetHttpCode()
        {
            return httpCode;
        }
    }
}
