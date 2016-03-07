===========
Quick start
===========

In 5 steps
----------

The framework can easily be installed on any ASP.NET 5 project.

Configuration file
~~~~~~~~~~~~~~~~~~

Add new nuget sources into your Nuget's configuration file

	https://www.myget.org/F/aspnetvnext/api/v3/index.json & https://www.myget.org/F/thabart/api/v3/index.json


**Note** : The default file location is : %APPDATA%\Nuget\Nuget.config

Your environment
~~~~~~~~~~~~~~~~

Before you can start using the framework. Be-sure that all the pre-requisistes are installed on your machine otherwise you'll not be able to develop an ASP.NET 5 project.
 * If the .NET version manager (DNVM) is not installed then open a command dos window and execute the powershell command
 * Upgrade the DNX tool to the latest version (current one is : 1.0.0-rc1-update1)
 * ASP.NET 5 project can be developed with Visual Studio 2015 or an open-source IDE on which you can install OmniSharp for example Visual Studio Code, ATOM, Brackets etc...

**Commands**:
.. code-block:: guess

    powershell -NoProfile -ExecutionPolicy unrestricted -Command "&{iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}"
    dnvm upgrade

Installation
~~~~~~~~~~~~

Add the Nuget package "SimpleIdentityServer.Host" into your project "SimpleIdentityServer.Host" : "1.0.0-rc1-final" and restore all the dependencies :

.. code-block:: guess

    dnu restore

Simple implementation
~~~~~~~~~~~~~~~~~~~~~

Swagger is used to interact with the Open-Id endpoints. 
It's easy to enable / disable it by creating a new instance of SwaggerOptions and pass it as parameter to the methods "AddSimpleIdentityServer" & "UseSimpleIdentityServer"

.. code-block:: guess

    _swaggerOptions = new SwaggerOptions
    {
        IsSwaggerEnabled = true
    };


Inject all the framework dependencies with the method "AddSimpleIdentityServer". 
The open-id assets are stored in memory, if you want to persist them into a SqlServer database refer to the next part.

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

Configure the application with the method "UseSimpleIdentityServer" :

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

Run
~~~

Run & enjoy the application ! 

.. image:: GetClients.gif


Advanced properties
-------------------

TO COMPLETE