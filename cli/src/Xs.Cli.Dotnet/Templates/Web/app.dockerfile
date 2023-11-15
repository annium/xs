FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine as builder
COPY . /code
RUN dotnet publish -c release -o /app /code

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
RUN apk add icu-dev icu-libs icu-data-full
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
COPY --from=builder /app /app
VOLUME [ "/app/configuration" ]
CMD ["/app/App"]