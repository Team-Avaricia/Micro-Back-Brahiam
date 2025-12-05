# ----------------------------------------------------
# Etapa 1: Build (Construcción)
# Utilizamos una imagen SDK para compilar y publicar el código
# ----------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Establece el directorio de trabajo dentro del contenedor
WORKDIR /src

# Copia los archivos de proyecto (.csproj) para restaurar dependencias
# Esto ayuda a optimizar el caché de Docker si solo cambian los archivos .cs
COPY src/API/API.csproj src/API/
COPY src/Core.Application/Core.Application.csproj src/Core.Application/
COPY src/Core.Domain/Core.Domain.csproj src/Core.Domain/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
RUN dotnet restore

# Copia el resto del código fuente
COPY . .

# Publica la aplicación, la salida estará en /app/publish
RUN dotnet publish -c Release -o /app/publish

# ----------------------------------------------------
# Etapa 2: Final (Ejecución)
# Utilizamos la imagen ASP.NET Runtime, que es mucho más ligera
# ----------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Establece el directorio de trabajo para la ejecución
WORKDIR /app

# Copia los archivos publicados desde la etapa de construcción
COPY --from=build /app/publish .

# Define el punto de entrada al iniciar el contenedor
# El puerto 8080 es el puerto interno del contenedor
ENTRYPOINT ["dotnet", "API.dll"]
# IMPORTANTE: Reemplaza "TuProyecto.dll" por el nombre real de tu archivo DLL principal