using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace src
{
    class Program
    {
        static void Main(string[] args)
        {
            string cpf = "";
            var bearerToken = new Token();

            var baseUri = new Uri("https://h-apigateway.conectagov.estaleiro.serpro.gov.br/");
            var encodedConsumerKey = System.Web.HttpUtility.UrlEncode("");
            var encodedConsumerKeySecret = System.Web.HttpUtility.UrlEncode("");
            var encodedPair = Base64Encode(String.Format("{0}:{1}", encodedConsumerKey, encodedConsumerKeySecret));

            var requestToken = new System.Net.Http.HttpRequestMessage
            {
                Method = System.Net.Http.HttpMethod.Post,
                RequestUri = new System.Uri(baseUri, "oauth2/jwt-token"),
                Content = new System.Net.Http.StringContent("grant_type=client_credentials")
            };

            requestToken.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };
            requestToken.Headers.TryAddWithoutValidation("Authorization", String.Format("Basic {0}", encodedPair));

            using (var client = new System.Net.Http.HttpClient())
            {
                var task = client.SendAsync(requestToken).ContinueWith((taskwithresponse) =>
                {
                    var bearerResult = taskwithresponse.Result;
                    var bearerData = bearerResult.Content.ReadAsStringAsync();
                    bearerData.Wait();
                    bearerToken = Newtonsoft.Json.JsonConvert.DeserializeObject<Token>(bearerData.Result);
                });

                task.Wait();

                //Console.WriteLine(bearerToken.access_token);     
                //Console.ReadKey();
            }

            using (var client = new System.Net.Http.HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken.access_token);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("x-cpf-usuario", "");

                var lc = new ListaCpf();
                lc.listaCpf.Add(cpf);

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(lc);
                var data = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = client.PostAsync(baseUri + "api-cpf-light/v2/consulta/cpf", data).Result;
                var retorno = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var dynJson = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(retorno);
                    Console.Clear();
                    Console.WriteLine(dynJson);
                    Console.ReadKey();
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine(response.StatusCode);
                    Console.ReadKey();
                }
            }
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public class Token
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
        }

        public class ListaCpf
        {
            public ListaCpf()
            {
                listaCpf = new System.Collections.Generic.List<string>();
            }
            public System.Collections.Generic.List<string> listaCpf { get; set; }
        }
    }
}
