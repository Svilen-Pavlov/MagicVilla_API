using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Services.IServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Text;

namespace MagicVilla_Web.Services
{
    public class BaseService : IBaseService
    {
        public APIResponse responseModel { get; set; }
        public IHttpClientFactory httpClient { get; set; }
        public BaseService(IHttpClientFactory httpClient)
        {
            this.responseModel = new();
            this.httpClient = httpClient;
        }
        public async Task<T> SendAsync<T>(APIRequest apiRequest)
        {
            try
            {
                var client = httpClient.CreateClient("MagicAPI"); // Create client to send request
                HttpRequestMessage message = new HttpRequestMessage(); // Create the request itself
                message.Headers.Add("Accept", "application/json"); // Add headers
                message.RequestUri = new Uri(apiRequest.Url);  // Add URL

                if (apiRequest.Data != null) //Add body data, only for create/update
                {
                    message.Content = new StringContent(JsonConvert.SerializeObject(apiRequest.Data),
                        Encoding.UTF8, "application/json");
                }

                switch (apiRequest.ApiType) // configure request method
                {
                    case StaticDetails.ApiType.POST:
                        message.Method = HttpMethod.Post;
                        break;
                    case StaticDetails.ApiType.PUT:
                        message.Method = HttpMethod.Put;
                        break;
                    case StaticDetails.ApiType.DELETE:
                        message.Method = HttpMethod.Delete;
                        break;
                    default:
                        message.Method = HttpMethod.Get;
                        break;
                }

                HttpResponseMessage apiResponseMessage = null;

                if (!string.IsNullOrEmpty(apiRequest.Token))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(StaticDetails.JWTAuthenticationHeaderName, apiRequest.Token); // first param = "Bearer"
                }
                apiResponseMessage = await client.SendAsync(message); // Send and receive response
                var content = await apiResponseMessage.Content.ReadAsStringAsync(); // HttpResponseMessage > string (JSON formatted)
                var headers = apiResponseMessage.Headers.AsEnumerable().ToList();
                var responseCode = apiResponseMessage.StatusCode; // I believe this should be here instead of what lector used below in the IF
                try // if the apiContent string is of type APIContent
                {
                    APIResponse APIResponse = JsonConvert.DeserializeObject<APIResponse>(content); // string to APIResponse type
                    if (APIResponse != null && (responseCode == System.Net.HttpStatusCode.BadRequest || responseCode == System.Net.HttpStatusCode.NotFound))  //lector said this should be checking deserializedAPIResponse but it is stripped of the responseCode if erronous
                    {
                        APIResponse.IsSuccess = false;
                    }

                    List<KeyValuePair<string, List<string>>> headersKVP = new();
                    foreach (var item in headers)
                    {
                        var key = item.Key;
                        var listhHeaderValues = item.Value.ToList();
                        var kvp = new KeyValuePair<string, List<string>>(key, listhHeaderValues);
                        headersKVP.Add(kvp);
                    }
                    APIResponse.Headers = headersKVP;

                    var result = JsonConvert.SerializeObject(APIResponse);
                    var returnObj = JsonConvert.DeserializeObject<T>(result);
                    return returnObj;
                }
                catch (Exception) // if the apiContent string NOT of type APIContent
                {
                    var deserializedGenericResponse = JsonConvert.DeserializeObject<T>(content); // JSON format to APIResponse type
                    return deserializedGenericResponse;
                }
            }
            catch (Exception ex)
            {
                var dto = new APIResponse
                {
                    ErrorMessages = new List<string> { Convert.ToString(ex.Message) },
                    IsSuccess = false
                };
                var result = JsonConvert.SerializeObject(dto);
                var deserializedResponse = JsonConvert.DeserializeObject<T>(result);
                return deserializedResponse;
            }
        }
    }
}
