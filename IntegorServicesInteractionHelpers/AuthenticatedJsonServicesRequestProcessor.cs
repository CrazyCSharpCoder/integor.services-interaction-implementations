using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Json;

using Microsoft.Extensions.Options;

using IntegorErrorsHandling;
using IntegorResponseDecoration.Parsing;

using IntegorServicesInteraction;

using ExtensibleRefreshJwtAuthentication.TokenServices;

using ExtensibleRefreshJwtAuthentication.Access;
using ExtensibleRefreshJwtAuthentication.Refresh;

namespace IntegorServicesInteractionHelpers
{
	using Internal;

	public class AuthenticatedJsonServicesRequestProcessor<TServiceConfiguration>
		where TServiceConfiguration : ServiceConfiguration, new()
	{
		private ISendRequestAccessTokenAccessor _accessTokenAccessor;
		private ISendRequestRefreshTokenAccessor _refreshTokenAccessor;

		private IDecoratedObjectParser<IEnumerable<IResponseError>, JsonElement> _errorsParser;
		private TServiceConfiguration _configuration;

		private string _urlPrefix;

		private delegate NetHttpRequestContext HttpRequestContextFactoryDelegate(HttpContent content, string baseAddress);

		public AuthenticatedJsonServicesRequestProcessor(
			ISendRequestAccessTokenAccessor accessTokenAccessor,
			ISendRequestRefreshTokenAccessor refreshTokenAccessor,
			
			IDecoratedObjectParser<IEnumerable<IResponseError>, JsonElement> errorsParser,
			IOptions<TServiceConfiguration> serviceOptions,

			string localUrlPrefix = "")
		{
			_accessTokenAccessor = accessTokenAccessor;
			_refreshTokenAccessor = refreshTokenAccessor;

			_errorsParser = errorsParser;
			_configuration = serviceOptions.Value;

			_urlPrefix = localUrlPrefix;
		}

		public async Task<ServiceResponse<TResult>> ProcessAsync<TBody, TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			HttpMethod method, string localPath = "", Dictionary<string, string>? queryParameters = null,
			TBody? dtoBody = null,
			UserAuthentication? authentication = null)
			where TBody : class
			where TResult : class
		{
			IEnumerable<HttpRequestContextFactoryDelegate> requestContextFactories =
				CreateHttpRequestContextFactories(authentication);

			string url = NetHttpRequestsHelpers.ComposeLocalUrl(queryParameters, _urlPrefix, localPath);

			HttpResponseMessage response = null!;

			foreach (HttpRequestContextFactoryDelegate reqContextFactory in requestContextFactories)
			{
				using HttpContent content = JsonContent.Create(dtoBody);

				NetHttpRequestContext requestContext = reqContextFactory.Invoke(content, _configuration.Url);

				using HttpClientHandler? clientHandler = requestContext.HttpClientHandler;
				using HttpClient httpClient = requestContext.HttpClient;

				using HttpRequestMessage request = new HttpRequestMessage(method, url)
				{
					Content = content
				};

				response = await NetHttpRequestsHelpers.SendAsync(httpClient, request);

				if (response.IsSuccessStatusCode)
					break;
			}

			JsonSerializerOptions jsonOptions = new JsonSerializerOptions()
			{
				PropertyNameCaseInsensitive = true
			};

			ServiceResponse<TResult> serviceResponse =
				await NetHttpRequestsHelpers.ParseJsonResponseAsync(
					response, jsonOptions,
					_errorsParser, parser,
					_accessTokenAccessor, _refreshTokenAccessor);

			response.Dispose();

			return serviceResponse;
		}

		public async Task<ServiceResponse<TResult>> ProcessAsync<TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			HttpMethod method, string localPath = "", Dictionary<string, string>? queryParameters = null,
			UserAuthentication? authentication = null)
			where TResult : class
		{
			return await ProcessAsync<object, TResult>(parser, method, localPath, queryParameters, null, authentication);
		}

		private IEnumerable<HttpRequestContextFactoryDelegate> CreateHttpRequestContextFactories(UserAuthentication? authentication)
		{
			if (authentication == null || authentication.AccessToken == null && authentication.RefreshToken == null)
				return new HttpRequestContextFactoryDelegate[]
				{
					(content, baseAddress) => new NetHttpRequestContext(new HttpClient() { BaseAddress = new Uri(baseAddress) }, null)
				};

			List<HttpRequestContextFactoryDelegate> factories = new List<HttpRequestContextFactoryDelegate>();

			if (authentication.AccessToken != null)
				factories.Add((content, baseAddress) => _accessTokenAccessor.AttachToRequest(baseAddress, content, authentication.AccessToken));

			if (authentication.RefreshToken != null)
				factories.Add((content, baseAddress) => _refreshTokenAccessor.AttachToRequest(baseAddress, content, authentication.RefreshToken));

			return factories;
		}
    }
}
