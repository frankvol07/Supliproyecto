using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;

public class ImpresoraFactura
{
    private PrintDocument printDocument;
    private string clienteNombre;
    private string clienteDocumento;
    private string clienteID;
    private string subTotal;
    private string igv;
    private string total;
    private List<Tuple<string, string, string, string>> productos;

    public ImpresoraFactura(string clienteNombre, string clienteDocumento, string clienteID, string subTotal,
                            string igv, string total, List<Tuple<string, string, string, string>> productos)
    {
        this.clienteNombre = clienteNombre;
        this.clienteDocumento = clienteDocumento;
        this.clienteID = clienteID;
        this.subTotal = subTotal;
        this.igv = igv;
        this.total = total;
        this.productos = productos;

        // Configura el documento de impresión
        printDocument = new PrintDocument();
        printDocument.DefaultPageSettings.PaperSize = new PaperSize("Receipt", 300, 600); // 80mm aprox.
        printDocument.PrintPage += new PrintPageEventHandler(GenerarFactura);
    }

    public void Imprimir()
    {
        printDocument.Print(); // Llama directamente al método Print
    }

    private void GenerarFactura(object sender, PrintPageEventArgs e)
    {
        Graphics graphics = e.Graphics;
        Font font = new Font("Courier New", 8);  // Monospaced para alinear correctamente
        int yPosition = 0;
        int leftMargin = 10;

        graphics.DrawString("** FACTURA **", new Font("Courier New", 10, FontStyle.Bold), Brushes.Black, leftMargin, yPosition);
        yPosition += 20;

        graphics.DrawString($"Cliente: {clienteNombre}", font, Brushes.Black, leftMargin, yPosition);
        yPosition += 15;
        graphics.DrawString($"Documento: {clienteDocumento}", font, Brushes.Black, leftMargin, yPosition);
        yPosition += 15;
        graphics.DrawString($"ID Cliente: {clienteID}", font, Brushes.Black, leftMargin, yPosition);
        yPosition += 20;

        graphics.DrawString("Productos:", new Font("Courier New", 9, FontStyle.Bold), Brushes.Black, leftMargin, yPosition);
        yPosition += 15;

        foreach (var producto in productos)
        {
            string nombreProducto = producto.Item1;
            string cantidad = producto.Item2;
            string precio = producto.Item3;
            string totalProducto = producto.Item4;

            graphics.DrawString($"{nombreProducto}", font, Brushes.Black, leftMargin, yPosition);
            yPosition += 15;
            graphics.DrawString($"Cant: {cantidad} Precio: {precio} Total: {totalProducto}", font, Brushes.Black, leftMargin, yPosition);
            yPosition += 15;
        }

        yPosition += 20;
        graphics.DrawString($"Subtotal: {subTotal}", font, Brushes.Black, leftMargin, yPosition);
        yPosition += 15;
        graphics.DrawString($"IGV: {igv}", font, Brushes.Black, leftMargin, yPosition);
        yPosition += 15;
        graphics.DrawString($"Total: {total}", new Font("Courier New", 9, FontStyle.Bold), Brushes.Black, leftMargin, yPosition);
    }
}
public class FacturaData
{
    public string ClienteNombre { get; set; }
    public string ClienteDocumento { get; set; }
    public string ClienteID { get; set; }
    public string SubTotal { get; set; }
    public string IGV { get; set; }
    public string Total { get; set; }
    public List<ProductoData> Productos { get; set; }
}

public class ProductoData
{
    public string NombreProducto { get; set; }
    public string Cantidad { get; set; }
    public string Precio { get; set; }
    public string TotalProducto { get; set; }
}

