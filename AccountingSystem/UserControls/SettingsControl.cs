using System;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Data;
using AccountingSystem.Helpers;
using AccountingSystem.Theme;
using System.Data.SqlClient;

namespace AccountingSystem.UserControls
{
    public class SettingsControl : UserControl
    {
        private TextBox txtName;
        private TextBox txtAddress;
        private TextBox txtPhone;
        private TextBox txtEmail;
        private TextBox txtCurrency;
        private NumericUpDown numTax;
        private TextBox txtFooter;

        public SettingsControl()
        {
            RightToLeft = RightToLeft.Yes;
            BackColor = ThemeManager.Background;
            Dock = DockStyle.Fill;
            AutoScroll = true;
            Build();
            LoadData();
        }

        private void Build()
        {
            var main = new Panel();
            main.Dock = DockStyle.Fill;
            main.BackColor = ThemeManager.Background;
            main.Padding = new Padding(30, 20, 30, 20);
            main.AutoScroll = true;
            main.RightToLeft = RightToLeft.Yes;
            Controls.Add(main);

            var title = new Label();
            title.Text = "⚙ إعدادات الشركة";
            title.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            title.ForeColor = ThemeManager.Text;
            title.Dock = DockStyle.Top;
            title.Height = 50;
            title.TextAlign = ContentAlignment.TopCenter;
            title.RightToLeft = RightToLeft.Yes;
            title.Padding = new Padding(0, 0, 20, 0);
            main.Controls.Add(title);

            // Card
            var card = new Panel();
            card.Dock = DockStyle.Top;
            card.Height = 500;
            card.BackColor = ThemeManager.Panel;
            card.Padding = new Padding(25, 20, 25, 20);
            card.RightToLeft = RightToLeft.Yes;
            main.Controls.Add(card);
            card.BringToFront();

            // Table
            var table = new TableLayoutPanel();
            table.Dock = DockStyle.Top;
            table.Height = 460;
            table.ColumnCount = 2;
            table.RowCount = 8;
            table.BackColor = ThemeManager.Panel;
            table.RightToLeft = RightToLeft.Yes;
            table.Padding = new Padding(10);

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180f));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400f));

            for (int i = 0; i < 8; i++)
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 52f));

            card.Controls.Add(table);

            int row = 0;

            // اسم الشركة
            table.Controls.Add(MakeLabel("اسم الشركة:"), 0, row);
            txtName = MakeTextBox();
            table.Controls.Add(txtName, 1, row);
            row++;

            // العنوان
            table.Controls.Add(MakeLabel("العنوان:"), 0, row);
            txtAddress = MakeTextBox();
            table.Controls.Add(txtAddress, 1, row);
            row++;

            // الهاتف
            table.Controls.Add(MakeLabel("الهاتف:"), 0, row);
            txtPhone = MakeTextBox();
            table.Controls.Add(txtPhone, 1, row);
            row++;

            // البريد
            table.Controls.Add(MakeLabel("البريد الإلكتروني:"), 0, row);
            txtEmail = MakeTextBox();
            table.Controls.Add(txtEmail, 1, row);
            row++;

            // العملة
            table.Controls.Add(MakeLabel("العملة:"), 0, row);
            txtCurrency = MakeTextBox();
            table.Controls.Add(txtCurrency, 1, row);
            row++;

            // الضريبة
            table.Controls.Add(MakeLabel("نسبة الضريبة %:"), 0, row);
            numTax = new NumericUpDown();
            numTax.Width = 400;
            numTax.DecimalPlaces = 2;
            numTax.Maximum = 100;
            numTax.BackColor = ThemeManager.PanelAlt;
            numTax.ForeColor = ThemeManager.Text;
            numTax.RightToLeft = RightToLeft.Yes;
            numTax.Font = ThemeManager.FontRegular;
            numTax.Margin = new Padding(5);
            table.Controls.Add(numTax, 1, row);
            row++;

            // تذييل الفاتورة
            table.Controls.Add(MakeLabel("تذييل الفاتورة:"), 0, row);
            txtFooter = new TextBox();
            txtFooter.Width = 400;
            txtFooter.Height = 55;
            txtFooter.Multiline = true;
            ThemeManager.ApplyTextBox(txtFooter);
            txtFooter.Margin = new Padding(5);
            table.Controls.Add(txtFooter, 1, row);
            row++;

            // Buttons
            table.Controls.Add(new Label(), 0, row);

            var btnPanel = new Panel();
            btnPanel.Width = 400;
            btnPanel.Height = 45;
            btnPanel.RightToLeft = RightToLeft.Yes;
            btnPanel.Margin = new Padding(5);

            var btnTest = new Button();
            btnTest.Text = "🔌 اختبار الاتصال";
            btnTest.Width = 180;
            btnTest.Height = 40;
            btnTest.Location = new Point(0, 0);
            ThemeManager.ApplyGhostButton(btnTest);
            btnTest.Click += delegate
            {
                if (DatabaseConnection.TestConnection())
                    UiHelper.Info("الاتصال بقاعدة البيانات ناجح ✅");
                else
                    UiHelper.Error("فشل الاتصال بقاعدة البيانات ❌");
            };
            btnPanel.Controls.Add(btnTest);

            var btnSave = new Button();
            btnSave.Text = "💾 حفظ الإعدادات";
            btnSave.Width = 200;
            btnSave.Height = 40;
            btnSave.Location = new Point(190, 0);
            ThemeManager.ApplySuccessButton(btnSave);
            btnSave.Click += BtnSave_Click;
            btnPanel.Controls.Add(btnSave);

            table.Controls.Add(btnPanel, 1, row);
        }

        private Label MakeLabel(string text)
        {
            var lbl = new Label();
            lbl.Text = text;
            lbl.Dock = DockStyle.Fill;
            lbl.TextAlign = ContentAlignment.MiddleRight;
            lbl.ForeColor = ThemeManager.Text;
            lbl.Font = ThemeManager.FontRegular;
            lbl.BackColor = Color.Transparent;
            lbl.RightToLeft = RightToLeft.Yes;
            lbl.Padding = new Padding(0, 0, 10, 0);
            return lbl;
        }

        private TextBox MakeTextBox()
        {
            var txt = new TextBox();
            txt.Width = 400;
            txt.Height = 36;
            ThemeManager.ApplyTextBox(txt);
            txt.Margin = new Padding(5);
            return txt;
        }

        private void LoadData()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new SqlCommand(
                    "SELECT CompanyName, Address, Phone, Email, Currency, TaxRate, InvoiceFooter " +
                    "FROM CompanySettings WHERE SettingId=1", conn))
                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                    {
                        txtName.Text = rd.IsDBNull(0) ? "" : rd.GetString(0);
                        txtAddress.Text = rd.IsDBNull(1) ? "" : rd.GetString(1);
                        txtPhone.Text = rd.IsDBNull(2) ? "" : rd.GetString(2);
                        txtEmail.Text = rd.IsDBNull(3) ? "" : rd.GetString(3);
                        txtCurrency.Text = rd.IsDBNull(4) ? "ر.ي" : rd.GetString(4);
                        numTax.Value = rd.IsDBNull(5) ? 0 : rd.GetDecimal(5);
                        txtFooter.Text = rd.IsDBNull(6) ? "" : rd.GetString(6);
                    }
                }
            }
            catch (Exception ex)
            {
                UiHelper.Error("فشل تحميل الإعدادات: " + ex.Message);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                using (var cmd = new SqlCommand(@"
                    UPDATE CompanySettings SET
                        CompanyName=@n, Address=@a, Phone=@p, Email=@e,
                        Currency=@c, TaxRate=@t, InvoiceFooter=@f,
                        UpdatedAt=GETDATE()
                    WHERE SettingId=1", conn))
                {
                    cmd.Parameters.AddWithValue("@n", txtName.Text.Trim());
                    cmd.Parameters.AddWithValue("@a", txtAddress.Text.Trim());
                    cmd.Parameters.AddWithValue("@p", txtPhone.Text.Trim());
                    cmd.Parameters.AddWithValue("@e", txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@c", txtCurrency.Text.Trim());
                    cmd.Parameters.AddWithValue("@t", numTax.Value);
                    cmd.Parameters.AddWithValue("@f", txtFooter.Text.Trim());
                    cmd.ExecuteNonQuery();
                }
                UiHelper.Info("تم حفظ الإعدادات بنجاح ✅");
            }
            catch (Exception ex)
            {
                UiHelper.Error("فشل الحفظ: " + ex.Message);
            }
        }
    }
}