using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Data;
using AccountingSystem.Helpers;
using AccountingSystem.Models;
using AccountingSystem.Repositories;
using AccountingSystem.Services;
using AccountingSystem.Theme;
using System.Data.SqlClient;

namespace AccountingSystem.UserControls
{
    public class PaymentsControl : UserControl
    {
        private ComboBox cboType;
        private ComboBox cboCustomer;
        private ComboBox cboSupplier;
        private NumericUpDown numAmount;
        private DateTimePicker dtpDate;
        private ComboBox cboMethod;
        private TextBox txtNotes;
        private DoubleBufferedGrid dgv;

        private CustomerRepository customerRepo = new CustomerRepository();
        private SupplierRepository supplierRepo = new SupplierRepository();
        private PaymentService paymentService = new PaymentService();

        public PaymentsControl()
        {
            RightToLeft = RightToLeft.Yes;
            BackColor = ThemeManager.Background;
            Dock = DockStyle.Fill;
            Build();
            LoadCombos();
            LoadData();
        }

        private void Build()
        {
            var formPanel = new TableLayoutPanel();
            formPanel.Dock = DockStyle.Top;
            formPanel.Height = 200;
            formPanel.BackColor = ThemeManager.Panel;
            formPanel.Padding = new Padding(20);
            formPanel.RightToLeft = RightToLeft.Yes;
            formPanel.ColumnCount = 6;
            formPanel.RowCount = 4;

            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            formPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
            formPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45f));
            formPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
            formPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60f));

            formPanel.Controls.Add(MakeLabel("النوع:"), 0, 0);
            formPanel.Controls.Add(MakeLabel("التاريخ:"), 2, 0);
            formPanel.Controls.Add(MakeLabel("المبلغ *:"), 4, 0);

            cboType = new ComboBox();
            cboType.Dock = DockStyle.Fill;
            cboType.DropDownStyle = ComboBoxStyle.DropDownList;
            ThemeManager.ApplyComboBox(cboType);
            cboType.Items.Add("دفعة عميل");
            cboType.Items.Add("دفعة لمورد");
            cboType.SelectedIndex = 0;
            cboType.SelectedIndexChanged += delegate { TogglePartyField(); };
            formPanel.Controls.Add(cboType, 1, 1);

            dtpDate = new DateTimePicker();
            dtpDate.Dock = DockStyle.Fill;
            dtpDate.Format = DateTimePickerFormat.Short;
            dtpDate.Value = DateTime.Now;
            formPanel.Controls.Add(dtpDate, 3, 1);

            numAmount = new NumericUpDown();
            numAmount.Dock = DockStyle.Fill;
            numAmount.DecimalPlaces = 2;
            numAmount.Maximum = 999999999;
            numAmount.BackColor = ThemeManager.PanelAlt;
            numAmount.ForeColor = ThemeManager.Text;
            numAmount.RightToLeft = RightToLeft.Yes;
            formPanel.Controls.Add(numAmount, 5, 1);

            formPanel.Controls.Add(MakeLabel("الجهة:"), 0, 2);
            formPanel.Controls.Add(MakeLabel("طريقة الدفع:"), 2, 2);
            formPanel.Controls.Add(MakeLabel("ملاحظات:"), 4, 2);

            cboCustomer = new ComboBox();
            cboCustomer.Dock = DockStyle.Fill;
            cboCustomer.DropDownStyle = ComboBoxStyle.DropDownList;
            ThemeManager.ApplyComboBox(cboCustomer);
            formPanel.Controls.Add(cboCustomer, 1, 3);

            cboSupplier = new ComboBox();
            cboSupplier.Dock = DockStyle.Fill;
            cboSupplier.DropDownStyle = ComboBoxStyle.DropDownList;
            ThemeManager.ApplyComboBox(cboSupplier);
            cboSupplier.Visible = false;
            formPanel.Controls.Add(cboSupplier, 1, 3);

            cboMethod = new ComboBox();
            cboMethod.Dock = DockStyle.Fill;
            cboMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            ThemeManager.ApplyComboBox(cboMethod);
            cboMethod.Items.Add("نقدي");
            cboMethod.Items.Add("شيك");
            cboMethod.Items.Add("تحويل");
            cboMethod.SelectedIndex = 0;
            formPanel.Controls.Add(cboMethod, 3, 3);

            txtNotes = new TextBox();
            txtNotes.Dock = DockStyle.Fill;
            ThemeManager.ApplyTextBox(txtNotes);
            formPanel.Controls.Add(txtNotes, 5, 3);

            Controls.Add(formPanel);

            // Action bar
            var actionBar = new Panel();
            actionBar.Dock = DockStyle.Top;
            actionBar.Height = 60;
            actionBar.BackColor = ThemeManager.Background;
            actionBar.Padding = new Padding(20, 10, 20, 10);
            actionBar.RightToLeft = RightToLeft.Yes;
            Controls.Add(actionBar);
            actionBar.BringToFront();

            var btnSave = new Button();
            btnSave.Text = "💾 حفظ الدفعة";
            btnSave.Width = 180;
            btnSave.Height = 40;
            btnSave.Dock = DockStyle.Right;
            btnSave.Margin = new Padding(5);
            ThemeManager.ApplySuccessButton(btnSave);
            btnSave.Click += BtnSave_Click;
            actionBar.Controls.Add(btnSave);

            var btnExport = new Button();
            btnExport.Text = "📊 تصدير";
            btnExport.Width = 120;
            btnExport.Height = 40;
            btnExport.Dock = DockStyle.Right;
            btnExport.Margin = new Padding(5);
            ThemeManager.ApplyGhostButton(btnExport);
            btnExport.Click += delegate { ExcelExporter.ExportDataGridView(dgv, "Payments"); };
            actionBar.Controls.Add(btnExport);

            dgv = new DoubleBufferedGrid();
            dgv.Dock = DockStyle.Fill;
            dgv.RightToLeft = RightToLeft.Yes;
            ThemeManager.ApplyGrid(dgv);
            dgv.Columns.Add(Col("PaymentDate", "التاريخ", 120));
            dgv.Columns.Add(Col("Amount", "المبلغ", 120, "N2"));
            dgv.Columns.Add(Col("PaymentMethod", "طريقة الدفع", 120));
            dgv.Columns.Add(Col("Party", "الجهة", 250));
            dgv.Columns.Add(Col("Notes", "ملاحظات", 250));
            Controls.Add(dgv);
            dgv.BringToFront();

            TogglePartyField();
        }

        private Label MakeLabel(string text)
        {
            var lbl = new Label();
            lbl.Text = text;
            lbl.Dock = DockStyle.Fill;
            lbl.TextAlign = ContentAlignment.MiddleRight;
            lbl.ForeColor = ThemeManager.Muted;
            lbl.Font = ThemeManager.FontSmall;
            lbl.BackColor = Color.Transparent;
            lbl.RightToLeft = RightToLeft.Yes;
            lbl.Margin = new Padding(5);
            return lbl;
        }

        private DataGridViewTextBoxColumn Col(string prop, string header, int width, string format)
        {
            var c = new DataGridViewTextBoxColumn();
            c.DataPropertyName = prop;
            c.Name = prop;
            c.HeaderText = header;
            c.Width = width;
            if (!string.IsNullOrEmpty(format)) c.DefaultCellStyle.Format = format;
            return c;
        }

        private DataGridViewTextBoxColumn Col(string prop, string header, int width)
        {
            return Col(prop, header, width, null);
        }

        private void TogglePartyField()
        {
            bool isCustomer = cboType.SelectedIndex == 0;
            cboCustomer.Visible = isCustomer;
            cboSupplier.Visible = !isCustomer;
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

            cboSupplier.Items.Clear();
            try
            {
                foreach (var s in supplierRepo.GetComboList())
                    cboSupplier.Items.Add(new ComboItem(s.SupplierId, s.Name));
            }
            catch { }
            if (cboSupplier.Items.Count > 0) cboSupplier.SelectedIndex = 0;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (numAmount.Value <= 0)
            {
                UiHelper.Warn("المبلغ يجب أن يكون أكبر من صفر.");
                return;
            }

            try
            {
                if (cboType.SelectedIndex == 0)
                {
                    var ci = cboCustomer.SelectedItem as ComboItem;
                    if (ci == null) { UiHelper.Warn("اختر عميلاً"); return; }

                    var payment = new Payment();
                    payment.PaymentDate = dtpDate.Value;
                    payment.Amount = numAmount.Value;
                    payment.PaymentMethod = cboMethod.SelectedItem.ToString();
                    payment.CustomerId = ci.Id;
                    payment.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();

                    paymentService.AddCustomerPayment(payment);
                }
                else
                {
                    var si = cboSupplier.SelectedItem as ComboItem;
                    if (si == null) { UiHelper.Warn("اختر مورداً"); return; }

                    var payment = new Payment();
                    payment.PaymentDate = dtpDate.Value;
                    payment.Amount = numAmount.Value;
                    payment.PaymentMethod = cboMethod.SelectedItem.ToString();
                    payment.SupplierId = si.Id;
                    payment.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();

                    paymentService.AddSupplierPayment(payment);
                }

                UiHelper.Info("تم حفظ الدفعة بنجاح ✅");
                numAmount.Value = 0;
                txtNotes.Text = "";
                LoadData();
            }
            catch (Exception ex)
            {
                UiHelper.Error("فشل الحفظ: " + ex.Message);
            }
        }

        private void LoadData()
        {
            try
            {
                var dt = new DataTable();
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new SqlCommand(@"
                    SELECT TOP 100 p.PaymentDate, p.Amount, p.PaymentMethod,
                           ISNULL(c.Name, s.Name) AS Party,
                           p.Notes
                    FROM Payments p
                    LEFT JOIN Customers c ON p.CustomerId = c.CustomerId
                    LEFT JOIN Suppliers s ON p.SupplierId = s.SupplierId
                    ORDER BY p.PaymentId DESC", conn))
                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);

                dgv.Rows.Clear();
                foreach (DataRow r in dt.Rows)
                    dgv.Rows.Add(r["PaymentDate"], r["Amount"], r["PaymentMethod"],
                        r["Party"], r["Notes"]);
            }
            catch { }
        }

        private class ComboItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public ComboItem(int id, string name) { Id = id; Name = name; }
            public override string ToString() { return Name; }
        }
    }
}