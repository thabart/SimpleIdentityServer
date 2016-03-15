FROM microsoft/aspnet

COPY SimpleIdentityServer/src /app

WORKDIR /app

ENV ASPNET_ENV docker

RUN curl -sL https://deb.nodesource.com/setup_5.x | bash -
RUN apt-get install -y nodejs git sqlite3 libsqlite3-dev
RUN npm install -g bower
RUN npm install -g gulp
RUN ["nuget", "sources", "Add", "-Name", "ASPNETVNEXT", "-Source", "https://www.myget.org/F/aspnetvnext/api/v3/index.json"]
RUN ["nuget", "sources", "Add", "-Name", "AspNetCi", "-Source", "https://www.myget.org/F/aspnetcidev/api/v3/index.json"]
RUN dnvm install 1.0.0-rc1-update1 -r coreclr
RUN ["dnu", "restore"]
RUN sqlite3 simpleidserver.db ".databases"
RUN dnx -p SimpleIdentityServer.DataAccess.SqlServer database update

EXPOSE 5000

ENTRYPOINT ["dnx", "-p", "SimpleIdentityServer.Startup", "web"]
