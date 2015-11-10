Feature: GetIdTokenViaImplicitWorkflow
	As a known client
	I want to use the implicit workflow to retrieve the id token or access token

# HAPPY PATHS

Scenario: Get the id token
	Given a mobile application MyHolidays is defined
	And scopes openid,PlanningApi are defined
	And the id_token signature algorithm is set to none for the client MyHolidays
	And the scopes openid,PlanningApi are assigned to the client MyHolidays
	And the grant-type implicit is supported by the client MyHolidays
	And the response-types id_token are supported by the client MyHolidays
	And a resource owner is authenticated
	| UserId               | UserName |
	| habarthierry@loki.be | thabart  |
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,PlanningApi
	
	When requesting an authorization
	| scope              | response_type | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid PlanningApi | id_token      | MyHolidays | http://localhost | none   | state1 | parameterNonce |

	Then the http status code is 301
	And decrypt the id_token parameter from the query string
	And the protected JWS header is returned
	| alg  |
	| none |
	And the audience parameter with value MyHolidays is returned by the JWS payload
	And the parameter nonce with value parameterNonce is returned by the JWS payload
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload

Scenario: Get the id token and access token via implicit workflow
	Given a mobile application MyHolidays is defined
	And scopes openid,PlanningApi are defined
	And the id_token signature algorithm is set to none for the client MyHolidays
	And the scopes openid,PlanningApi are assigned to the client MyHolidays
	And the grant-type implicit is supported by the client MyHolidays
	And the response-types id_token,token are supported by the client MyHolidays
	And a resource owner is authenticated
	| UserId               | UserName |
	| habarthierry@loki.be | thabart  |
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,PlanningApi
	
	When requesting an authorization
	| scope              | response_type  | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid PlanningApi | id_token token | MyHolidays | http://localhost | none   | state1 | parameterNonce |

	
	Then the http status code is 301
	And decrypt the id_token parameter from the query string
	And the protected JWS header is returned
	| alg  |
	| none |
	And the audience parameter with value MyHolidays is returned by the JWS payload
	And the parameter nonce with value parameterNonce is returned by the JWS payload
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload

	And the callback contains the following query name access_token

Scenario: Get a signed id_token
	Given a mobile application MyHolidays is defined
	And create a RSA key
	And scopes openid,PlanningApi are defined
	And the id_token signature algorithm is set to RS256 for the client MyHolidays
	And the scopes openid,PlanningApi are assigned to the client MyHolidays
	And the grant-type implicit is supported by the client MyHolidays
	And the response-types id_token are supported by the client MyHolidays
	And a resource owner is authenticated
	| UserId               | UserName |
	| habarthierry@loki.be | thabart  |
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,PlanningApi
	
	When requesting an authorization
	| scope              | response_type | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid PlanningApi | id_token      | MyHolidays | http://localhost | none   | state1 | parameterNonce |

	Then the http status code is 301
	And decrypt the id_token parameter from the query string
	And the protected JWS header is returned
	| alg   |
	| RS256 |
	And the audience parameter with value MyHolidays is returned by the JWS payload
	And the parameter nonce with value parameterNonce is returned by the JWS payload
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload
	And the signature of the JWS payload is valid



