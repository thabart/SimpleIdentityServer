FROM microsoft/aspnet

COPY SimpleIdentityServer/src /app

WORKDIR /app

RUN curl -sL https://deb.nodesource.com/setup_5.x | bash -
RUN apt-get install --yes nodejs
RUN npm install -g bower
RUN npm install -g gulp
RUN ["nuget", "sources", "Add", "-Name", "ASPNETVNEXT", "-Source", "https://www.myget.org/F/aspnetvnext/api/v3/index.json"]
RUN ["dnu", "restore"]

EXPOSE 5000/tcp
ENTRYPOINT ["dnx", "-p", "SimpleIdentityServer.Startup", "web"]
