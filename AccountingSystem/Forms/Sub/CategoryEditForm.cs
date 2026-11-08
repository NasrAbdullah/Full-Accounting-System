using System;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Helpers;
using AccountingSystem.Models;
using AccountingSystem.Repositories;
using AccountingSystem.Theme;

namespace AccountingSystem.Forms.Sub
{
    public class CategoryEditForm : Form
    {
        private Category editing;
        private CategoryRepository repo = new CategoryRepository();
        private TextBox txtName;
        private TextBox txtNotes;
        private ErrorProvider err;

        public CategoryEditForm(Category category)
        {
            editing = category;
            ThemeManager.ApplyForm(this);
            Text = category == null ? "إضافة تصنيف" : "تعديل تصنيف";
            Size = new Size(420, 300);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BuildUi();
            if (category != null)
            {
                txtName.Text = category.Name;
                txtNotes.Text = category.Notes ?? "";
            }
        }

        private void BuildUi()
        {
            err = new ErrorProvider();
            err.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            var lbl1 = new Label();
            lbl1.Text = "اسم التصنيف *";
            lbl1.Location = new Point(30, 20);
            lbl1.AutoSize = true;
            lbl1.ForeColor = ThemeManager.Muted;
            lbl1.Font = ThemeManager.FontSmall;
            Controls.Add(lbl1);

            txtName = new TextBox();
            txtName.Location = new Point(30, 45);
            txtName.Width = 340;
            ThemeManager.ApplyTextBox(txtName);
            Controls.Add(txtName);

            var lbl2 = new Label();
            lbl2.Text = "ملاحظات";
            lbl2.Location = new Point(30, 90);
            lbl2.AutoSize = true;
            lbl2.ForeColor = ThemeManager.Muted;
            lbl2.Font = ThemeManager.FontSmall;
            Controls.Add(lbl2);

            txtNotes = new TextBox();
            txtNotes.Location = new Point(30, 115);
            txtNotes.Width = 340;
            txtNotes.Height = 60;
            txtNotes.Multiline = true;
            ThemeManager.ApplyTextBox(txtNotes);
            Controls.Add(txtNotes);

            var btnSave = new Button();
            btnSave.Text = "💾 حفظ";
            btnSave.Location = new Point(30, 195);
            btnSave.Width = 160;
            ThemeManager.ApplySuccessButton(btnSave);
            btnSave.Click += BtnSave_Click;
            Controls.Add(btnSave);

            var btnCancel = new Button();
            btnCancel.Text = "إلغاء";
            btnCancel.Location = new Point(210, 195);
            btnCancel.Width = 160;
            ThemeManager.ApplyGhostButton(btnCancel);
            btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnCancel);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            err.Clear();
            if (!ValidationHelper.Required(txtName.Text))
            {
                err.SetError(txtName, "اسم التصنيف مطلوب");
                return;
            }

            var c = editing != null ? editing : new Category();
            c.Name = txtName.Text.Trim();
            c.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();

            try
            {
                if (editing == null) repo.Add(c);
                else repo.Update(c);
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