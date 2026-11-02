# Inventario Fácil — CRUD completo (Participantes 5 y 6)

Aplicación de escritorio en C# (Windows Forms, .NET 8) con SQL Server.

## Qué incluye
**Parte 1 — Participante 5 (Wilson Rosa)**
- Estructura del proyecto (Models, Data, Forms).
- Conexión a SQL Server (`Data/Conexion.cs`).
- **Crear** productos (HU-01) con validaciones básicas: campos obligatorios y código único.
- **Consultar** productos (HU-02) en una grilla, con mensaje cuando no hay registros.

**Parte 2 — Participante 6 (Starlyn Castillo)**
- **Modificar** productos (HU-03): se hace clic en un producto del listado, sus datos se cargan en el formulario, se editan y se pulsa **Modificar** (UPDATE parametrizado).
- **Eliminar** productos (HU-04): se selecciona el producto y se pulsa **Eliminar**; pide confirmación. Si se responde *No*, el producto se conserva.
- Validaciones al modificar (HU-07): campos obligatorios y código único sin contar el propio producto.
- Proyecto de pruebas `InventarioFacil.Tests` (MSTest) con pruebas unitarias y de integración del CRUD.

## Cómo ejecutarlo
1. Abrir SQL Server (o LocalDB) y ejecutar `Database/01_CrearBaseDatos.sql`.
2. Si tu servidor no es `(localdb)\MSSQLLocalDB`, cambiar `Server=` en `Data/Conexion.cs`
   (por ejemplo `.\SQLEXPRESS` o `localhost`).
3. Abrir `InventarioFacil.sln` en Visual Studio 2022, dejar **InventarioFacil** como proyecto de inicio y presionar F5.

## Cómo correr las pruebas
1. Haber ejecutado el script de la base de datos (paso 1).
2. En Visual Studio: menú **Prueba → Ejecutar todas las pruebas** (o abrir el **Explorador de pruebas**).
   También sirve `dotnet test` desde la carpeta de la solución.
3. Las pruebas crean productos temporales con código `T6-...` y los borran al terminar; no tocan los datos del inventario.

## Estructura
```
InventarioFacil.sln
InventarioFacil/
├── Database/01_CrearBaseDatos.sql
├── Data/Conexion.cs
├── Data/ProductoRepository.cs      (Insertar, ObtenerTodos, ExisteCodigo | Actualizar, Eliminar, ExisteCodigoEnOtro, ObtenerPorCodigo)
├── Models/Producto.cs
├── Forms/FrmPrincipal.cs           (Guardar, Limpiar | Modificar, Eliminar)
├── Program.cs
└── InventarioFacil.csproj
InventarioFacil.Tests/
├── ProductoRepositoryTests.cs
└── InventarioFacil.Tests.csproj
```
