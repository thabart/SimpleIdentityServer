Feature: GetAccessTokenWithResourceOwnerGrantType
	As a resource owner and user of the client
	I should be able to retrieve an access token with my credentials

# HAPPY PATHS
Scenario: Retrieve an access token without defining scopes
	Given a resource owner with username thierry and password loki is defined
	And a mobile application MyHolidays is defined

	When requesting an access token via resource owner grant-type
	| client_id  | username | password |
	| MyHolidays | thierry  | loki     |

	Then http result is 200
	And access token is generated

Scenario: Retrieve an access token with two scopes
	Given a resource owner with username thierry and password loki is defined
	And a mobile application MyHolidays is defined	
	And the scopes are defined
	| Name   | IsInternal |
	| roles  | true       |
	| openid | true       |

	And the scopes roles,openid are assigned to the client MyHolidays

	When requesting an access token via resource owner grant-type
	| client_id  | username | password | scope        |
	| MyHolidays | thierry  | loki     | roles openid |

	Then http result is 200
	And access token is generated
	And access token have the correct scopes : roles,roles

# EXCEPTION SCENARIOS
Scenario: Retrieve an access token with missing username
	Given a resource owner with username thierry and password loki is defined
	And a mobile application MyHolidays is defined
	And the scopes are defined
	| Name   | IsInternal |
	| roles  | true       |
	| openid | true       |

	And the scopes roles,openid are assigned to the client MyHolidays

	When requesting an access token via resource owner grant-type
	| client_id  | password | scope        |
	| MyHolidays | loki     | roles openid |

	Then http result is 400
	And the error is invalid_request

Scenario: Retrieve an access token with missing client id
	Given a resource owner with username thierry and password loki is defined
	And a mobile application MyHolidays is defined
	And the scopes are defined
	| Name   | IsInternal |
	| roles  | true       |
	| openid | true       |

	And the scopes roles,openid are assigned to the client MyHolidays

	When requesting an access token via resource owner grant-type
	| username | password | scope        |
	| thierry  | loki     | roles openid |

	Then http result is 400
	And the error is invalid_request

Scenario: Retrieve an access token with none existing scope
	Given a resource owner with username thierry and password loki is defined
	And a mobile application MyHolidays is defined	
	And the scopes are defined
	| Name   | IsInternal |
	| roles  | true       |
	| openid | true       |
	And the scopes roles,openid are assigned to the client MyHolidays

	When requesting an access token via resource owner grant-type
	| client_id  | username | password | scope                |
	| MyHolidays | thierry  | loki     | roles openid profile |

	Then http result is 400
	And the error is invalid_scope

Scenario: Retrieve an access token with a scope not allowed
	Given a resource owner with username thierry and password loki is defined
	And a mobile application MyHolidays is defined
	And the scopes are defined
	| Name   | IsInternal |
	| roles  | true       |
	| openid | true       |

	And the scopes roles are assigned to the client MyHolidays

	When requesting an access token via resource owner grant-type
	| client_id  | username | password | scope        |
	| MyHolidays | thierry  | loki     | roles openid |

	Then http result is 400
	And the error is invalid_scope

Scenario: Retrieve an access token with duplicate scopes
	Given a resource owner with username thierry and password loki is defined
	And a mobile application MyHolidays is defined
	And the scopes are defined
	| Name   | IsInternal |
	| roles  | true       |
	| openid | true       |
	And the scopes roles are assigned to the client MyHolidays

	When requesting an access token via resource owner grant-type
	| client_id  | username | password | scope       |
	| MyHolidays | thierry  | loki     | roles roles |

	Then http result is 400
	And the error is invalid_request

Scenario: Retrieve an access token for a none existing client_id
	Given a resource owner with username thierry and password loki is defined
	And a mobile application MyHolidays is defined
	And the scopes are defined
	| Name   | IsInternal |
	| roles  | true       |
	| openid | true       |
	And the scopes roles,openid are assigned to the client MyHolidays

	When requesting an access token via resource owner grant-type
	| client_id        | username | password | scope        |
	| ClientNotAllowed | thierry  | loki     | roles openid |

	Then http result is 400
	And the error is invalid_client

Scenario: Retrieve an access token with not valid credentials
	Given a resource owner with username thierry and password loki is defined
	And a mobile application MyHolidays is defined
	And the scopes are defined
	| Name   | IsInternal |
	| roles  | true       |
	| openid | true       |
	And the scopes roles,openid are assigned to the client MyHolidays

	When requesting an access token via resource owner grant-type
	| client_id  | username | password | scope        |
	| MyHolidays | thierry  | notvalid | roles openid |

	Then http result is 400
	And the error is invalid_grant

