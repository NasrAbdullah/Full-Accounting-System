using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using AccountingSystem.Data;
using AccountingSystem.Theme;
using System.Data.SqlClient;

namespace AccountingSystem.Helpers
{
    /// <summary>
    /// طباعة الفواتير (بيع / شراء)
    /// </summary>
    public class InvoicePrinter
    {
        private int invoiceId;
        private string invoiceType; // "Sale" or "Purchase"
        private string companyName = "";
        private string companyAddress = "";
        private string companyPhone = "";
        private string currency = "ر.ي";
        private string footerText = "";

        private DataTable headerData;
        private DataTable itemsData;

        private int currentItemIndex = 0;
        private int yPos = 0;

        public InvoicePrinter(int id, string type)
        {
            invoiceId = id;
            invoiceType = type;
            LoadCompanySettings();
            LoadInvoiceData();
        }

        private void LoadCompanySettings()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new SqlCommand(
                    "SELECT CompanyName, Address, Phone, Currency, InvoiceFooter " +
                    "FROM CompanySettings WHERE SettingId=1", conn))
                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        companyName = rd.IsDBNull(0) ? "شركتي" : rd.GetString(0);
                        companyAddress = rd.IsDBNull(1) ? "" : rd.GetString(1);
                        companyPhone = rd.IsDBNull(2) ? "" : rd.GetString(2);
                        currency = rd.IsDBNull(3) ? "ر.ي" : rd.GetString(3);
                        footerText = rd.IsDBNull(4) ? "" : rd.GetString(4);
                    }
                }
            }
            catch { }
        }

        private void LoadInvoiceData()
        {
            string sqlHeader, sqlItems;

            if (invoiceType == "Sale")
            {
                sqlHeader = @"
                    SELECT s.InvoiceNumber, s.SaleDate, c.Name AS PartyName,
                           c.Phone AS PartyPhone, c.Address AS PartyAddress,
                           s.Subtotal, s.Discount, s.Tax, s.GrandTotal, s.Paid,
                           s.Remaining, s.PaymentMethod, s.Notes
                    FROM Sales s
                    JOIN Customers c ON s.CustomerId = c.CustomerId
                    WHERE s.SaleId = @id";

                sqlItems = @"
                    SELECT p.ProductCode, p.Name AS ProductName,
                           si.Quantity, si.SalePrice, si.Discount, si.Total
                    FROM SaleItems si
                    JOIN Products p ON si.ProductId = p.ProductId
                    WHERE si.SaleId = @id";
            }
            else
            {
                sqlHeader = @"
                    SELECT p.InvoiceNumber, p.PurchaseDate, s.Name AS PartyName,
                           s.Phone AS PartyPhone, s.Address AS PartyAddress,
                           p.Subtotal, p.Discount, p.Tax, p.GrandTotal, p.Paid,
                           p.Remaining, p.PaymentMethod, p.Notes
                    FROM Purchases p
                    JOIN Suppliers s ON p.SupplierId = s.SupplierId
                    WHERE p.PurchaseId = @id";

                sqlItems = @"
                    SELECT pr.ProductCode, pr.Name AS ProductName,
                           pi.Quantity, pi.PurchasePrice AS SalePrice, pi.Discount, pi.Total
                    FROM PurchaseItems pi
                    JOIN Products pr ON pi.ProductId = pr.ProductId
                    WHERE pi.PurchaseId = @id";
            }

            using (var conn = DatabaseConnection.GetConnection())
            {
                using (var cmd = new SqlCommand(sqlHeader, conn))
                {
                    cmd.Parameters.AddWithValue("@id", invoiceId);
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        headerData = new DataTable();
                        da.Fill(headerData);
                    }
                }
                using (var cmd = new SqlCommand(sqlItems, conn))
                {
                    cmd.Parameters.AddWithValue("@id", invoiceId);
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        itemsData = new DataTable();
                        da.Fill(itemsData);
                    }
                }
            }
        }

        public void Print()
        {
            var pd = new PrintDocument();
            pd.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
            pd.PrintPage += Pd_PrintPage;

            var preview = new PrintPreviewDialog();
            preview.Document = pd;
            preview.Width = 900;
            preview.Height = 700;
            preview.RightToLeft = RightToLeft.Yes;
            preview.Text = "معاينة الطباعة";
            preview.ShowDialog();
        }

        public void PrintDirect()
        {
            var pd = new PrintDocument();
            pd.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
            pd.PrintPage += Pd_PrintPage;

            var dlg = new PrintDialog();
            dlg.Document = pd;
          
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                pd.Print();
            }
        }

        private void Pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int pageWidth = e.PageBounds.Width;
            int margin = 50;
            int y = margin;

            Font titleFont = new Font("Arial", 20F, FontStyle.Bold);
            Font subFont = new Font("Arial", 11F, FontStyle.Bold);
            Font regularFont = new Font("Arial", 10F);
            Font smallFont = new Font("Arial", 9F);
            Font headerFont = new Font("Arial", 10F, FontStyle.Bold);

            Brush blackBrush = Brushes.Black;
            Brush grayBrush = Brushes.DimGray;

            var rightFormat = new StringFormat();
            rightFormat.Alignment = StringAlignment.Far;
            rightFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;

            var leftFormat = new StringFormat();
            leftFormat.Alignment = StringAlignment.Near;

            var centerFormat = new StringFormat();
            centerFormat.Alignment = StringAlignment.Center;

            // ============ HEADER ============
            // Company name (right aligned for RTL)
            g.DrawString(companyName, titleFont, blackBrush,
                new RectangleF(margin, y, pageWidth - margin * 2, 40), rightFormat);
            y += 42;

            // Company info
            string infoLine = "";
            if (!string.IsNullOrEmpty(companyPhone)) infoLine += "هاتف: " + companyPhone + "   ";
            if (!string.IsNullOrEmpty(companyAddress)) infoLine += "العنوان: " + companyAddress;
            g.DrawString(infoLine, smallFont, grayBrush,
                new RectangleF(margin, y, pageWidth - margin * 2, 20), rightFormat);
            y += 25;

            // Divider line
            using (var pen = new Pen(Color.Black, 2))
                g.DrawLine(pen, margin, y, pageWidth - margin, y);
            y += 15;

            // ============ INVOICE TITLE ============
            string invoiceTitle = invoiceType == "Sale" ? "فاتورة بيع" : "فاتورة شراء";
            g.DrawString(invoiceTitle, subFont, blackBrush,
                new RectangleF(margin, y, pageWidth - margin * 2, 30), centerFormat);
            y += 35;

            // ============ INVOICE INFO ============
            if (headerData == null || headerData.Rows.Count == 0) return;
            var header = headerData.Rows[0];

            string invoiceNum = "رقم الفاتورة: " + header["InvoiceNumber"];
            string invoiceDate = "التاريخ: " + Convert.ToDateTime(header["SaleDate"]).ToString("yyyy/MM/dd");

            g.DrawString(invoiceNum, regularFont, blackBrush,
                new RectangleF(pageWidth / 2, y, pageWidth / 2 - margin, 22), rightFormat);
            g.DrawString(invoiceDate, regularFont, blackBrush,
                new RectangleF(margin, y, pageWidth / 2 - margin, 22), leftFormat);
            y += 26;

            // Party info
            string partyLabel = invoiceType == "Sale" ? "العميل" : "المورد";
            string partyInfo = partyLabel + ": " + header["PartyName"];
            g.DrawString(partyInfo, regularFont, blackBrush,
                new RectangleF(margin, y, pageWidth - margin * 2, 22), rightFormat);
            y += 22;

            string partyContact = "";
            if (header["PartyPhone"] != DBNull.Value && !string.IsNullOrEmpty(header["PartyPhone"].ToString()))
                partyContact += "هاتف: " + header["PartyPhone"] + "   ";
            if (header["PartyAddress"] != DBNull.Value && !string.IsNullOrEmpty(header["PartyAddress"].ToString()))
                partyContact += "العنوان: " + header["PartyAddress"];

            if (!string.IsNullOrEmpty(partyContact))
            {
                g.DrawString(partyContact, smallFont, grayBrush,
                    new RectangleF(margin, y, pageWidth - margin * 2, 20), rightFormat);
                y += 22;
            }

            y += 10;

            // ============ ITEMS TABLE ============
            int tableLeft = margin;
            int tableRight = pageWidth - margin;
            int tableWidth = tableRight - tableLeft;

            int colWidth1 = 50;   // #
            int colWidth2 = (int)(tableWidth * 0.35);  // Product name
            int colWidth3 = (int)(tableWidth * 0.15);  // Qty
            int colWidth4 = (int)(tableWidth * 0.15);  // Price
            int colWidth5 = (int)(tableWidth * 0.15);  // Discount
            int colWidth6 = (int)(tableWidth * 0.15);  // Total

            // Header row background
            using (var brush = new SolidBrush(Color.FromArgb(230, 230, 230)))
            {
                g.FillRectangle(brush, tableLeft, y, tableWidth, 30);
            }

            // Header row border
            using (var pen = new Pen(Color.Black, 1))
            {
                g.DrawRectangle(pen, tableLeft, y, tableWidth, 30);
            }

            // Header columns
            int x = tableRight;
            g.DrawString("#", headerFont, blackBrush, x - colWidth1 + 5, y + 8);
            x -= colWidth1;

            g.DrawString("الصنف", headerFont, blackBrush, x - colWidth2 + 5, y + 8);
            x -= colWidth2;

            g.DrawString("الكمية", headerFont, blackBrush, x - colWidth3 + 5, y + 8);
            x -= colWidth3;

            g.DrawString("السعر", headerFont, blackBrush, x - colWidth4 + 5, y + 8);
            x -= colWidth4;

            g.DrawString("الخصم", headerFont, blackBrush, x - colWidth5 + 5, y + 8);
            x -= colWidth5;

            g.DrawString("الإجمالي", headerFont, blackBrush, x - colWidth6 + 5, y + 8);

            y += 30;

            // Item rows
            int rowNum = 1;
            foreach (DataRow item in itemsData.Rows)
            {
                int rowHeight = 28;

                // Row background (alternating)
                if (rowNum % 2 == 0)
                {
                    using (var brush = new SolidBrush(Color.FromArgb(248, 248, 248)))
                    {
                        g.FillRectangle(brush, tableLeft, y, tableWidth, rowHeight);
                    }
                }

                using (var pen = new Pen(Color.Gray, 1))
                {
                    g.DrawRectangle(pen, tableLeft, y, tableWidth, rowHeight);
                }

                x = tableRight;
                g.DrawString(rowNum.ToString(), regularFont, blackBrush, x - colWidth1 + 5, y + 6);
                x -= colWidth1;

                string prodName = item["ProductName"].ToString();
                if (prodName.Length > 30) prodName = prodName.Substring(0, 30);
                g.DrawString(prodName, regularFont, blackBrush, x - colWidth2 + 5, y + 6);
                x -= colWidth2;

                g.DrawString(Convert.ToDecimal(item["Quantity"]).ToString("N2"),
                    regularFont, blackBrush, x - colWidth3 + 5, y + 6);
                x -= colWidth3;

                g.DrawString(Convert.ToDecimal(item["SalePrice"]).ToString("N2"),
                    regularFont, blackBrush, x - colWidth4 + 5, y + 6);
                x -= colWidth4;

                g.DrawString(Convert.ToDecimal(item["Discount"]).ToString("N2"),
                    regularFont, blackBrush, x - colWidth5 + 5, y + 6);
                x -= colWidth5;

                g.DrawString(Convert.ToDecimal(item["Total"]).ToString("N2"),
                    regularFont, blackBrush, x - colWidth6 + 5, y + 6);

                y += rowHeight;
                rowNum++;
            }

            y += 15;

            // ============ TOTALS ============
            int totalX = pageWidth - margin - 250;
            int totalWidth = 250;

            DrawTotalLine(g, ref y, totalX, totalWidth, "المجموع الفرعي:",
                Convert.ToDecimal(header["Subtotal"]).ToString("N2") + " " + currency,
                regularFont, blackBrush);

            if (Convert.ToDecimal(header["Discount"]) > 0)
            {
                DrawTotalLine(g, ref y, totalX, totalWidth, "الخصم:",
                    "-" + Convert.ToDecimal(header["Discount"]).ToString("N2") + " " + currency,
                    regularFont, Brushes.Red);
            }

            if (Convert.ToDecimal(header["Tax"]) > 0)
            {
                DrawTotalLine(g, ref y, totalX, totalWidth, "الضريبة:",
                    Convert.ToDecimal(header["Tax"]).ToString("N2") + " " + currency,
                    regularFont, blackBrush);
            }

            // Grand total with border
            using (var brush = new SolidBrush(Color.FromArgb(230, 230, 230)))
            {
                g.FillRectangle(brush, totalX, y, totalWidth, 32);
            }
            using (var pen = new Pen(Color.Black, 2))
            {
                g.DrawRectangle(pen, totalX, y, totalWidth, 32);
            }

            g.DrawString("الإجمالي:", headerFont, blackBrush, totalX + totalWidth - 80, y + 8);
            g.DrawString(Convert.ToDecimal(header["GrandTotal"]).ToString("N2") + " " + currency,
                headerFont, blackBrush, totalX + 5, y + 8);
            y += 40;

            DrawTotalLine(g, ref y, totalX, totalWidth, "المدفوع:",
                Convert.ToDecimal(header["Paid"]).ToString("N2") + " " + currency,
                regularFont, Brushes.Green);

            if (Convert.ToDecimal(header["Remaining"]) > 0)
            {
                DrawTotalLine(g, ref y, totalX, totalWidth, "المتبقي:",
                    Convert.ToDecimal(header["Remaining"]).ToString("N2") + " " + currency,
                    regularFont, Brushes.Red);
            }

            DrawTotalLine(g, ref y, totalX, totalWidth, "طريقة الدفع:",
                header["PaymentMethod"] != DBNull.Value ? header["PaymentMethod"].ToString() : "-",
                regularFont, blackBrush);

            y += 30;

            // ============ FOOTER ============
            using (var pen = new Pen(Color.Black, 1))
                g.DrawLine(pen, margin, y, pageWidth - margin, y);
            y += 10;

            string footer = string.IsNullOrEmpty(footerText) ? "شكراً لتعاملكم معنا" : footerText;
            g.DrawString(footer, regularFont, grayBrush,
                new RectangleF(margin, y, pageWidth - margin * 2, 20), centerFormat);
            y += 25;

            g.DrawString("تمت الطباعة في: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm"),
                smallFont, grayBrush,
                new RectangleF(margin, y, pageWidth - margin * 2, 20), centerFormat);

            e.HasMorePages = false;
        }

        private void DrawTotalLine(Graphics g, ref int y, int x, int width,
            string label, string value, Font font, Brush brush)
        {
            g.DrawString(label, font, Brushes.Black, x + width - 100, y);
            g.DrawString(value, font, brush, x + 5, y);
            y += 24;
        }

        /// <summary>
        /// طباعة مباشرة لفاتورة بيع
        /// </summary>
        public static void PrintSale(int saleId)
        {
            var p = new InvoicePrinter(saleId, "Sale");
            p.Print();
        }

        /// <summary>
        /// طباعة مباشرة لفاتورة شراء
        /// </summary>
        public static void PrintPurchase(int purchaseId)
        {
            var p = new InvoicePrinter(purchaseId, "Purchase");
            p.Print();
        }
    }
}