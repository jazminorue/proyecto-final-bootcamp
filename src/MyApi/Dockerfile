# Etapa 1: Compilacion de la API
FROM ://microsoft.com AS build
WORKDIR /app

# Copiar archivos de proyecto y restaurar dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiar el resto del codigo y compilar la aplicacion
COPY . ./
RUN dotnet publish -c Release -o out

# Etapa 2: Entorno de ejecucion optimizado
FROM ://microsoft.com AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Configurar variables de entorno por defecto
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MyApi.dll"]
