# 1. Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos primero el archivo del proyecto para restaurar dependencias
# Esto es más eficiente para Docker
COPY ["ControlTaxisApp/ControlTaxisApp.csproj", "ControlTaxisApp/"]
RUN dotnet restore "ControlTaxisApp/ControlTaxisApp.csproj"

# Copiamos todo el resto del código
COPY . .

# Nos movemos a la subcarpeta del proyecto y publicamos
WORKDIR "/src/ControlTaxisApp"
RUN dotnet publish -c Release -o /app

# 2. Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
# Copiamos los archivos publicados desde la etapa anterior
COPY --from=build /app .

# Punto de entrada de tu aplicación
ENTRYPOINT ["dotnet", "ControlTaxisApp.dll"]