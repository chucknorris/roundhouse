FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
ARG VERSION

LABEL maintainer="erik@brandstadmoen.net"

WORKDIR /app

COPY product/roundhouse.console/roundhouse.console.csproj product/roundhouse.console/roundhouse.console.csproj
COPY product/roundhouse.tests/roundhouse.tests.csproj product/roundhouse.tests/roundhouse.tests.csproj
COPY product/roundhouse.databases.sqlserverce/roundhouse.databases.sqlserverce.csproj product/roundhouse.databases.sqlserverce/roundhouse.databases.sqlserverce.csproj
COPY product/roundhouse.databases.access/roundhouse.databases.access.csproj product/roundhouse.databases.access/roundhouse.databases.access.csproj
COPY product/roundhouse.bottles.deployers/roundhouse.bottles.deployers.csproj product/roundhouse.bottles.deployers/roundhouse.bottles.deployers.csproj
COPY product/roundhouse.databases.oracle/roundhouse.databases.oracle.csproj product/roundhouse.databases.oracle/roundhouse.databases.oracle.csproj
COPY product/roundhouse.databases.postgresql/roundhouse.databases.postgresql.csproj product/roundhouse.databases.postgresql/roundhouse.databases.postgresql.csproj
COPY product/roundhouse.tasks/roundhouse.tasks.csproj product/roundhouse.tasks/roundhouse.tasks.csproj
COPY product/roundhouse.test.merged/roundhouse.test.merged.csproj product/roundhouse.test.merged/roundhouse.test.merged.csproj
COPY product/roundhouse.databases.sqlite/roundhouse.databases.sqlite.csproj product/roundhouse.databases.sqlite/roundhouse.databases.sqlite.csproj
COPY product/roundhouse.databases.sqlserver2000/roundhouse.databases.sqlserver2000.csproj product/roundhouse.databases.sqlserver2000/roundhouse.databases.sqlserver2000.csproj
COPY product/roundhouse.lib/roundhouse.lib.csproj product/roundhouse.lib/roundhouse.lib.csproj
COPY product/roundhouse.databases.sqlserver/roundhouse.databases.sqlserver.csproj product/roundhouse.databases.sqlserver/roundhouse.databases.sqlserver.csproj
COPY product/roundhouse.tests.integration/roundhouse.tests.integration.csproj product/roundhouse.tests.integration/roundhouse.tests.integration.csproj
COPY product/roundhouse.core/roundhouse.core.csproj product/roundhouse.core/roundhouse.core.csproj
COPY product/roundhouse.databases.mysql/roundhouse.databases.mysql.csproj product/roundhouse.databases.mysql/roundhouse.databases.mysql.csproj
COPY roundhouse.sln roundhouse.sln
COPY build/Extract-Resource.ps1 build/Extract-Resource.ps1 

RUN dotnet restore

#RUN ls -l ~/.nuget/packages/mysql.data/*
RUN ./build/Extract-Resource.ps1 -File "$(find ~/.nuget/packages/mysql.data/ -name MySql.Data.dll | tail -1)" -ResourceName MySql.Data.keywords.txt -OutFile generated/MySql.Data/keywords.txt

COPY . ./

RUN dotnet publish -v q -nologo --no-restore product/roundhouse.console -o /app/out -p:TargetFramework=netcoreapp2.1 -p:Version="${VERSION}" -p:Configuration=Build -p:Platform="Any CPU"

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

WORKDIR /app
COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "rh.dll"]
