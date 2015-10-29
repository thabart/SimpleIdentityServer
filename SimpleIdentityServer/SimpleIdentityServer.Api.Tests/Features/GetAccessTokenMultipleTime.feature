Feature: GetAccessTokenMultipleTime
	As an authenticated user
	I request several times an access token

Scenario: Request 3 times an access token
	Given a resource owner with username thierry and password loki is defined
	And a mobile application MyHolidays is defined
	And allowed number of requests is 2
	And sliding time is 0.2

	When requesting access tokens
	| client_id  | username | password |
	| MyHolidays | thierry  | loki     |
	| MyHolidays | thierry  | loki     |
	| MyHolidays | thierry  | loki     |

	Then 2 access tokens are generated
	And the errors should be returned
	| Message                          |
	| Allow 2 requests per 0.2 minutes |

	And the http responses should be returned
	| StatusCode | NumberOfRemainingRequests | NumberOfRequests |
	| 200        | 1                         | 2                |
	| 200        | 0                         | 2                |
	| 429        | 0                         | 2                |

Scenario: Request 5 times an access token
	Given a resource owner with username thierry and password loki is defined
	And a mobile application MyHolidays is defined
	And allowed number of requests is 2
	And sliding time is 0.2

	When requesting access tokens
	| client_id  | username | password |
	| MyHolidays | thierry  | loki     |
	| MyHolidays | thierry  | loki     |
	| MyHolidays | thierry  | loki     |
	| MyHolidays | thierry  | loki     |
	| MyHolidays | thierry  | loki     |

	Then 2 access tokens are generated
	And the errors should be returned
	| Message                          |
	| Allow 2 requests per 0.2 minutes |
	| Allow 2 requests per 0.2 minutes |
	| Allow 2 requests per 0.2 minutes |
	
	And the http responses should be returned
	| StatusCode | NumberOfRemainingRequests | NumberOfRequests |
	| 200        | 1                         | 2                |
	| 200        | 0                         | 2                |
	| 429        | 0                         | 2                |
	| 429        | 0                         | 2                |
	| 429        | 0                         | 2                |

Scenario: Request 3 times an access token wait for 3 seconds and request 2 access tokens
	Given a resource owner with username thierry and password loki is defined
	And a mobile application MyHolidays is defined
	And allowed number of requests is 2
	And sliding time is 0.2

	When requesting access tokens
	| client_id  | username | password |
	| MyHolidays | thierry  | loki     |
	| MyHolidays | thierry  | loki     |
	| MyHolidays | thierry  | loki     |
	And waiting for 3000 seconds
	And requesting access tokens
	| client_id  | username | password |
	| MyHolidays | thierry  | loki     |
	| MyHolidays | thierry  | loki     |

	Then 4 access tokens are generated
	
	And the errors should be returned
	| Message                          |
	| Allow 2 requests per 0.2 minutes |
	
	And the http responses should be returned
	| StatusCode | NumberOfRemainingRequests | NumberOfRequests |
	| 200        | 1                         | 2                |
	| 200        | 0                         | 2                |
	| 429        | 0                         | 2                |
	| 200        | 1                         | 2                |
	| 200        | 0                         | 2                |