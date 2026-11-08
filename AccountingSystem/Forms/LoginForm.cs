using System;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Helpers;
using AccountingSystem.Repositories;
using AccountingSystem.Theme;

namespace AccountingSystem.Forms
{
    public class LoginForm : Form
    {
        private TextBox txtUser;
        private TextBox txtPass;
        private Button btnLogin;
        private Label lblMsg;

        public LoginForm()
        {
            ThemeManager.ApplyForm(this);
            Text = "تسجيل الدخول";
            Size = new Size(420, 500);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BuildUi();
        }

        private void BuildUi()
        {
            var logo = new Label();
            logo.Text = "💰";
            logo.Font = new Font("Segoe UI Emoji", 42F);
            logo.ForeColor = ThemeManager.Primary;
            logo.AutoSize = true;
            logo.Location = new Point((Width - 90) / 2, 30);
            Controls.Add(logo);

            var title = new Label();
            title.Text = "النظام المحاسبي المتكامل";
            title.Font = ThemeManager.FontTitle;
            title.ForeColor = ThemeManager.Text;
            title.AutoSize = true;
            title.Location = new Point((Width - title.PreferredWidth) / 2, 120);
            Controls.Add(title);

            var sub = new Label();
            sub.Text = "تسجيل الدخول للمتابعة";
            sub.Font = ThemeManager.FontSmall;
            sub.ForeColor = ThemeManager.Muted;
            sub.AutoSize = true;
            sub.Location = new Point((Width - sub.PreferredWidth) / 2, 160);
            Controls.Add(sub);

            AddFieldLabel("اسم المستخدم", 210);
            txtUser = new TextBox();
            txtUser.Location = new Point(50, 235);
            txtUser.Width = 320;
            txtUser.Text = "admin";
            ThemeManager.ApplyTextBox(txtUser);
            Controls.Add(txtUser);

            AddFieldLabel("كلمة المرور", 285);
            txtPass = new TextBox();
            txtPass.Location = new Point(50, 310);
            txtPass.Width = 320;
            txtPass.UseSystemPasswordChar = true;
            ThemeManager.ApplyTextBox(txtPass);
            Controls.Add(txtPass);

            lblMsg = new Label();
            lblMsg.Location = new Point(50, 350);
            lblMsg.Width = 320;
            lblMsg.Height = 24;
            lblMsg.ForeColor = ThemeManager.Danger;
            lblMsg.Font = ThemeManager.FontSmall;
            Controls.Add(lblMsg);

            btnLogin = new Button();
            btnLogin.Text = "دخول";
            btnLogin.Location = new Point(50, 385);
            btnLogin.Width = 320;
            btnLogin.Height = 42;
            ThemeManager.ApplyPrimaryButton(btnLogin);
            btnLogin.Click += BtnLogin_Click;
            Controls.Add(btnLogin);

            AcceptButton = btnLogin;
        }

        private void AddFieldLabel(string text, int y)
        {
            var lbl = new Label();
            lbl.Text = text;
            lbl.Location = new Point(50, y);
            lbl.AutoSize = true;
            lbl.ForeColor = ThemeManager.Muted;
            lbl.Font = ThemeManager.FontSmall;
            Controls.Add(lbl);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            lblMsg.Text = string.Empty;
            string username = txtUser.Text.Trim();
            string password = txtPass.Text;

            if (!ValidationHelper.Required(username) || !ValidationHelper.Required(password))
            {
                lblMsg.Text = "يرجى إدخال اسم المستخدم وكلمة المرور.";
                return;
            }

            try
            {
                var repo = new UserRepository();
                var user = repo.GetByUsername(username);

                if (user == null)
                {
                    lblMsg.Text = "اسم المستخدم أو كلمة المرور غير صحيحة.";
                    return;
                }

                bool ok;
                if (user.PasswordHash == "$2a$11$REPLACE_ON_FIRST_RUN")
                {
                    ok = password == "admin123";
                    if (ok)
                    {
                        string newHash = PasswordHasher.Hash(password);
                        repo.UpdatePassword(user.UserId, newHash);
                    }
                }
                else
                {
                    ok = PasswordHasher.Verify(password, user.PasswordHash);
                }

                if (!ok)
                {
                    lblMsg.Text = "اسم المستخدم أو كلمة المرور غير صحيحة.";
                    return;
                }
                if (!user.IsActive)
                {
                    lblMsg.Text = "هذا الحساب معطّل.";
                    return;
                }

                Session.CurrentUser = user;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblMsg.Text = "خطأ: " + ex.Message;
            }
        }
    }
}