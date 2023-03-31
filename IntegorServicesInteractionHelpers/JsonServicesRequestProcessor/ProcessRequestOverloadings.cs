using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Text.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;

using IntegorErrorsHandling;
using IntegorResponseDecoration.Parsing;

using IntegorServicesInteraction;

namespace IntegorServicesInteractionHelpers
{
	public partial class JsonServicesRequestProcessor<TServiceConfiguration>
		where TServiceConfiguration : ServiceConfiguration
	{
		private const string _setCookieHeader = "Set-Cookie";

		private TServiceConfiguration _configuration;
		private IDecoratedObjectParser<IEnumerable<IResponseError>, JsonElement> _errorsParser;

		private string _urlPrefix;

		public JsonServicesRequestProcessor(
			TServiceConfiguration configuration,
			IDecoratedObjectParser<IEnumerable<IResponseError>, JsonElement> errorsParser,
			string localUrlPrefix = "")
        {
			_configuration = configuration;
			_errorsParser = errorsParser;

			_urlPrefix = localUrlPrefix;
		}

		public async Task<ServiceResponse<TResult>> ProcessAsync<TBody, TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			string localPath, HttpMethod method, TBody dtoBody, Dictionary<string, string>? cookie = null)
			where TResult : class
		{
			HttpContent content = JsonContent.Create(dtoBody);

			HttpRequestMessage request = new HttpRequestMessage(method, Path.Combine(_urlPrefix, localPath))
			{
				Content = content
			};

			CookieContainer? cookieContainer = cookie != null ? CreateCookieContainer(cookie) : null;

			using HttpClientHandler handler = CreateHttpClientHandler(cookieContainer);
			using HttpClient httpClient = new HttpClient(handler) { BaseAddress = new Uri(_configuration.GetFullApiPath()) };

			using HttpResponseMessage response = await httpClient.SendAsync(request);
			using Stream responseStream = await response.Content.ReadAsStreamAsync();

			JsonSerializerOptions jsonOptions = new JsonSerializerOptions()
			{
				PropertyNameCaseInsensitive = true
			};

			int statusCode = (int)response.StatusCode;
			JsonElement body = await JsonSerializer.DeserializeAsync<JsonElement>(responseStream, jsonOptions);

			IEnumerable<string>? setCookieHeaders = response.Headers.Contains(_setCookieHeader) ? response.Headers.GetValues(_setCookieHeader) : null;

			var parsingResult = parser.ParseDecorated(body);

			if (parsingResult.Success)
				return new ServiceResponse<TResult>(statusCode, parsingResult.Value, setCookieHeaders);

			var errorDecorationResult = _errorsParser.ParseDecorated(body);
			IEnumerable<IResponseError> errors = errorDecorationResult.Value!;

			return new ServiceResponse<TResult>((int)response.StatusCode, errors, setCookieHeaders);
		}

		public async Task<ServiceResponse<TResult>> ProcessAsync<TBody, TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			HttpMethod method, TBody dtoBody, Dictionary<string, string>? cookie = null)
			where TResult : class
		{
			return await ProcessAsync(parser, "", method, dtoBody, cookie);
		}

		public async Task<ServiceResponse<TResult>> ProcessAsync<TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			string localPath, HttpMethod method, Dictionary<string, string>? cookie = null)
			where TResult : class
		{
			return await ProcessAsync<object, TResult>(parser, localPath, method, null!, cookie);
		}

		public async Task<ServiceResponse<TResult>> ProcessAsync<TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			HttpMethod method, Dictionary<string, string>? cookie = null)
			where TResult : class
		{
			return await ProcessAsync(parser, "", method, cookie);
		}

		private CookieContainer CreateCookieContainer(Dictionary<string, string> cookies)
		{
			CookieContainer container = new CookieContainer();

			foreach (KeyValuePair<string, string> cookieEntry in cookies)
			{
				// TODO make HttpOnly and secure
				Cookie cookie = new Cookie(cookieEntry.Key, cookieEntry.Value)
				{
					Domain = new Uri(_configuration.Url).Host
				};
				container.Add(cookie);
			}

			return container;
		}

		private HttpClientHandler CreateHttpClientHandler(CookieContainer? cookieContainer)
		{
			HttpClientHandler handler = new HttpClientHandler();

			if (cookieContainer != null)
				handler.CookieContainer = cookieContainer;

			return handler;
		}
	}
}
