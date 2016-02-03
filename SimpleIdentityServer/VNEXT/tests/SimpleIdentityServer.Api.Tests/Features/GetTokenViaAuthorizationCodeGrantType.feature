Feature: GetTokenViaAuthorizationCodeGrantType
	As an authenticated user
	I want to retrieve my access token and id_token via the authorization code workflow

Background:
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name        | IsInternal | Claims |
	| openid      | true       |        |
	| PlanningApi | false      |        |
	| profile     | true       | name   |
	
	And the id_token signature algorithm is set to none for the client MyHolidays
	And the scopes openid,profile,PlanningApi are assigned to the client MyHolidays
	And the client secret MyHolidays is assigned to the client MyHolidays
	And the grant-type authorization_code is supported by the client MyHolidays
	And the response-types code are supported by the client MyHolidays
	And create a resource owner
	| Id                   | Name    |
	| habarthierry@loki.be | thabart |
	And authenticate the resource owner
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,PlanningApi,profile
	And requesting an authorization code
	| scope                      | response_type | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid PlanningApi profile | code          | MyHolidays | http://localhost | none   | state1 | parameterNonce |

# Check the authentication mechanism : client_secret_basic
Scenario: request an id token and access token via the authorization grant type flow. The client credentials are passed via client_secret_basic
	Given the token endpoint authentication method client_secret_basic is assigned to the client MyHolidays
	When requesting a token with basic client authentication for the client id MyHolidays and client secret MyHolidays
	| grant_type         | redirect_uri     | client_id  |
	| authorization_code | http://localhost | MyHolidays |

	Then the following token is returned
	| TokenType |
	| Bearer    |
	And decrypt the id_token parameter from the response
	And the protected JWS header is returned
	| Alg  |
	| none |
	And the parameter nonce with value parameterNonce is returned by the JWS payload
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload

# Check the authentication mechanism : client_secret_post
Scenario: request an id token and access token via the authorization grant type flow. The client credentials are passed via client_secret_post method
	Given the token endpoint authentication method client_secret_post is assigned to the client MyHolidays
	When requesting a token by using a client_secret_post authentication mechanism
	| grant_type         | redirect_uri     | client_id  | client_secret |
	| authorization_code | http://localhost | MyHolidays | MyHolidays    |

	Then the following token is returned
	| TokenType |
	| Bearer    |
	And decrypt the id_token parameter from the response
	And the protected JWS header is returned
	| Alg  |
	| none |
	And the parameter nonce with value parameterNonce is returned by the JWS payload
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload
	
# Check the authentication mechanism : client_secret_jwt
Scenario: request an id token and access token via the authorization grant type flow. The client credentials are passed via client_secret_jwt authentication method
	Given the token endpoint authentication method client_secret_jwt is assigned to the client MyHolidays
	And add json web keys
	| Kid | Alg    | Operation | Kty | Use |
	| 1   | RS256  | Sign      | RSA | Sig |
	| 2   | RSA1_5 | Encrypt   | RSA | Enc |
	And create a request to retrieve a token
	| grant_type         | redirect_uri     | client_assertion_type                                  |
	| authorization_code | http://localhost | urn:ietf:params:oauth:client-assertion-type:jwt-bearer |
	And create the JWS payload
	| iss        | sub        | jti |
	| MyHolidays | MyHolidays | 1   |
	And assign audiences http://localhost/identity to the JWS payload
	And add json web keys 1,2 to the client MyHolidays
	And expiration time 300 in seconds to the JWS payload
	And sign the jws payload with 1 kid
	And encrypt the jws token with 2 kid, encryption algorithm A128CBC_HS256 and password MyHolidays
	And set the client assertion value
	And set the client id MyHolidays into the request

	When retrieve token via client assertion authentication
	
	Then the following token is returned
	| TokenType |
	| Bearer    |
	And decrypt the id_token parameter from the response
	And the protected JWS header is returned
	| Alg  |
	| none |
	And the parameter nonce with value parameterNonce is returned by the JWS payload
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload

# Check the authentication mechanism : private_key_jwt
Scenario: request an id token and access token via the authorization grant type flow. The client credentials are passed via private_key_jwt authentication method
	Given the token endpoint authentication method private_key_jwt is assigned to the client MyHolidays
	And add json web keys
	| Kid | Alg   | Operation | Kty | Use |
	| 1   | RS256 | Sign      | RSA | Sig |
	And create a request to retrieve a token
	| grant_type         | redirect_uri     | client_assertion_type                                  |
	| authorization_code | http://localhost | urn:ietf:params:oauth:client-assertion-type:jwt-bearer |
	And create the JWS payload
	| iss        | sub        | jti |
	| MyHolidays | MyHolidays | 1   |
	And assign audiences http://localhost/identity to the JWS payload
	And add json web keys 1 to the client MyHolidays
	And expiration time 300 in seconds to the JWS payload
	And sign the jws payload with 1 kid
	And set the client assertion value

	When retrieve token via client assertion authentication
	
	Then the following token is returned
	| TokenType |
	| Bearer    |
	And decrypt the id_token parameter from the response
	And the protected JWS header is returned
	| Alg  |
	| none |
	And the parameter nonce with value parameterNonce is returned by the JWS payload
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload