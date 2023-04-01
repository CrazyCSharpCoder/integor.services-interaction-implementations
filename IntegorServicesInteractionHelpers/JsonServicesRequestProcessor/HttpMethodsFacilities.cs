using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;

using IntegorServicesInteraction;
using IntegorResponseDecoration.Parsing;

namespace IntegorServicesInteractionHelpers
{
	public partial class JsonServicesRequestProcessor<TServiceConfiguration>
		where TServiceConfiguration : ServiceConfiguration
	{
		public async Task<ServiceResponse<TResult>> ProcessGetAsync<TBody, TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			string localPath = "", TBody? dtoBody = null,
			AuthenticationMethods authMethod = AuthenticationMethods.None, string? authenticationToken = null)
			where TResult : class
			where TBody : class
		{
			return await ProcessAsync(parser, HttpMethod.Get, localPath, dtoBody, authMethod, authenticationToken);
		}

		public async Task<ServiceResponse<TResult>> ProcessPostAsync<TBody, TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			string localPath = "", TBody? dtoBody = null,
			AuthenticationMethods authMethod = AuthenticationMethods.None, string? authenticationToken = null)
			where TResult : class
			where TBody : class
		{
			return await ProcessAsync(parser, HttpMethod.Post, localPath, dtoBody, authMethod, authenticationToken);
		}

		public async Task<ServiceResponse<TResult>> ProcessPutAsync<TBody, TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			string localPath = "", TBody? dtoBody = null,
			AuthenticationMethods authMethod = AuthenticationMethods.None, string? authenticationToken = null)
			where TResult : class
			where TBody : class
		{
			return await ProcessAsync(parser, HttpMethod.Put, localPath, dtoBody, authMethod, authenticationToken);
		}

		public async Task<ServiceResponse<TResult>> ProcessDeleteAsync<TBody, TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			string localPath = "", TBody? dtoBody = null,
			AuthenticationMethods authMethod = AuthenticationMethods.None, string? authenticationToken = null)
			where TResult : class
			where TBody: class
		{
			return await ProcessAsync(parser, HttpMethod.Delete, localPath, dtoBody, authMethod, authenticationToken);
		}
	}
}
