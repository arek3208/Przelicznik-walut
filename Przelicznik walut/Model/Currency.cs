using Newtonsoft.Json;

namespace App2.Model
{
	public class Currency
	{
		public Currency(string name, string code, double rate)
		{
			Name = name;
			Code = code;
			Rate = rate;
		}
			

		[JsonProperty(PropertyName = "currency")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "code")]
		public string Code { get; set; }

		[JsonProperty(PropertyName = "mid")]
		public double Rate { get; set; }

		public override string ToString()
		{
			return Code + ' ' + Name;
		}
	}
}