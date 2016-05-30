#!/bin/bash

set -e

host="$1"
shift

until psql -h "$host" -U "postgres" -c '\l'; do
  >&2 echo "Postgres is unavailable - sleeping"
  sleep 1
done

>&2 echo "Postgres is up - executing command"
d SimpleIdentityServer.DataAccess.SqlServer && dotnet ef database update