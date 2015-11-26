Feature: GetUserInformation
	A resource owner is authenticated
	A user is trying to fetch the resource owner information from the user info endpoint.

# HAPPY PATHS

Scenario: Fetch the user information for the scope profile
	Given a mobile application MyHolidays is defined
	And set the name of the issuer http://localhost/identity
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name    | IsInternal | Claims                                                                                                                                  |
	| openid  | true       |                                                                                                                                         |
	| profile | true       | name family_name given_name middle_name nickname preferred_username profile picture website gender birthdate zoneinfo locale updated_at |
	And the scopes openid,profile are assigned to the client MyHolidays
	And the grant-type implicit is supported by the client MyHolidays
	And the response-types token,id_token are supported by the client MyHolidays
	And create a resource owner
	| Id                   | Name    | GivenName | FamilyName | MiddleName | NickName | PreferredUserName | Profile | Picture | WebSite | Email | EmailVerified | Gender | BirthDate  | ZoneInfo | Locale | PhoneNumber | PhoneNumberVerified |
	| habarthierry@loki.be | thabart | givename  | familyname | middlename | nickname | preferredusername | profile | picture | website | email | true          | M      | 1989-10-07 | fr       | fr     | 00          | true                |
	And the following address is assigned to the resource owner
	| Formatted | StreetAddress | Locality | Region | PostalCode | Country |
	| formatted | streetaddress | locality | region | postalcode | country |
	And authenticate the resource owner
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,profile
	And requesting an access token	
	| scope          | response_type  | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid profile | token id_token | MyHolidays | http://localhost | none   | state1 | parameterNonce |
	
	When requesting user information

	Then HTTP status code is 200
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload
	And the claim iss with value http://localhost/identity is returned by the JWS payload
	And the claim name with value thabart is returned by the JWS payload
	And the claim family_name with value familyname is returned by the JWS payload
	And the claim given_name with value givename is returned by the JWS payload
	And the claim middle_name with value middlename is returned by the JWS payload
	And the claim nickname with value nickname is returned by the JWS payload
	And the claim preferred_username with value preferredusername is returned by the JWS payload
	And the claim profile with value profile is returned by the JWS payload
	And the claim picture with value picture is returned by the JWS payload
	And the claim website with value website is returned by the JWS payload
	And the claim gender with value M is returned by the JWS payload
	And the claim birthdate with value 1989-10-07 is returned by the JWS payload
	And the claim zoneinfo with value fr is returned by the JWS payload
	And the claim locale with value fr is returned by the JWS payload
	
Scenario: Fetch the user information for the scope address
	Given a mobile application MyHolidays is defined
	And set the name of the issuer http://localhost/identity
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name    | IsInternal | Claims  |
	| openid  | true       |         |
	| address | true       | address |
	And the scopes openid,address are assigned to the client MyHolidays
	And the grant-type implicit is supported by the client MyHolidays
	And the response-types token,id_token are supported by the client MyHolidays
	And create a resource owner
	| Id                   | Name    |
	| habarthierry@loki.be | thabart |
	And the following address is assigned to the resource owner
	| Formatted | StreetAddress | Locality | Region | PostalCode | Country |
	| formatted | streetaddress | locality | region | postalcode | country |
	And authenticate the resource owner
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,address
	And requesting an access token	
	| scope          | response_type  | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid address | token id_token | MyHolidays | http://localhost | none   | state1 | parameterNonce |
	
	When requesting user information

	Then HTTP status code is 200
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload
	And the claim iss with value http://localhost/identity is returned by the JWS payload
	And the returned address is
	| Formatted | StreetAddress | Locality | Region | PostalCode | Country |
	| formatted | streetaddress | locality | region | postalcode | country |
	
Scenario: Fetch the user information for the scope email
	Given a mobile application MyHolidays is defined
	And set the name of the issuer http://localhost/identity
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name   | IsInternal | Claims               |
	| openid | true       |                      |
	| email  | true       | email email_verified |
	And the scopes openid,email are assigned to the client MyHolidays
	And the grant-type implicit is supported by the client MyHolidays
	And the response-types token,id_token are supported by the client MyHolidays
	And create a resource owner
	| Id                   | Name    | Email                   | EmailVerified |
	| habarthierry@loki.be | thabart | habarthierry@hotmail.fr | true          |
	And authenticate the resource owner
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,email
	And requesting an access token	
	| scope        | response_type  | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid email | token id_token | MyHolidays | http://localhost | none   | state1 | parameterNonce |
	
	When requesting user information

	Then HTTP status code is 200
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload
	And the claim iss with value http://localhost/identity is returned by the JWS payload
	And the claim email with value habarthierry@hotmail.fr is returned by the JWS payload
	And the claim email_verified with value True is returned by the JWS payload

Scenario: Fetch the user information for the scope phone
	Given a mobile application MyHolidays is defined
	And set the name of the issuer http://localhost/identity
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name   | IsInternal | Claims                             |
	| openid | true       |                                    |
	| phone  | true       | phone_number phone_number_verified |
	And the scopes openid,phone are assigned to the client MyHolidays
	And the grant-type implicit is supported by the client MyHolidays
	And the response-types token,id_token are supported by the client MyHolidays
	And create a resource owner
	| Id                   | Name    | PhoneNumber | PhoneNumberVerified |
	| habarthierry@loki.be | thabart | 007         | false               |
	And authenticate the resource owner
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,phone
	And requesting an access token	
	| scope        | response_type  | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid phone | token id_token | MyHolidays | http://localhost | none   | state1 | parameterNonce |
	
	When requesting user information

	Then HTTP status code is 200
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload
	And the claim iss with value http://localhost/identity is returned by the JWS payload
	And the claim phone_number with value 007 is returned by the JWS payload
	And the claim phone_number_verified with value False is returned by the JWS payload
		
Scenario: Fetch the user information for the scope phone and email
	Given a mobile application MyHolidays is defined
	And set the name of the issuer http://localhost/identity
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name   | IsInternal | Claims                             |
	| openid | true       |                                    |
	| phone  | true       | phone_number phone_number_verified |
	| email  | true       | email email_verified               |
	And the scopes openid,phone,email are assigned to the client MyHolidays
	And the grant-type implicit is supported by the client MyHolidays
	And the response-types token,id_token are supported by the client MyHolidays
	And create a resource owner
	| Id                   | Name    | PhoneNumber | PhoneNumberVerified | Email                   | EmailVerified |
	| habarthierry@loki.be | thabart | 007         | false               | habarthierry@hotmail.fr | true          |
	And authenticate the resource owner
	And the consent has been given by the resource owner habarthierry@loki.be for the client MyHolidays and scopes openid,phone,email
	And requesting an access token	
	| scope              | response_type  | client_id  | redirect_uri     | prompt | state  | nonce          |
	| openid phone email | token id_token | MyHolidays | http://localhost | none   | state1 | parameterNonce |
	
	When requesting user information

	Then HTTP status code is 200
	And the claim sub with value habarthierry@loki.be is returned by the JWS payload
	And the claim iss with value http://localhost/identity is returned by the JWS payload
	And the claim phone_number with value 007 is returned by the JWS payload
	And the claim phone_number_verified with value False is returned by the JWS payload
	And the claim email with value habarthierry@hotmail.fr is returned by the JWS payload
	And the claim email_verified with value True is returned by the JWS payload