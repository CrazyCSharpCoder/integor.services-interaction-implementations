using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using System.Net.Http;

using Microsoft.Extensions.Options;

using IntegorServicesInteraction;
using IntegorServicesInteraction.Authorization;

using IntegorPublicDto.Authorization.Users;

using IntegorErrorsHandling;
using IntegorResponseDecoration.Parsing;

using IntegorServicesInteractionHelpers;

namespace IntegorAuthorizationInteraction
{
	public class AuthorizationServiceUsersApi : IAuthorizationServiceUsersApi
	{
		private const string _apiPrefix = "users";

		private const string _getByIdPath = "by-id/{0}";
		private const string _getByEmailPath = "by-email/{0}";

		private JsonServicesRequestProcessor<AuthorizationServiceConfiguration> _requestProcessor;

		private IDecoratedObjectParser<UserAccountInfoDto, JsonElement> _userParser;

		public AuthorizationServiceUsersApi(
			IDecoratedObjectParser<IEnumerable<IResponseError>, JsonElement> errorsParser,
			IDecoratedObjectParser<UserAccountInfoDto, JsonElement> userParser,
			IOptions<AuthorizationServiceConfiguration> serviceOptions)
        {
			_userParser = userParser;

			_requestProcessor =
				new JsonServicesRequestProcessor<AuthorizationServiceConfiguration>(
					errorsParser, serviceOptions, _apiPrefix);
		}

		public async Task<ServiceResponse<UserAccountInfoDto>> GetByIdAsync(int id)
		{
			string path = string.Format(_getByIdPath, id);
			return await _requestProcessor.ProcessAsync(_userParser, HttpMethod.Get, path);
		}

		public async Task<ServiceResponse<UserAccountInfoDto>> GetByEmailAsync(string email)
		{
			string path = string.Format(_getByEmailPath, email);
			return await _requestProcessor.ProcessAsync(_userParser, HttpMethod.Get, path);
		}
	}
}
