FROM microsoft/dotnet:2.2-sdk-alpine as builder
COPY . /src
RUN mv /src/registry/src/nuget.docker.config /src/nuget.config
RUN dotnet publish -c release -o /app /src/registry/src/Xs.Registry.Shared

FROM microsoft/dotnet:2.2-runtime-alpine
WORKDIR /app/data
COPY --from=builder /app /app
VOLUME [ "/app/data" ]
CMD ["dotnet", "/app/Xs.Registry.Shared.dll"]