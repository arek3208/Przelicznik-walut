using System.Collections.Generic;
using System.Threading.Tasks;
using App2.Model;
using Refit;

namespace App2.Interface
{
	interface INbpApi
	{
		[Get("/api/exchangerates/tables/A?format=JSON")]
		Task<List<ResponseItem>> GetRates();
	}
}