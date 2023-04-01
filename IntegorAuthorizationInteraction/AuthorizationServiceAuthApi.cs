using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

using IntegorGlobalConstants;

using IntegorServicesInteraction;
using IntegorServicesInteraction.Authorization;

using IntegorPublicDto.Authorization.Users;
using IntegorPublicDto.Authorization.Users.Input;

using IntegorErrorsHandling;
using IntegorResponseDecoration.Parsing;

using IntegorServicesInteractionHelpers;
using ExtensibleRefreshJwtAuthentication.Access.Tokens;
using ExtensibleRefreshJwtAuthentication.Refresh.Tokens;

namespace IntegorAuthorizationInteraction
{
	public class AuthorizationServiceAuthApi : IAuthorizationServiceAuthApi
	{
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
				configuration, accessTokenAccessor, refreshTokenAccessor, errorsParser, "auth");
		}

		public async Task<ServiceResponse<UserAccountInfoDto>> RegisterAsync(RegisterUserDto dto)
		{
			return await _requestProcessor.ProcessPostAsync(_userParser, _registerPath, dto);
		}

		public async Task<ServiceResponse<UserAccountInfoDto>> LoginAsync(LoginUserDto dto)
		{
			return await _requestProcessor.ProcessPostAsync(_userParser, _loginPath, dto);
		}

		public async Task<ServiceResponse<UserAccountInfoDto>> RefreshAsync(string refreshToken)
		{
			return await _requestProcessor.ProcessPostAsync(_userParser, _refreshPath, AuthenticationMethods.Refresh, refreshToken);
		}
	}
}
