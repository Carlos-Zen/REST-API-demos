using Huobi.Rest.CSharp.Demo.Model;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;


namespace Huobi.Rest.CSharp.Demo
{
    /// <summary>
    /// GitHub:https://github.com/huobiapi/REST-API-demos
    /// </summary>
    public class HuobiApi
    {

        #region HuoBiApi configuration information
        /// <summary>
        /// API URL domain name
        /// </summary>
        private readonly string HUOBI_HOST = string.Empty;
        /// <summary>
        /// API URL
        /// </summary>
        private readonly string HUOBI_HOST_URL = string.Empty;
        /// <summary>
        /// Encryption method
        /// </summary>
        private const string HUOBI_SIGNATURE_METHOD = "HmacSHA256";
        /// <summary>
        /// API version
        /// </summary>
        private const int HUOBI_SIGNATURE_VERSION = 2;
        /// <summary>
        /// ACCESS_KEY
        /// </summary>
        private readonly string ACCESS_KEY = string.Empty;
        /// <summary>
        /// SECRET_KEY()
        /// </summary>
        private readonly string SECRET_KEY = string.Empty;
        /// <summary>
        ///PRIVATE_KEY()
        /// </summary>
        private readonly string PRIVATE_KEY = string.Empty;
        #endregion

        #region HuoBiApi interfaces
        private const string API_ACCOUNBT_BALANCE = "/v1/account/accounts/{0}/balance";
        private const string API_ACCOUNBT_ALL = "/v1/account/accounts";
        private const string API_ORDERS_PLACE = "/v1/order/orders/place";
        #endregion

        #region construct function
        private RestClient client;//Restful request client
        

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Huobi.Rest.CSharp.Demo.HuobiApi"/> class.
        /// </summary>
        /// <param name="accessKey">Access key.</param>
        /// <param name="secretKey">Secret key.</param>
        /// <param name="privateKey">ECDSA algorithm private key.</param>
        /// <param name="huobi_host">Huobi host.</param>
        public HuobiApi(string accessKey, string secretKey, string privateKey,  string huobi_host = "api.huobi.pro")
        {
            ACCESS_KEY = accessKey;
            SECRET_KEY = secretKey;
            PRIVATE_KEY = privateKey;
            HUOBI_HOST = huobi_host;
            HUOBI_HOST_URL = "https://" + HUOBI_HOST;
            if (string.IsNullOrEmpty(ACCESS_KEY))
                throw new ArgumentException("ACCESS_KEY Cannt Be Null Or Empty");
            if (string.IsNullOrEmpty(SECRET_KEY))
                throw new ArgumentException("SECRET_KEY  Cannt Be Null Or Empty");
            if (string.IsNullOrEmpty(PRIVATE_KEY))
                throw new ArgumentException("ECDSA PRIVATE_KEY  Cannt Be Null Or Empty");
            if (string.IsNullOrEmpty(HUOBI_HOST))
                throw new ArgumentException("HUOBI_HOST  Cannt Be Null Or Empty");
            client = new RestClient(HUOBI_HOST_URL);
            client.AddDefaultHeader("Content-Type", "application/json");
            client.AddDefaultHeader("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.71 Safari/537.36");
        }
        #endregion

        #region HuoBiApi interface methods
        /// <summary>
        /// Get all accounts
        /// </summary>
        /// <returns></returns>
        public List<Account> GetAllAccount()
        {
            var result = SendRequest<List<Account>>(API_ACCOUNBT_ALL);
            return result.Data;
        }
        
        /// <summary>
        /// Place order
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public HBResponse<long> OrderPlace(OrderPlaceRequest req)
        {
            var bodyParas = new Dictionary<string, string>();
            var result = SendRequest<long, OrderPlaceRequest>(API_ORDERS_PLACE, req);
            return result;
        }
        #endregion

        #region HTTP request method
        /// <summary>
        /// Make a REST GET request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resourcePath"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private HBResponse<T> SendRequest<T>(string resourcePath, string parameters = "") where T : new()
        {
            parameters = UriEncodeParameterValue(GetCommonParameters() + parameters);//请求参数
            var sign = GetSignatureStr(Method.GET, HUOBI_HOST, resourcePath, parameters);//签名
            var privateSign = GetPrivateSignatureStr(PRIVATE_KEY, sign);
            var signUrl = UrlEncode(sign);
            var privateSignUrl = UrlEncode(privateSign);
            parameters += $"&Signature={signUrl}";
            parameters += $"&PrivateSignature={privateSignUrl}";

            var url = $"{HUOBI_HOST_URL}{resourcePath}?{parameters}";
            Console.WriteLine(url);
            var request = new RestRequest(url, Method.GET);
            var result = client.Execute<HBResponse<T>>(request);
            return result.Data;
        }

        /// <summary>
        /// Make a REST POST request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="resourcePath"></param>
        /// <param name="postParameters"></param>
        /// <returns></returns>
        private HBResponse<T> SendRequest<T, P>(string resourcePath, P postParameters) where T : new()
        {
            var parameters = UriEncodeParameterValue(GetCommonParameters());//请求参数
            var sign = GetSignatureStr(Method.POST, HUOBI_HOST, resourcePath, parameters);//签名
            var privateSign = GetPrivateSignatureStr(PRIVATE_KEY, sign);
            var signUrl = UrlEncode(sign);
            var privateSignUrl = UrlEncode(privateSign);
            parameters += $"&Signature={signUrl}";
            parameters += $"&PrivateSignature={privateSignUrl}";

            var url = $"{HUOBI_HOST_URL}{resourcePath}?{parameters}";
            Console.WriteLine(url);
            var request = new RestRequest(url, Method.POST);
            request.AddJsonBody(postParameters);
            foreach (var item in request.Parameters)
            {
                item.Value = item.Value.ToString().Replace("_", "-");
            }
            var result = client.Execute<HBResponse<T>>(request);
            return result.Data;
        }
        /// <summary>
        /// Get common request parameters
        /// </summary>
        /// <returns></returns>
        private string GetCommonParameters()
        {
            return $"AccessKeyId={ACCESS_KEY}&SignatureMethod={HUOBI_SIGNATURE_METHOD}&SignatureVersion={HUOBI_SIGNATURE_VERSION}&Timestamp={DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")}";
        }
        /// <summary>
        /// Uri encode parameter values
        /// </summary>
        /// <param name="parameters">parameter string</param>
        /// <returns></returns>
        private string UriEncodeParameterValue(string parameters)
        {
            var sb = new StringBuilder();
            var paraArray = parameters.Split('&');
            var sortDic = new SortedDictionary<string, string>();
            foreach (var item in paraArray)
            {
                var para = item.Split('=');
                sortDic.Add(para.First(), UrlEncode(para.Last()));
            }
            foreach (var item in sortDic)
            {
                sb.Append(item.Key).Append("=").Append(item.Value).Append("&");
            }
            return sb.ToString().TrimEnd('&');
        }
        /// <summary>
        /// Url encode string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string UrlEncode(string str)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in str)
            {
                if (HttpUtility.UrlEncode(c.ToString(), Encoding.UTF8).Length > 1)
                {
                    builder.Append(HttpUtility.UrlEncode(c.ToString(), Encoding.UTF8).ToUpper());
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }
        /// <summary>
        /// Hmacsha256 encryption
        /// </summary>
        /// <param name="text"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        private static string CalculateSignature256(string text, string secretKey)
        {
            using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                return Convert.ToBase64String(hashmessage);
            }
        }
        /// <summary>
        /// Sign URL request parametes 
        /// </summary>
        /// <param name="method">request method</param>
        /// <param name="host">API url domain name</param>
        /// <param name="resourcePath">url address</param>
        /// <param name="parameters">request parameters</param>
        /// <returns></returns>
        private string GetSignatureStr(Method method, string host, string resourcePath, string parameters)
        {
            var sign = string.Empty;
            StringBuilder sb = new StringBuilder();
            sb.Append(method.ToString().ToUpper()).Append("\n")
                .Append(host).Append("\n")
                .Append(resourcePath).Append("\n");
            //parameters ordering
            var paraArray = parameters.Split('&');
            List<string> parametersList = new List<string>();
            foreach (var item in paraArray)
            {
                parametersList.Add(item);
            }
            parametersList.Sort(delegate(string s1, string s2) { return string.CompareOrdinal(s1, s2); });
            foreach (var item in parametersList)
            {
                sb.Append(item).Append("&");
            }
            sign = sb.ToString().TrimEnd('&');
            //calculate the parameters with the defined encryption method
            sign = CalculateSignature256(sign, SECRET_KEY);

            return sign;
        }

        /// <summary>
        /// Sign with ECDsa encryption method with the generated ECDsa private key
        /// </summary>
        /// <param name="privateKeyStr"></param>
        /// <param name="signData"></param>
        /// <returns></returns>
        private String GetPrivateSignatureStr(string privateKeyStr, string signData)
        {
            var privateSignedData = string.Empty;
            try
            {
                byte[] keyBytes = Convert.FromBase64String(privateKeyStr);
                CngKey cng = CngKey.Import(keyBytes, CngKeyBlobFormat.Pkcs8PrivateBlob);

                ECDsaCng dsa = new ECDsaCng(cng)
                {
                    HashAlgorithm = CngAlgorithm.Sha256
                };

                byte[] signDataBytes = Encoding.UTF8.GetBytes(signData);
                privateSignedData = Convert.ToBase64String(dsa.SignData(signDataBytes));
            }
            catch(CryptographicException e)
            {
                Console.WriteLine("Private signature error because: " + e.Message);
            }
          
            return privateSignedData;
        }
        #endregion



    }
}
