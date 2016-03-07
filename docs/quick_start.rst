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
 * If the .NET version manager (DNVM) is not installed then open a command dos window and execute the command

.. code-block:: guess

    powershell -NoProfile -ExecutionPolicy unrestricted -Command "&{iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}"

 * Upgrade the DNX tool to the latest version (current one is : 1.0.0-rc1-update1) :

.. code-block:: guess

    dnvm upgrade

 * ASP.NET 5 project can be developed with Visual Studio 2015 or an open-source IDE on which you can install OmniSharp for example Visual Studio Code, ATOM, Brackets etc...

Installation
~~~~~~~~~~~~

Add the Nuget package "SimpleIdentityServer.Host" into your project "SimpleIdentityServer.Host" : "1.0.0-rc1-final" and restore all the dependencies :

.. code-block:: guess

    dnu restore

Simple implementation
~~~~~~~~~~~~~~~~~~~~~

Swagger is used to interact with the Open-Id endpoints. It's easy to enable / disable it by creating a new instance of SwaggerOptions and pass it as parameter
to the methods "AddSimpleIdentityServer" & "UseSimpleIdentityServer"

.. code-block:: guess

    _swaggerOptions = new SwaggerOptions
    {
        IsSwaggerEnabled = true
    };


To inject the framework dependencies use the method "AddSimpleIdentityServer". In the sample the Open-Id assets are stored in Memory :

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

Finally configure the application with the method "UseSimpleIdentityServer" :

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

Advanced properties
-------------------

TO COMPLETE