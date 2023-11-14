using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.DTO;
using MagicVilla_Web.Services.IServices;

namespace MagicVilla_Web.Services
{
    public class VillaNumberService : BaseService, IVillaNumberService
    {
        private readonly IHttpClientFactory _clientFactory;
        private string _villaUrl;
        private static string apiURLRoutePrefix = "/api/v1/VillaNumberAPI/";
        public VillaNumberService(IHttpClientFactory clientFactory, IConfiguration configuration) : base(clientFactory)
        {
            _clientFactory = clientFactory;
            _villaUrl = configuration.GetValue<string>("ServiceUrls:VillaAPI");
        }
        public Task<T> GetAllAsync<T>(string token)
        {
            return SendAsync<T>(new APIRequest
            {
                Url = _villaUrl + apiURLRoutePrefix,
                ApiType = StaticDetails.ApiType.GET,
                Token = token
            });
        }
        public Task<T> GetAllAsync<T>(string token, int pageSize, int pageNumber)
        {
            return SendAsync<T>(new APIRequest
            {
                Url = _villaUrl + apiURLRoutePrefix + "?pageSize=" + pageSize + "&pageNumber=" + pageNumber,
                ApiType = StaticDetails.ApiType.GET,
                Token = token
            });
        }

        public Task<T> GetAsync<T>(int id, string token)
        {
            return SendAsync<T>(new APIRequest
            {
                Url = _villaUrl + apiURLRoutePrefix + id,
                ApiType = StaticDetails.ApiType.GET,
                Token = token
            });
        }
        public Task<T> CreateAsync<T>(VillaNumberCreateDTO dto, string token)
        {
            return SendAsync<T>(new APIRequest
            {
                Url = _villaUrl + apiURLRoutePrefix,
                ApiType = StaticDetails.ApiType.POST,
                Data = dto,
                Token = token
            });
        }

        public Task<T> DeleteAsync<T>(int id, string token)
        {
            return SendAsync<T>(new APIRequest
            {
                Url = _villaUrl + apiURLRoutePrefix + id,
                ApiType = StaticDetails.ApiType.DELETE,
                Token = token
            });
        }

        public Task<T> UpdateAsync<T>(VillaNumberUpdateDTO dto, string token)
        {
            return SendAsync<T>(new APIRequest
            {
                Url = _villaUrl + apiURLRoutePrefix + dto.VillaNo,
                ApiType = StaticDetails.ApiType.PUT,
                Data = dto,
                Token = token
            });
        }
    }
}
