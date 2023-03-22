using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

using System.Net.Http;
using System.Net.Http.Json;

using IntegorServicesInteraction;
using IntegorServicesInteraction.Authorization;

using IntegorPublicDto.Authorization.Users;
using IntegorPublicDto.Authorization.Users.Input;

using IntegorErrorsHandling;
using IntegorResponseDecoration.Parsing;

namespace IntegorAuthorizationInteraction
{

	public class AuthorizationServiceAuthApi : IAuthorizationServiceAuthApi
	{
		private const string _urlPrefix = "auth";

		private AuthorizationServiceConfiguration _config;
		private IDecoratedObjectParser<UserAccountInfoDto, JsonElement> _userParser;

		public AuthorizationServiceAuthApi(
			AuthorizationServiceConfiguration config,
			IDecoratedObjectParser<IEnumerable<IResponseError>, JsonElement> errorsParser,
			IDecoratedObjectParser<UserAccountInfoDto, JsonElement> userParser)
        {
			_config = config;
			_userParser = userParser;
        }

        public async Task<ServiceResponse<UserAccountInfoDto>> LoginAsync(LoginUserDto dto)
		{
			throw new NotImplementedException();
		}

		public Task<ServiceResponse<UserAccountInfoDto>> RefreshAsync(string refreshToken)
		{
			throw new NotImplementedException();
		}

		public Task<ServiceResponse<UserAccountInfoDto>> RegisterAsync(RegisterUserDto dto)
		{
			throw new NotImplementedException();
		}
	}
}
