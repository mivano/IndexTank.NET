using System.Web;

namespace IndexTank
{
    public class AuthenticationFailedException : HttpException
    {
        public AuthenticationFailedException(HttpCodeException source) :
            base(source.Message)
        {
        }
    }
}
