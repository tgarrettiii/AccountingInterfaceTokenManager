#build context path should be relative to Solution file or above SRC_PREFIX value

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS sdk
RUN apk add git

FROM sdk AS build
ARG SRC_PREFIX=.
ENV APP_NAME AccountingInterfaceTokenManager
WORKDIR /repo
COPY ["./${SRC_PREFIX}/${APP_NAME}.sln", "./${SRC_PREFIX}/"]
COPY ["./${SRC_PREFIX}/AccountingInterfaceTokenManager/AccountingInterfaceTokenManager.csproj", "./${SRC_PREFIX}/AccountingInterfaceTokenManager/"]
COPY ["./${SRC_PREFIX}/TestToken/TestToken.csproj", "./${SRC_PREFIX}/TestToken/"]
COPY ["./${SRC_PREFIX}/Nuget.Config", "./${SRC_PREFIX}"]

# main restore and build
#RUN cd ${SRC_PREFIX}; dotnet tool restore --configfile Nuget.Config
RUN cd ${SRC_PREFIX}; dotnet restore "${APP_NAME}.sln" --configfile Nuget.Config
COPY . .
RUN cd ${SRC_PREFIX}; dotnet build "${APP_NAME}.sln" --configuration Release

# run tests
#RUN cd ${SRC_PREFIX}; dotnet test "${APP_NAME}.sln" -r /coverage -s ./XPlatCodeCoverage.runsettings
#RUN mv /coverage/*/*.xml /coverage

FROM build AS publish
ARG SRC_PREFIX=.
ENV APP_NAME AccountingInterfaceTokenManager
LABEL output=${APP_NAME}
WORKDIR /repo/${SRC_PREFIX}

# create client libs
RUN dotnet pack "AccountingInterfaceTokenManager/AccountingInterfaceTokenManager.csproj" --include-source --include-symbols --no-build -o /

# create coverage report
#RUN dotnet tool run reportgenerator "-reports:/coverage/coverage.cobertura.xml" "-targetdir:/coverage" -reporttypes:Html

# keep application specific files a separate layer
#WORKDIR /publish
#RUN mkdir /app; mv ${APP_NAME}* nlog.config appsettings.json web.config Scripts/  /app

# keep common runtimes a separate layer
#WORKDIR /runtimes
#RUN mv /publish/runtimes/ .

#FROM base AS final
#WORKDIR /app
#COPY --from=publish /runtimes .
#COPY --from=publish /publish .
#COPY --from=publish /app .
#ENV ASPNETCORE_URLS=http://0.0.0.0:5000
#ENTRYPOINT ["dotnet", "IsoStandards.Web.dll"]
