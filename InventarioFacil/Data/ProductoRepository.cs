using InventarioFacil.Models;
using Microsoft.Data.SqlClient;

namespace InventarioFacil.Data;

/// <summary>
/// Acceso a datos de la tabla Productos.
/// Parte 1 (Participante 5): Crear y Consultar.
/// Parte 2 (Participante 6): Actualizar y Eliminar.
/// </summary>
public class ProductoRepository
{
    // ---------- CREAR (HU-01) ----------
    public void Insertar(Producto p)
    {
        const string sql = @"INSERT INTO dbo.Productos (Codigo, Nombre, Categoria, Precio, Cantidad)
                             VALUES (@Codigo, @Nombre, @Categoria, @Precio, @Cantidad);";

        using var cn = Conexion.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Codigo", p.Codigo);
        cmd.Parameters.AddWithValue("@Nombre", p.Nombre);
        cmd.Parameters.AddWithValue("@Categoria", p.Categoria);
        cmd.Parameters.AddWithValue("@Precio", p.Precio);
        cmd.Parameters.AddWithValue("@Cantidad", p.Cantidad);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    // ---------- CONSULTAR (HU-02) ----------
    public List<Producto> ObtenerTodos()
    {
        const string sql = @"SELECT IdProducto, Codigo, Nombre, Categoria, Precio, Cantidad
                             FROM dbo.Productos
                             ORDER BY Nombre;";

        var lista = new List<Producto>();
        using var cn = Conexion.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cn.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            lista.Add(new Producto
            {
                IdProducto = dr.GetInt32(0),
                Codigo = dr.GetString(1),
                Nombre = dr.GetString(2),
                Categoria = dr.GetString(3),
                Precio = dr.GetDecimal(4),
                Cantidad = dr.GetInt32(5)
            });
        }
        return lista;
    }

    // ---------- Apoyo a validación de código único (HU-07) ----------
    public bool ExisteCodigo(string codigo)
    {
        const string sql = "SELECT COUNT(1) FROM dbo.Productos WHERE Codigo = @Codigo;";

        using var cn = Conexion.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Codigo", codigo);
        cn.Open();
        return (int)cmd.ExecuteScalar()! > 0;
    }

    // =====================================================================
    // Parte 2 (Participante 6 - Starlyn Castillo): Actualizar y Eliminar
    // =====================================================================

    // ---------- MODIFICAR (HU-03) ----------
    /// <summary>
    /// Actualiza todos los datos del producto indicado por IdProducto.
    /// Devuelve false si el producto ya no existe.
    /// </summary>
    public bool Actualizar(Producto p)
    {
        const string sql = @"UPDATE dbo.Productos
                             SET Codigo = @Codigo, Nombre = @Nombre, Categoria = @Categoria,
                                 Precio = @Precio, Cantidad = @Cantidad
                             WHERE IdProducto = @IdProducto;";

        using var cn = Conexion.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@IdProducto", p.IdProducto);
        cmd.Parameters.AddWithValue("@Codigo", p.Codigo);
        cmd.Parameters.AddWithValue("@Nombre", p.Nombre);
        cmd.Parameters.AddWithValue("@Categoria", p.Categoria);
        cmd.Parameters.AddWithValue("@Precio", p.Precio);
        cmd.Parameters.AddWithValue("@Cantidad", p.Cantidad);
        cn.Open();
        return cmd.ExecuteNonQuery() > 0;
    }

    // ---------- ELIMINAR (HU-04) ----------
    /// <summary>
    /// Elimina el producto indicado. Devuelve false si el producto ya no existe.
    /// </summary>
    public bool Eliminar(int idProducto)
    {
        const string sql = "DELETE FROM dbo.Productos WHERE IdProducto = @IdProducto;";

        using var cn = Conexion.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@IdProducto", idProducto);
        cn.Open();
        return cmd.ExecuteNonQuery() > 0;
    }

    // ---------- Código único al modificar (HU-07) ----------
    /// <summary>
    /// Indica si el código ya lo usa OTRO producto (ignora el producto que se está editando).
    /// </summary>
    public bool ExisteCodigoEnOtro(string codigo, int idProducto)
    {
        const string sql = "SELECT COUNT(1) FROM dbo.Productos WHERE Codigo = @Codigo AND IdProducto <> @IdProducto;";

        using var cn = Conexion.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Codigo", codigo);
        cmd.Parameters.AddWithValue("@IdProducto", idProducto);
        cn.Open();
        return (int)cmd.ExecuteScalar()! > 0;
    }

    // ---------- Apoyo a pruebas ----------
    /// <summary>Busca un producto por su código. Devuelve null si no existe.</summary>
    public Producto? ObtenerPorCodigo(string codigo)
    {
        const string sql = @"SELECT IdProducto, Codigo, Nombre, Categoria, Precio, Cantidad
                             FROM dbo.Productos
                             WHERE Codigo = @Codigo;";

        using var cn = Conexion.Crear();
        using var cmd = new SqlCommand(sql, cn);
        cmd.Parameters.AddWithValue("@Codigo", codigo);
        cn.Open();
        using var dr = cmd.ExecuteReader();
        if (!dr.Read()) return null;

        return new Producto
        {
            IdProducto = dr.GetInt32(0),
            Codigo = dr.GetString(1),
            Nombre = dr.GetString(2),
            Categoria = dr.GetString(3),
            Precio = dr.GetDecimal(4),
            Cantidad = dr.GetInt32(5)
        };
    }
}
