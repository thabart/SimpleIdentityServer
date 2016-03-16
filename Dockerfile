FROM microsoft/aspnet

ENV ASPNET_ENV docker
ENV DNX_USER_HOME /opt/dnx
ENV PATH $PATH:$DNX_USER_HOME/runtimes/default/bin

COPY SimpleIdentityServer/src /app

WORKDIR /app

RUN curl -sL https://deb.nodesource.com/setup_5.x | bash -
RUN apt-get install -y nodejs git sqlite3 libsqlite3-dev libunwind8 libssl-dev
RUN npm install -g bower
RUN npm install -g gulp
RUN ["nuget", "sources", "Add", "-Name", "ASPNETVNEXT", "-Source", "https://www.myget.org/F/aspnetvnext/api/v3/index.json"]
RUN ["nuget", "sources", "Add", "-Name", "AspNetCi", "-Source", "https://www.myget.org/F/aspnetcidev/api/v3/index.json"]
RUN ["dnu", "restore"]
RUN sqlite3 simpleidserver.db ".databases"
RUN bash -c "source $DNX_USER_HOME/dnvm/dnvm.sh \
		&& dnvm install 1.0.0-rc1-update1 -alias default -r coreclr \
		&& dnvm alias default | xargs -i ln -s $DNX_USER_HOME/runtimes/{} $DNX_USER_HOME/runtimes/default \
		&& dnvm use 1.0.0-rc1-update1 -r coreclr \
		&& dnx -p SimpleIdentityServer.DataAccess.SqlServer ef database update"

EXPOSE 5000

ENTRYPOINT ["/opt/dnx/runtimes/dnx-coreclr-linux-x64.1.0.0-rc1-update1/bin/dnx", "-p", "SimpleIdentityServer.Startup", "web"]