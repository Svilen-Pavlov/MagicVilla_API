using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.DTO;

namespace MagicVilla_Web.Services.IServices
{
    public class VillaNumberService : BaseService, IVillaNumberService
    {
        private readonly IHttpClientFactory _clientFactory;
        private string _villaUrl;
        private static string apiURLRoutePrefix = "/api/VillaNumberAPI/";
        public VillaNumberService(IHttpClientFactory clientFactory, IConfiguration configuration) : base(clientFactory)
        {
            _clientFactory = clientFactory;
            _villaUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");
        }
        public Task<T> GetAsync<T>()
        {
            return SendAsync<T>(new APIRequest
            {
                Url = _villaUrl + apiURLRoutePrefix,
                ApiType = StaticDetails.ApiType.GET,
            });
        }

        public Task<T> GetAsync<T>(int id)
        {
            return SendAsync<T>(new APIRequest
            {
                Url = _villaUrl + apiURLRoutePrefix + id,
                ApiType = StaticDetails.ApiType.GET,
            });
        }
        public Task<T> CreateAsync<T>(VillaNumberCreateDTO dto)
        {
            return SendAsync<T>(new APIRequest
            {
                Url = _villaUrl + apiURLRoutePrefix,
                ApiType = StaticDetails.ApiType.POST,
                Data = dto
            });
        }

        public Task<T> DeleteAsync<T>(int id)
        {
            return SendAsync<T>(new APIRequest
            {
                Url = _villaUrl + apiURLRoutePrefix + id,
                ApiType = StaticDetails.ApiType.DELETE,
            });
        }

        public Task<T> UpdateAsync<T>(VillaNumberUpdateDTO dto)
        {
            return SendAsync<T>(new APIRequest
            {
                Url = _villaUrl + apiURLRoutePrefix + dto.VillaNo,
                ApiType = StaticDetails.ApiType.PUT,
                Data = dto
            });
        }
    }
}
