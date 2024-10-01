
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime;
using UnityEngine;

namespace RMC.MyProject.AWS
{
    /// <summary>
    /// Handle the Amazon AWS operations.
    ///
    /// * <see cref="https://aws.amazon.com/cognito/"/>
    /// * <see cref="https://aws.amazon.com/dynamodb"/>
    /// </summary>
    public class AWSController 
    {
        //  Internal Class --------------------------------
        public class Response
        {
            public string Data { get; set; }
            public bool IsSuccess { get; set; }
            public string ErrorMessage { get; set; }
            public Response()
            {
                IsSuccess = false;
                ErrorMessage = "Undefined";
            }
        }
        
        //  Properties ------------------------------------
        public bool IsUserLoggedIn { get { return _isUserLoggedIn; }  }

        //  Fields ----------------------------------------
        private readonly AmazonCognitoIdentityProviderClient _cognitoService;
        private bool _isUserLoggedIn = false;
        
        
        // Initialization ---------------------------------
        public AWSController()
        {
            _cognitoService = new AmazonCognitoIdentityProviderClient
            (
                new AnonymousAWSCredentials(),
                AWSConstants.AWSRegionEndpoint
                );
        }
        
        //  Methods ---------------------------------------
        /// <summary>
        /// Sign up a new user.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="password">The user's password.</param>
        /// <param name="nickname">The nickname to use.</param>
        /// <returns>A Boolean value indicating whether the user was confirmed.</returns>
        public async Task<Response> SignUpAsync(string email, string password, string nickname)
        {
            Response response = new Response();
            if (IsUserLoggedIn)
            {
                response.IsSuccess = false;
                response.ErrorMessage = $"SignUpAsync() failed. IsUserLoggedIn must not be {IsUserLoggedIn}.";
                Debug.LogError($"{response.ErrorMessage}");
                return response;
            }
            
            var nicknameAttrs = new AttributeType
            {
                //Generally Optional. Required here. I chose to add this in the Amazon AWS console.
                Name = "nickname", 
                Value = nickname,
            };
            
            var versionAttrs = new AttributeType
            {
                //Generally Optional. Required here. I chose to add this in the Amazon AWS console.
                Name = "custom:app_version", 
                Value = "0.0.1",
            };

            var userAttrsList = new List<AttributeType>();
            userAttrsList.Add(nicknameAttrs);
            userAttrsList.Add(versionAttrs);

            var signUpRequest = new SignUpRequest
            {
                UserAttributes = userAttrsList,
                Username = email,
                ClientId = AWSConstants.AWSClientId,
                Password = password
            };

            try
            {
                SignUpResponse signUpResponse = await _cognitoService.SignUpAsync(signUpRequest);
                
                bool isSuccessful =signUpResponse.HttpStatusCode == HttpStatusCode.OK;
                if (isSuccessful)
                {
                    _isUserLoggedIn = true;
                }
                response.IsSuccess = true;
            }
            catch (Exception e)
            {
                response.ErrorMessage = $"{e.Message}";
            }
            
            return response;

        }

        /// <summary>
        /// Initiate authorization.
        /// </summary>
        /// <param name="clientId">The client Id of the application.</param>
        /// <param name="email">The name of the user who is authenticating.</param>
        /// <param name="password">The password for the user who is authenticating.</param>
        /// <returns>The response from the initiate auth request.</returns>
        public async Task<Response> SignInAsync(string email, string password)
        {
            Response response = new Response();
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                response.IsSuccess = false;
                response.ErrorMessage = $"Bad parameters. Details...";
                Debug.LogError($"{response.ErrorMessage}");
                return response;
            }
            
            var authParameters = new Dictionary<string, string>();
            authParameters.Add("USERNAME", email);
            authParameters.Add("PASSWORD", password);

            var authRequest = new InitiateAuthRequest
            {
                ClientId = AWSConstants.AWSClientId,
                AuthParameters = authParameters,
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
            };

            try
            {
                InitiateAuthResponse initiateAuthResponse = await _cognitoService.InitiateAuthAsync(authRequest);
                response.Data = initiateAuthResponse.AuthenticationResult.IdToken;
                response.IsSuccess = true;
                _isUserLoggedIn = true;
            }
            catch (Exception e)
            {
                response.IsSuccess = false;
                response.ErrorMessage = e.Message;
            }
        
            return response;
        }


        //  Event Handlers --------------------------------
    }
}