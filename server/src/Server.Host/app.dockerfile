FROM registry.annium.com/dotnet/sdk:9.0-alpine as builder
COPY . /src
RUN dotnet publish -c release -o /app /src/server/src/Server.Host

FROM registry.annium.com/dotnet/aspnet:9.0-alpine
WORKDIR /app
COPY --from=builder /app /app
VOLUME [ "/app/certs", "/app/configuration", "/app/data" ]
CMD ["/app/Server.Host"]