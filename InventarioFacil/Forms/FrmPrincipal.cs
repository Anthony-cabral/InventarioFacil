using InventarioFacil.Data;
using InventarioFacil.Models;

namespace InventarioFacil.Forms;

/// <summary>
/// Pantalla principal: formulario de datos del producto y grilla con el listado.
/// Parte 1 (Participante 5): Crear y Consultar.
/// Parte 2 (Participante 6): Modificar y Eliminar (seleccionando un producto del listado).
/// </summary>
public class FrmPrincipal : Form
{
    private readonly ProductoRepository _repo = new();

    private readonly TextBox txtCodigo = new() { Width = 200, MaxLength = 30 };
    private readonly TextBox txtNombre = new() { Width = 200, MaxLength = 100 };
    private readonly TextBox txtCategoria = new() { Width = 200, MaxLength = 60 };
    private readonly NumericUpDown nudPrecio = new() { Width = 200, DecimalPlaces = 2, Maximum = 99999999, Minimum = 0 };
    private readonly NumericUpDown nudCantidad = new() { Width = 200, Maximum = 1000000, Minimum = 0 };
    private readonly Button btnGuardar = new() { Text = "Guardar", Width = 85, Height = 32 };
    private readonly Button btnModificar = new() { Text = "Modificar", Width = 85, Height = 32, Enabled = false };
    private readonly Button btnEliminar = new() { Text = "Eliminar", Width = 85, Height = 32, Enabled = false };
    private readonly Button btnLimpiar = new() { Text = "Limpiar", Width = 85, Height = 32 };

    // Producto elegido en la grilla para modificar o eliminar (null = ninguno)
    private Producto? _seleccionado;
    private readonly DataGridView grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        RowHeadersVisible = false,
        BackgroundColor = SystemColors.Window
    };
    private readonly Label lblVacio = new()
    {
        Text = "No hay productos registrados.",
        Dock = DockStyle.Bottom,
        TextAlign = ContentAlignment.MiddleCenter,
        Height = 28,
        Visible = false
    };

    public FrmPrincipal()
    {
        Text = "Inventario Fácil";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(820, 560);
        MinimumSize = new Size(700, 480);

        ConstruirInterfaz();

        btnGuardar.Click += (_, _) => GuardarProducto();
        btnLimpiar.Click += (_, _) => LimpiarFormulario();
        btnModificar.Click += (_, _) => ModificarProducto();
        btnEliminar.Click += (_, _) => EliminarProducto();
        grid.CellClick += (_, e) => SeleccionarProducto(e.RowIndex);
        grid.DataBindingComplete += (_, _) => grid.ClearSelection();
        Load += (_, _) => AlCargar();
    }

    // ---------- Interfaz ----------
    private void ConstruirInterfaz()
    {
        var panelForm = new GroupBox { Text = "Datos del producto", Dock = DockStyle.Top, Height = 180, Padding = new Padding(12) };

        var tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 3 };
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        tabla.Controls.Add(Etiqueta("Código:"), 0, 0);
        tabla.Controls.Add(txtCodigo, 1, 0);
        tabla.Controls.Add(Etiqueta("Nombre:"), 2, 0);
        tabla.Controls.Add(txtNombre, 3, 0);
        tabla.Controls.Add(Etiqueta("Categoría:"), 0, 1);
        tabla.Controls.Add(txtCategoria, 1, 1);
        tabla.Controls.Add(Etiqueta("Precio:"), 2, 1);
        tabla.Controls.Add(nudPrecio, 3, 1);
        tabla.Controls.Add(Etiqueta("Cantidad:"), 0, 2);
        tabla.Controls.Add(nudCantidad, 1, 2);

        var botones = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
        botones.Controls.Add(btnGuardar);
        botones.Controls.Add(btnModificar);
        botones.Controls.Add(btnEliminar);
        botones.Controls.Add(btnLimpiar);
        tabla.Controls.Add(botones, 2, 2);
        tabla.SetColumnSpan(botones, 2);

        panelForm.Controls.Add(tabla);

        var panelGrid = new GroupBox { Text = "Listado de productos", Dock = DockStyle.Fill, Padding = new Padding(8) };
        panelGrid.Controls.Add(grid);
        panelGrid.Controls.Add(lblVacio);

        Controls.Add(panelGrid);
        Controls.Add(panelForm);
    }

    private static Label Etiqueta(string texto) =>
        new() { Text = texto, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 8, 3) };

    // ---------- Carga inicial ----------
    private void AlCargar()
    {
        if (!Conexion.Probar(out var mensaje))
        {
            MessageBox.Show(mensaje + "\n\nVerifica que SQL Server esté activo y que ejecutaste el script 01_CrearBaseDatos.sql.",
                "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            btnGuardar.Enabled = false;
            return;
        }
        CargarListado();
    }

    // ---------- Consultar (HU-02) ----------
    private void CargarListado()
    {
        try
        {
            var productos = _repo.ObtenerTodos();
            grid.DataSource = productos;

            if (grid.Columns.Contains("IdProducto"))
                grid.Columns["IdProducto"]!.Visible = false;
            if (grid.Columns.Contains("Precio"))
                grid.Columns["Precio"]!.DefaultCellStyle.Format = "N2";

            lblVacio.Visible = productos.Count == 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo cargar el listado: " + ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ---------- Crear (HU-01) con validaciones básicas (HU-07) ----------
    private void GuardarProducto()
    {
        var codigo = txtCodigo.Text.Trim();
        var nombre = txtNombre.Text.Trim();
        var categoria = txtCategoria.Text.Trim();

        if (codigo.Length == 0 || nombre.Length == 0 || categoria.Length == 0)
        {
            Aviso("Código, nombre y categoría son obligatorios.");
            return;
        }
        if (nudPrecio.Value < 0)
        {
            Aviso("El precio no puede ser negativo.");
            return;
        }
        if (nudCantidad.Value < 0)
        {
            Aviso("La cantidad no puede ser negativa.");
            return;
        }

        try
        {
            if (_repo.ExisteCodigo(codigo))
            {
                Aviso("Ya existe un producto con ese código.");
                txtCodigo.Focus();
                return;
            }

            _repo.Insertar(new Producto
            {
                Codigo = codigo,
                Nombre = nombre,
                Categoria = categoria,
                Precio = nudPrecio.Value,
                Cantidad = (int)nudCantidad.Value
            });

            MessageBox.Show("Producto guardado correctamente.", "Inventario Fácil",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
            CargarListado();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo guardar el producto: " + ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // =====================================================================
    // Parte 2 (Participante 6 - Starlyn Castillo): Modificar y Eliminar
    // =====================================================================

    // ---------- Seleccionar producto del listado (HU-03 / HU-04) ----------
    private void SeleccionarProducto(int fila)
    {
        if (fila < 0) return; // clic en el encabezado de la grilla
        if (grid.Rows[fila].DataBoundItem is not Producto p) return;

        _seleccionado = p;
        txtCodigo.Text = p.Codigo;
        txtNombre.Text = p.Nombre;
        txtCategoria.Text = p.Categoria;
        nudPrecio.Value = Math.Min(p.Precio, nudPrecio.Maximum);
        nudCantidad.Value = Math.Min(p.Cantidad, nudCantidad.Maximum);

        btnModificar.Enabled = true;
        btnEliminar.Enabled = true;
    }

    // ---------- Modificar (HU-03) con validaciones (HU-07) ----------
    private void ModificarProducto()
    {
        if (_seleccionado is null)
        {
            Aviso("Seleccione un producto del listado.");
            return;
        }

        var codigo = txtCodigo.Text.Trim();
        var nombre = txtNombre.Text.Trim();
        var categoria = txtCategoria.Text.Trim();

        if (codigo.Length == 0 || nombre.Length == 0 || categoria.Length == 0)
        {
            Aviso("Código, nombre y categoría son obligatorios.");
            return;
        }
        // Precio y cantidad negativos no se pueden escribir (Minimum = 0 en los NumericUpDown)
        // y además la tabla tiene CHECK (Precio >= 0) y CHECK (Cantidad >= 0).

        try
        {
            if (_repo.ExisteCodigoEnOtro(codigo, _seleccionado.IdProducto))
            {
                Aviso("Ya existe otro producto con ese código.");
                txtCodigo.Focus();
                return;
            }

            var actualizado = _repo.Actualizar(new Producto
            {
                IdProducto = _seleccionado.IdProducto,
                Codigo = codigo,
                Nombre = nombre,
                Categoria = categoria,
                Precio = nudPrecio.Value,
                Cantidad = (int)nudCantidad.Value
            });

            if (actualizado)
                MessageBox.Show("Producto modificado correctamente.", "Inventario Fácil",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                Aviso("El producto ya no existe en la base de datos.");

            LimpiarFormulario();
            CargarListado();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo modificar el producto: " + ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ---------- Eliminar (HU-04) con confirmación ----------
    private void EliminarProducto()
    {
        if (_seleccionado is null)
        {
            Aviso("Seleccione un producto del listado.");
            return;
        }

        var respuesta = MessageBox.Show(
            $"¿Seguro que desea eliminar el producto \"{_seleccionado.Nombre}\" ({_seleccionado.Codigo})?",
            "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (respuesta != DialogResult.Yes) return; // CP-06: al cancelar, el producto se conserva

        try
        {
            if (_repo.Eliminar(_seleccionado.IdProducto))
                MessageBox.Show("Producto eliminado correctamente.", "Inventario Fácil",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                Aviso("El producto ya no existe en la base de datos.");

            LimpiarFormulario();
            CargarListado();
        }
        catch (Exception ex)
        {
            MessageBox.Show("No se pudo eliminar el producto: " + ex.Message, "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LimpiarFormulario()
    {
        _seleccionado = null;
        btnModificar.Enabled = false;
        btnEliminar.Enabled = false;
        grid.ClearSelection();

        txtCodigo.Clear();
        txtNombre.Clear();
        txtCategoria.Clear();
        nudPrecio.Value = 0;
        nudCantidad.Value = 0;
        txtCodigo.Focus();
    }

    private static void Aviso(string texto) =>
        MessageBox.Show(texto, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
