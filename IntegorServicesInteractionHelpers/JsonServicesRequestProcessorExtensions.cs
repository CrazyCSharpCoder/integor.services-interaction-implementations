using IntegorResponseDecoration.Parsing;
using IntegorServicesInteraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IntegorServicesInteractionHelpers
{
	public static class JsonServicesRequestProcessorExtensions
	{
		public static async Task<ServiceResponse<TResult>> GetAsync<TServiceConfiguration, TBody, TResult>(
			this JsonServicesRequestProcessor<TServiceConfiguration> requestProcessor,
			IDecoratedObjectParser<TResult, JsonElement> parser,
			string localPath = "", TBody? dtoBody = null,
			AuthenticationMethods authMethod = AuthenticationMethods.None, string? authenticationToken = null)
			where TServiceConfiguration : ServiceConfiguration
			where TResult : class
			where TBody : class
		{
			return await requestProcessor.ProcessAsync(parser, HttpMethod.Get, localPath, dtoBody, authMethod, authenticationToken);
		}

		public static async Task<ServiceResponse<TResult>> PostAsync<TServiceConfiguration, TBody, TResult>(
			this JsonServicesRequestProcessor<TServiceConfiguration> requestProcessor,
			IDecoratedObjectParser<TResult, JsonElement> parser,
			string localPath = "", TBody? dtoBody = null,
			AuthenticationMethods authMethod = AuthenticationMethods.None, string? authenticationToken = null)
			where TServiceConfiguration : ServiceConfiguration
			where TResult : class
			where TBody : class
		{
			return await requestProcessor.ProcessAsync(parser, HttpMethod.Post, localPath, dtoBody, authMethod, authenticationToken);
		}

		public static async Task<ServiceResponse<TResult>> PutAsync<TServiceConfiguration, TBody, TResult>(
			this JsonServicesRequestProcessor<TServiceConfiguration> requestProcessor,
			IDecoratedObjectParser<TResult, JsonElement> parser,
			string localPath = "", TBody? dtoBody = null,
			AuthenticationMethods authMethod = AuthenticationMethods.None, string? authenticationToken = null)
			where TServiceConfiguration : ServiceConfiguration
			where TResult : class
			where TBody : class
		{
			return await requestProcessor.ProcessAsync(parser, HttpMethod.Put, localPath, dtoBody, authMethod, authenticationToken);
		}

		public static async Task<ServiceResponse<TResult>> DeleteAsync<TServiceConfiguration, TBody, TResult>(
			this JsonServicesRequestProcessor<TServiceConfiguration> requestProcessor,
			IDecoratedObjectParser<TResult, JsonElement> parser,
			string localPath = "", TBody? dtoBody = null,
			AuthenticationMethods authMethod = AuthenticationMethods.None, string? authenticationToken = null)
			where TServiceConfiguration : ServiceConfiguration
			where TResult : class
			where TBody : class
		{
			return await requestProcessor.ProcessAsync(parser, HttpMethod.Delete, localPath, dtoBody, authMethod, authenticationToken);
		}

		// For no body
		public static async Task<ServiceResponse<TResult>> GetAsync<TServiceConfiguration, TResult>(
			this JsonServicesRequestProcessor<TServiceConfiguration> requestProcessor,
			IDecoratedObjectParser<TResult, JsonElement> parser, string localPath = "",
			AuthenticationMethods authMethod = AuthenticationMethods.None, string? authenticationToken = null)
			where TServiceConfiguration : ServiceConfiguration
			where TResult : class
		{
			return await requestProcessor.ProcessAsync(parser, HttpMethod.Get, localPath, authMethod, authenticationToken);
		}

		public static async Task<ServiceResponse<TResult>> PostAsync<TServiceConfiguration, TResult>(
			this JsonServicesRequestProcessor<TServiceConfiguration> requestProcessor,
			IDecoratedObjectParser<TResult, JsonElement> parser, string localPath = "",
			AuthenticationMethods authMethod = AuthenticationMethods.None, string? authenticationToken = null)
			where TServiceConfiguration : ServiceConfiguration
			where TResult : class
		{
			return await requestProcessor.ProcessAsync(parser, HttpMethod.Post, localPath, authMethod, authenticationToken);
		}

		public static async Task<ServiceResponse<TResult>> PutAsync<TServiceConfiguration, TResult>(
			this JsonServicesRequestProcessor<TServiceConfiguration> requestProcessor,
			IDecoratedObjectParser<TResult, JsonElement> parser, string localPath = "",
			AuthenticationMethods authMethod = AuthenticationMethods.None, string? authenticationToken = null)
			where TServiceConfiguration : ServiceConfiguration
			where TResult : class
		{
			return await requestProcessor.ProcessAsync(parser, HttpMethod.Put, localPath, authMethod, authenticationToken);
		}

		public static async Task<ServiceResponse<TResult>> DeleteAsync<TServiceConfiguration, TResult>(
			this JsonServicesRequestProcessor<TServiceConfiguration> requestProcessor,
			IDecoratedObjectParser<TResult, JsonElement> parser, string localPath = "",
			AuthenticationMethods authMethod = AuthenticationMethods.None, string? authenticationToken = null)
			where TServiceConfiguration : ServiceConfiguration
			where TResult : class
		{
			return await requestProcessor.ProcessAsync(parser, HttpMethod.Delete, localPath, authMethod, authenticationToken);
		}
	}
}
