===========
Quick start
===========

In 5 steps
----------

The framework can "easily" be installed on any ASP.NET 5 project.

Nuget feeds
~~~~~~~~~~~

Add two new feeds into your Nuget's configuration file

	https://www.myget.org/F/aspnetvnext/api/v3/index.json & https://www.myget.org/F/thabart/api/v3/index.json


**Note** : The default file location is : *%APPDATA%\Nuget\Nuget.config*

Your environment
~~~~~~~~~~~~~~~~

Before you can start using the framework. Be-sure that all the pre-requisistes are installed on your machine otherwise you'll not be able to develop an ASP.NET 5 project.
 
 * If the .NET version manager (DNVM) is not installed then open a command dos window and execute the powershell command.
 * Upgrade the DNX tool to the latest version (the current one is : *1.0.0-rc1-update1*).
 * ASP.NET 5 project can be developed with Visual Studio 2015 or an open-source IDE on which you can install OmniSharp for example Visual Studio Code, ATOM, Brackets etc...

**Commands**:

.. code-block:: guess

    powershell -NoProfile -ExecutionPolicy unrestricted -Command "&{iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}"
    dnvm upgrade

Installation
~~~~~~~~~~~~

Add the Nuget package "SimpleIdentityServer.Host - 1.0.0-rc1-final" into your project and restore all the dependencies :

.. code-block:: guess

    dnu restore

Startup
~~~~~~~

The request pipeline can be configured in the Startup class, it is used to handle all requests made to the application.
It provides the entry point for an application, and is required for all applications. 
The startup class must define a *Configure* method, and may optionally also define a *ConfigureServices*.

Configure method
****************

The *Configure* method is used by the developer to register a set of middlewares which will interact with the HTTP request / response pipeline, for example :
the extension method *UseCookieAuthentication* adds a new middleware which is trying to authenticate the incoming HTTP requests.
Use the extension method *UseSimpleIdentityServer* to add Simple Identity Server into your ASP.NET 5 hosted project.
The snippet code below configures an In Memory identity server instance :

.. code-block:: guess

    services.AddSimpleIdentityServer(new DataSourceOptions {
        DataSourceType = DataSourceTypes.InMemory,
        ConnectionString = connectionString,
        Clients = Clients.Get(),
        JsonWebKeys = JsonWebKeys.Get(),
        ResourceOwners = ResourceOwners.Get(),
        Scopes = Scopes.Get(),
        Translations = Translations.Get()
        }, _swaggerOptions);

In the above example all open-id assets are stored into memory. 
They can also be persisted into an SqlServer database. For more information about the configuration please refer to the next part.

ConfigureServices method
************************

The *ConfigureServices* method is called before *Configure*. It configures and registers services that are used by your application.
Simple Identity Server cannot work if its dependencies are not registered. Call the method *AddSimpleIdentityServer* to register all the dependencies :

.. code-block:: guess

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


For more details about the properties, please refer to the next part.

SwaggerOptions
**************

You've probably noticed that SwaggerOptions is passed to the procedures *UseSimpleIdentityServer* and *AddSimpleIdentityServer*. 
It should be used only if you desired to interact with the different OpenId end-points via Swagger. 
It's preferable to disable it in a product environment.

.. code-block:: guess

    _swaggerOptions = new SwaggerOptions
    {
        IsSwaggerEnabled = true
    };


Run
~~~

Open a command prompt, navigate to your project and execute the following command :

.. code-block:: guess

    dnx web

At the end you should be able to navigate to the home screen :

TODO : Add GIF file of the home screen

The sample project can be found here : https://github.com/thabart/SimpleIdentityServer/tree/master/SimpleIdentityServer/src/SimpleIdentityServer.Startup

Options
-------

DataSourceOptions
~~~~~~~~~~~~~~~~~

TODO

HostingOptions
~~~~~~~~~~~~~~

TODO

