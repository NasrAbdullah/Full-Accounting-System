using System;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Helpers;
using AccountingSystem.Models;
using AccountingSystem.Repositories;
using AccountingSystem.Services;
using AccountingSystem.Theme;

namespace AccountingSystem.Forms.Sub
{
    public class SalesInvoiceForm : Form
    {
        private TextBox txtInvoice;
        private DateTimePicker dtpDate;
        private ComboBox cboCustomer;
        private ComboBox cboPayment;

        private DataGridView dgvItems;
        private ComboBox cboProduct;
        private TextBox txtBarcodeSearch;
        private NumericUpDown numQty;
        private NumericUpDown numPrice;
        private NumericUpDown numDisc;

        private NumericUpDown numInvoiceDisc;
        private NumericUpDown numTax;
        private NumericUpDown numPaid;

        private Label lblSubtotal;
        private Label lblDiscTotal;
        private Label lblTaxTotal;
        private Label lblGrand;
        private Label lblRemaining;

        private ProductRepository productRepo = new ProductRepository();
        private CustomerRepository customerRepo = new CustomerRepository();

        public SalesInvoiceForm()
        {
            ThemeManager.ApplyForm(this);
            Text = "فاتورة بيع جديدة";
            WindowState = FormWindowState.Maximized;
            BuildUi();
            LoadCombos();
            txtInvoice.Text = InvoiceNumberGenerator.NextSale();
            dtpDate.Value = DateTime.Now;
        }

        private void BuildUi()
        {
            var main = new TableLayoutPanel();
            main.Dock = DockStyle.Fill;
            main.ColumnCount = 1;
            main.RowCount = 4;
            main.BackColor = ThemeManager.Background;
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 220));
            Controls.Add(main);

            // HEADER
            var header = new Panel();
            header.Dock = DockStyle.Fill;
            header.BackColor = ThemeManager.Panel;
            header.Padding = new Padding(15);
            main.Controls.Add(header, 0, 0);

            AddLabel(header, "رقم الفاتورة:", 15, 20);
            txtInvoice = new TextBox();
            txtInvoice.Location = new Point(15, 42);
            txtInvoice.Width = 180;
            txtInvoice.ReadOnly = true;
            ThemeManager.ApplyTextBox(txtInvoice);
            header.Controls.Add(txtInvoice);

            AddLabel(header, "التاريخ:", 215, 20);
            dtpDate = new DateTimePicker();
            dtpDate.Location = new Point(215, 42);
            dtpDate.Width = 150;
            dtpDate.Format = DateTimePickerFormat.Short;
            header.Controls.Add(dtpDate);

            AddLabel(header, "العميل:", 385, 20);
            cboCustomer = new ComboBox();
            cboCustomer.Location = new Point(385, 42);
            cboCustomer.Width = 280;
            cboCustomer.DropDownStyle = ComboBoxStyle.DropDownList;
            ThemeManager.ApplyComboBox(cboCustomer);
            header.Controls.Add(cboCustomer);

            AddLabel(header, "طريقة الدفع:", 685, 20);
            cboPayment = new ComboBox();
            cboPayment.Location = new Point(685, 42);
            cboPayment.Width = 180;
            cboPayment.DropDownStyle = ComboBoxStyle.DropDownList;
            ThemeManager.ApplyComboBox(cboPayment);
            cboPayment.Items.Add("نقدي");
            cboPayment.Items.Add("آجل");
            cboPayment.Items.Add("شيك");
            cboPayment.Items.Add("تحويل");
            cboPayment.SelectedIndex = 0;
            header.Controls.Add(cboPayment);

            // PRODUCT ROW
            var prodPanel = new Panel();
            prodPanel.Dock = DockStyle.Fill;
            prodPanel.BackColor = ThemeManager.Panel;
            prodPanel.Padding = new Padding(15);
            main.Controls.Add(prodPanel, 0, 1);

            AddLabel(prodPanel, "المنتج:", 15, 10);
            cboProduct = new ComboBox();
            cboProduct.Location = new Point(15, 32);
            cboProduct.Width = 320;
            cboProduct.DropDownStyle = ComboBoxStyle.DropDownList;
            ThemeManager.ApplyComboBox(cboProduct);
            cboProduct.SelectedIndexChanged += OnProductSelected;
            prodPanel.Controls.Add(cboProduct);

            AddLabel(prodPanel, "الكمية:", 355, 10);
            numQty = MakeNum(355, 32, 100, 0);
            prodPanel.Controls.Add(numQty);

            AddLabel(prodPanel, "السعر:", 475, 10);
            numPrice = MakeNum(475, 32, 120, 2);
            prodPanel.Controls.Add(numPrice);

            AddLabel(prodPanel, "خصم:", 615, 10);
            numDisc = MakeNum(615, 32, 100, 2);
            prodPanel.Controls.Add(numDisc);

            var btnAdd = new Button();
            btnAdd.Text = "➕ إضافة";
            btnAdd.Location = new Point(735, 30);
            btnAdd.Width = 130;
            btnAdd.Height = 32;
            ThemeManager.ApplyPrimaryButton(btnAdd);
            btnAdd.Click += BtnAdd_Click;
            prodPanel.Controls.Add(btnAdd);

            var btnRemove = new Button();
            btnRemove.Text = "❌ حذف";
            btnRemove.Location = new Point(875, 30);
            btnRemove.Width = 130;
            btnRemove.Height = 32;
            ThemeManager.ApplyDangerButton(btnRemove);
            btnRemove.Click += BtnRemove_Click;
            prodPanel.Controls.Add(btnRemove);

            // GRID
            dgvItems = new DoubleBufferedGrid();
            dgvItems.Dock = DockStyle.Fill;
            ThemeManager.ApplyGrid(dgvItems);
            dgvItems.ReadOnly = false;
            dgvItems.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvItems.AllowUserToAddRows = true;
            dgvItems.AllowUserToDeleteRows = true;

            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductId", HeaderText = "ID", Visible = false });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductName", HeaderText = "المنتج", Width = 250, ReadOnly = true });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "الكمية", Width = 100 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "SalePrice", HeaderText = "السعر", Width = 110 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "PurchasePrice", HeaderText = "PP", Visible = false });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Discount", HeaderText = "الخصم", Width = 90 });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "Total", HeaderText = "الإجمالي", Width = 120, ReadOnly = true });

            dgvItems.CellEndEdit += DgvItems_CellEndEdit;
            dgvItems.RowsRemoved += delegate { RecalcTotals(); };

            main.Controls.Add(dgvItems, 0, 2);

            // FOOTER
            var footer = new Panel();
            footer.Dock = DockStyle.Fill;
            footer.BackColor = ThemeManager.Panel;
            footer.Padding = new Padding(15);
            main.Controls.Add(footer, 0, 3);

            int x = 20; int y = 15;

            AddLabel(footer, "خصم الفاتورة:", x, y);
            numInvoiceDisc = MakeNum(x, y + 25, 130, 2);
            numInvoiceDisc.ValueChanged += delegate { RecalcTotals(); };
            footer.Controls.Add(numInvoiceDisc); x += 150;

            AddLabel(footer, "الضريبة:", x, y);
            numTax = MakeNum(x, y + 25, 130, 2);
            numTax.ValueChanged += delegate { RecalcTotals(); };
            footer.Controls.Add(numTax); x += 150;

            AddLabel(footer, "المدفوع:", x, y);
            numPaid = MakeNum(x, y + 25, 140, 2);
            numPaid.ValueChanged += delegate { RecalcTotals(); };
            footer.Controls.Add(numPaid); x += 165;

            lblSubtotal = AddSummary(footer, "المجموع الفرعي:", ref x, y, Color.Empty);
            lblDiscTotal = AddSummary(footer, "إجمالي الخصم:", ref x, y, Color.Empty);
            lblTaxTotal = AddSummary(footer, "الضريبة:", ref x, y, Color.Empty);
            lblGrand = AddSummary(footer, "الإجمالي:", ref x, y, ThemeManager.Success);
            lblRemaining = AddSummary(footer, "المتبقي:", ref x, y, ThemeManager.Danger);

            int bx = 20; int by = 130;
            var btnSave = new Button();
            btnSave.Text = "💾 حفظ الفاتورة";
            btnSave.Location = new Point(bx, by);
            btnSave.Width = 200;
            btnSave.Height = 45;
            ThemeManager.ApplySuccessButton(btnSave);
            btnSave.Click += BtnSave_Click;
            footer.Controls.Add(btnSave);

            var btnCancel = new Button();
            btnCancel.Text = "❌ إلغاء";
            btnCancel.Location = new Point(bx + 220, by);
            btnCancel.Width = 140;
            btnCancel.Height = 45;
            ThemeManager.ApplyDangerButton(btnCancel);
            btnCancel.Click += delegate
            {
                if (UiHelper.Confirm("هل أنت متأكد من إلغاء الفاتورة؟")) Close();
            };
            footer.Controls.Add(btnCancel);

            var btnClear = new Button();
            btnClear.Text = "🔄 تفريغ";
            btnClear.Location = new Point(bx + 380, by);
            btnClear.Width = 140;
            btnClear.Height = 45;
            ThemeManager.ApplyGhostButton(btnClear);
            btnClear.Click += delegate { dgvItems.Rows.Clear(); RecalcTotals(); };
            footer.Controls.Add(btnClear);
        }

        private NumericUpDown MakeNum(int x, int y, int w, int dec)
        {
            var n = new NumericUpDown();
            n.Location = new Point(x, y);
            n.Width = w;
            n.DecimalPlaces = dec;
            n.Maximum = 999999999;
            n.BackColor = ThemeManager.PanelAlt;
            n.ForeColor = ThemeManager.Text;
            return n;
        }

        private void AddLabel(Control p, string text, int x, int y)
        {
            var lbl = new Label();
            lbl.Text = text;
            lbl.Location = new Point(x, y);
            lbl.AutoSize = true;
            lbl.ForeColor = ThemeManager.Muted;
            lbl.Font = ThemeManager.FontSmall;
            p.Controls.Add(lbl);
        }

        private Label AddSummary(Panel p, string title, ref int x, int y, Color color)
        {
            var lblT = new Label();
            lblT.Text = title;
            lblT.Location = new Point(x, y);
            lblT.AutoSize = true;
            lblT.ForeColor = ThemeManager.Muted;
            lblT.Font = ThemeManager.FontSmall;
            p.Controls.Add(lblT);

            var val = new Label();
            val.Text = "0.00";
            val.Location = new Point(x, y + 25);
            val.AutoSize = true;
            val.Font = ThemeManager.FontBold;
            val.ForeColor = color.IsEmpty ? ThemeManager.Text : color;
            p.Controls.Add(val);
            x += 150;
            return val;
        }

        private void LoadCombos()
        {
            cboCustomer.Items.Clear();
            try
            {
                foreach (var c in customerRepo.GetComboList())
                    cboCustomer.Items.Add(new ComboItem(c.CustomerId, c.Name));
            }
            catch { }
            if (cboCustomer.Items.Count > 0) cboCustomer.SelectedIndex = 0;

            cboProduct.Items.Clear();
            try
            {
                foreach (var p in productRepo.GetActiveList())
                    cboProduct.Items.Add(new ProductItem(p));
            }
            catch { }
        }
        private void BarcodeSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;

            string code = txtBarcodeSearch.Text.Trim();
            if (string.IsNullOrEmpty(code)) return;

            // ابحث في المنتجات
            foreach (var item in cboProduct.Items)
            {
                var pi = item as ProductItem;
                if (pi == null) continue;

                if (pi.Product.Barcode == code || pi.Product.ProductCode == code)
                {
                    cboProduct.SelectedItem = pi;
                    OnProductSelected(null, null);
                    numQty.Focus();
                    numQty.Select(0, numQty.Text.Length);
                    txtBarcodeSearch.Clear();
                    return;
                }
            }

            UiHelper.Warn("لم يتم العثور على منتج بهذا الباركود: " + code);
            txtBarcodeSearch.SelectAll();
        }


        private void OnProductSelected(object sender, EventArgs e)
        {
            var pi = cboProduct.SelectedItem as ProductItem;
            if (pi != null)
            {
                numPrice.Value = Math.Max(0, Math.Min(numPrice.Maximum, pi.Product.SalePrice));
                numQty.Value = 1;
                numDisc.Value = 0;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var pi = cboProduct.SelectedItem as ProductItem;
            if (pi == null) { UiHelper.Warn("اختر منتجاً أولاً"); return; }
            if (numQty.Value <= 0) { UiHelper.Warn("الكمية يجب أن تكون أكبر من صفر"); return; }

            var p = pi.Product;
            decimal qty = numQty.Value;
            decimal price = numPrice.Value;
            decimal disc = numDisc.Value;

            if (qty > p.Quantity)
            {
                UiHelper.Warn("الكمية المطلوبة غير متوفرة في المخزون.\nالمتاح: " + p.Quantity.ToString("N2"));
                return;
            }

            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (row.IsNewRow) continue;
                if (row.Cells["ProductId"].Value != null &&
                    (int)row.Cells["ProductId"].Value == p.ProductId)
                { UiHelper.Warn("المنتج مضاف بالفعل في الفاتورة"); return; }
            }

            decimal total = qty * price - disc;
            dgvItems.Rows.Add(p.ProductId, p.Name, qty, price, p.PurchasePrice, disc, total);
            RecalcTotals();

            numQty.Value = 0;
            numDisc.Value = 0;
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (dgvItems.CurrentRow == null || dgvItems.CurrentRow.IsNewRow) return;
            dgvItems.Rows.Remove(dgvItems.CurrentRow);
            RecalcTotals();
        }

        private void DgvItems_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvItems.Rows[e.RowIndex];
            if (row.IsNewRow) return;

            decimal qty;
            decimal price;
            decimal disc;
            decimal.TryParse(Convert.ToString(row.Cells["Quantity"].Value), out qty);
            decimal.TryParse(Convert.ToString(row.Cells["SalePrice"].Value), out price);
            decimal.TryParse(Convert.ToString(row.Cells["Discount"].Value), out disc);

            if (row.Cells["ProductId"].Value != null)
            {
                int prodId = (int)row.Cells["ProductId"].Value;
                try
                {
                    var p = productRepo.GetById(prodId);
                    if (p != null && qty > p.Quantity)
                    {
                        UiHelper.Warn("الكمية المطلوبة غير متوفرة في المخزون للمنتج: " + p.Name +
                            "\nالمتاح: " + p.Quantity.ToString("N2"));
                        row.Cells["Quantity"].Value = p.Quantity;
                        qty = p.Quantity;
                    }
                }
                catch { }
            }

            row.Cells["Total"].Value = qty * price - disc;
            RecalcTotals();
        }

        private void RecalcTotals()
        {
            decimal subtotal = 0, totalDisc = 0;
            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (row.IsNewRow) continue;
                decimal t;
                if (decimal.TryParse(Convert.ToString(row.Cells["Total"].Value), out t))
                    subtotal += t;
                decimal d;
                if (decimal.TryParse(Convert.ToString(row.Cells["Discount"].Value), out d))
                    totalDisc += d;
            }

            decimal invoiceDisc = numInvoiceDisc.Value;
            decimal tax = numTax.Value;
            decimal grand = subtotal - invoiceDisc + tax;
            if (grand < 0) grand = 0;

            decimal paid = numPaid.Value;
            decimal remaining = grand - paid;
            if (remaining < 0) remaining = 0;

            lblSubtotal.Text = subtotal.ToString("N2");
            lblDiscTotal.Text = totalDisc.ToString("N2");
            lblTaxTotal.Text = tax.ToString("N2");
            lblGrand.Text = grand.ToString("N2");
            lblRemaining.Text = remaining.ToString("N2");
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var ci = cboCustomer.SelectedItem as ComboItem;
            if (ci == null) { UiHelper.Warn("اختر عميلاً"); return; }

            bool hasRows = false;
            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (!row.IsNewRow && row.Cells["ProductId"].Value != null)
                { hasRows = true; break; }
            }
            if (!hasRows) { UiHelper.Warn("لا توجد منتجات في الفاتورة"); return; }

            string method = cboPayment.SelectedItem != null ? cboPayment.SelectedItem.ToString() : "نقدي";

            var sale = new Sale();
            sale.InvoiceNumber = txtInvoice.Text;
            sale.SaleDate = dtpDate.Value;
            sale.CustomerId = ci.Id;
            sale.PaymentMethod = method;
            sale.Discount = numInvoiceDisc.Value;
            sale.Tax = numTax.Value;
            if (Session.CurrentUser != null)
                sale.UserId = Session.CurrentUser.UserId;

            decimal subtotal = 0;
            foreach (DataGridViewRow row in dgvItems.Rows)
            {
                if (row.IsNewRow) continue;
                if (row.Cells["ProductId"].Value == null) continue;

                var it = new SaleItem();
                it.ProductId = (int)row.Cells["ProductId"].Value;
                it.ProductName = Convert.ToString(row.Cells["ProductName"].Value);
                it.Quantity = Convert.ToDecimal(row.Cells["Quantity"].Value ?? 0);
                it.SalePrice = Convert.ToDecimal(row.Cells["SalePrice"].Value ?? 0);
                it.Discount = Convert.ToDecimal(row.Cells["Discount"].Value ?? 0);
                it.Total = Convert.ToDecimal(row.Cells["Total"].Value ?? 0);

                sale.Items.Add(it);
                subtotal += it.Total;
            }

            sale.Subtotal = subtotal;
            sale.GrandTotal = subtotal - sale.Discount + sale.Tax;
            if (sale.GrandTotal < 0) sale.GrandTotal = 0;
            sale.Paid = numPaid.Value;
            sale.Remaining = sale.GrandTotal - sale.Paid;

            try
            {
                var service = new SaleService();
                service.CreateSale(sale);
                UiHelper.Info("تم حفظ الفاتورة بنجاح\nرقم: " + sale.InvoiceNumber);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                UiHelper.Error("فشل حفظ الفاتورة:\n" + ex.Message);
            }
        }

        private class ComboItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public ComboItem(int id, string name) { Id = id; Name = name; }
            public override string ToString() { return Name; }
        }

        private class ProductItem
        {
            public Product Product { get; set; }
            public ProductItem(Product p) { Product = p; }
            public override string ToString()
            {
                return Product.ProductCode + " - " + Product.Name +
                    " (متاح: " + Product.Quantity.ToString("N0") + ")";
            }
        }
    }
}