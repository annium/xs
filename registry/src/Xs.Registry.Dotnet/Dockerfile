FROM microsoft/dotnet:2.2-sdk-alpine as builder
COPY . /src
RUN mv /src/registry/src/nuget.docker.config /src/nuget.config
RUN dotnet publish -c release -o /app /src/registry/src/Xs.Registry.Dotnet

FROM microsoft/dotnet:2.2-runtime-alpine
WORKDIR /app
COPY --from=builder /app /app
VOLUME [ "/app/configuration", "/app/data" ]
CMD ["dotnet", "/app/Xs.Registry.Dotnet.dll"]