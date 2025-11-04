FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5030

ENV ASPNETCORE_URLS=http://+:5030
ENV ASPNETCORE_ENVIRONMENT=Development

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["FastPay.Api/FastPay.Api.csproj", "FastPay.Api/"]
COPY ["FastPay.Application/FastPay.Application.csproj", "FastPay.Application/"]
COPY ["FastPay.Domain/FastPay.Domain.csproj", "FastPay.Domain/"]
COPY ["FastPay.Infra.Data/FastPay.Infra.Data.csproj", "FastPay.Infra.Data/"]
COPY ["FastPay.Infra.IoC/FastPay.Infra.IoC.csproj", "FastPay.Infra.IoC/"]
RUN dotnet restore "FastPay.Api/FastPay.Api.csproj"

COPY . .
WORKDIR "/src/FastPay.Api"
RUN dotnet publish "FastPay.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
ENTRYPOINT ["dotnet", "FastPay.Api.dll"]
