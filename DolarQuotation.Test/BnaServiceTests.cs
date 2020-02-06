using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using UsdQuotation.Services;

namespace UsdQuotation.Test
{
    public class BnaServiceTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task GetUsdToday_ReturnUsdQuotationOfBna_WhenUrlReturnOk()
        {
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(@"<div id='cotizacionesCercanas'>
                    <table class='table table-bordered cotizador' style='float:none; width:100%; text-align: center;'>
                    <thead>
                    <tr>
                    <th>Monedas</th>
                    <th>Compra</th>
                    <th>Venta</th>
                    <th>Fecha</th>
                    </tr>
                    </thead>
                    <tbody>
                    <tr>
                    <td>Dolar U.S.A</td>
                    <td class='dest'>58,0000</td>
                    <td class='dest'>63,0000</td>
                    <td>4/2/2020</td>
                    </tr>
                    <tr>
                    <td>Dolar U.S.A</td>
                    <td class='dest'>58,0000</td>
                    <td class='dest'>63,0000</td>
                    <td>5/2/2020</td>
                    </tr>
                    </tbody>
                    </table>
                    </div>"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var service = new BnaService(httpClientFactoryMock.Object,
                new HttpClientPoliciesSettings
                {
                    ClientName = "test"
                }, 
                new BnaSettings
                {
                    EndPoint = "https://bna.com.ar/Cotizador/HistoricoPrincipales?id=billetes"
                });

            var result = await service.GetUsdToday();

            Assert.AreEqual(result.Date.ToString("d"), "5/2/2020");
        }
    }
}