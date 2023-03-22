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

using IntegorPublicDto.Authorization.Users;
using IntegorServicesInteraction;

namespace IntegorServicesInteractionHelpers
{
	public abstract class JsonServicesRequestProcessor<TServiceConfiguration>
		where TServiceConfiguration : ServiceConfiguration
	{
		private TServiceConfiguration _configuration;
		private IDecoratedObjectParser<IEnumerable<IResponseError>, JsonElement> _errorsParser;

		public JsonServicesRequestProcessor(
			TServiceConfiguration configuration,
			IDecoratedObjectParser<IEnumerable<IResponseError>, JsonElement> errorsParser)
        {
			_configuration = configuration;
			_errorsParser = errorsParser;
		}

		protected async Task<ServiceResponse<TResult>> ProcessAsync<TBody, TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			HttpMethod method, string localPath, TBody dtoBody, Dictionary<string, string>? cookie = null)
			where TResult : class
		{
			HttpContent content = JsonContent.Create(dtoBody);

			HttpRequestMessage request = new HttpRequestMessage(method, localPath)
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
			// TODO make Set-Cookie as constant
			IEnumerable<string> setCookies = response.Headers.GetValues("Set-Cookie");
			JsonElement body = await JsonSerializer.DeserializeAsync<JsonElement>(responseStream, jsonOptions);

			var parsingResult = parser.ParseDecorated(body);

			if (parsingResult.Success)
				// TODO append cookies
				return new ServiceResponse<TResult>(statusCode, parsingResult.Value, setCookies);

			// TODO consider situations when authorization microservice is switched off
			var errorDecorationResult = _errorsParser.ParseDecorated(body);
			IEnumerable<IResponseError> errors = errorDecorationResult.Value!;

			return new ServiceResponse<TResult>((int)response.StatusCode, errors, setCookies);
		}

		private CookieContainer CreateCookieContainer(Dictionary<string, string> cookie)
		{
			CookieContainer container = new CookieContainer();

			foreach (KeyValuePair<string, string> cookieEntry in cookie)
				container.Add(new Cookie(cookieEntry.Key, cookieEntry.Value));

			return container;
		}

		private HttpClientHandler CreateHttpClientHandler(CookieContainer? cookieContainer)
		{
			HttpClientHandler handler = new HttpClientHandler();

			if (cookieContainer != null)
				handler.CookieContainer = cookieContainer;

			return handler;
		}

		protected async Task<ServiceResponse<TResult>> ProcessGetAsync<TBody, TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			string localPath, TBody dtoBody, Dictionary<string, string>? cookie = null)
			where TResult : class
		{
			return await ProcessAsync(parser, HttpMethod.Get, localPath, dtoBody, cookie);
		}

		protected async Task<ServiceResponse<TResult>> ProcessPostAsync<TBody, TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			string localPath, TBody dtoBody, Dictionary<string, string>? cookie = null)
			where TResult : class
		{
			return await ProcessAsync(parser, HttpMethod.Post, localPath, dtoBody, cookie);
		}

		protected async Task<ServiceResponse<TResult>> ProcessPutAsync<TBody, TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			string localPath, TBody dtoBody, Dictionary<string, string>? cookie = null)
			where TResult : class
		{
			return await ProcessAsync(parser, HttpMethod.Put, localPath, dtoBody, cookie);
		}

		protected async Task<ServiceResponse<TResult>> ProcessDeleteAsync<TBody, TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			string localPath, TBody dtoBody, Dictionary<string, string>? cookie = null)
			where TResult : class
		{
			return await ProcessAsync(parser, HttpMethod.Delete, localPath, dtoBody, cookie);
		}
	}
}
