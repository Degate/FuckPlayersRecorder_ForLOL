﻿using System.Text;
using System.Text.Json;

namespace FuckPlayersRecorder_ForLOL.Service.Request
{
    public abstract class AbstractRequestService
    {
        protected HttpClientHandler _httpClientHandler;
        protected HttpClient _httpClient;

        protected HttpRequestMessage PrepareRequest(HttpMethod httpMethod,
                                                    string relativeUrl,
                                                    IEnumerable<string> queryParameters,
                                                    dynamic body)
        {
            var url = queryParameters == null ? relativeUrl : relativeUrl + BuildQueryParameterString(queryParameters);
            var request = new HttpRequestMessage(httpMethod, url);

            if (body != null)
            {
                var json = JsonSerializer.Serialize(body);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return request;
        }

        protected string BuildQueryParameterString(IEnumerable<string> queryParameters)
        {
            return "?" + string.Join("&", queryParameters.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        protected async Task<string> GetResponseContentAsync(HttpResponseMessage response)
        {
            return await response.Content.ReadAsStringAsync();
        }
        protected async Task<byte[]> GetResponseContentByByteArrayAsync(HttpResponseMessage response)
        {
            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
