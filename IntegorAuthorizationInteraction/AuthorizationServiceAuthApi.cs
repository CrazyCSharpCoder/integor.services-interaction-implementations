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

		private const string _registerPath = "register";
		private const string _loginPath = "login";
		private const string _logoutPath = "logout";

		private const string _getMePath = "me";
		private const string _refreshPath = "refresh";

		private AuthenticatedJsonServicesRequestProcessor<AuthorizationServiceConfiguration> _requestProcessor;

		private IDecoratedObjectParser<UserAccountInfoDto, JsonElement> _userParser;

		public AuthorizationServiceAuthApi(
			ISendRequestAccessTokenAccessor accessTokenAccessor,
			ISendRequestRefreshTokenAccessor refreshTokenAccessor,

			IDecoratedObjectParser<IEnumerable<IResponseError>, JsonElement> errorsParser,
			IDecoratedObjectParser<UserAccountInfoDto, JsonElement> userParser,

			IOptions<AuthorizationServiceConfiguration> serviceOptions)
        {
			_userParser = userParser;
			_requestProcessor =
				new AuthenticatedJsonServicesRequestProcessor<AuthorizationServiceConfiguration>(
					accessTokenAccessor, refreshTokenAccessor, errorsParser, serviceOptions, _apiPrefix);
		}

		public async Task<ServiceResponse<UserAccountInfoDto>> RegisterAsync(RegisterUserDto dto)
		{
			return await _requestProcessor.ProcessAsync(_userParser, HttpMethod.Post, _registerPath, dtoBody: dto);
		}

		public async Task<ServiceResponse<UserAccountInfoDto>> LoginAsync(LoginUserDto dto)
		{
			return await _requestProcessor.ProcessAsync(_userParser, HttpMethod.Post, _loginPath, dtoBody: dto);
		}

		public async Task<ServiceResponse<UserAccountInfoDto>> LogoutAsync(string? accessToken = null, string? refreshToken = null)
		{
			UserAuthentication authentication = new UserAuthentication(accessToken, refreshToken);
			return await _requestProcessor.ProcessAsync(_userParser, HttpMethod.Post, _logoutPath, authentication: authentication);
		}


		public async Task<ServiceResponse<UserAccountInfoDto>> GetMeAsync(string accessToken)
		{
			UserAuthentication authentication = new UserAuthentication(accessToken, null);
			return await _requestProcessor.ProcessAsync(_userParser, HttpMethod.Get, _getMePath, authentication: authentication);
		}

		public async Task<ServiceResponse<UserAccountInfoDto>> RefreshAsync(string refreshToken)
		{
			UserAuthentication authentication = new UserAuthentication(null, refreshToken);
			return await _requestProcessor.ProcessAsync(_userParser, HttpMethod.Post, _refreshPath, authentication: authentication);
		}
	}
}
