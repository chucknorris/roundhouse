FROM microsoft/dotnet:2.2-sdk

LABEL maintainer="erik@brandstadmoen.net"

ARG roundhouse_version

ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet tool install dotnet-roundhouse -g --version $roundhouse_version

ENTRYPOINT [ "rh" ]
