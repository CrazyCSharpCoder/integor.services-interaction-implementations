using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Text.Json;
using System.Net.Http;

using IntegorErrorsHandling;
using IntegorResponseDecoration.Parsing;

using IntegorServicesInteraction;
using IntegorServicesInteraction.Exceptions;

using ExtensibleRefreshJwtAuthentication.Access;
using ExtensibleRefreshJwtAuthentication.Refresh;

namespace IntegorServicesInteractionHelpers.Internal
{
	internal static class NetHttpRequestsHelpers
	{
		public static string ComposeLocalUrl(Dictionary<string, string>? queryParameters, params string[] pathSegments)
		{
			string url = Path.Join(pathSegments);

			if (queryParameters != null && queryParameters.Count != 0)
			{
				IEnumerable<string> strQueryParameters = queryParameters.Select(paramToValue => $"{paramToValue.Key}={paramToValue.Value}");
				string queryString = string.Join('&', strQueryParameters);

				url = $"{url}?{queryString}";
			}

			return url;
		}

		public static async Task<ServiceResponse<TResult>> ParseJsonResponseAsync<TResult>(
			HttpResponseMessage response, JsonSerializerOptions jsonOptions,
			IDecoratedObjectParser<IEnumerable<IResponseError>, JsonElement> errorsParser,
			IDecoratedObjectParser<TResult, JsonElement> bodyParser,
			ISendRequestAccessTokenAccessor? accessTokenAccessor = null,
			ISendRequestRefreshTokenAccessor? refreshTokenAccessor = null)
			where TResult : class
		{
			int statusCode = (int)response.StatusCode;
			using Stream responseStream = await response.Content.ReadAsStreamAsync();

			JsonElement body = await JsonSerializer.DeserializeAsync<JsonElement>(responseStream, jsonOptions);

			string? accessToken = accessTokenAccessor?.GetFromResponse(response);
			string? refreshToken = refreshTokenAccessor?.GetFromResponse(response);

			UserAuthentication? authentication = new UserAuthentication(accessToken, refreshToken);

			var parsingResult = bodyParser.ParseDecorated(body);

			if (parsingResult.Success)
				return new ServiceResponse<TResult>(statusCode, authentication, parsingResult.Value);

			var errorDecorationResult = errorsParser.ParseDecorated(body);
			IEnumerable<IResponseError> errors = errorDecorationResult.Value!;

			return new ServiceResponse<TResult>((int)response.StatusCode, authentication, errors);
		}

		public static async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpRequestMessage request)
		{
			try
			{
				return await client.SendAsync(request);
			}
			catch (Exception exc)
			{
				throw new ServiceConnectionFailedException(exc);
			}
		}
	}
}
