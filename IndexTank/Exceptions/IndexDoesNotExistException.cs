using System;

namespace IndexTank
{
    public class IndexDoesNotExistException : Exception
    {

        public IndexDoesNotExistException(HttpCodeException source)
            : base(source.Message)
        {
        }

        public IndexDoesNotExistException(String message) :
            base(message)
        {
        }
    }
}
