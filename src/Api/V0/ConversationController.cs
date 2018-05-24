using dnctest.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dnctest.Api.V0
{
	[Produces("application/json")]
	[Route("api/v0/[controller]/[action]")]
	public class ConversationController : Controller
	{
		[HttpPost]
		public async Task<ConversationResponse> Customer([FromBody]MessagePayload payload)
		{

			var sentiment = await AzureLanguage.ProcessSentiment(payload.messages.Select( x => x.message).ToList());
			var entities = await AzureLanguage.ProcessLinkedEntities(payload.messages.Select(x => x.message).ToList());
			var language = await AzureLanguage.ProcessLanguage(payload.messages.Select(x => x.message).ToList());
			//var energy = await AzureLanguage.ProcessEnergy();

			var response = new ConversationResponse();
			response.sentiment = sentiment;
			response.entities = entities;
			response.language = language;
			//response.energy = energy;

			return response;
		}
		[HttpPost]
		public async Task<ConversationResponse> Representative([FromBody]MessagePayload payload)
		{

			//var sentiment = await AzureLanguage.ProcessSentiment(payload.messages.Select(x => x.message).ToList());
			var entities = await AzureLanguage.ProcessLinkedEntities(payload.messages.Select(x => x.message).ToList());
			//var language = await AzureLanguage.ProcessLanguage(payload.messages.Select(x => x.message).ToList());
			//var energy = await AzureLanguage.ProcessEnergy();

			var response = new ConversationResponse();
			//response.sentiment = sentiment;
			response.entities = entities;
			//response.language = language;
			//response.energy = energy;

			return response;
		}

		[HttpPost]
		public async Task<ConversationResponse> Energy()
		{
			var energy = await AzureLanguage.ProcessEnergy();

			var response = new ConversationResponse();
			//response.sentiment = sentiment;
			//response.entities = entities;

			double[] typical = new double[336];
			double[] counts = new double[336];

			energy = energy.OrderBy(y => y.Timestamp).ToList();

			int i = 0;
			foreach(var e in energy.Take(energy.Count() - 336).Select(y => y.PrimaryValue))
			{
				typical[i] += e;
				counts[i]++;
				i++;
				if(i > 335)
				{
					i = 0;
				}
			}

			for(int j = 0; j < 336; j++)
			{
				typical[j] = typical[j] / (counts[j]);
			}

		
			response.energyCurrent = energy.Skip(energy.Count - 336).Take(336).Select(x => x.PrimaryValue).ToList();
			response.energyTypical = energy.Take(336).Select(x => x.PrimaryValue).ToList();
			response.energyTypical = typical.ToList();
			return response;
		}
	
		public class MessagePayload
		{
			public List<MessageObject> messages = new List<MessageObject>();
		}
		public class MessageObject
		{
			public String type;
			public String message;
		}
		public class ConversationResponse
		{
			public double sentiment;
			public List<ResponseEntities> entities;
			public List<double> energyCurrent;
			public List<double> energyTypical;
			public String language;
		}
	}
}