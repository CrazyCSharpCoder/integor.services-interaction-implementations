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

using IntegorServicesInteractionHelpers.Internal;

namespace IntegorServicesInteractionHelpers
{
	public class JsonServicesRequestProcessor<TServiceConfiguration>
		where TServiceConfiguration : ServiceConfiguration, new()
	{
		private IDecoratedObjectParser<IEnumerable<IResponseError>, JsonElement> _errorsParser;
		private TServiceConfiguration _configuration;

		private string _urlPrefix;

		public JsonServicesRequestProcessor(
			IDecoratedObjectParser<IEnumerable<IResponseError>, JsonElement> errorsParser,
			IOptions<TServiceConfiguration> serviceOptions,

			string localUrlPrefix = "")
        {
			_errorsParser = errorsParser;
			_configuration = serviceOptions.Value;

			_urlPrefix = localUrlPrefix;
		}

		public async Task<ServiceResponse<TResult>> ProcessAsync<TBody, TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			HttpMethod method, string localPath = "", Dictionary<string, string>? queryParameters = null,
			TBody? dtoBody = null)
			where TBody : class
			where TResult : class
		{
			string url = NetHttpRequestsHelpers.ComposeLocalUrl(queryParameters, _urlPrefix, localPath);
			using HttpContent content = JsonContent.Create(dtoBody);

			using HttpClient httpClient = new HttpClient() { BaseAddress = new Uri(_configuration.Url) };

			using HttpRequestMessage request = new HttpRequestMessage(method, url)
			{
				Content = content
			};

			using HttpResponseMessage response = await httpClient.SendAsync(request);

			JsonSerializerOptions jsonOptions = new JsonSerializerOptions()
			{
				PropertyNameCaseInsensitive = true
			};

			return await NetHttpRequestsHelpers.ParseJsonResponseAsync(
				response, jsonOptions,
				_errorsParser, parser);
		}

		public async Task<ServiceResponse<TResult>> ProcessAsync<TResult>(
			IDecoratedObjectParser<TResult, JsonElement> parser,
			HttpMethod method, string localPath = "", Dictionary<string, string>? queryParameters = null)

			where TResult : class
		{
			return await ProcessAsync<object, TResult>(parser, method, localPath, queryParameters, null);
		}
	}
}
