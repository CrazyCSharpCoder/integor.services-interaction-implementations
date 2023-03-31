using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using IntegorServicesInteraction;
using IntegorResponseDecoration.Parsing;

namespace IntegorServicesInteractionHelpers
{
	public partial class JsonServicesRequestProcessor<TServiceConfiguration>
		where TServiceConfiguration : ServiceConfiguration
	{
		public async Task<ServiceResponse<TResult>> ProcessGetAsync<TBody, TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			TBody dtoBody, Dictionary<string, string>? cookie = null)
			where TResult : class
		{
			return await ProcessAsync(parser, HttpMethod.Get, dtoBody, cookie);
		}

		public async Task<ServiceResponse<TResult>> ProcessPostAsync<TBody, TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			TBody dtoBody, Dictionary<string, string>? cookie = null)
			where TResult : class
		{
			return await ProcessAsync(parser, HttpMethod.Post, dtoBody, cookie);
		}

		public async Task<ServiceResponse<TResult>> ProcessPutAsync<TBody, TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			TBody dtoBody, Dictionary<string, string>? cookie = null)
			where TResult : class
		{
			return await ProcessAsync(parser, HttpMethod.Put, dtoBody, cookie);
		}

		public async Task<ServiceResponse<TResult>> ProcessDeleteAsync<TBody, TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			TBody dtoBody, Dictionary<string, string>? cookie = null)
			where TResult : class
		{
			return await ProcessAsync(parser, HttpMethod.Delete, dtoBody, cookie);
		}
	}
}
