Feature: GetAuthorizationCode
	As a resource owner and user of the client
	I should be able to retrieve an authorization code


# HAPPY PATH
Scenario: Whether the user is authenticated or not we want to re-authenticate him
	Given a mobile application MyHolidays is defined
	And scopes openid,PlanningApi are defined
	And the scopes openid,PlanningApi are assigned to the client MyHolidays

	When requesting an authorization code
	| scope              | response_type | client_id  | redirect_uri     | prompt |
	| openid PlanningApi | code          | MyHolidays | http://localhost | login  |


	Then HTTP status code is 301
	And redirect to /Authenticate controller

Scenario: A user is authenticated and we want to display only the consent screen
	Given a mobile application MyHolidays is defined
	And scopes openid,PlanningApi are defined
	And the scopes openid,PlanningApi are assigned to the client MyHolidays
	And a resource owner is authenticated
	| UserId               | UserName |
	| habarthierry@loki.be | thabart  |

	When requesting an authorization code
	| scope              | response_type | client_id  | redirect_uri     | prompt  |
	| openid PlanningApi | code          | MyHolidays | http://localhost | consent |


	Then HTTP status code is 301
	And redirect to /Consent controller

Scenario: A user is not authenticated and we want to display only the consent screen
	Given a mobile application MyHolidays is defined
	And scopes openid,PlanningApi are defined
	And the scopes openid,PlanningApi are assigned to the client MyHolidays

	When requesting an authorization code
	| scope              | response_type | client_id  | redirect_uri     | prompt  |
	| openid PlanningApi | code          | MyHolidays | http://localhost | consent |


	Then HTTP status code is 301
	And redirect to /Authenticate controller


# ERRORS
Scenario: A user is not authenticated but we want to directly retrieve the authorization code into the callback
	Given a mobile application MyHolidays is defined
	And scopes openid,PlanningApi are defined
	And the scopes openid,PlanningApi are assigned to the client MyHolidays

	When requesting an authorization code
	| scope              | response_type | client_id  | redirect_uri     | prompt | state  |
	| openid PlanningApi | code          | MyHolidays | http://localhost | none   | state1 |

	Then HTTP status code is 400
	And the error returned is
	| error          | state  |
	| login_required | state1 |
