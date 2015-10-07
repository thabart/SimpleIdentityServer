Feature: GetAccessTokenWithResourceOwnerGrantType
	As a resource owner and user of the client
	I should be able to retrieve an access token with my credentials

Scenario: Retrieve an access token
	Given a resource owner with username thierry and password loki is defined
	And a mobile application MyHolidays is defined
	When requesting an access token via resource owner grant-type
