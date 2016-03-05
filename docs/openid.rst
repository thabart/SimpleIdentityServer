=================
OpenId / OAUTH2.0
=================

A client uses the authorization grant to obtain an access token, which is a string representing an authorization granted to the client by the resource owner.
There're four grant-types (RFC-6749_):

Authorization code
------------------

* Applicable to a web application profile
* Main steps
    * Resource owner grants access to the protected resource
    * Authorization server returns an authorization code to the client (redirecting to the callback URI with authoriztion code in query)
    * Client exchanges this authorization code for an acces token with the authorization server

Implicit workflow
-----------------

* Applicable to user-agent based application such as javascript (under the context of a webrowser)
* Main steps
    * Resource owner grants access to the protected resource
    * Access token is immediately returned to the client application in a hash fragment of the callback URL

Resource owner credentials workflow
-----------------------------------

* Application for nativate application
* Main steps
    * The resource owner enters his credentials
    * Client exhcanges the credentials with authorization server to get an access token

Client credentials workflow
---------------------------

* Main steps :
    * Client application exchanges its own credentials for an access token. Protected resources is not owned by a specific user.

Hybrid workflow
---------------

TO COMPLETE

.. _RFC-6749: https://tools.ietf.org/html/rfc6749#section-1.3