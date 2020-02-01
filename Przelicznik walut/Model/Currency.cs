using Newtonsoft.Json;

namespace App2.Model
{
	public class Currency
	{
		public Currency(string name, string code, double rate)
		{
			this.name = name;
			this.code = code;
			this.rate = rate;
		}
			

		[JsonProperty(PropertyName = "currency")]
		public string name { get; set; }

		[JsonProperty(PropertyName = "code")]
		public string code { get; set; }

		[JsonProperty(PropertyName = "mid")]
		public double rate { get; set; }

		public override string ToString()
		{
			return code + ' ' + name;
		}
	}
}