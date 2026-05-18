# PTClandBus — Prueba Técnica ClandBus

Solución completa de la prueba técnica de ClandBus.
Stack: C# .NET 8 Console App + SQL Server.

---

## Requisitos

- .NET 8 SDK

---

## Configuración

Renombra `appsettings.example.json` a `appsettings.json` e ingresa tus credenciales:

```bash
cp appsettings.example.json appsettings.json
```

```json
{
  "Acumatica": {
    "BaseUrl": "https://soporte.clandbus.com/demo",
    "Username": "tu_usuario",
    "Password": "tu_password",
    "Company": "MEXDEMO",
    "Branch": "PUEBLA",
    "Endpoint": "DefaultExt",
    "Version": "1.0.0"
  }
}
```

> `appsettings.json` está en `.gitignore` y no se sube al repositorio.

---

## Ejecución

```bash
dotnet run
```

### Qué hace el programa

1. Inicia sesión en Acumatica via cookies de sesión
2. Obtiene las últimas 5 órdenes de venta en estado "On Hold"
3. Actualiza la descripción de las 2 primeras órdenes
4. Ejecuta "Remove Hold" sobre esas mismas 2 órdenes (quedan en estado "Open")
5. Cierra la sesión

---

## Scripts SQL

| Archivo                        | Propósito                                                         |
| ------------------------------ | ----------------------------------------------------------------- |
| `sql/01_delete_historicos.sql` | Elimina registros con `[Date] < '2025-01-01'` en lotes de 10,000 |
| `sql/02_create_index.sql`      | Crea índice `([Date], [Username])` con `ONLINE = ON`              |
