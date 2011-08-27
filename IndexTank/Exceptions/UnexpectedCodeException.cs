using System;

namespace IndexTank
{
    public class UnexpectedCodeException : Exception
    {
        public readonly int httpCode;

        public UnexpectedCodeException(HttpCodeException source) :
            base(source.Message)
        {
            this.httpCode = source.GetHttpCode();
        }
    }
}
