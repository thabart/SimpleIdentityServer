Feature: GetAccessTokenMultipleTime
	As an authenticated user
	I request several times an access token

Scenario: Request 3 times an access token
	Given a resource owner with username thierry and password loki is defined
	And a mobile application MyHolidays is defined

	When requesting an access token 3 times
	| client_id  | username | password |
	| MyHolidays | thierry  | loki     |

	Then the result should be
