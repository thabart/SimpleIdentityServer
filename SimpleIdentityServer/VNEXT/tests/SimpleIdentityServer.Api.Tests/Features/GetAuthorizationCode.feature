Feature: GetAuthorizationCode
	As a resource owner and user of the client
	I should be able to retrieve an authorization code


# HAPPY PATH
Scenario: Whether the resource owner is authenticated or not we want to re-authenticate him
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name        | IsInternal |
	| PlanningApi | true       |
	| openid      | true       |
	And the scopes openid,PlanningApi are assigned to the client MyHolidays

	When requesting an authorization code
	| scope              | response_type | client_id  | redirect_uri     | prompt |
	| openid PlanningApi | code          | MyHolidays | http://localhost | login  |


	Then HTTP status code is 301
	And redirect to /Authenticate controller

Scenario: A resource owner is authenticated and we want to display only the consent screen
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name        | IsInternal |
	| PlanningApi | true       |
	| openid      | true       |
	And the scopes openid,PlanningApi are assigned to the client MyHolidays
	And create a resource owner
	| Id                   | Name    |
	| habarthierry@loki.be | thabart |
	And authenticate the resource owner

	When requesting an authorization code
	| scope              | response_type | client_id  | redirect_uri     | prompt  |
	| openid PlanningApi | code          | MyHolidays | http://localhost | consent |


	Then HTTP status code is 301
	And redirect to /Consent controller

Scenario: A resource owner is not authenticated and we want to display only the consent screen
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name        | IsInternal |
	| PlanningApi | true       |
	| openid      | true       |
	And the scopes openid,PlanningApi are assigned to the client MyHolidays

	When requesting an authorization code
	| scope              | response_type | client_id  | redirect_uri     | prompt  |
	| openid PlanningApi | code          | MyHolidays | http://localhost | consent |


	Then HTTP status code is 301
	And redirect to /Authenticate controller

Scenario: A resource owner is authenticated and he already has given his consent. We want to retrieve an authorization code for his consent
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name        | IsInternal |
	| PlanningApi | true       |
	| openid      | true       |
	And the scopes openid,PlanningApi are assigned to the client MyHolidays
	And create a resource owner
	| Id                   | Name    |
	| habarthierry@loki.be | thabart |
	And authenticate the resource owner
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,PlanningApi

	When requesting an authorization code
	| scope              | response_type | client_id  | redirect_uri     | prompt | state  |
	| openid PlanningApi | code          | MyHolidays | http://localhost | none   | state1 |


	Then HTTP status code is 301
	And redirect to callback http://localhost
	And the query string state with value state1 is returned
	And the query string code exists

# TEST THE DIFFERENT RESPONSE_MODE PARAMETER VALUES
Scenario: A resource owner is authenticated and he already has given his consent. We want to retrieve an authorization code in the fragment
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name        | IsInternal |
	| PlanningApi | true       |
	| openid      | true       |
	And the scopes openid,PlanningApi are assigned to the client MyHolidays
	And create a resource owner
	| Id                   | Name    |
	| habarthierry@loki.be | thabart |
	And authenticate the resource owner
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,PlanningApi

	When requesting an authorization code
	| scope              | response_type | client_id  | redirect_uri     | prompt | state  | response_mode |
	| openid PlanningApi | code          | MyHolidays | http://localhost | none   | state1 | fragment      |


	Then HTTP status code is 301
	And redirect to callback http://localhost
	And the fragment contains the query state with the value state1
	And the fragment contains the query string code

Scenario: A resource owner is authenticated and he already has given his consent. We want to retrieve an authorization code in the post
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name        | IsInternal |
	| PlanningApi | true       |
	| openid      | true       |
	And the scopes openid,PlanningApi are assigned to the client MyHolidays
	And create a resource owner
	| Id                   | Name    |
	| habarthierry@loki.be | thabart |
	And authenticate the resource owner
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,PlanningApi

	When requesting an authorization code
	| scope              | response_type | client_id  | redirect_uri     | prompt | state  | response_mode |
	| openid PlanningApi | code          | MyHolidays | http://localhost | none   | state1 | form_post     |


	Then HTTP status code is 301
	And redirect to callback http://localhost/Form
		
# THE PROMPT PARAMETER IS NOT SPECIFIED
Scenario: a resource owner is not authenticated. We want to retrieve an authorization code and the prompt parameter value is not specified
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name        | IsInternal |
	| PlanningApi | true       |
	| openid      | true       |
	And the scopes openid,PlanningApi are assigned to the client MyHolidays
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,PlanningApi

	When requesting an authorization code
	| scope              | response_type | client_id  | redirect_uri     | state  |
	| openid PlanningApi | code          | MyHolidays | http://localhost | state1 |
	
	
	Then HTTP status code is 301
	And redirect to /Authenticate controller

Scenario: a resource owner is authenticated. We want to retrieve an authorization code and the prompt parameter value is not specified
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name        | IsInternal |
	| PlanningApi | true       |
	| openid      | true       |
	And the scopes openid,PlanningApi are assigned to the client MyHolidays
	And create a resource owner
	| Id                   | Name    |
	| habarthierry@loki.be | thabart |
	And authenticate the resource owner

	When requesting an authorization code
	| scope              | response_type | client_id  | redirect_uri     | state  |
	| openid PlanningApi | code          | MyHolidays | http://localhost | state1 |
	
	
	Then HTTP status code is 301
	And redirect to /Consent controller

# ERRORS
Scenario: A resource owner is not authenticated but we want to directly retrieve the authorization code into the callback
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name        | IsInternal |
	| PlanningApi | true       |
	| openid      | true       |
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
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name        | IsInternal |
	| PlanningApi | true       |
	| openid      | true       |
	And the scopes openid,PlanningApi are assigned to the client MyHolidays
	And create a resource owner
	| Id                   | Name    |
	| habarthierry@loki.be | thabart |
	And authenticate the resource owner

	When requesting an authorization code
	| scope              | response_type | client_id  | redirect_uri     | prompt | state  |
	| openid PlanningApi | code          | MyHolidays | http://localhost | none   | state1 |

	Then HTTP status code is 400
	And the error returned is
	| error                | state  |
	| interaction_required | state1 |

Scenario: a resource owner is not authenticated and we want to retrieve an authorization code by passing a malformed redirection_uri
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name        | IsInternal |
	| PlanningApi | true       |
	| openid      | true       |
	And the scopes openid,PlanningApi are assigned to the client MyHolidays

	When requesting an authorization code
	| scope              | response_type | client_id  | redirect_uri        | prompt | state  |
	| openid PlanningApi | code          | MyHolidays | localhost?invalid+2 | none   | state1 |

	Then HTTP status code is 400
	And the error returned is
	| error           | state  |
	| invalid_request | state1 |

Scenario: a resource owner is not authenticated and we want to retrieve an authorization code with prompt equal to none and login
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name        | IsInternal |
	| PlanningApi | true       |
	| openid      | true       |
	And the scopes openid,PlanningApi are assigned to the client MyHolidays

	When requesting an authorization code
	| scope              | response_type | client_id  | redirect_uri     | prompt     | state  |
	| openid PlanningApi | code          | MyHolidays | http://localhost | none login | state1 |

	Then HTTP status code is 400
	And the error returned is
	| error           | state  |
	| invalid_request | state1 |