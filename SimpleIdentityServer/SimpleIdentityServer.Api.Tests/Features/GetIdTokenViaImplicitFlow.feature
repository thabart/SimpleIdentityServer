Feature: GetIdTokenViaImplicitWorkflow
	As a known client
	I want to use the implicit workflow to retrieve the id token or access token

# HAPPY PATHS
Scenario: Get the id token
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name        | IsInternal |
	| openid      | true       |
	| PlanningApi | false      |

	And the id_token signature algorithm is set to none for the client MyHolidays
	And the scopes openid,PlanningApi are assigned to the client MyHolidays
	And the grant-type implicit is supported by the client MyHolidays
	And the response-types id_token are supported by the client MyHolidays
	And create a resource owner
	| Id                   | Name    |
	| habarthierry@loki.be | thabart |
	And authenticate the resource owner
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,PlanningApi
	
	When requesting an authorization
	| scope              | response_type | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid PlanningApi | id_token      | MyHolidays | http://localhost | none   | state1 | parameterNonce |

	Then the http status code is 301
	And decrypt the id_token parameter from the query string
	And the protected JWS header is returned
	| Alg  |
	| none |
	And the audience parameter with value MyHolidays is returned by the JWS payload
	And the parameter nonce with value parameterNonce is returned by the JWS payload
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload

Scenario: Get the id token and access token via implicit workflow
	Given a mobile application MyHolidays is defined	
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name        | IsInternal |
	| openid      | true       |
	| PlanningApi | false      |
	And the id_token signature algorithm is set to none for the client MyHolidays
	And the scopes openid,PlanningApi are assigned to the client MyHolidays
	And the grant-type implicit is supported by the client MyHolidays
	And the response-types id_token,token are supported by the client MyHolidays
	And create a resource owner
	| Id                   | Name    |
	| habarthierry@loki.be | thabart |
	And authenticate the resource owner
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,PlanningApi
	
	When requesting an authorization
	| scope              | response_type  | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid PlanningApi | id_token token | MyHolidays | http://localhost | none   | state1 | parameterNonce |

	
	Then the http status code is 301
	And decrypt the id_token parameter from the query string
	And the protected JWS header is returned
	| Alg  |
	| none |
	And the audience parameter with value MyHolidays is returned by the JWS payload
	And the parameter nonce with value parameterNonce is returned by the JWS payload
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload

	And the callback contains the following query name access_token

Scenario: Get an id token and check if the claims returned in the token are correct	
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name    | IsInternal | Claims |
	| openid  | true       |        |
	| profile | true       | name   |

	And the id_token signature algorithm is set to none for the client MyHolidays
	And the scopes openid,profile are assigned to the client MyHolidays
	And the grant-type implicit is supported by the client MyHolidays
	And the response-types id_token are supported by the client MyHolidays
	And create a resource owner
	| Id                   | Name    |
	| habarthierry@loki.be | thabart |
	And authenticate the resource owner
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,profile
	
	When requesting an authorization
	| scope          | response_type | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid profile | id_token      | MyHolidays | http://localhost | none   | state1 | parameterNonce |

	Then the http status code is 301
	And decrypt the id_token parameter from the query string
	And the protected JWS header is returned
	| Alg  |
	| none |
	And the audience parameter with value MyHolidays is returned by the JWS payload
	And the parameter nonce with value parameterNonce is returned by the JWS payload
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload
	And the claim name with value thabart is returned by the JWS payload
		
# DIFFERENT SCENARIOS TO CHECK THE SIGNATURE & ENCRYPTION
Scenario: Get a signed id_token
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And add json web keys
	| Kid | Alg   | Operation | Kty | Use |
	| 1   | RS256 | Sign      | RSA | Sig |

	And the scopes are defined
	| Name        | IsInternal |
	| openid      | true       |
	| PlanningApi | false      |

	And the id_token signature algorithm is set to RS256 for the client MyHolidays
	And the scopes openid,PlanningApi are assigned to the client MyHolidays
	And the grant-type implicit is supported by the client MyHolidays
	And the response-types id_token are supported by the client MyHolidays
	And create a resource owner
	| Id                   | Name    |
	| habarthierry@loki.be | thabart |
	And authenticate the resource owner
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,PlanningApi
	
	When requesting an authorization
	| scope              | response_type | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid PlanningApi | id_token      | MyHolidays | http://localhost | none   | state1 | parameterNonce |

	Then the http status code is 301
	And decrypt the id_token parameter from the query string
	And check the signature is correct with the kid 1
	And the protected JWS header is returned
	| Alg   |
	| RS256 |
	And the audience parameter with value MyHolidays is returned by the JWS payload
	And the parameter nonce with value parameterNonce is returned by the JWS payload
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload
	And the signature of the JWS payload is valid

Scenario: Get an encrypted id token and check if the claims returned in the token are correct	
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And add json web keys
	| Kid | Alg    | Operation | Kty | Use |
	| 1   | RSA1_5 | Encrypt   | RSA | Enc |

	And the scopes are defined
	| Name    | IsInternal | Claims |
	| openid  | true       |        |
	| profile | true       | name   |

	And the id_token signature algorithm is set to none for the client MyHolidays
	And the id_token encrypted response alg is set to RSA1_5 for the client MyHolidays
	And the id_token encrypted response enc is set to A128CBC-HS256 for the client MyHolidays
	And the scopes openid,profile are assigned to the client MyHolidays
	And the grant-type implicit is supported by the client MyHolidays
	And the response-types id_token are supported by the client MyHolidays
	And create a resource owner
	| Id                   | Name    |
	| habarthierry@loki.be | thabart |
	And authenticate the resource owner
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,profile
	
	When requesting an authorization
	| scope          | response_type | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid profile | id_token      | MyHolidays | http://localhost | none   | state1 | parameterNonce |

	Then the http status code is 301
	And decrypt the jwe parameter from the query string with the following kid 1
	And the protected JWS header is returned
	| Alg  |
	| none |
	And the audience parameter with value MyHolidays is returned by the JWS payload
	And the parameter nonce with value parameterNonce is returned by the JWS payload
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload
	And the claim name with value thabart is returned by the JWS payload

Scenario: Get a signed and encrypted id token and check if the claims returned in the token are correct
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And add json web keys
	| Kid | Alg    | Operation | Kty | Use |
	| 1   | RSA1_5 | Encrypt   | RSA | Enc |
	| 2   | RS256  | Sign      | RSA | Sig |

	And the scopes are defined
	| Name    | IsInternal | Claims |
	| openid  | true       |        |
	| profile | true       | name   |

	And the id_token signature algorithm is set to RS256 for the client MyHolidays
	And the id_token encrypted response alg is set to RSA1_5 for the client MyHolidays
	And the id_token encrypted response enc is set to A128CBC-HS256 for the client MyHolidays
	And the scopes openid,profile are assigned to the client MyHolidays
	And the grant-type implicit is supported by the client MyHolidays
	And the response-types id_token are supported by the client MyHolidays
	And create a resource owner
	| Id                   | Name    |
	| habarthierry@loki.be | thabart |
	And authenticate the resource owner
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,profile
	
	When requesting an authorization
	| scope          | response_type | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid profile | id_token      | MyHolidays | http://localhost | none   | state1 | parameterNonce |

	Then the http status code is 301
	And decrypt the jwe parameter from the query string with the following kid 1
	And check the signature is correct with the kid 2
	And the protected JWS header is returned
	| Alg   |
	| RS256 |
	And the audience parameter with value MyHolidays is returned by the JWS payload
	And the parameter nonce with value parameterNonce is returned by the JWS payload
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload
	And the claim name with value thabart is returned by the JWS payload