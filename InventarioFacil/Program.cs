using InventarioFacil.Forms;

namespace InventarioFacil;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new FrmPrincipal());
    }
}
