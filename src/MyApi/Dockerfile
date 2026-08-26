# Etapa 1: Compilación de la API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app

# Copiar archivo de proyecto y restaurar dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiar el resto del código y compilar la aplicación
COPY . ./
RUN dotnet publish -c Release -o /app/out

# Etapa 2: Entorno de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/out .

# Configurar variables de entorno
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "MyApi.dll"]
