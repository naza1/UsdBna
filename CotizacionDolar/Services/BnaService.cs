using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;

namespace UsdQuotation.Services
{
    public class BnaService : IBnaService
    {
        private readonly HttpClient _httpClient;
        private readonly BnaSettings _bnaSettings;

        public BnaService(IHttpClientFactory httpClientFactory,
            HttpClientPoliciesSettings bnaClientPoliciesSettings,
            BnaSettings bnaSettings)
        {
            _httpClient = httpClientFactory.CreateClient(bnaClientPoliciesSettings.ClientName);
            _bnaSettings = bnaSettings;
        }

        public async Task<Usd> GetUsdToday()
        {
            // Construct URL
            var dateUrl = System.Web.HttpUtility.UrlEncode($"{DateTime.Now:dd/MM/yyyy}");
            var uri = new Uri(_bnaSettings.EndPoint + "&fecha=" + dateUrl);

            // Create HTTP transport objects
            var httpRequest = new HttpRequestMessage
            {
                RequestUri = uri, 
                Method = new HttpMethod("GET")
            };

            var httpResponse = await _httpClient.SendAsync(httpRequest).ConfigureAwait(false);

            var htmlPage = await httpResponse.Content.ReadAsStringAsync();

            return GetDataFromHtml(htmlPage);
        }

        private Usd GetDataFromHtml(string htmlPage)
        {
            var parser = new HtmlParser();
            var document = parser.ParseDocument(htmlPage);

            if (!document.GetElementsByTagName("tr").ElementAt(1).GetElementsByTagName("td").ElementAt(0).InnerHtml.Equals(_bnaSettings.Validation))
                return null;

            var usdToday = document.GetElementsByTagName("tr").LastOrDefault();

            if (usdToday == null) 
                return null;

            var buy = usdToday.GetElementsByTagName("td").ElementAt(1).InnerHtml;
            var sale = usdToday.GetElementsByTagName("td").ElementAt(2).InnerHtml;

            return new Usd
            {
                Date = DateTime.Parse(usdToday.GetElementsByTagName("td").ElementAt(3).InnerHtml),
                SaleValue = sale,
                BuyValue = buy
            };
        }
    }
}
