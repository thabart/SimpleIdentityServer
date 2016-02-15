# Simple identity server

Build : [![Build status](https://ci.appveyor.com/api/projects/status/ctvpsd79ovexlsdb?svg=true)](https://ci.appveyor.com/project/thabart/simpleidentityserver) 

Chat : [![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/thabart/SimpleIdentityServer?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge) 

Code coverage : [![Coverage Status](https://coveralls.io/repos/thabart/SimpleIdentityServer/badge.svg?branch=master&service=github)](https://coveralls.io/github/thabart/SimpleIdentityServer?branch=master)

MyGet : [![thabart MyGet Build Status](https://www.myget.org/BuildSource/Badge/thabart?identifier=a03dadd0-d105-4bb7-88d6-4cb4271dbb07)](https://www.myget.org/)

![openid_certified](https://cloud.githubusercontent.com/assets/1454075/7611268/4d19de32-f97b-11e4-895b-31b2455a7ca6.png)

[Certified](http://openid.net/certification/) OpenID Connect implementation.

Deployed on azure : [![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)

__Live demo__: http://simpleidentityserver.azurewebsites.net

## Overview

SimpleIdentityServer is an implementation of the OpenId authentication mechanism. It allows any application to implement Single Sign-On (identity token) and controls access to protected resources such as REST.API.

## Quick start

The Framework can easily be installed on any ASP.NET 5 project. 
In your solution add the following snippet code in the 'Configure' procedure :

```csharp
app.UseSimpleIdentityServer(new HostingOptions
    {
    IsDeveloperModeEnabled = false,
    IsMicrosoftAuthenticationEnabled = true,
    MicrosoftClientId = Configuration["Microsoft:ClientId"],
    MicrosoftClientSecret = Configuration["Microsoft:ClientSecret"],
    IsFacebookAuthenticationEnabled = true,
    FacebookClientId = Configuration["Facebook:ClientId"],
	FacebookClientSecret = Configuration["Facebook:ClientSecret"]
}, _swaggerOptions);
```

Add the dependencies like this :

```csharp
services.AddSimpleIdentityServer(new DataSourceOptions {
	DataSourceType = DataSourceTypes.InMemory,
    ConnectionString = connectionString,
    Clients = Clients.Get(),
    JsonWebKeys = JsonWebKeys.Get(),
    ResourceOwners = ResourceOwners.Get(),
    Scopes = Scopes.Get(),
	Translations = Translations.Get()
    }, _swaggerOptions);
```

## Contacts
* [Website](http://thabart.github.io/SimpleIdentityServer)
* You can follow me on twitter [#SimpleIdentityServer](https://twitter.com/simpleidserver)