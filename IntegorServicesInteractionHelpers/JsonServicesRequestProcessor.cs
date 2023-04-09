using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Json;

using IntegorErrorsHandling;
using IntegorResponseDecoration.Parsing;

using IntegorServicesInteraction;
using IntegorServicesInteraction.Exceptions;

using ExtensibleRefreshJwtAuthentication.TokenServices;

using ExtensibleRefreshJwtAuthentication.Access.Tokens;
using ExtensibleRefreshJwtAuthentication.Refresh.Tokens;

namespace IntegorServicesInteractionHelpers
{
	public class JsonServicesRequestProcessor<TServiceConfiguration>
		where TServiceConfiguration : ServiceConfiguration
	{
		private TServiceConfiguration _configuration;

		private ISendRequestAccessTokenAccessor _accessTokenAccessor;
		private ISendRequestRefreshTokenAccessor _refreshTokenAccessor;

		private IDecoratedObjectParser<IEnumerable<IResponseError>, JsonElement> _errorsParser;

		private string _urlPrefix;

		public JsonServicesRequestProcessor(
			TServiceConfiguration configuration,

			ISendRequestAccessTokenAccessor accessTokenAccessor,
			ISendRequestRefreshTokenAccessor refreshTokenAccessor,

			IDecoratedObjectParser<IEnumerable<IResponseError>, JsonElement> errorsParser,
			string localUrlPrefix = "")
        {
			_configuration = configuration;

			_accessTokenAccessor = accessTokenAccessor;
			_refreshTokenAccessor = refreshTokenAccessor;

			_errorsParser = errorsParser;

			_urlPrefix = localUrlPrefix;
		}

		public async Task<ServiceResponse<TResult>> ProcessAsync<TBody, TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			HttpMethod method, string localPath = "", TBody? dtoBody = null,
			AuthenticationMethods authMethod = AuthenticationMethods.None, string? authenticationToken = null)
			where TBody : class
			where TResult : class
		{
			if (authMethod != AuthenticationMethods.None && authenticationToken == null)
				throw new ArgumentException($"Cannot proceed authentications as {authenticationToken} equals null");

			using HttpContent content = JsonContent.Create(dtoBody);
			NetHttpRequestContext context = CreateNetHttpRequestContext(_configuration.Url, content, authMethod, authenticationToken);

			using HttpClient httpClient = context.HttpClient;
			using HttpClientHandler? handler = context.HttpClientHandler;

			HttpRequestMessage request = new HttpRequestMessage(method, Path.Combine(_urlPrefix, localPath))
			{
				Content = content
			};

			HttpResponseMessage? response = null;

			try
			{
				response = await httpClient.SendAsync(request);
			}
			catch (Exception exc)
			{
				throw new ServiceConnectionFailedException(exc);
			}

			using Stream responseStream = await response.Content.ReadAsStreamAsync();

			JsonSerializerOptions jsonOptions = new JsonSerializerOptions()
			{
				PropertyNameCaseInsensitive = true
			};

			ServiceResponse<TResult> serviceResponse = await ParseResponseAsync(
				response, jsonOptions, parser,
				_accessTokenAccessor, _refreshTokenAccessor);
			response.Dispose();

			return serviceResponse;
		}

		private NetHttpRequestContext? AuthenticateNetHttpRequestContext(string baseUrl, HttpContent content, AuthenticationMethods authMethod, string? authToken)
		{
			return authMethod switch
			{
				AuthenticationMethods.Access => _accessTokenAccessor.AttachToRequest(baseUrl, content, authToken!),
				AuthenticationMethods.Refresh => _refreshTokenAccessor.AttachToRequest(baseUrl, content, authToken!),
				_ => null
			};
		}

		private NetHttpRequestContext CreateNetHttpRequestContext(
			string baseUrl, HttpContent content,
			AuthenticationMethods authMethod, string? authToken = null)
		{
			NetHttpRequestContext? context = AuthenticateNetHttpRequestContext(baseUrl, content, authMethod, authToken);

			if (context != null)
				return context;

			HttpClient httpClient = new HttpClient() { BaseAddress = new Uri(baseUrl) };
			return new NetHttpRequestContext(httpClient, null);
		}

		private async Task<ServiceResponse<TResult>> ParseResponseAsync<TResult>(
			HttpResponseMessage response, JsonSerializerOptions jsonOptions,
			IDecoratedObjectParser<TResult, JsonElement> parser,
			ISendRequestAccessTokenAccessor accessTokenAccessor,
			ISendRequestRefreshTokenAccessor refreshTokenAccessor)
			where TResult : class
		{
			int statusCode = (int)response.StatusCode;
			using Stream responseStream = await response.Content.ReadAsStreamAsync();

			JsonElement body = await JsonSerializer.DeserializeAsync<JsonElement>(responseStream, jsonOptions);

			string? accessToken = accessTokenAccessor.GetFromResponse(response);
			string? refreshToken = refreshTokenAccessor.GetFromResponse(response);

			UserAuthentication? authentication = new UserAuthentication(accessToken, refreshToken);

			var parsingResult = parser.ParseDecorated(body);

			if (parsingResult.Success)
				return new ServiceResponse<TResult>(statusCode, authentication, parsingResult.Value);

			var errorDecorationResult = _errorsParser.ParseDecorated(body);
			IEnumerable<IResponseError> errors = errorDecorationResult.Value!;

			return new ServiceResponse<TResult>((int)response.StatusCode, authentication, errors);
		}

		public async Task<ServiceResponse<TResult>> ProcessAsync<TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			HttpMethod method, string localPath = "",
			AuthenticationMethods authMethod = AuthenticationMethods.None, string? authenticationToken = null)
			where TResult : class
		{
			return await ProcessAsync<object, TResult>(parser, method, localPath, null, authMethod, authenticationToken);
		}
	}
}
