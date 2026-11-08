using System;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Helpers;
using AccountingSystem.Models;
using AccountingSystem.Repositories;
using AccountingSystem.Theme;

namespace AccountingSystem.UserControls
{
    public class IncomeControl : UserControl
    {
        private ComboBox cboCategory;
        private TextBox txtDesc;
        private NumericUpDown numAmount;
        private DateTimePicker dtpDate;
        private ComboBox cboMethod;
        private TextBox txtNotes;
        private DoubleBufferedGrid dgv;
        private Label lblTotal;

        private IncomeRepository repo = new IncomeRepository();

        public IncomeControl()
        {
            RightToLeft = RightToLeft.Yes;
            BackColor = ThemeManager.Background;
            Dock = DockStyle.Fill;
            Build();
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
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

            formPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
            formPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45f));
            formPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
            formPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60f));

            formPanel.Controls.Add(MakeLabel("التصنيف *:"), 0, 0);
            formPanel.Controls.Add(MakeLabel("المبلغ *:"), 2, 0);
            formPanel.Controls.Add(MakeLabel("التاريخ:"), 4, 0);

            cboCategory = new ComboBox();
            cboCategory.Dock = DockStyle.Fill;
            cboCategory.DropDownStyle = ComboBoxStyle.DropDown;
            ThemeManager.ApplyComboBox(cboCategory);
            cboCategory.Items.Add("مبيعات أخرى");
            cboCategory.Items.Add("خدمات");
            cboCategory.Items.Add("فوائد");
            cboCategory.Items.Add("أخرى");
            formPanel.Controls.Add(cboCategory, 1, 1);

            numAmount = new NumericUpDown();
            numAmount.Dock = DockStyle.Fill;
            numAmount.DecimalPlaces = 2;
            numAmount.Maximum = 999999999;
            numAmount.BackColor = ThemeManager.PanelAlt;
            numAmount.ForeColor = ThemeManager.Text;
            numAmount.RightToLeft = RightToLeft.Yes;
            formPanel.Controls.Add(numAmount, 3, 1);

            dtpDate = new DateTimePicker();
            dtpDate.Dock = DockStyle.Fill;
            dtpDate.Format = DateTimePickerFormat.Short;
            dtpDate.Value = DateTime.Now;
            formPanel.Controls.Add(dtpDate, 5, 1);

            formPanel.Controls.Add(MakeLabel("الوصف:"), 0, 2);
            formPanel.Controls.Add(MakeLabel("طريقة الاستلام:"), 2, 2);
            formPanel.Controls.Add(MakeLabel("ملاحظات:"), 4, 2);

            txtDesc = new TextBox();
            txtDesc.Dock = DockStyle.Fill;
            ThemeManager.ApplyTextBox(txtDesc);
            formPanel.Controls.Add(txtDesc, 1, 3);

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

            var actionBar = new Panel();
            actionBar.Dock = DockStyle.Top;
            actionBar.Height = 60;
            actionBar.BackColor = ThemeManager.Background;
            actionBar.Padding = new Padding(20, 10, 20, 10);
            actionBar.RightToLeft = RightToLeft.Yes;
            Controls.Add(actionBar);
            actionBar.BringToFront();

            var btnSave = new Button();
            btnSave.Text = "💾 حفظ";
            btnSave.Width = 150;
            btnSave.Height = 40;
            btnSave.Dock = DockStyle.Right;
            btnSave.Margin = new Padding(5);
            ThemeManager.ApplySuccessButton(btnSave);
            btnSave.Click += BtnSave_Click;
            actionBar.Controls.Add(btnSave);

            lblTotal = new Label();
            lblTotal.Text = "الإجمالي: 0.00";
            lblTotal.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTotal.ForeColor = ThemeManager.Success;
            lblTotal.Dock = DockStyle.Left;
            lblTotal.Width = 300;
            lblTotal.TextAlign = ContentAlignment.MiddleLeft;
            lblTotal.RightToLeft = RightToLeft.Yes;
            actionBar.Controls.Add(lblTotal);

            dgv = new DoubleBufferedGrid();
            dgv.Dock = DockStyle.Fill;
            dgv.RightToLeft = RightToLeft.Yes;
            ThemeManager.ApplyGrid(dgv);
            dgv.Columns.Add(Col("IncomeId", "ID", 60));
            dgv.Columns.Add(Col("IncomeDate", "التاريخ", 120));
            dgv.Columns.Add(Col("Category", "التصنيف", 120));
            dgv.Columns.Add(Col("Description", "الوصف", 250));
            dgv.Columns.Add(Col("Amount", "المبلغ", 120, "N2"));
            dgv.Columns.Add(Col("PaymentMethod", "طريقة الاستلام", 120));

            var delCol = new DataGridViewButtonColumn();
            delCol.Name = "Delete";
            delCol.HeaderText = "حذف";
            delCol.Text = "🗑";
            delCol.UseColumnTextForButtonValue = true;
            delCol.Width = 70;
            delCol.FlatStyle = FlatStyle.Flat;
            dgv.Columns.Add(delCol);

            dgv.CellContentClick += Dgv_CellContentClick;
            Controls.Add(dgv);
            dgv.BringToFront();
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

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cboCategory.Text))
            {
                UiHelper.Warn("اختر أو أدخل التصنيف.");
                return;
            }
            if (numAmount.Value <= 0)
            {
                UiHelper.Warn("المبلغ يجب أن يكون أكبر من صفر.");
                return;
            }

            try
            {
                var inc = new Income();
                inc.Category = cboCategory.Text.Trim();
                inc.Description = string.IsNullOrWhiteSpace(txtDesc.Text) ? null : txtDesc.Text.Trim();
                inc.Amount = numAmount.Value;
                inc.IncomeDate = dtpDate.Value;
                inc.PaymentMethod = cboMethod.SelectedItem.ToString();
                inc.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();

                repo.Add(inc);
                UiHelper.Info("تم الحفظ بنجاح ✅");
                numAmount.Value = 0;
                txtDesc.Text = "";
                txtNotes.Text = "";
                LoadData();
            }
            catch (Exception ex)
            {
                UiHelper.Error("فشل الحفظ: " + ex.Message);
            }
        }

        private void Dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgv.Columns[e.ColumnIndex].Name != "Delete") return;

            int id = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["IncomeId"].Value);
            if (!UiHelper.Confirm("هل أنت متأكد من حذف هذا الإيراد؟")) return;

            try { repo.Delete(id); LoadData(); }
            catch (Exception ex) { UiHelper.Error(ex.Message); }
        }

        private void LoadData()
        {
            try
            {
                var list = repo.GetAll(null, null);
                dgv.Rows.Clear();
                decimal total = 0;
                foreach (var i in list)
                {
                    dgv.Rows.Add(i.IncomeId, i.IncomeDate.ToString("yyyy-MM-dd"),
                        i.Category, i.Description, i.Amount, i.PaymentMethod);
                    total += i.Amount;
                }
                lblTotal.Text = "الإجمالي: " + total.ToString("N2");
            }
            catch { }
        }
    }
}