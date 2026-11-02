using Microsoft.Data.SqlClient;

namespace InventarioFacil.Data;

/// <summary>
/// Centraliza la cadena de conexión a SQL Server.
/// Si tu instancia es otra (por ejemplo .\SQLEXPRESS), cambia solo el valor de Server.
/// </summary>
public static class Conexion
{
    private const string CadenaConexion =
        "Server=(localdb)\\MSSQLLocalDB;Database=InventarioFacil;Trusted_Connection=True;TrustServerCertificate=True;";

    public static SqlConnection Crear() => new SqlConnection(CadenaConexion);

    /// <summary>Prueba que la app logra conectarse a SQL Server.</summary>
    public static bool Probar(out string mensaje)
    {
        try
        {
            using var cn = Crear();
            cn.Open();
            mensaje = "Conexión exitosa.";
            return true;
        }
        catch (Exception ex)
        {
            mensaje = "No se pudo conectar a SQL Server: " + ex.Message;
            return false;
        }
    }
}
