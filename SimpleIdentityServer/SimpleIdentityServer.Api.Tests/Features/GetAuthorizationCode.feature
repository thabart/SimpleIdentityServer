Feature: GetAuthorizationCode
	As a resource owner and user of the client
	I should be able to retrieve an authorization code

Scenario: A not authenticated resource owner is trying to retrieve an authorization code
	Given a mobile application MyHolidays is defined
	And scopes openid,PlanningApi are defined
	And the scopes openid,PlanningApi are assigned to the client MyHolidays

	When requesting an authorization code
	| scope              | response_type | client_id  | redirect_uri     | prompt |
	| openid PlanningApi | code          | MyHolidays | http://localhost | login  |


	Then HTTP status code is 301
	And redirect to /Authenticate controller
	And contains authorization code