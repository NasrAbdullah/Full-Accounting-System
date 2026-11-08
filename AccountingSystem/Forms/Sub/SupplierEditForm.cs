using System;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Helpers;
using AccountingSystem.Models;
using AccountingSystem.Repositories;
using AccountingSystem.Theme;

namespace AccountingSystem.Forms.Sub
{
    public class SupplierEditForm : Form
    {
        private Supplier editing;
        private SupplierRepository repo = new SupplierRepository();

        private TextBox txtName;
        private TextBox txtPhone;
        private TextBox txtEmail;
        private TextBox txtAddress;
        private NumericUpDown numOpening;
        private TextBox txtNotes;
        private CheckBox chkActive;
        private ErrorProvider err;

        public SupplierEditForm(Supplier supplier)
        {
            editing = supplier;
            ThemeManager.ApplyForm(this);
            Text = supplier == null ? "إضافة مورد" : "تعديل مورد";
            Size = new Size(480, 640);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BuildUi();
            if (supplier != null) LoadValues(supplier);
        }

        private void BuildUi()
        {
            err = new ErrorProvider();
            err.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            int y = 25;

            AddLabel("الاسم *", y); y += 22;
            txtName = new TextBox();
            txtName.Location = new Point(30, y);
            txtName.Width = 400;
            ThemeManager.ApplyTextBox(txtName);
            Controls.Add(txtName); y += 55;

            AddLabel("الهاتف", y); y += 22;
            txtPhone = new TextBox();
            txtPhone.Location = new Point(30, y);
            txtPhone.Width = 400;
            ThemeManager.ApplyTextBox(txtPhone);
            Controls.Add(txtPhone); y += 55;

            AddLabel("البريد الإلكتروني", y); y += 22;
            txtEmail = new TextBox();
            txtEmail.Location = new Point(30, y);
            txtEmail.Width = 400;
            ThemeManager.ApplyTextBox(txtEmail);
            Controls.Add(txtEmail); y += 55;

            AddLabel("العنوان", y); y += 22;
            txtAddress = new TextBox();
            txtAddress.Location = new Point(30, y);
            txtAddress.Width = 400;
            ThemeManager.ApplyTextBox(txtAddress);
            Controls.Add(txtAddress); y += 55;

            AddLabel("الرصيد الافتتاحي", y); y += 22;
            numOpening = new NumericUpDown();
            numOpening.Location = new Point(30, y);
            numOpening.Width = 400;
            numOpening.DecimalPlaces = 2;
            numOpening.Maximum = 999999999;
            numOpening.Minimum = -999999999;
            numOpening.BackColor = ThemeManager.PanelAlt;
            numOpening.ForeColor = ThemeManager.Text;
            Controls.Add(numOpening); y += 55;

            AddLabel("ملاحظات", y); y += 22;
            txtNotes = new TextBox();
            txtNotes.Location = new Point(30, y);
            txtNotes.Width = 400;
            txtNotes.Height = 60;
            txtNotes.Multiline = true;
            ThemeManager.ApplyTextBox(txtNotes);
            Controls.Add(txtNotes); y += 75;

            chkActive = new CheckBox();
            chkActive.Text = "نشط";
            chkActive.Location = new Point(30, y);
            chkActive.ForeColor = ThemeManager.Text;
            chkActive.Checked = true;
            Controls.Add(chkActive); y += 40;

            var btnSave = new Button();
            btnSave.Text = "💾 حفظ";
            btnSave.Location = new Point(30, y);
            btnSave.Width = 180;
            ThemeManager.ApplySuccessButton(btnSave);
            btnSave.Click += BtnSave_Click;
            Controls.Add(btnSave);

            var btnCancel = new Button();
            btnCancel.Text = "إلغاء";
            btnCancel.Location = new Point(220, y);
            btnCancel.Width = 180;
            ThemeManager.ApplyGhostButton(btnCancel);
            btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnCancel);
        }

        private void AddLabel(string text, int y)
        {
            var lbl = new Label();
            lbl.Text = text;
            lbl.Location = new Point(30, y);
            lbl.AutoSize = true;
            lbl.ForeColor = ThemeManager.Muted;
            lbl.Font = ThemeManager.FontSmall;
            Controls.Add(lbl);
        }

        private void LoadValues(Supplier s)
        {
            txtName.Text = s.Name;
            txtPhone.Text = s.Phone ?? "";
            txtEmail.Text = s.Email ?? "";
            txtAddress.Text = s.Address ?? "";
            numOpening.Value = Math.Max(numOpening.Minimum, Math.Min(numOpening.Maximum, s.OpeningBalance));
            txtNotes.Text = s.Notes ?? "";
            chkActive.Checked = s.IsActive;
        }

        private bool ValidateForm()
        {
            err.Clear();
            bool ok = true;
            if (!ValidationHelper.Required(txtName.Text))
            { err.SetError(txtName, "الاسم مطلوب"); ok = false; }
            if (!ValidationHelper.IsPhone(txtPhone.Text))
            { err.SetError(txtPhone, "رقم الهاتف غير صحيح"); ok = false; }
            if (!ValidationHelper.IsEmail(txtEmail.Text))
            { err.SetError(txtEmail, "البريد غير صحيح"); ok = false; }
            return ok;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            var s = editing != null ? editing : new Supplier();
            s.Name = txtName.Text.Trim();
            s.Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim();
            s.Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
            s.Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim();
            s.OpeningBalance = numOpening.Value;
            s.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();
            s.IsActive = chkActive.Checked;

            try
            {
                if (editing == null) repo.Add(s);
                else repo.Update(s);
                UiHelper.Info("تم الحفظ بنجاح");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                UiHelper.Error("فشل الحفظ: " + ex.Message);
            }
        }
    }
}