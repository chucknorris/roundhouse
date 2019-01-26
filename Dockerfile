FROM microsoft/dotnet:2.2-sdk

MAINTAINER erik@brandstadmoen.net

ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet tool install dotnet-roundhouse -g

ENTRYPOINT [ "rh" ]
