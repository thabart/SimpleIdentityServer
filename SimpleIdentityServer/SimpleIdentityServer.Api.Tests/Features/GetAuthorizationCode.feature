Feature: GetAuthorizationCode
	As a resource owner and user of the client
	I should be able to retrieve an authorization code


# HAPPY PATH
Scenario: Whether the resource owner is authenticated or not we want to re-authenticate him
	Given a mobile application MyHolidays is defined
	And scopes openid,PlanningApi are defined
	And the scopes openid,PlanningApi are assigned to the client MyHolidays

	When requesting an authorization code
	| scope              | response_type | client_id  | redirect_uri     | prompt |
	| openid PlanningApi | code          | MyHolidays | http://localhost | login  |


	Then HTTP status code is 301
	And redirect to /Authenticate controller

Scenario: A resource owner is authenticated and we want to display only the consent screen
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

Scenario: A resource owner is not authenticated and we want to display only the consent screen
	Given a mobile application MyHolidays is defined
	And scopes openid,PlanningApi are defined
	And the scopes openid,PlanningApi are assigned to the client MyHolidays

	When requesting an authorization code
	| scope              | response_type | client_id  | redirect_uri     | prompt  |
	| openid PlanningApi | code          | MyHolidays | http://localhost | consent |


	Then HTTP status code is 301
	And redirect to /Authenticate controller

Scenario: A resource owner is authenticated and he already has given his consent. We want to retrieve an authorization code for his consent
	Given a mobile application MyHolidays is defined
	And scopes openid,PlanningApi are defined
	And the scopes openid,PlanningApi are assigned to the client MyHolidays
	And a resource owner is authenticated
	| UserId               | UserName |
	| habarthierry@loki.be | thabart  |
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,PlanningApi

	When requesting an authorization code
	| scope              | response_type | client_id  | redirect_uri     | prompt | state  |
	| openid PlanningApi | code          | MyHolidays | http://localhost | none   | state1 |


	Then HTTP status code is 301
	And redirect to callback http://localhost
	And the state state1 is returned in the callback

# ERRORS
Scenario: A resource owner is not authenticated but we want to directly retrieve the authorization code into the callback
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

Scenario: a resource owner is authenticated and we want to retrieve the authorization code into the callback without his consent
	Given a mobile application MyHolidays is defined
	And scopes openid,PlanningApi are defined
	And the scopes openid,PlanningApi are assigned to the client MyHolidays
	And a resource owner is authenticated
	| UserId               | UserName |
	| habarthierry@loki.be | thabart  |

	When requesting an authorization code
	| scope              | response_type | client_id  | redirect_uri     | prompt | state  |
	| openid PlanningApi | code          | MyHolidays | http://localhost | none   | state1 |

	Then HTTP status code is 400
	And the error returned is
	| error                | state  |
	| interaction_required | state1 |