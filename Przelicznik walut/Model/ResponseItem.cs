using System.Collections.Generic;
using Newtonsoft.Json;

namespace App2.Model
{
	public class ResponseItem
	{
		[JsonProperty(PropertyName = "rates")]
		public List<Currency> Rates { get; set; }
	}
}