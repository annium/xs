FROM registry.annium.com/dotnet/sdk:8.0-alpine as builder
COPY . /src
RUN dotnet publish -c release -o /app /src/xs/server/src/Server.Host

FROM registry.annium.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=builder /app /app
VOLUME [ "/app/certs", "/app/configuration", "/app/data" ]
CMD ["/app/Server.Host"]