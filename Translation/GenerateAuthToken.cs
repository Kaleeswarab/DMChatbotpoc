using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace CoreBot.Translation
{
    public class GenerateAuthToken
    {
      
        static string url = "https://luischatbotintegration.cognitiveservices.azure.com/sts/v1.0/issueToken";
        private Uri ServiceUrl = new Uri(url);

         private const string OcpApimSubscriptionKeyHeader = "Ocp-Apim-Subscription-Key";
        
        private static readonly TimeSpan TokenCacheDuration = new TimeSpan(0, 5, 0);

        private string _storedTokenValue = string.Empty;

        private DateTime _storedTokenTime = DateTime.MinValue;

        public string SubscriptionKey { get; }

        public HttpStatusCode RequestStatusCode { get; private set; }
        public async Task<string> GetAccessTokenAsync(string cognitiveServiceTikenUri)
        {
            if (string.IsNullOrWhiteSpace(this.SubscriptionKey))
            {
                return string.Empty;
            }

            // Return the cached token if it is stil valid
            if ((DateTime.Now - _storedTokenTime) < TokenCacheDuration)
            {
                return _storedTokenValue;
            }

            try
            {
                ServiceUrl = new Uri(cognitiveServiceTikenUri);

                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Post;
                    request.RequestUri = ServiceUrl;
                    request.Content = new StringContent(string.Empty);
                    request.Headers.TryAddWithoutValidation(OcpApimSubscriptionKeyHeader, SubscriptionKey);
                    client.Timeout = TimeSpan.FromSeconds(180);
                    var response = await client.SendAsync(request);
                    this.RequestStatusCode = response.StatusCode;
                    response.EnsureSuccessStatusCode();
                    var token = await response.Content.ReadAsStringAsync();
                    _storedTokenTime = DateTime.Now;
                    _storedTokenValue = "Bearer " + token;  //cached for 5 mins
                    return _storedTokenValue;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }



        public GenerateAuthToken(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key), "A subscription key is required");
            }

            this.SubscriptionKey = key;
            this.RequestStatusCode = HttpStatusCode.InternalServerError;
        }
    }
}
