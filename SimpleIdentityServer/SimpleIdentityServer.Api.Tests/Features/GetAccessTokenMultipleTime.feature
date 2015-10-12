Feature: GetAccessTokenMultipleTime
	As an authenticated user
	I request several times an access token

Scenario: Request 3 times an access token
	Given I have entered 50 into the calculator
	And I have entered 70 into the calculator
	When I press add
	Then the result should be 120 on the screen
