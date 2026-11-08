using System;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Data;
using AccountingSystem.Helpers;
using AccountingSystem.Theme;
using System.Data.SqlClient;

namespace AccountingSystem.UserControls
{
    public class BackupControl : UserControl
    {
        private TextBox txtBackupPath;
        private TextBox txtRestorePath;

        public BackupControl()
        {
            RightToLeft = RightToLeft.Yes;
            BackColor = ThemeManager.Background;
            Dock = DockStyle.Fill;
            AutoScroll = true;
            Build();
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

            // ============ Title ============
            var title = new Label();
            title.Text = "💾 النسخ الاحتياطي واستعادة البيانات";
            title.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            title.ForeColor = ThemeManager.Text;
            title.Dock = DockStyle.Top;
            title.Height = 50;
            title.TextAlign = ContentAlignment.MiddleRight;
            title.RightToLeft = RightToLeft.Yes;
            title.Padding = new Padding(0, 0, 20, 0);
            main.Controls.Add(title);

            // ============ Backup Card ============
            var backupCard = new Panel();
            backupCard.Dock = DockStyle.Top;
            backupCard.Height = 200;
            backupCard.BackColor = ThemeManager.Panel;
            backupCard.Padding = new Padding(25, 20, 25, 20);
            backupCard.Margin = new Padding(0, 10, 0, 15);
            backupCard.RightToLeft = RightToLeft.Yes;
            main.Controls.Add(backupCard);
            backupCard.BringToFront();

            // Backup section title (right aligned)
            var lblBackup = new Label();
            lblBackup.Text = "📦 إنشاء نسخة احتياطية";
            lblBackup.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblBackup.ForeColor = ThemeManager.Success;
            lblBackup.Dock = DockStyle.Top;
            lblBackup.Height = 40;
            lblBackup.TextAlign = ContentAlignment.MiddleRight;
            lblBackup.RightToLeft = RightToLeft.Yes;
            backupCard.Controls.Add(lblBackup);

            // Path label (right aligned)
            var lblPath1 = new Label();
            lblPath1.Text = "مكان حفظ النسخة:";
            lblPath1.ForeColor = ThemeManager.Muted;
            lblPath1.Font = ThemeManager.FontSmall;
            lblPath1.Dock = DockStyle.Top;
            lblPath1.Height = 25;
            lblPath1.TextAlign = ContentAlignment.MiddleRight;
            lblPath1.RightToLeft = RightToLeft.Yes;
            backupCard.Controls.Add(lblPath1);
            lblPath1.BringToFront();

            // Path row
            var pathRow1 = new Panel();
            pathRow1.Dock = DockStyle.Top;
            pathRow1.Height = 45;
            pathRow1.BackColor = Color.Transparent;
            pathRow1.RightToLeft = RightToLeft.Yes;
            backupCard.Controls.Add(pathRow1);
            pathRow1.BringToFront();

            // Browse button on LEFT
            var btnBrowse1 = new Button();
            btnBrowse1.Text = "📁 استعراض";
            btnBrowse1.Width = 130;
            btnBrowse1.Height = 36;
            btnBrowse1.Location = new Point(0, 4);
            btnBrowse1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            ThemeManager.ApplyGhostButton(btnBrowse1);
            btnBrowse1.Click += delegate
            {
                var sfd = new SaveFileDialog();
                sfd.Filter = "Backup Files (*.bak)|*.bak";
                sfd.FileName = "backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bak";
                if (sfd.ShowDialog() == DialogResult.OK)
                    txtBackupPath.Text = sfd.FileName;
            };
            pathRow1.Controls.Add(btnBrowse1);

            // TextBox fills remaining space from LEFT
            txtBackupPath = new TextBox();
            txtBackupPath.Location = new Point(140, 4);
            txtBackupPath.Height = 36;
            txtBackupPath.Width = pathRow1.Width - 150;
            txtBackupPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtBackupPath.Text = @"D:\Accounting\backup_" + DateTime.Now.ToString("yyyyMMdd") + ".bak";
            ThemeManager.ApplyTextBox(txtBackupPath);
            txtBackupPath.RightToLeft = RightToLeft.No;
            pathRow1.Controls.Add(txtBackupPath);

            // Backup action button (full width, right aligned text)
            var btnBackup = new Button();
            btnBackup.Text = "▶ إنشاء نسخة احتياطية الآن";
            btnBackup.Dock = DockStyle.Top;
            btnBackup.Height = 45;
            btnBackup.Margin = new Padding(0, 15, 0, 0);
            btnBackup.TextAlign = ContentAlignment.MiddleRight;
            btnBackup.Padding = new Padding(0, 0, 20, 0);
            ThemeManager.ApplySuccessButton(btnBackup);
            btnBackup.Click += BtnBackup_Click;
            backupCard.Controls.Add(btnBackup);
            btnBackup.BringToFront();

            // ============ Spacer ============
            var spacer = new Panel();
            spacer.Dock = DockStyle.Top;
            spacer.Height = 20;
            main.Controls.Add(spacer);
            spacer.BringToFront();

            // ============ Restore Card ============
            var restoreCard = new Panel();
            restoreCard.Dock = DockStyle.Top;
            restoreCard.Height = 235;
            restoreCard.BackColor = ThemeManager.Panel;
            restoreCard.Padding = new Padding(25, 20, 25, 20);
            restoreCard.Margin = new Padding(0, 10, 0, 10);
            restoreCard.RightToLeft = RightToLeft.Yes;
            main.Controls.Add(restoreCard);
            restoreCard.BringToFront();

            var lblRestore = new Label();
            lblRestore.Text = "🔄 استعادة من نسخة احتياطية";
            lblRestore.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblRestore.ForeColor = ThemeManager.Warning;
            lblRestore.Dock = DockStyle.Top;
            lblRestore.Height = 40;
            lblRestore.TextAlign = ContentAlignment.MiddleRight;
            lblRestore.RightToLeft = RightToLeft.Yes;
            restoreCard.Controls.Add(lblRestore);

            var lblPath2 = new Label();
            lblPath2.Text = "ملف النسخة الاحتياطية:";
            lblPath2.ForeColor = ThemeManager.Muted;
            lblPath2.Font = ThemeManager.FontSmall;
            lblPath2.Dock = DockStyle.Top;
            lblPath2.Height = 25;
            lblPath2.TextAlign = ContentAlignment.MiddleRight;
            lblPath2.RightToLeft = RightToLeft.Yes;
            restoreCard.Controls.Add(lblPath2);
            lblPath2.BringToFront();

            var pathRow2 = new Panel();
            pathRow2.Dock = DockStyle.Top;
            pathRow2.Height = 45;
            pathRow2.BackColor = Color.Transparent;
            pathRow2.RightToLeft = RightToLeft.Yes;
            restoreCard.Controls.Add(pathRow2);
            pathRow2.BringToFront();

            var btnBrowse2 = new Button();
            btnBrowse2.Text = "📁 استعراض";
            btnBrowse2.Width = 130;
            btnBrowse2.Height = 36;
            btnBrowse2.Location = new Point(0, 4);
            btnBrowse2.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            ThemeManager.ApplyGhostButton(btnBrowse2);
            btnBrowse2.Click += delegate
            {
                var ofd = new OpenFileDialog();
                ofd.Filter = "Backup Files (*.bak)|*.bak";
                if (ofd.ShowDialog() == DialogResult.OK)
                    txtRestorePath.Text = ofd.FileName;
            };
            pathRow2.Controls.Add(btnBrowse2);

            txtRestorePath = new TextBox();
            txtRestorePath.Location = new Point(140, 4);
            txtRestorePath.Height = 36;
            txtRestorePath.Width = pathRow2.Width - 150;
            txtRestorePath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ThemeManager.ApplyTextBox(txtRestorePath);
            txtRestorePath.RightToLeft = RightToLeft.No;
            pathRow2.Controls.Add(txtRestorePath);

            var warn = new Label();
            warn.Text = "⚠ تحذير: الاستعادة ستستبدل كل البيانات الحالية!";
            warn.ForeColor = ThemeManager.Danger;
            warn.Font = ThemeManager.FontBold;
            warn.Dock = DockStyle.Top;
            warn.Height = 30;
            warn.TextAlign = ContentAlignment.MiddleRight;
            warn.RightToLeft = RightToLeft.Yes;
            restoreCard.Controls.Add(warn);
            warn.BringToFront();

            var btnRestore = new Button();
            btnRestore.Text = "⚠ استعادة البيانات";
            btnRestore.Dock = DockStyle.Top;
            btnRestore.Height = 45;
            btnRestore.Margin = new Padding(0, 10, 0, 0);
            btnRestore.TextAlign = ContentAlignment.MiddleRight;
            btnRestore.Padding = new Padding(0, 0, 20, 0);
            ThemeManager.ApplyDangerButton(btnRestore);
            btnRestore.Click += BtnRestore_Click;
            restoreCard.Controls.Add(btnRestore);
            btnRestore.BringToFront();

            // ============ Info Card ============
            var infoCard = new Panel();
            infoCard.Dock = DockStyle.Top;
            infoCard.Height = 150;
            infoCard.BackColor = ThemeManager.Panel;
            infoCard.Padding = new Padding(25, 20, 25, 20);
            infoCard.Margin = new Padding(0, 10, 0, 10);
            infoCard.RightToLeft = RightToLeft.Yes;
            main.Controls.Add(infoCard);
            infoCard.BringToFront();

            var lblInfo = new Label();
            lblInfo.Text = "ℹ معلومات";
            lblInfo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblInfo.ForeColor = ThemeManager.Primary;
            lblInfo.Dock = DockStyle.Top;
            lblInfo.Height = 30;
            lblInfo.TextAlign = ContentAlignment.MiddleRight;
            lblInfo.RightToLeft = RightToLeft.Yes;
            infoCard.Controls.Add(lblInfo);

            var txtInfo = new Label();
            txtInfo.Text = "• يُنصح بأخذ نسخة احتياطية يومياً في نهاية العمل\n" +
                          "• يمكن تخزين النسخة على فلاش USB أو قرص خارجي\n" +
                          "• للاستعادة: اختر الملف ثم اضغط استعادة البيانات";
            txtInfo.ForeColor = ThemeManager.Muted;
            txtInfo.Font = new Font("Segoe UI", 10F);
            txtInfo.Dock = DockStyle.Fill;
            txtInfo.TextAlign = ContentAlignment.TopRight;
            txtInfo.RightToLeft = RightToLeft.Yes;
            infoCard.Controls.Add(txtInfo);
            txtInfo.BringToFront();
        }

        private void BtnBackup_Click(object sender, EventArgs e)
        {
            string path = txtBackupPath.Text.Trim();
            if (string.IsNullOrWhiteSpace(path))
            {
                UiHelper.Warn("حدد مكان الحفظ.");
                return;
            }

            try
            {
                using (var conn = DatabaseConnection.GetMasterConnection())
                using (var cmd = new SqlCommand(
                    "BACKUP DATABASE AccountingDB TO DISK = @path WITH INIT", conn))
                {
                    cmd.Parameters.AddWithValue("@path", path);
                    cmd.CommandTimeout = 300;
                    cmd.ExecuteNonQuery();
                }

                UiHelper.Info("تم إنشاء النسخة الاحتياطية بنجاح ✅\n\n" + path);
            }
            catch (Exception ex)
            {
                UiHelper.Error("فشل النسخ الاحتياطي:\n" + ex.Message);
            }
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            string path = txtRestorePath.Text.Trim();
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            {
                UiHelper.Warn("حدد ملف نسخة احتياطية صالح.");
                return;
            }

            if (!UiHelper.Confirm("⚠ تحذير: سيتم حذف كل البيانات الحالية!\n\nهل أنت متأكد؟"))
                return;

            try
            {
                using (var conn = DatabaseConnection.GetMasterConnection())
                {
                    using (var cmd1 = new SqlCommand(
                        "ALTER DATABASE AccountingDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE", conn))
                    {
                        cmd1.CommandTimeout = 60;
                        cmd1.ExecuteNonQuery();
                    }

                    using (var cmd2 = new SqlCommand(
                        "RESTORE DATABASE AccountingDB FROM DISK = @path WITH REPLACE", conn))
                    {
                        cmd2.Parameters.AddWithValue("@path", path);
                        cmd2.CommandTimeout = 300;
                        cmd2.ExecuteNonQuery();
                    }

                    using (var cmd3 = new SqlCommand(
                        "ALTER DATABASE AccountingDB SET MULTI_USER", conn))
                    {
                        cmd3.CommandTimeout = 60;
                        cmd3.ExecuteNonQuery();
                    }
                }

                UiHelper.Info("تمت الاستعادة بنجاح ✅\nسيتم إعادة تشغيل البرنامج.");
                Application.Restart();
            }
            catch (Exception ex)
            {
                UiHelper.Error("فشل الاستعادة:\n" + ex.Message);
            }
        }
    }
}