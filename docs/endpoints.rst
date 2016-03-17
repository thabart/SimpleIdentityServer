=========
Endpoints
=========

Authorization
-------------

TO COMPLETE

Token
-----

TO COMPLETE

User information
----------------

User information endpoint returns a set of claims about the authenticated user. 
The list is represented by a JSON object that contains a collection of name and value pairs for the Claims.
An HTTP GET / POST request is sent by the client to the user information endpoint. An access-token obtained from Open-Id provider needs to be specified as a BearerToken_ othewise an unauthorized error is returned.

There're three different ways to pass an access token :

 1. Normal way is by using the Authorization header.
 2. Sending the access token in the HTTP request entity-body.
 3. Passing it in the HTTP request URI.

Depending on the client configuration, claims can be returned either in a clear JSON object or a JWT (JWS / JWE).
More information can be found in ClientRegistration_ documentation.

Discovery
---------

Discovery endpoint is an indirection layer. It contains the required informations to interact with the OpenId provider (SimpleIdentityServer).
According to Glenn Block author of the book "Designing Evolvable Web APIs with ASP.NET" the advantage of using this layer is : " *By creating discovery documents at API entry points,
we can enable clients to dynamically identify the location of certains resources without having to hardcode URIs into the client application* ".

The url of the discovery endpoint is *http://localhost/.well-known/openid-configuration*.
When an HTTP GET request is executed against it, a JSON object is returned with the following properties. (Fore more information you can refer to the official OpenIdConnectDiscovery_ documentation)

+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+
| Parameter                             | Description                                                                                           | Values                                                                             |
+=======================================+=======================================================================================================+====================================================================================+
| authorization_endpoint                | URL of the Authorization endpoint                                                                     | /authorization                                                                     |
+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+
| check_session_iframe                  | URL of an OP iframe that supports cross-origin communications                                         | **NOT SUPPORTED**                                                                  |
+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+
| claims_parameter_supported            | Boolean value specifying whether the OP supports use of the claims parameter                          | true                                                                               |
+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+
| claims_supported                      | JSON array containing a list of Claims Names                                                          | [sub, name, family_name, given_name etc..]                                         |
+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+
| end_session_endpoint                  | URL at the OP to which an RP can perform a redirect to request that the End-User be logged out        | **NOT SUPPORTED**                                                                  |
+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+
| grant_types_supported                 | JSON array containing a list of grant-types                                                           | [authorization_code, password, refresh_token, implicit]                            |
+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+
| id_token_signing_alg_values_supported | JSON array containing a list of JWS signing algorithms                                                | [RS256]                                                                            |
+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+
| issuer                                | Issuer identifier                                                                                     | identifier                                                                         |
+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+
| jwks_uri                              | URL of the Json Web Key Set                                                                           | /jwks                                                                              |
+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+
| request_parameter_supported           | Boolean value specifying whether the OP supports use of the request parameter                         | true                                                                               |
+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+
| request_uri_parameter_supported       | Boolean value specifying whether the OP supports use of the request_uri parameter                     | true                                                                               |
+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+
| require_request_uri_registration      | Boolean value specifying whether the OP required any request_uri used to be pre-registered            | false                                                                              |
+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+
| response_modes_supported              | JSON array containing a list of response_mode values                                                  | [query, fragment]                                                                  |
+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+
| revocation_endpoint                   | URL used to notify the authorization server that a refresh or access token is not longer valid        | **NOT SUPPORTED**                                                                  |
+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+
| scopes_supported                      | JSON array containing a list of scopes                                                                | [address, email, openid, phone, profile, role]                                     |
+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+
| subject_types_supported               | JSON array containing a list of subject identifier types                                              | [public]                                                                           |
+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+
| token_endpoint                        | URL of the token endpoint                                                                             | /token                                                                             |
+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+
| token_endpoint_auth_methods_supported | JSON array containing a list of Client Authentication methods supported by this Token Endpoint        | [client_secret_basic, client_secret_post, client_secret_jwt, private_key_jwt]      |
+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+
| userinfo_endpoint                     | URL of the userinformation endpoint                                                                   | /userinfo                                                                          |
+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+
| version                               | Version of the contract                                                                               | 1.0                                                                                |
+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+
| registration_endpoint                 | URL of the dynamic client registration endpoint                                                       | /registration                                                                      |
+---------------------------------------+-------------------------------------------------------------------------------------------------------+------------------------------------------------------------------------------------+


Json Web Keys
-------------

TO COMPLETE

Registration
------------

TO COMPLETE

Introspection
-------------

TO COMPLETE

.. _OpenIdConnectDiscovery: http://openid.net/specs/openid-connect-discovery-1_0.html
.. _UserInformation: http://openid.net/specs/openid-connect-core-1_0.html#UserInfo
.. _BearerToken: http://tools.ietf.org/html/rfc6750
.. _ClientRegistration: http://openid.net/specs/openid-connect-registration-1_0.html#ClientMetadata
