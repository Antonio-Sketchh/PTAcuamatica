# PTClandBus — Prueba Técnica ClandBus

Solución completa de la prueba técnica de ClandBus.
Stack: C# .NET 8 Console App + SQL Server.

---

## Requisitos

- .NET 8 SDK

---

## Configuración

Crea el archivo `appsettings.json` en la raíz del proyecto:

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
