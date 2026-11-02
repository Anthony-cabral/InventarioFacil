using InventarioFacil.Data;
using InventarioFacil.Models;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InventarioFacil.Tests;

/// <summary>
/// Pruebas del CRUD contra la base de datos InventarioFacil (Participante 6).
/// Requisito: haber ejecutado Database/01_CrearBaseDatos.sql en el mismo servidor de Data/Conexion.cs.
/// Cada prueba crea sus propios productos con código "T6-..." y los borra al terminar,
/// así no se tocan los datos reales del inventario.
/// </summary>
[TestClass]
public class ProductoRepositoryTests
{
    private readonly ProductoRepository _repo = new();
    private readonly List<string> _codigosCreados = new();

    [TestInitialize]
    public void VerificarConexion()
    {
        if (!Conexion.Probar(out var mensaje))
            Assert.Inconclusive(mensaje + " Ejecute primero 01_CrearBaseDatos.sql.");
    }

    [TestCleanup]
    public void LimpiarDatosDePrueba()
    {
        foreach (var codigo in _codigosCreados)
        {
            var p = _repo.ObtenerPorCodigo(codigo);
            if (p != null) _repo.Eliminar(p.IdProducto);
        }
    }

    // Crea un producto de prueba con un código único y lo devuelve ya guardado (con su Id).
    private Producto CrearProductoDePrueba(decimal precio = 100m, int cantidad = 10)
    {
        var codigo = "T6-" + Guid.NewGuid().ToString("N")[..10];
        _codigosCreados.Add(codigo);

        _repo.Insertar(new Producto
        {
            Codigo = codigo,
            Nombre = "Producto de prueba",
            Categoria = "Pruebas",
            Precio = precio,
            Cantidad = cantidad
        });

        return _repo.ObtenerPorCodigo(codigo)!;
    }

    // ---------- HU-03 Modificar ----------

    [TestMethod]
    public void CP04_Actualizar_PrecioYCantidad_SeGuardanEnLaBaseDeDatos()
    {
        var p = CrearProductoDePrueba(precio: 100m, cantidad: 10);

        p.Precio = 199.50m;
        p.Cantidad = 25;
        var resultado = _repo.Actualizar(p);

        var guardado = _repo.ObtenerPorCodigo(p.Codigo)!;
        Assert.IsTrue(resultado);
        Assert.AreEqual(199.50m, guardado.Precio);
        Assert.AreEqual(25, guardado.Cantidad);
    }

    [TestMethod]
    public void Actualizar_NombreYCategoria_SeReflejanAlConsultar()
    {
        var p = CrearProductoDePrueba();

        p.Nombre = "Nombre editado";
        p.Categoria = "Categoria editada";
        _repo.Actualizar(p);

        var enListado = _repo.ObtenerTodos().Single(x => x.IdProducto == p.IdProducto);
        Assert.AreEqual("Nombre editado", enListado.Nombre);
        Assert.AreEqual("Categoria editada", enListado.Categoria);
    }

    [TestMethod]
    public void Actualizar_ProductoInexistente_DevuelveFalse()
    {
        var inexistente = new Producto
        {
            IdProducto = -1,
            Codigo = "NO-EXISTE",
            Nombre = "X",
            Categoria = "X",
            Precio = 1,
            Cantidad = 1
        };

        Assert.IsFalse(_repo.Actualizar(inexistente));
    }

    [TestMethod]
    public void Actualizar_CantidadNegativa_LaBaseDeDatosLaRechaza()
    {
        var p = CrearProductoDePrueba();
        p.Cantidad = -5;

        Assert.ThrowsException<SqlException>(() => _repo.Actualizar(p));
    }

    // ---------- HU-07 Código único al modificar ----------

    [TestMethod]
    public void ExisteCodigoEnOtro_MismoProducto_DevuelveFalse()
    {
        var p = CrearProductoDePrueba();

        // Guardar sin cambiar el código no debe contar como duplicado
        Assert.IsFalse(_repo.ExisteCodigoEnOtro(p.Codigo, p.IdProducto));
    }

    [TestMethod]
    public void ExisteCodigoEnOtro_CodigoDeOtroProducto_DevuelveTrue()
    {
        var a = CrearProductoDePrueba();
        var b = CrearProductoDePrueba();

        // Intentar ponerle a B el código de A
        Assert.IsTrue(_repo.ExisteCodigoEnOtro(a.Codigo, b.IdProducto));
    }

    // ---------- HU-04 Eliminar ----------

    [TestMethod]
    public void CP05_Eliminar_ProductoExistente_DesapareceDelListado()
    {
        var p = CrearProductoDePrueba();

        var resultado = _repo.Eliminar(p.IdProducto);

        Assert.IsTrue(resultado);
        Assert.IsNull(_repo.ObtenerPorCodigo(p.Codigo));
        Assert.IsFalse(_repo.ObtenerTodos().Any(x => x.IdProducto == p.IdProducto));
    }

    [TestMethod]
    public void Eliminar_ProductoInexistente_DevuelveFalse()
    {
        Assert.IsFalse(_repo.Eliminar(-1));
    }

    // ---------- Integración: flujo completo del CRUD ----------

    [TestMethod]
    public void Integracion_CrearConsultarModificarEliminar()
    {
        // Crear (HU-01)
        var p = CrearProductoDePrueba(precio: 50m, cantidad: 5);
        Assert.IsNotNull(p);

        // Consultar (HU-02)
        Assert.IsTrue(_repo.ObtenerTodos().Any(x => x.Codigo == p.Codigo));

        // Modificar (HU-03)
        p.Cantidad = 8;
        Assert.IsTrue(_repo.Actualizar(p));
        Assert.AreEqual(8, _repo.ObtenerPorCodigo(p.Codigo)!.Cantidad);

        // Eliminar (HU-04)
        Assert.IsTrue(_repo.Eliminar(p.IdProducto));
        Assert.IsNull(_repo.ObtenerPorCodigo(p.Codigo));
    }
}
