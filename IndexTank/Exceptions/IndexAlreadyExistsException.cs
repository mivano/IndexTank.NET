using System;

namespace IndexTank
{
    public class IndexAlreadyExistsException : Exception
    {

        public IndexAlreadyExistsException(string message) :
            base(message)
        {
        }

        public IndexAlreadyExistsException(HttpCodeException source)
            : base(source.Message)
        {

        }
    }
}