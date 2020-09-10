using System;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using Microsoft.Extensions.Configuration;

namespace CoreBot.Translation
{
   
    public class Translator
    {
        string GlobalToken = "";
        public readonly string subscriptionKey;// = "dff6db994ae74d32aeb4e62e6a937002";
        public readonly string translatorUri;// = "https://api.microsofttranslator.com/v2/Http.svc/";
        public readonly string cognitiveServicesTokenUri;// = "https://westeurope.api.cognitive.microsoft.com/sts/v1.0/issueToken";

       public Translator(IConfiguration configuration)
        {
            if (!string.IsNullOrEmpty(configuration["ocp-apim-subscription-key"]))
            {
                subscriptionKey = configuration["ocp-apim-subscription-key"];
            }
            if (!string.IsNullOrEmpty(configuration["translatoruri"]))
            {
                translatorUri = configuration["translatoruri"];
            }
            if (!string.IsNullOrEmpty(configuration["cognitiveservicestokenuri"]))
            {
                cognitiveServicesTokenUri = configuration["cognitiveservicestokenuri"];
            }
        }

        public string Translation(string languagecode, string text)
        {
            try
            {
                string uri = translatorUri + "/Translate?text=" + text + "&from=en" + "&to=" + languagecode + "";

                WebRequest translationWebRequest = WebRequest.Create(uri);
                var getaccesstoken = Task.Run(GetBearerTokenForTranslator).Result;
                translationWebRequest.Headers.Add("Authorization", getaccesstoken);

                WebResponse response = null;
                response = translationWebRequest.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encode = Encoding.GetEncoding("utf-8");

                StreamReader translatedStream = new StreamReader(stream, encode);
                XmlDocument xTranslation = new XmlDocument();
                xTranslation.LoadXml(translatedStream.ReadToEnd());

                return xTranslation.InnerText;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> GetBearerTokenForTranslator()
        {
          //  var azureSubscriptionKey = "634b13ecc55646e8b188352b76ff84a7";
            // var azureSubscriptionKey = configuration;
            var azureAuthToken = new GenerateAuthToken(subscriptionKey);
            var token = await azureAuthToken.GetAccessTokenAsync(cognitiveServicesTokenUri);

            GlobalToken = token;
            return GlobalToken;

        }

        public string TranslateToEnglish(string languagecode, string text)
        {
            try
            {


                string uri = translatorUri + "/Translate?text=" + text + "&from=" + languagecode + "&to=en";

                WebRequest translationWebRequest = WebRequest.Create(uri);
                var getaccesstoken = Task.Run(GetBearerTokenForTranslator).Result;
                translationWebRequest.Headers.Add("Authorization", getaccesstoken);

                WebResponse response = null;
                response = translationWebRequest.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encode = Encoding.GetEncoding("utf-8");

                StreamReader translatedStream = new StreamReader(stream, encode);
                XmlDocument xTranslation = new XmlDocument();
                xTranslation.LoadXml(translatedStream.ReadToEnd());

                return xTranslation.InnerText;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string DetectLanguageCode(string input)
        {
            try
            {
                string uri = translatorUri + "/Detect?text=" + input + "";
                WebRequest translationWebRequest = WebRequest.Create(uri);
                var getaccesstoken = Task.Run(GetBearerTokenForTranslator).Result;
                translationWebRequest.Headers.Add("Authorization", getaccesstoken);


                WebResponse response = null;
                response = translationWebRequest.GetResponse();
                Stream stream = response.GetResponseStream();
                Encoding encode = Encoding.GetEncoding("utf-8");
                StreamReader translatedStream = new StreamReader(stream, encode);
                XmlDocument xTranslation = new XmlDocument();
                xTranslation.LoadXml(translatedStream.ReadToEnd());
                return xTranslation.InnerText;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }

}
