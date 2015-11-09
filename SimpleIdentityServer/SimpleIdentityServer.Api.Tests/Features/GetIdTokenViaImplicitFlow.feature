Feature: GetIdTokenViaImplicitWorkflow
	As a client
	I want authenticate the user and retrieve his id_token via the implicit_workflow

Scenario: Get the id token	Given a mobile application MyHolidays is defined
	Given a mobile application MyHolidays is defined
	And scopes openid,PlanningApi are defined
	And the id_token signature algorithm is set to none for the client MyHolidays
	And the scopes openid,PlanningApi are assigned to the client MyHolidays
	And the grant-type implicit is supported by the client MyHolidays
	And the response-type id_token is supported by the client MyHolidays
	And a resource owner is authenticated
	| UserId               | UserName |
	| habarthierry@loki.be | thabart  |
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,PlanningApi
	
	When requesting an authorization code
	| scope              | response_type | client_id  | redirect_uri     | prompt | state  | nonce |
	| openid PlanningApi | id_token      | MyHolidays | http://localhost | none   | state1 | nonce |

	Then the http status code is 301
	And decrypt the id_token parameter from the query string
	And the protected JWS header is returned
	| alg  |
	| none |

