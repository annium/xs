FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine as builder
COPY . /code
RUN dotnet publish -c release -o /app /code/server/src/Server.Host

FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine
WORKDIR /app
COPY --from=builder /app /app
VOLUME [ "/app/certs", "/app/configuration", "/app/data" ]
CMD ["/app/Server.Host"]