FROM registry.annium.com/dotnet/sdk:10.0-alpine AS builder
COPY . /src
RUN dotnet publish -c release -o /app /src/server/src/Annium.Xs.Server.Host

FROM registry.annium.com/dotnet/aspnet:10.0-alpine
WORKDIR /app
COPY --from=builder /app /app
VOLUME [ "/app/certs", "/app/configuration", "/app/data" ]
CMD ["/app/Annium.Xs.Server.Host"]