using Newtonsoft.Json;

namespace App2.Model
{
	public class Currency
	{
		[JsonProperty(PropertyName = "currency")]
		public string name { get; set; }

		[JsonProperty(PropertyName = "code")]
		public string code { get; set; }

		[JsonProperty(PropertyName = "mid")]
		public double rate { get; set; }
	}
}