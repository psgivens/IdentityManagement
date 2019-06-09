FROM dotnet-stage AS build-env
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./Common.FSharp ./Common.FSharp
COPY ./IdentityManagement ./IdentityManagement

WORKDIR /app/IdentityManagement
RUN dotnet restore

# copy everything else and build
RUN dotnet publish -c Debug -o out

# build runtime image
FROM microsoft/dotnet:2.2-sdk 

RUN mkdir /vsdbg
COPY --from=build-env /vsdbg/ /vsdbg/

WORKDIR /app
COPY --from=build-env /app/ .

WORKDIR /app/IdentityManagement
# ENTRYPOINT ["dotnet", "watch", "--project", "IdentityManagement.Api", "run"]

