using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.DTO;
using MagicVilla_Web.Services.IServices;

namespace MagicVilla_Web.Services
{
    public class AuthService : BaseService, IAuthService
    {
        private readonly IHttpClientFactory _clientFactory;
        private string _authUrl;
        private static string apiURLRoutePrefix = "/api/UsersAuth";
        public AuthService(IHttpClientFactory clientFactory, IConfiguration configuration) : base(clientFactory)
        {
            _clientFactory = clientFactory;
            _authUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");
        }

        public Task<T> LoginAsync<T>(LoginRequestDTO obj)
        {
            return SendAsync<T>(new APIRequest
            {
                Url = _authUrl + apiURLRoutePrefix + "/login",
                ApiType = StaticDetails.ApiType.POST,
                Data = obj
            });
        }

        public Task<T> RegisterAsync<T>(RegistrationRequestDTO obj)
        {
            return SendAsync<T>(new APIRequest
            {
                Url = _authUrl + apiURLRoutePrefix + "/register",
                ApiType = StaticDetails.ApiType.POST,
                Data = obj
            });
        }
    }
}
