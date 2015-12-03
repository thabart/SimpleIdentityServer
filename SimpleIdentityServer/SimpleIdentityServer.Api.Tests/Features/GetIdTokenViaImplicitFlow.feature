Feature: GetIdTokenViaImplicitWorkflow
	As a known client
	I want to use the implicit workflow to retrieve the id token or access token

# GENERATE THE IDENTITY TOKEN VIA SCOPES
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
	And create an authorization request
	| scope              | response_type | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid PlanningApi | id_token      | MyHolidays | http://localhost | none   | state1 | parameterNonce |
	
	When requesting an authorization

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
	And create an authorization request
	| scope              | response_type  | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid PlanningApi | id_token token | MyHolidays | http://localhost | none   | state1 | parameterNonce |
	
	When requesting an authorization
	
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
	And create an authorization request
	| scope          | response_type | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid profile | id_token      | MyHolidays | http://localhost | none   | state1 | parameterNonce |
	
	When requesting an authorization

	Then the http status code is 301
	And decrypt the id_token parameter from the query string
	And the protected JWS header is returned
	| Alg  |
	| none |
	And the audience parameter with value MyHolidays is returned by the JWS payload
	And the parameter nonce with value parameterNonce is returned by the JWS payload
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload
	And the claim name with value thabart is returned by the JWS payload
		
# USE THE CLAIMS PARAMETER TO GENERATE THE IDENTITY TOKEN
Scenario: Get an identity token by using the claims parameter: {id_token : { name: { essential : 'true' }}}
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name   | IsInternal |
	| openid | true       |

	And the id_token signature algorithm is set to none for the client MyHolidays
	And the scopes openid are assigned to the client MyHolidays
	And the response-types id_token are supported by the client MyHolidays
	And create a resource owner
	| Id                   | Name    |
	| habarthierry@loki.be | thabart |
	And authenticate the resource owner	
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and claims name
	And create an authorization request
	| scope  | response_type | client_id  | redirect_uri     | prompt | state  | nonce          | claims                                                                    |
	| openid | id_token      | MyHolidays | http://localhost | none   | state1 | parameterNonce | %7B%22id_token%22%3A+%7B%22name%22%3A+%7B%22essential%22%3A+true%7D%7D%7D |
		
	When requesting an authorization
		
	Then the http status code is 301
	And decrypt the id_token parameter from the query string
	And the protected JWS header is returned
	| Alg  |
	| none |
	And the JWS payload contains 2 claims
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload	
	And the claim name with value thabart is returned by the JWS payload	

Scenario: Get an identity token by using the claims parameter : {id_token : { "name" : { essential : 'true' }, "email" : { essential : 'true' }}}
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name   | IsInternal |
	| openid | true       |

	And the id_token signature algorithm is set to none for the client MyHolidays
	And the scopes openid are assigned to the client MyHolidays
	And the response-types id_token are supported by the client MyHolidays
	And create a resource owner
	| Id                   | Name    | Email                   |
	| habarthierry@loki.be | thabart | habarthierry@hotmail.fr |
	And authenticate the resource owner	
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and claims name,email
	And create an authorization request
	| scope  | response_type | client_id  | redirect_uri     | prompt | state  | nonce          | claims                                                                                                                     |
	| openid | id_token      | MyHolidays | http://localhost | none   | state1 | parameterNonce | %7B%22id_token%22%3A+%7B%22name%22%3A+%7B%22essential%22%3A+true%7D+%2C+%22email%22%3A+%7B%22essential%22%3A+true%7D%7D%7D |
		
	When requesting an authorization
		
	Then the http status code is 301
	And decrypt the id_token parameter from the query string
	And the protected JWS header is returned
	| Alg  |
	| none |
	And the JWS payload contains 3 claims
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload	
	And the claim name with value thabart is returned by the JWS payload		
	And the claim email with value habarthierry@hotmail.fr is returned by the JWS payload

Scenario: Get an identity token by using the claims parameter : {id_token : { name : { value : 'fake' }}}
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name   | IsInternal |
	| openid | true       |

	And the id_token signature algorithm is set to none for the client MyHolidays
	And the scopes openid are assigned to the client MyHolidays
	And the response-types id_token are supported by the client MyHolidays
	And create a resource owner
	| Id                   | Name    |
	| habarthierry@loki.be | thabart |
	And authenticate the resource owner	
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and claims name
	And create an authorization request
	| scope  | response_type | client_id  | redirect_uri     | prompt | state  | nonce          | claims                                                                      |
	| openid | id_token      | MyHolidays | http://localhost | none   | state1 | parameterNonce | %7B%22id_token%22%3A+%7B%22name%22%3A+%7B%22value%22%3A+%22fake%22%7D%7D%7D |
		
	When requesting an authorization
		
	Then the http status code is 400
	And the error code is invalid_grant

Scenario: Get an identity token by using the claims parameter : {id_token : { name : { value : 'thabart' }, email : 'fake'}}
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name   | IsInternal |
	| openid | true       |

	And the id_token signature algorithm is set to none for the client MyHolidays
	And the scopes openid are assigned to the client MyHolidays
	And the response-types id_token are supported by the client MyHolidays
	And create a resource owner
	| Id                   | Name    |
	| habarthierry@loki.be | thabart |
	And authenticate the resource owner	
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and claims name,email
	And create an authorization request
	| scope  | response_type | client_id  | redirect_uri     | prompt | state  | nonce          | claims                                                                                                                            |
	| openid | id_token      | MyHolidays | http://localhost | none   | state1 | parameterNonce | %7B%22id_token%22%3A+%7B%22name%22%3A+%7B%22value%22%3A+%22thabart%22%7D+%2C+%22email%22%3A+%7B%22value%22%3A+%22fake%22%7D%7D%7D |
		
	When requesting an authorization
		
	Then the http status code is 400
	And the error code is invalid_grant

Scenario: Get an identity token by using the claims parameter : {id_token : { name : { value : 'thabart' }, email : 'fake'}}. The resource owner gives his consent only for the claim email
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name   | IsInternal |
	| openid | true       |

	And the id_token signature algorithm is set to none for the client MyHolidays
	And the scopes openid are assigned to the client MyHolidays
	And the response-types id_token are supported by the client MyHolidays
	And create a resource owner
	| Id                   | Name    |
	| habarthierry@loki.be | thabart |
	And authenticate the resource owner	
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and claims name
	And create an authorization request
	| scope  | response_type | client_id  | redirect_uri     | prompt | state  | nonce          | claims                                                                                                                            |
	| openid | id_token      | MyHolidays | http://localhost | none   | state1 | parameterNonce | %7B%22id_token%22%3A+%7B%22name%22%3A+%7B%22value%22%3A+%22thabart%22%7D+%2C+%22email%22%3A+%7B%22value%22%3A+%22fake%22%7D%7D%7D |
		
	When requesting an authorization
		
	Then the http status code is 400
	And the error code is interaction_required

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
	And create an authorization request
	| scope              | response_type | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid PlanningApi | id_token      | MyHolidays | http://localhost | none   | state1 | parameterNonce |
	
	When requesting an authorization

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
	And create an authorization request
	| scope          | response_type | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid profile | id_token      | MyHolidays | http://localhost | none   | state1 | parameterNonce |
	
	When requesting an authorization

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
	And create an authorization request
	| scope          | response_type | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid profile | id_token      | MyHolidays | http://localhost | none   | state1 | parameterNonce |
	
	When requesting an authorization

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

# USE THE REQUEST PARAMETER
Scenario: Get an identity token by setting the request parameter with signed authorization request
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And add json web keys
	| Kid | Alg   | Operation | Kty | Use |
	| 1   | RS256 | Sign      | RSA | Sig |

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
	And create an authorization request
	| scope              | response_type | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid PlanningApi | id_token      | MyHolidays | http://localhost | none   | state1 | parameterNonce |
	And sign the authorization request with 1 kid and algorithm RS256
	
	When requesting an authorization

	Then the http status code is 301
	And decrypt the id_token parameter from the query string
	And the protected JWS header is returned
	| Alg  |
	| none |
	And the audience parameter with value MyHolidays is returned by the JWS payload
	And the parameter nonce with value parameterNonce is returned by the JWS payload
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload

Scenario: Get an identity token by setting the request parameter with signed and encrypted authorization request
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And add json web keys
	| Kid | Alg    | Operation | Kty | Use |
	| 1   | RS256  | Sign      | RSA | Sig |
	| 2   | RSA1_5 | Encrypt   | RSA | Enc |

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
	And create an authorization request
	| scope              | response_type | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid PlanningApi | id_token      | MyHolidays | http://localhost | none   | state1 | parameterNonce |
	And sign the authorization request with 1 kid and algorithm RS256
	And encrypt the authorization request with 2 kid, JweAlg: RSA1_5 and JweEnc: A128CBC_HS256
	
	When requesting an authorization

	Then the http status code is 301
	And decrypt the id_token parameter from the query string
	And the protected JWS header is returned
	| Alg  |
	| none |
	And the audience parameter with value MyHolidays is returned by the JWS payload
	And the parameter nonce with value parameterNonce is returned by the JWS payload
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload

# USE THE REQUEST_URI