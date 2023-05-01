using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

using IntegorServicesInteraction;
using IntegorServicesInteraction.Authorization;

using IntegorPublicDto.Authorization.Users;
using IntegorPublicDto.Authorization.Users.Input;

using IntegorErrorsHandling;
using IntegorResponseDecoration.Parsing;

using IntegorServicesInteractionHelpers;
using ExtensibleRefreshJwtAuthentication.Access;
using ExtensibleRefreshJwtAuthentication.Refresh;

namespace IntegorAuthorizationInteraction
{
	public class AuthorizationServiceAuthApi : IAuthorizationServiceAuthApi
	{
		private const string _apiPrefix = "auth";

		private const string _getMePath = "me";
		private const string _registerPath = "register";
		private const string _loginPath = "login";
		private const string _refreshPath = "refresh";

		private JsonServicesRequestProcessor<AuthorizationServiceConfiguration> _requestProcessor;

		private IDecoratedObjectParser<UserAccountInfoDto, JsonElement> _userParser;

		public AuthorizationServiceAuthApi(
			AuthorizationServiceConfiguration configuration,
			ISendRequestAccessTokenAccessor accessTokenAccessor,
			ISendRequestRefreshTokenAccessor refreshTokenAccessor,
			IDecoratedObjectParser<IEnumerable<IResponseError>, JsonElement> errorsParser,
			IDecoratedObjectParser<UserAccountInfoDto, JsonElement> userParser)
        {
			_userParser = userParser;
			_requestProcessor = new JsonServicesRequestProcessor<AuthorizationServiceConfiguration>(
				configuration, accessTokenAccessor, refreshTokenAccessor, errorsParser, _apiPrefix);
		}

		public async Task<ServiceResponse<UserAccountInfoDto>> RegisterAsync(RegisterUserDto dto)
		{
			return await _requestProcessor.PostAsync(_userParser, _registerPath, dto);
		}

		public async Task<ServiceResponse<UserAccountInfoDto>> LoginAsync(LoginUserDto dto)
		{
			return await _requestProcessor.PostAsync(_userParser, _loginPath, dto);
		}

		public Task<ServiceResponse<UserAccountInfoDto>> GetMeAsync(string accessToken)
		{
			return _requestProcessor.GetAsync(_userParser, _getMePath, AuthenticationMethods.Access, accessToken);
		}

		public async Task<ServiceResponse<UserAccountInfoDto>> RefreshAsync(string refreshToken)
		{
			return await _requestProcessor.PostAsync(_userParser, _refreshPath, AuthenticationMethods.Refresh, refreshToken);
		}
	}
}
