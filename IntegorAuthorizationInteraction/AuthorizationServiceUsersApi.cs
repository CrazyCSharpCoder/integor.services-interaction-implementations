using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

using IntegorServicesInteraction;
using IntegorServicesInteraction.Authorization;

using IntegorPublicDto.Authorization.Users;

using IntegorErrorsHandling;
using IntegorResponseDecoration.Parsing;

using IntegorServicesInteractionHelpers;
using ExtensibleRefreshJwtAuthentication.Access;
using ExtensibleRefreshJwtAuthentication.Refresh;

namespace IntegorAuthorizationInteraction
{
	public class AuthorizationServiceUsersApi : IAuthorizationServiceUsersApi
	{
		private const string _apiPrefix = "users";

		private const string _getByIdPath = "by-id/{0}";
		private const string _getByEmailPath = "by-email/{0}";

		private JsonServicesRequestProcessor<AuthorizationServiceConfiguration> _requestProcessor;

		private IDecoratedObjectParser<UserAccountInfoDto, JsonElement> _userParser;

		public AuthorizationServiceUsersApi(AuthorizationServiceConfiguration configuration,
			ISendRequestAccessTokenAccessor accessTokenAccessor,
			ISendRequestRefreshTokenAccessor refreshTokenAccessor,
			IDecoratedObjectParser<IEnumerable<IResponseError>, JsonElement> errorsParser,
			IDecoratedObjectParser<UserAccountInfoDto, JsonElement> userParser)
        {
			_userParser = userParser;
			_requestProcessor = new JsonServicesRequestProcessor<AuthorizationServiceConfiguration>(
				configuration, accessTokenAccessor, refreshTokenAccessor, errorsParser, _apiPrefix);
		}

		public Task<ServiceResponse<UserAccountInfoDto>> GetByIdAsync(int id)
		{
			return _requestProcessor.GetAsync(_userParser, string.Format(_getByIdPath, id));
		}

		public Task<ServiceResponse<UserAccountInfoDto>> GetByEmailAsync(string email)
		{
			return _requestProcessor.GetAsync(_userParser, string.Format(_getByEmailPath, email));
		}
	}
}
