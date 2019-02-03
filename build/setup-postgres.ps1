#! /usr/bin/env pwsh

docker pull postgres
docker run -d --name postgres-rh-test -e 'POSTGRES_PASSWORD=monkeybusiness' -p 5432:5432 postgres

