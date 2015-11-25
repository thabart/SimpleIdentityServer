Feature: GetUserInformation
	A resource owner is authenticated
	A user is trying to fetch the resource owner information from the user info endpoint.

Scenario: Fetch the user information for the scope profile
	Given a mobile application MyHolidays is defined
	And the redirection uri http://localhost is assigned to the client MyHolidays
	And the scopes are defined
	| Name    | IsInternal | Claims                                                                                                                                  |
	| openid  | true       |                                                                                                                                         |
	| profile | true       | name family_name given_name middle_name nickname preferred_username profile picture website gender birthdate zoneinfo locale updated_at |
	And the scopes openid,profile are assigned to the client MyHolidays
	And the grant-type implicit is supported by the client MyHolidays
	And the response-types token,id_token are supported by the client MyHolidays
	And create a resource owner
	| Id                   | Name    | GivenName | FamilyName | NickName | PreferredUserName | Profile | Picture | WebSite | Email | EmailVerified | Gender | BirthDate  | ZoneInfo | Locale | PhoneNumber | PhoneNumberVerified |
	| habarthierry@loki.be | thabart | givename  | familyname | nickname | preferredusername | profile | picture | website | email | true          | M      | 1989-10-07 | fr       | fr     | 00          | true                |
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
	And the claim name with value thabart is returned by the JWS payload
	And the claim website with value website is returned by the JWS payload
	And the claim profile with value profile is returned by the JWS payload
	And the claim locale with value fr is returned by the JWS payload


