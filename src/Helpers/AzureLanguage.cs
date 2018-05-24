using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace dnctest.Helpers
{
	public static class AzureLanguage
	{
		private const string azureApiKey = "XXXX";
		private const string gilmondPassword = "XX";
		private const string gilmondUsername = "XX";
		public static async Task<double> ProcessSentiment(List<string> messages)
		{
			var client = new HttpClient();
			client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", azureApiKey);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var payload = new RequestModelSentiment();

			int i = 0;
			foreach (var m in messages)
			{
				payload.documents.Add(new RequestDocument
				{
					id = i,
					text = m
				});
				i++;
			}


			var response = await client.PostAsync("https://westeurope.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment", new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(payload), System.Text.Encoding.UTF8, "application/json"));
			var text = await response.Content.ReadAsStringAsync();
			var decoded = JsonConvert.DeserializeObject<ResponseModelSentiment>(text);

			if (decoded.statusCode != "200" && decoded.statusCode != null)
			{
				throw new Exception("failed request " + decoded.statusCode);
			}

			double average = 0;
			foreach (var doc in decoded.documents)
			{
				average += doc.score;
			}
			average = average / decoded.documents.Count();

			return average - 0.5;
		}
		public static async Task<String> ProcessLanguage(List<string> messages)
		{
			return "EN";
			var client = new HttpClient();
			client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", azureApiKey);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var payload = new RequestModelSentiment();

			int i = 0;
			var doc = new RequestDocument
			{
				id = i,
				text = ""
			};
			foreach (var m in messages)
			{
				doc.text = doc.text + " " + m;
			}

			payload.documents.Add(doc);


			var response = await client.PostAsync("https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/languages", new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(payload), System.Text.Encoding.UTF8, "application/json"));
			var text = await response.Content.ReadAsStringAsync();
			var decoded = JsonConvert.DeserializeObject<ResponseModelLanguage>(text);

			if (decoded.statusCode != "200" && decoded.statusCode != null)
			{
				throw new Exception("failed request " + decoded.statusCode);
			}

			throw new NotImplementedException();
		}

		public static async Task<List<ResponseEntities>> ProcessLinkedEntities(List<string> messages)
		{
			var client = new HttpClient();
			client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", azureApiKey);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var payload = new RequestModelSentiment();

			int i = 0;
			foreach (var m in messages)
			{
				payload.documents.Add(new RequestDocument
				{
					id = i,
					language = "en",
					text = m
				});
				i++;
			}

			var response = await client.PostAsync("https://westeurope.api.cognitive.microsoft.com/text/analytics/v2.0/entities", new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(payload), System.Text.Encoding.UTF8, "application/json"));
			var text = await response.Content.ReadAsStringAsync();
			var decoded = JsonConvert.DeserializeObject<ResponseModelSentiment>(text);

			if (decoded.statusCode != "200" && decoded.statusCode != null)
			{
				throw new Exception("failed request " + decoded.statusCode);
			}

			var proc = decoded.documents.Select(x => x.entities).SelectMany(y => y).ToList();
			return proc;
		}

		public static async Task<List<ImportDataModel>> ProcessEnergy()
		{

			var client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var byteArray = Encoding.ASCII.GetBytes(gilmondUsername + ":" + gilmondPassword);
			client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

			var payload = new RequestModelEnergy();
			payload.deviceId = "9B-E4-B8-8D-7D-48-48-D9";
			payload.start = "2018-01-01T18:19:26.202Z";
			payload.end = "2018-01-31T18:19:26.202Z";
			payload.deviceType = "ESME";

			var response = await client.PostAsync("https://smartdccdemo.gilmond.com/dcc/readactivepowerimport", new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(payload), System.Text.Encoding.UTF8, "application/json"));
			if (response.StatusCode != System.Net.HttpStatusCode.OK)
			{
				throw new Exception("failed request " + response.StatusCode);
			}
			var correlationHeader = response.Headers.GetValues("x-correlation-id").FirstOrDefault();

			var response2 = await client.GetAsync("https://smartdccdemo.gilmond.com/receiveresponse?correlationId=" + correlationHeader);
			if (response2.StatusCode != System.Net.HttpStatusCode.OK)
			{
				throw new Exception("failed request " + response2.StatusCode);
			}

			var text = await response2.Content.ReadAsStringAsync();
			var textst = text.Substring(1, text.Length - 2).Replace(@"\n", "").Replace(@"\", "");
			var decoded = JsonConvert.DeserializeObject<ResponseModelEnergy>(textst);

			foreach(var item in decoded.ActiveImportData)
			{
				item.TimestampDT = DateTime.Parse(item.Timestamp, null, System.Globalization.DateTimeStyles.RoundtripKind);
			}

			return decoded.ActiveImportData;

		}
	}
	public class ImportDataModel
	{
		public double PrimaryValue;
		public double SecondaryValue;
		public string Timestamp;
		public DateTime TimestampDT;
	}
	public class ResponseModelEnergy
	{
		public List<ImportDataModel> ActiveImportData = new List<ImportDataModel>();
	}
	public class RequestModelEnergy
	{
		public string start;
		public string end;
		public string deviceId;
		public string deviceType;
	}
	public class ConversationResponse
	{
		public String message;
	}
	public class ResponseDocument
	{
		public float score;
		public string id;
		public List<ResponseEntities> entities = new List<ResponseEntities>();
	}
	public class ResponseEntities
	{
		public string name;
		public List<ResponseMatches> matches;
		public string wikipediaLanguage;
		public string wikipediaId;
		public string wikipediaUrl;
		public string bingId;
	}

	public class ResponseMatches
	{
		public string text;
		public string offset;
		public string length;

	}
	public class RequestDocument
	{
		public int id;
		public string text;
		public string language;
		public List<DetectedLanguage> detectedLanguages = new List<DetectedLanguage>();
	}

	public class DetectedLanguage
	{
		public String name;
		public String iso6391Name;
		public String score;
	}

	public class RequestModelSentiment
	{
		public List<RequestDocument> documents = new List<RequestDocument>();

	}

	public class ResponseModelLanguage
	{
		[DataMember(IsRequired = false)]
		public string statusCode;
		[DataMember(IsRequired = false)]
		public List<RequestDocument> documents = new List<RequestDocument>();
		[DataMember(IsRequired = false)]
		public List<string> errors;
	}

	public class ResponseModelSentiment
	{
		[DataMember(IsRequired = false)]
		public string statusCode;
		[DataMember(IsRequired = false)]
		public List<ResponseDocument> documents = new List<ResponseDocument>();
		[DataMember(IsRequired = false)]
		public List<string> errors;
	}
}
