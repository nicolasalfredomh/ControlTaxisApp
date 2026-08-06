# 1. Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ControlTaxisApp/ControlTaxisApp.csproj", "ControlTaxisApp/"]
RUN dotnet restore "ControlTaxisApp/ControlTaxisApp.csproj"

COPY . .

WORKDIR "/src/ControlTaxisApp"
RUN dotnet publish -c Release -o /app

# 2. Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:10.0

# --- CORRECCIÓN AQUÍ: Instalamos sqlite3 Y libgdiplus ---
# libgdiplus es indispensable para System.Drawing en entornos Linux
RUN apt-get update && \
    apt-get install -y sqlite3 libgdiplus && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "ControlTaxisApp.dll"]