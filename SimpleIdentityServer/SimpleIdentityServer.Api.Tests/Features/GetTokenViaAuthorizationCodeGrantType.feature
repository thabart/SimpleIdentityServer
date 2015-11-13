Feature: GetTokenViaAuthorizationCodeGrantType
	As an authenticated user
	I want to retrieve my access token and id_token via the authorization code workflow

Background:
	Given a mobile application MyHolidays is defined
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
	And a resource owner is authenticated
	| UserId               | UserName |
	| habarthierry@loki.be | thabart  |
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,PlanningApi,profile
	And requesting an authorization code
	| scope                      | response_type | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid PlanningApi profile | code          | MyHolidays | http://localhost | none   | state1 | parameterNonce |

Scenario: request an id token and access token via the authorization grant type flow	
	When requesting a token with basic client authentication for the client id MyHolidays and client secret MyHolidays
	| grant_type         | client_id  | redirect_uri     |
	| authorization_code | MyHolidays | http://localhost |

	Then the following token is returned
	| TokenType |
	| Bearer    |
	And decrypt the id_token parameter from the response
	And the protected JWS header is returned
	| alg  |
	| none |
	And the parameter nonce with value parameterNonce is returned by the JWS payload
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload

	