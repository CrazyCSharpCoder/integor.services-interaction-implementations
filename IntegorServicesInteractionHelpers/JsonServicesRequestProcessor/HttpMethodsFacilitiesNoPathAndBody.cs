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
	public abstract partial class JsonServicesRequestProcessor<TServiceConfiguration>
		where TServiceConfiguration : ServiceConfiguration
	{
		protected async Task<ServiceResponse<TResult>> ProcessGetAsync<TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			Dictionary<string, string>? cookie = null)
			where TResult : class
		{
			return await ProcessAsync(parser, HttpMethod.Get, cookie);
		}

		protected async Task<ServiceResponse<TResult>> ProcessPostAsync<TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			Dictionary<string, string>? cookie = null)
			where TResult : class
		{
			return await ProcessAsync(parser, HttpMethod.Post, cookie);
		}

		protected async Task<ServiceResponse<TResult>> ProcessPutAsync<TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			Dictionary<string, string>? cookie = null)
			where TResult : class
		{
			return await ProcessAsync(parser, HttpMethod.Put, cookie);
		}

		protected async Task<ServiceResponse<TResult>> ProcessDeleteAsync<TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			Dictionary<string, string>? cookie = null)
			where TResult : class
		{
			return await ProcessAsync(parser, HttpMethod.Delete, cookie);
		}
	}
}
