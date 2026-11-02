-- Inventario Fácil | Script de base de datos (SQL Server)
IF DB_ID('InventarioFacil') IS NULL
    CREATE DATABASE InventarioFacil;
GO

USE InventarioFacil;
GO

IF OBJECT_ID('dbo.Productos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Productos
    (
        IdProducto INT IDENTITY(1,1) CONSTRAINT PK_Productos PRIMARY KEY,
        Codigo     VARCHAR(30)   NOT NULL CONSTRAINT UQ_Productos_Codigo UNIQUE,
        Nombre     VARCHAR(100)  NOT NULL,
        Categoria  VARCHAR(60)   NOT NULL,
        Precio     DECIMAL(10,2) NOT NULL CONSTRAINT CK_Productos_Precio CHECK (Precio >= 0),
        Cantidad   INT           NOT NULL CONSTRAINT CK_Productos_Cantidad CHECK (Cantidad >= 0)
    );
END
GO

-- Datos de prueba (opcional)
IF NOT EXISTS (SELECT 1 FROM dbo.Productos)
BEGIN
    INSERT INTO dbo.Productos (Codigo, Nombre, Categoria, Precio, Cantidad) VALUES
    ('P-001', 'Arroz Premium 5 lb', 'Granos', 185.00, 40),
    ('P-002', 'Aceite de Soya 1 L', 'Aceites', 210.50, 25);
END
GO
