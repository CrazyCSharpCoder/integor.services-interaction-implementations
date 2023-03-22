﻿using System;
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

namespace IntegorAuthorizationInteraction
{
	public class AuthorizationServiceAuthApi : JsonServicesRequestProcessor<AuthorizationServiceConfiguration>, IAuthorizationServiceAuthApi
	{
		private const string _registerPath = "register";
		private const string _loginPath = "login";
		private const string _refreshPath = "refresh";

		private IDecoratedObjectParser<UserAccountInfoDto, JsonElement> _userParser;

		public AuthorizationServiceAuthApi(
			AuthorizationServiceConfiguration configuration,
			IDecoratedObjectParser<IEnumerable<IResponseError>, JsonElement> errorsParser,
			IDecoratedObjectParser<UserAccountInfoDto, JsonElement> userParser)
			: base(configuration, errorsParser, "auth")
        {
			_userParser = userParser;
        }

		public async Task<ServiceResponse<UserAccountInfoDto>> RegisterAsync(RegisterUserDto dto)
		{
			return await ProcessPostAsync(_userParser, _registerPath, dto);
		}

		public async Task<ServiceResponse<UserAccountInfoDto>> LoginAsync(LoginUserDto dto)
		{
			return await ProcessPostAsync(_userParser, _loginPath, dto);
		}

		public async Task<ServiceResponse<UserAccountInfoDto>> RefreshAsync(string refreshToken)
		{
			// TODO place cookie name to IntegorGlobalConstants
			Dictionary<string, string> cookie = new Dictionary<string, string>()
			{
				{ "AuthenticationRefresh", refreshToken }
			};

			return await ProcessPostAsync(_userParser, _refreshPath, cookie);
		}
	}
}
