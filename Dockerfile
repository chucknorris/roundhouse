FROM mcr.microsoft.com/dotnet/core/sdk:3.1

LABEL maintainer="erik@brandstadmoen.net"

ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet tool install dotnet-roundhouse -g

ENTRYPOINT [ "rh" ]
