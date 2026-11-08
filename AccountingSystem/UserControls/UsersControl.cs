using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Helpers;
using AccountingSystem.Theme;

namespace AccountingSystem.UserControls
{
    public class UsersControl : UserControl
    {
        private DoubleBufferedGrid dgv;

        public UsersControl()
        {
            RightToLeft = RightToLeft.Yes;
            
            
            
            BackColor = ThemeManager.Background;
            Dock = DockStyle.Fill;
            Build();
            LoadData();
        }

        private void Build()
        {
            var toolbar = new Panel();
            toolbar.RightToLeft = RightToLeft.Yes;
            toolbar.Dock = DockStyle.Top;
            toolbar.Height = 60;
            toolbar.
            RightToLeft = RightToLeft.Yes;
            
            
          //
            
            
            BackColor = ThemeManager.Background;
            Controls.Add(toolbar);

            var btnRefresh = new Button();
            btnRefresh.Text = "🔄 تحديث";
            btnRefresh.Width = 100;
            btnRefresh.Location = new Point(20, 13);
            ThemeManager.ApplyGhostButton(btnRefresh);
            btnRefresh.Click += delegate { LoadData(); };
            toolbar.Controls.Add(btnRefresh);

            var lblInfo = new Label();
            lblInfo.Text = "⚠ إدارة المستخدمين قيد التطوير — يمكنك إضافة مستخدمين مباشرة في قاعدة البيانات";
            lblInfo.Location = new Point(140, 20);
            lblInfo.AutoSize = true;
            lblInfo.ForeColor = ThemeManager.Warning;
            lblInfo.Font = ThemeManager.FontSmall;
            toolbar.Controls.Add(lblInfo);

            dgv = new DoubleBufferedGrid();
            dgv.RightToLeft = RightToLeft.Yes;
            dgv.Dock = DockStyle.Fill;
            ThemeManager.ApplyGrid(dgv);
            dgv.Columns.Add(Col("UserId", "ID", 60));
            dgv.Columns.Add(Col("Username", "اسم المستخدم", 180));
            dgv.Columns.Add(Col("FullName", "الاسم الكامل", 250));
            dgv.Columns.Add(Col("RoleName", "الدور", 150));
            dgv.Columns.Add(Col("IsActive", "نشط", 100));
            dgv.Columns.Add(Col("CreatedAt", "تاريخ الإنشاء", 180));
            Controls.Add(dgv);
            dgv.BringToFront();
        }

        private DataGridViewTextBoxColumn Col(string prop, string header, int width)
        {
            var c = new DataGridViewTextBoxColumn();
            c.DataPropertyName = prop;
            c.Name = prop;
            c.HeaderText = header;
            c.Width = width;
            return c;
        }

        private void LoadData()
        {
            try
            {
                using (var conn = Data.DatabaseConnection.GetConnection())
                using (var cmd = new System.Data.SqlClient.SqlCommand(@"
                    SELECT u.UserId, u.Username, u.FullName, r.Name AS RoleName,
                           u.IsActive, u.CreatedAt
                    FROM Users u JOIN Roles r ON u.RoleId = r.RoleId
                    ORDER BY u.UserId", conn))
                using (var rd = cmd.ExecuteReader())
                {
                    dgv.Rows.Clear();
                    while (rd.Read())
                    {
                        dgv.Rows.Add(rd.GetInt32(0), rd.GetString(1), rd.GetString(2),
                            rd.GetString(3), rd.GetBoolean(4) ? "نعم" : "لا",
                            rd.GetDateTime(5).ToString("yyyy-MM-dd"));
                    }
                }
            }
            catch { }
        }
    }
}