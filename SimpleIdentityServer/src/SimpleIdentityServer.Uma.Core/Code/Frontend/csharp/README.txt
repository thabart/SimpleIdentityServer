Install the following nuget packages to your client project :
- SimpleIdentityServer.Uma.Client
- SimpleIdentityServer.Client

To get an RPT token don't forget to pass correct parameters to the function "RptProvider.GetAuthorization" :
- scopes : you need them to execute certains operations on the resource, for example : to add a new customer, 
you need the "add" scope on the resource "person".
- clientId & clientSecret : they are used to retrieve an access token valid for the scopes "uma_authorization" & "uma_protection".
- idToken : you can pass complementary information to the UMA server, to help him determine if a resource owner has correct permissions.
- authorizationUrl : the open-id metadata URL
- umaUrl : uma metadata URL