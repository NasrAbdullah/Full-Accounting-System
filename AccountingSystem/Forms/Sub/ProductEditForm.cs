using System;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Helpers;
using AccountingSystem.Models;
using AccountingSystem.Repositories;
using AccountingSystem.Theme;

namespace AccountingSystem.Forms.Sub
{
    public class ProductEditForm : Form
    {
        private Product editing;
        private ProductRepository repo = new ProductRepository();
        private CategoryRepository catRepo = new CategoryRepository();

        private TextBox txtCode;
        private TextBox txtName;
        private ComboBox cboCategory;
        private NumericUpDown numPurchase;
        private NumericUpDown numSale;
        private NumericUpDown numQuantity;
        private NumericUpDown numMin;
        private TextBox txtUnit;
        private TextBox txtBarcode;
        private TextBox txtDesc;
        private CheckBox chkActive;
        private ErrorProvider err;

        public ProductEditForm(Product product)
        {
            editing = product;
            ThemeManager.ApplyForm(this);
            Text = product == null ? "إضافة منتج" : "تعديل منتج";
            Size = new Size(520, 720);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BuildUi();
            LoadCategories();
            if (product != null) LoadValues(product);
            else txtCode.Text = GenerateCode();
        }

        private void BuildUi()
        {
            err = new ErrorProvider();
            err.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            int y = 20;
            int lblY;

            lblY = y; AddLabel("كود المنتج *", 30, lblY); y += 22;
            txtCode = new TextBox();
            txtCode.Location = new Point(30, y);
            txtCode.Width = 440;
            ThemeManager.ApplyTextBox(txtCode);
            Controls.Add(txtCode); y += 50;

            lblY = y; AddLabel("اسم المنتج *", 30, lblY); y += 22;
            txtName = new TextBox();
            txtName.Location = new Point(30, y);
            txtName.Width = 440;
            ThemeManager.ApplyTextBox(txtName);
            Controls.Add(txtName); y += 50;

            lblY = y; AddLabel("التصنيف", 30, lblY); y += 22;
            cboCategory = new ComboBox();
            cboCategory.Location = new Point(30, y);
            cboCategory.Width = 440;
            cboCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            ThemeManager.ApplyComboBox(cboCategory);
            Controls.Add(cboCategory); y += 50;

            lblY = y; AddLabel("سعر الشراء", 30, lblY); y += 22;
            numPurchase = MakeNum(30, y, 210);
            Controls.Add(numPurchase);
            AddLabel("سعر البيع", 260, lblY);
            numSale = MakeNum(260, y, 210);
            Controls.Add(numSale); y += 50;

            lblY = y; AddLabel("الكمية", 30, lblY); y += 22;
            numQuantity = MakeNum(30, y, 210);
            Controls.Add(numQuantity);
            AddLabel("الحد الأدنى", 260, lblY);
            numMin = MakeNum(260, y, 210);
            Controls.Add(numMin); y += 50;

            lblY = y; AddLabel("الوحدة", 30, lblY); y += 22;
            txtUnit = new TextBox();
            txtUnit.Location = new Point(30, y);
            txtUnit.Width = 210;
            ThemeManager.ApplyTextBox(txtUnit);
            Controls.Add(txtUnit);
            AddLabel("الباركود", 260, lblY);
            txtBarcode = new TextBox();
            txtBarcode.Location = new Point(260, y);
            txtBarcode.Width = 210;
            ThemeManager.ApplyTextBox(txtBarcode);
            Controls.Add(txtBarcode); y += 50;

            lblY = y; AddLabel("الوصف", 30, lblY); y += 22;
            txtDesc = new TextBox();
            txtDesc.Location = new Point(30, y);
            txtDesc.Width = 440;
            txtDesc.Height = 55;
            txtDesc.Multiline = true;
            ThemeManager.ApplyTextBox(txtDesc);
            Controls.Add(txtDesc); y += 65;

            chkActive = new CheckBox();
            chkActive.Text = "نشط";
            chkActive.Location = new Point(30, y);
            chkActive.ForeColor = ThemeManager.Text;
            chkActive.Checked = true;
            Controls.Add(chkActive); y += 40;

            var btnSave = new Button();
            btnSave.Text = "💾 حفظ";
            btnSave.Location = new Point(30, y);
            btnSave.Width = 210;
            ThemeManager.ApplySuccessButton(btnSave);
            btnSave.Click += BtnSave_Click;
            Controls.Add(btnSave);

            var btnCancel = new Button();
            btnCancel.Text = "إلغاء";
            btnCancel.Location = new Point(260, y);
            btnCancel.Width = 210;
            ThemeManager.ApplyGhostButton(btnCancel);
            btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnCancel);
        }

        private NumericUpDown MakeNum(int x, int y, int w)
        {
            var n = new NumericUpDown();
            n.Location = new Point(x, y);
            n.Width = w;
            n.DecimalPlaces = 2;
            n.Maximum = 999999999;
            n.BackColor = ThemeManager.PanelAlt;
            n.ForeColor = ThemeManager.Text;
            return n;
        }

        private void AddLabel(string text, int x, int y)
        {
            var lbl = new Label();
            lbl.Text = text;
            lbl.Location = new Point(x, y);
            lbl.AutoSize = true;
            lbl.ForeColor = ThemeManager.Muted;
            lbl.Font = ThemeManager.FontSmall;
            Controls.Add(lbl);
        }

        private string GenerateCode()
        {
            return "P-" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        private void LoadCategories()
        {
            cboCategory.Items.Clear();
            cboCategory.Items.Add(new ComboItem(0, "-- بدون تصنيف --"));
            try
            {
                var list = catRepo.GetAll(null);
                foreach (var c in list)
                    cboCategory.Items.Add(new ComboItem(c.CategoryId, c.Name));
            }
            catch { }
            cboCategory.SelectedIndex = 0;
        }

        private void LoadValues(Product p)
        {
            txtCode.Text = p.ProductCode;
            txtName.Text = p.Name;
            numPurchase.Value = Math.Max(0, Math.Min(numPurchase.Maximum, p.PurchasePrice));
            numSale.Value = Math.Max(0, Math.Min(numSale.Maximum, p.SalePrice));
            numQuantity.Value = Math.Max(0, Math.Min(numQuantity.Maximum, p.Quantity));
            numMin.Value = Math.Max(0, Math.Min(numMin.Maximum, p.MinimumStock));
            txtUnit.Text = p.Unit ?? "";
            txtBarcode.Text = p.Barcode ?? "";
            txtDesc.Text = p.Description ?? "";
            chkActive.Checked = p.IsActive;

            int catId = p.CategoryId.HasValue ? p.CategoryId.Value : 0;
            for (int i = 0; i < cboCategory.Items.Count; i++)
            {
                var ci = cboCategory.Items[i] as ComboItem;
                if (ci != null && ci.Id == catId) { cboCategory.SelectedIndex = i; break; }
            }
        }

        private bool ValidateForm()
        {
            err.Clear();
            bool ok = true;
            if (!ValidationHelper.Required(txtCode.Text))
            { err.SetError(txtCode, "كود المنتج مطلوب"); ok = false; }
            if (!ValidationHelper.Required(txtName.Text))
            { err.SetError(txtName, "اسم المنتج مطلوب"); ok = false; }

            string code = txtCode.Text.Trim();
            int exclude = editing != null ? editing.ProductId : 0;
            if (ValidationHelper.Required(code) && repo.CodeExists(code, exclude))
            { err.SetError(txtCode, "كود المنتج مستخدم بالفعل"); ok = false; }

            if (numSale.Value > 0 && numPurchase.Value > 0 && numSale.Value < numPurchase.Value)
            {
                if (!UiHelper.Confirm("سعر البيع أقل من سعر الشراء. هل تريد المتابعة؟"))
                    ok = false;
            }
            return ok;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            var p = editing != null ? editing : new Product();
            p.ProductCode = txtCode.Text.Trim();
            p.Name = txtName.Text.Trim();
            var sel = cboCategory.SelectedItem as ComboItem;
            p.CategoryId = (sel != null && sel.Id > 0) ? (int?)sel.Id : null;
            p.PurchasePrice = numPurchase.Value;
            p.SalePrice = numSale.Value;
            p.Quantity = numQuantity.Value;
            p.MinimumStock = numMin.Value;
            p.Unit = string.IsNullOrWhiteSpace(txtUnit.Text) ? null : txtUnit.Text.Trim();
            p.Barcode = string.IsNullOrWhiteSpace(txtBarcode.Text) ? null : txtBarcode.Text.Trim();
            p.Description = string.IsNullOrWhiteSpace(txtDesc.Text) ? null : txtDesc.Text.Trim();
            p.IsActive = chkActive.Checked;

            try
            {
                if (editing == null) repo.Add(p);
                else repo.Update(p);
                UiHelper.Info("تم الحفظ بنجاح");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                UiHelper.Error("فشل الحفظ: " + ex.Message);
            }
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