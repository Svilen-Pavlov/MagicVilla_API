using System.Net;
using System.Transactions;

namespace MagicVilla_VillaAPI.Models
{
    public class APIResponse
    {
        private HttpStatusCode statusCode;

        public APIResponse()
        {
            ErrorMessages = new List<string>();
        }
        public HttpStatusCode StatusCode
        {
            get => statusCode;
            set
            {
                statusCode = value;
                int statusCodeNumber = (int)value;
                if (statusCodeNumber >= 200 && statusCodeNumber <= 299)
                {
                    this.IsSuccess = true;
                }
                if (statusCodeNumber >= 400 && statusCodeNumber <= 499)
                {
                    this.IsSuccess = false;
                }
            }
        }

        public bool IsSuccess { get; set; } = true;

        public List<string> ErrorMessages { get; set; }

        public object Result { get; set; }
    }
}
