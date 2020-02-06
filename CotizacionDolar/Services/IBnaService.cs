using System.Threading.Tasks;

namespace UsdQuotation.Services
{
    public interface IBnaService
    {
        public Task<Usd> GetUsdToday();
    }
}