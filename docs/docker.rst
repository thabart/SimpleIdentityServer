======
Docker
======

We are using docker to easily deploy and start the application inside a docker container.
There're four components deployed :
 1. SimpleIdentityServer : Open-Id provider.
 2. SimpleIdentityServerManagerApi : Web.Api service used to manage the open-id assets.
 3. SimpleIdentityServerManager : Angularjs website used to manage the open-id assets.
 4. Database : Open-Id assets are stored for now in a SQLite3 database.

It's pretty easy to deploy & run the application, in a command prompt execute the command below. 
If you're working on Windows be-sure that your environment is correcty set-up. 
Armen Shirmoon explains in a very well written blog post, how to easily run Docker on Windows. If your Windows environment is not ready
I invit you to read this article_.

.. code-block:: guess

	docker run -t -d -p 5000:5000 identitycontrib/identityserver

Once the docker container has been started, browse the URL : http://localhost:5000 to access to SimpleIdentityServer.

.. _article: http://dotnetliberty.com/index.php/2015/10/25/asp-net-5-running-in-docker-on-windows/
