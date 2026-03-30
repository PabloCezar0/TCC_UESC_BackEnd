# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar sln e csproj para restaurar dependências
COPY Rota.sln ./
COPY Rota.API/Rota.API.csproj Rota.API/
COPY Rota.Application/Rota.Application.csproj Rota.Application/
COPY Rota.Domain/Rota.Domain.csproj Rota.Domain/
COPY Rota.Infra.Data/Rota.Infra.Data.csproj Rota.Infra.Data/
COPY Rota.Infra.IoC/Rota.Infra.IoC.csproj Rota.Infra.IoC/

RUN dotnet restore Rota.sln

RUN dotnet tool install --global dotnet-ef --version 8.0.0
ENV PATH="$PATH:/root/.dotnet/tools"
# Copiar todo o código e publicar
COPY . .
WORKDIR /src/Rota.API
#RUN dotnet ef database update -p ../Rota.Infra.Data -s .
RUN dotnet publish -c Release -o /app/publish

# Etapa final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
RUN apt-get update && apt-get install -y netcat-openbsd && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .

# # copia o entrypoint da raiz
 COPY entrypoint.sh /app/entrypoint.sh
 RUN chmod +x /app/entrypoint.sh

ENTRYPOINT ["/app/entrypoint.sh"]
#ENTRYPOINT ["dotnet", "Rota.API.dll"]
