# Backend - Sistema de Compras

ASP.NET Core 8 Web API. SQL Server via EF Core.

## Base de datos

El proyect usa **SQL Server LocalDB** por defecto (`appsettings.json`). Al ejecutar `dotnet run`, la aplicacion aplica automaticamente las migraciones y crea datos iniciales (departamentos, unidades de medida, proveedores, empleados y articulos de ejemplo) — no hace falta correr `dotnet ef database update` a mano.

Requisitos: tener **SQL Server Express LocalDB** instalado (viene con el workload "ASP.NET y desarrollo web" del instalador de Visual Studio, o se puede instalar por separado desde la pagina de descargas de SQL Server Express).

### Si tu SQL Server no es LocalDB

Si en tu maquina usas otra instancia (por ejemplo SQL Server Express con nombre `SQLEXPRESS`, o una instancia con otro nombre), no edites `appsettings.json` directamente. En su lugar, crea un archivo `appsettings.Local.json` en esta carpeta (esta ignorado por git, asi que no afecta a nadie mas) con tu cadena de conexion:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=SistemaDeComprasDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

Este archivo sobreescribe la cadena de conexion por defecto solo en tu maquina.

## Ejecutar

```
cd backend
dotnet run
```

Swagger disponible en `/swagger` en modo Development.
