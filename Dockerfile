FROM microsoft/aspnet

ENV ASPNET_ENV docker
ENV DNX_USER_HOME /opt/dnx

COPY SimpleIdentityServer/src /app

WORKDIR /app

RUN curl -sL https://deb.nodesource.com/setup_5.x | bash -
RUN apt-get install -y nodejs git sqlite3 libsqlite3-dev
RUN npm install -g bower
RUN npm install -g gulp
RUN ["nuget", "sources", "Add", "-Name", "ASPNETVNEXT", "-Source", "https://www.myget.org/F/aspnetvnext/api/v3/index.json"]
RUN ["nuget", "sources", "Add", "-Name", "AspNetCi", "-Source", "https://www.myget.org/F/aspnetcidev/api/v3/index.json"]
RUN bash -c "source $DNX_USER_HOME/dnvm/dnvm.sh \
		&& dnvm install 1.0.0-rc1-update1 -alias default -r coreclr \
		&& dnvm use 1.0.0-rc1-update1 -r coreclr -p"
RUN ["dnu", "restore"]
RUN sqlite3 simpleidserver.db ".databases"
RUN dnx -p SimpleIdentityServer.DataAccess.SqlServer ef database update

EXPOSE 5000

ENTRYPOINT ["dnx", "-p", "SimpleIdentityServer.Startup", "--framework", "dnxcore50", "web"]
