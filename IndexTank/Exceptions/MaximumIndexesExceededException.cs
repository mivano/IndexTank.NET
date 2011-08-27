using System;

namespace IndexTank
{
    public class MaximumIndexesExceededException : Exception
    {

        public MaximumIndexesExceededException(HttpCodeException source) :
            base(source.Message)
        {
        }
    }
}
