# Simple identity server

Build : [![Build status](https://ci.appveyor.com/api/projects/status/ctvpsd79ovexlsdb?svg=true)](https://ci.appveyor.com/project/thabart/simpleidentityserver) [![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/thabart/SimpleIdentityServer?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

![openid_certified](https://cloud.githubusercontent.com/assets/1454075/7611268/4d19de32-f97b-11e4-895b-31b2455a7ca6.png)

[Certified](http://openid.net/certification/) OpenID Connect implementation.

Deployed on azure : [![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)

__Live demo__: http://simpleidentityserver.azurewebsites.net

__Swagger contract__ : http://simpleidentityserver.azurewebsites.net/swagger/ui/index

Testing : https://op.certification.openid.net:60360/

## Use the semantic logging

The Semantic Logging Application Block (SLAB) is implemented on the project. The library used to consume the ETW events is "Microsoft.Practices.EntrepriseLibrary.SemanticLogging".
It's very easy to plugin-in an existing or custom library to consume the events and store them to a data-sources such as : Windows Azure table, Elastic Search or a flat file.
You can refer to the sample project "SimpleIdentityServer.Logging.Consumer", if you wish to implement an out of process events consumer.
The screenshot below is coming from "Kibana", a powerful visualization tool from elastic-search. The tool can be downloaded [here!](https://www.elastic.co/downloads/kibana).

![alt text](https://github.com/thabart/SimpleIdentityServer/blob/master/images/Kibana-Monitoring.png "Kibana dashboard")

The Kibana configuration files are available under the folder "Kibana-Exports". They can be uploaded to your Kibana instance by following the [tutorial!](https://www.elastic.co/guide/en/kibana/3.0/saving-and-loading-dashboards.html).
