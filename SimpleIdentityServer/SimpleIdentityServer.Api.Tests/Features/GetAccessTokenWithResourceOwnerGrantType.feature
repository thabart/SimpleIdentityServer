Feature: GetAccessTokenWithResourceOwnerGrantType
	As a resource owner and user of the client
	I should be able to retrieve an access token with my credentials

Scenario: Retrieve an access token without defining scopes
	Given a resource owner with username thierry and password loki is defined
	And a mobile application MyHolidays is defined
	When requesting an access token via resource owner grant-type
	Then http result is 200
	And access token is generated

Scenario: Retrieve an access token with one scope
	Given a resource owner with username thierry and password loki is defined
	And a mobile application MyHolidays is defined
	And scopes roles,openid are defined
