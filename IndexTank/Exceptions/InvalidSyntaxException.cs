using System;

namespace IndexTank
{
    public class InvalidSyntaxException : Exception
    {

        public InvalidSyntaxException(HttpCodeException source) :
            base(source.Message)
        {
        }
    }
}