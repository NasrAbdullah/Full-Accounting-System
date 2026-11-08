using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Helpers;
using AccountingSystem.Theme;
using AccountingSystem.UserControls;

namespace AccountingSystem.Forms
{
    public class MainForm : Form
    {
        private Panel panelSidebar;
        private Panel panelHeader;
        private Panel panelContent;
        private Panel panelBreadcrumb;

        private Label lblHeaderTitle;
        private Label lblBreadcrumb;
        private Label lblClock;
        private Label lblUser;
        private Label lblNotifications;

        private ModernTextBox txtGlobalSearch;
        private Timer clockTimer;
        private List<NavButton> navButtons = new List<NavButton>();
        private NavButton activeNav;

        // Map of nav labels -> controls
        private Dictionary<string, Func<UserControl>> controlFactories;

        public MainForm()
        {
            DesignSystem.ApplyForm(this);
            Text = "النظام المحاسبي المتكامل - v2.0";
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1200, 700);

            this.RightToLeftLayout = false;
            this.RightToLeft = RightToLeft.Yes;

            InitializeFactories();
            BuildUi();
            SetupShortcuts();
            StartClock();

            LoadControl(new DashboardControl(), "لوحة التحكم", FindNavButton("لوحة التحكم"));
        }

        private void InitializeFactories()
        {
            controlFactories = new Dictionary<string, Func<UserControl>>();
            controlFactories["لوحة التحكم"] = () => new DashboardControl();
            controlFactories["المبيعات"] = () => new SalesControl();
            controlFactories["المشتريات"] = () => new PurchasesControl();
            controlFactories["العملاء"] = () => new CustomersControl();
            controlFactories["الموردون"] = () => new SuppliersControl();
            controlFactories["المنتجات"] = () => new ProductsControl();
            controlFactories["التصنيفات"] = () => new CategoriesControl();
            controlFactories["المخزون"] = () => new InventoryControl();
            controlFactories["المدفوعات"] = () => new PaymentsControl();
            controlFactories["المصروفات"] = () => new ExpensesControl();
            controlFactories["الإيرادات"] = () => new IncomeControl();
            controlFactories["التقارير"] = () => new ReportsControl();
            controlFactories["المستخدمون"] = () => new UsersControl();
            controlFactories["الإعدادات"] = () => new SettingsControl();
            controlFactories["النسخ الاحتياطي"] = () => new BackupControl();
        }

        private void SetupShortcuts()
        {
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+K - Global Search
            if (e.Control && e.KeyCode == Keys.K)
            {
                txtGlobalSearch.Focus();
                txtGlobalSearch.SelectAll();
                e.Handled = true;
            }
            // F5 - Refresh
            if (e.KeyCode == Keys.F5)
            {
                if (activeNav != null) activeNav.PerformLayout();
                LoadControl(
                    controlFactories[activeNav != null ? activeNav.Text : "لوحة التحكم"](),
                    activeNav != null ? activeNav.Text : "لوحة التحكم",
                    activeNav);
                e.Handled = true;
            }
            // F1 - Help
            if (e.KeyCode == Keys.F1)
            {
                UiHelper.Info(
                    "اختصارات لوحة المفاتيح:\n\n" +
                    "Ctrl+K  - البحث الشامل\n" +
                    "F5      - تحديث الصفحة\n" +
                    "F1      - المساعدة\n" +
                    "Ctrl+L  - تسجيل الخروج\n" +
                    "Ctrl+Q  - خروج\n" +
                    "Esc     - إغلاق النافذة",
                    "المساعدة");
                e.Handled = true;
            }
            // Ctrl+L - Logout
            if (e.Control && e.KeyCode == Keys.L)
            {
                if (UiHelper.Confirm("هل أنت متأكد من تسجيل الخروج؟"))
                {
                    Session.CurrentUser = null;
                    Application.Restart();
                }
                e.Handled = true;
            }
            // Ctrl+Q - Exit
            if (e.Control && e.KeyCode == Keys.Q)
            {
                if (UiHelper.Confirm("هل أنت متأكد من الخروج؟"))
                    Application.Exit();
                e.Handled = true;
            }
        }

        private void StartClock()
        {
            clockTimer = new Timer();
            clockTimer.Interval = 1000;
            clockTimer.Tick += delegate { UpdateClock(); };
            clockTimer.Start();
            UpdateClock();
        }

        private void UpdateClock()
        {
            if (lblClock == null) return;
            var now = DateTime.Now;
            var culture = new System.Globalization.CultureInfo("ar-EG");
            string time = now.ToString("hh:mm:ss tt", culture);
            string date = now.ToString("dddd، dd MMMM yyyy", culture);
            lblClock.Text = "🕐 " + time + "   │   📅 " + date;
        }

        private void BuildUi()
        {
            panelContent = new Panel();
            panelContent.Dock = DockStyle.Fill;
            panelContent.BackColor = DesignSystem.Colors.Background;
            panelContent.Padding = new Padding(20);
            panelContent.RightToLeft = RightToLeft.Yes;
            this.Controls.Add(panelContent);

            panelBreadcrumb = new Panel();
            panelBreadcrumb.Dock = DockStyle.Top;
            panelBreadcrumb.Height = 32;
            panelBreadcrumb.BackColor = DesignSystem.Colors.Background;
            panelBreadcrumb.Padding = new Padding(20, 0, 20, 0);
            panelBreadcrumb.RightToLeft = RightToLeft.Yes;
            this.Controls.Add(panelBreadcrumb);

            lblBreadcrumb = new Label();
            lblBreadcrumb.Text = "🏠 الرئيسية";
            lblBreadcrumb.Font = DesignSystem.Fonts.Small;
            lblBreadcrumb.ForeColor = DesignSystem.Colors.TextMuted;
            lblBreadcrumb.Dock = DockStyle.Fill;
            lblBreadcrumb.TextAlign = ContentAlignment.MiddleRight;
            lblBreadcrumb.RightToLeft = RightToLeft.Yes;
            lblBreadcrumb.BackColor = Color.Transparent;
            panelBreadcrumb.Controls.Add(lblBreadcrumb);

            panelHeader = new Panel();
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Height = DesignSystem.Sizes.HeaderHeight;
            panelHeader.BackColor = DesignSystem.Colors.Surface;
            panelHeader.RightToLeft = RightToLeft.Yes;
            this.Controls.Add(panelHeader);
            BuildHeader();

            panelSidebar = new Panel();
            panelSidebar.Dock = DockStyle.Right;
            panelSidebar.Width = DesignSystem.Sizes.SidebarWidth;
            panelSidebar.BackColor = DesignSystem.Colors.Sidebar;
            panelSidebar.RightToLeft = RightToLeft.Yes;
            this.Controls.Add(panelSidebar);
            BuildSidebar();
        }

        private void BuildHeader()
        {
            // ============ LEFT SIDE (icons + user) ============
            var leftPanel = new Panel();
            leftPanel.Dock = DockStyle.Left;
            leftPanel.Width = 500;
            leftPanel.BackColor = Color.Transparent;
            panelHeader.Controls.Add(leftPanel);

            // Logout button
            var btnLogout = new ModernButton();
            btnLogout.Text = "خروج";
            btnLogout.IconText = DesignSystem.Icons.Logout;
            btnLogout.Variant = ButtonVariant.Ghost;
            btnLogout.Width = 110;
            btnLogout.Height = 38;
            btnLogout.Location = new Point(10, 15);
            btnLogout.Click += delegate
            {
                if (UiHelper.Confirm("هل أنت متأكد من تسجيل الخروج؟"))
                {
                    Session.CurrentUser = null;
                    Application.Restart();
                }
            };
            leftPanel.Controls.Add(btnLogout);

            // User info
            string name = Session.CurrentUser != null ? Session.CurrentUser.FullName : "";
            string role = Session.CurrentUser != null ? Session.CurrentUser.RoleName : "";
            lblUser = new Label();
            lblUser.Text = "👤  " + name + "  │  " + role;
            lblUser.Font = DesignSystem.Fonts.SmallBold;
            lblUser.ForeColor = DesignSystem.Colors.TextPrimary;
            lblUser.AutoSize = false;
            lblUser.Width = 240;
            lblUser.Height = 34;
            lblUser.Location = new Point(130, 17);
            lblUser.TextAlign = ContentAlignment.MiddleRight;
            lblUser.RightToLeft = RightToLeft.Yes;
            lblUser.BackColor = Color.Transparent;
            leftPanel.Controls.Add(lblUser);

            // Notifications
            var btnNotif = new ModernButton();
            btnNotif.Text = "";
            btnNotif.IconText = "🔔";
            btnNotif.Variant = ButtonVariant.Ghost;
            btnNotif.Width = 42;
            btnNotif.Height = 38;
            btnNotif.Location = new Point(380, 15);
            btnNotif.Click += delegate { ShowNotifications(); };
            leftPanel.Controls.Add(btnNotif);

            lblNotifications = new Label();
            lblNotifications.Text = "";
            lblNotifications.Font = DesignSystem.Fonts.Tiny;
            lblNotifications.ForeColor = Color.White;
            lblNotifications.BackColor = DesignSystem.Colors.Danger;
            lblNotifications.AutoSize = false;
            lblNotifications.Width = 18;
            lblNotifications.Height = 18;
            lblNotifications.TextAlign = ContentAlignment.MiddleCenter;
            lblNotifications.Location = new Point(408, 10);
            lblNotifications.Visible = false;
            leftPanel.Controls.Add(lblNotifications);

            // ============ CENTER (Title + Clock) ============
            var centerPanel = new Panel();
            centerPanel.Dock = DockStyle.Fill;
            centerPanel.BackColor = Color.Transparent;
            panelHeader.Controls.Add(centerPanel);
            centerPanel.BringToFront();

            lblHeaderTitle = new Label();
            lblHeaderTitle.Text = "لوحة التحكم";
            lblHeaderTitle.Font = DesignSystem.Fonts.H2;
            lblHeaderTitle.ForeColor = DesignSystem.Colors.TextPrimary;
            lblHeaderTitle.AutoSize = false;
            lblHeaderTitle.Dock = DockStyle.Top;
            lblHeaderTitle.Height = 36;
            lblHeaderTitle.Padding = new Padding(0, 0, 20, 0);
            lblHeaderTitle.TextAlign = ContentAlignment.MiddleRight;
            lblHeaderTitle.RightToLeft = RightToLeft.Yes;
            lblHeaderTitle.BackColor = Color.Transparent;
            centerPanel.Controls.Add(lblHeaderTitle);

            lblClock = new Label();
            lblClock.Text = "";
            lblClock.Font = DesignSystem.Fonts.Small;
            lblClock.ForeColor = DesignSystem.Colors.Warning;
            lblClock.AutoSize = false;
            lblClock.Dock = DockStyle.Bottom;
            lblClock.Height = 26;
            lblClock.Padding = new Padding(0, 0, 20, 0);
            lblClock.TextAlign = ContentAlignment.MiddleRight;
            lblClock.RightToLeft = RightToLeft.Yes;
            lblClock.BackColor = Color.Transparent;
            centerPanel.Controls.Add(lblClock);

            // ============ RIGHT SIDE (Search) ============
            var searchPanel = new Panel();
            searchPanel.Dock = DockStyle.Right;
            searchPanel.Width = 320;
            searchPanel.BackColor = Color.Transparent;
            panelHeader.Controls.Add(searchPanel);
            searchPanel.BringToFront();

            var searchBox = new ModernTextBox();
            searchBox.Placeholder = "بحث شامل...  (Ctrl+K)";
            searchBox.IconText = "🔍";
            searchBox.Width = 290;
            searchBox.Location = new Point(15, 15);
            searchBox.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left;
            searchBox.KeyDown += GlobalSearch_KeyDown;
            searchPanel.Controls.Add(searchBox);
            txtGlobalSearch = searchBox;
        }

        private void BuildSidebar()
        {
            // Logo
            var logoPanel = new Panel();
            logoPanel.Dock = DockStyle.Top;
            logoPanel.Height = 70;
            logoPanel.BackColor = Color.Transparent;
            panelSidebar.Controls.Add(logoPanel);

            var lblLogo = new Label();
            lblLogo.Text = "💰 المحاسبة";
            lblLogo.Font = DesignSystem.Fonts.H1;
            lblLogo.ForeColor = DesignSystem.Colors.Primary;
            lblLogo.Dock = DockStyle.Fill;
            lblLogo.TextAlign = ContentAlignment.MiddleCenter;
            lblLogo.BackColor = Color.Transparent;
            logoPanel.Controls.Add(lblLogo);

            var lblVersion = new Label();
            lblVersion.Text = "v2.0";
            lblVersion.Font = DesignSystem.Fonts.Tiny;
            lblVersion.ForeColor = DesignSystem.Colors.TextMuted;
            lblVersion.Dock = DockStyle.Bottom;
            lblVersion.Height = 18;
            lblVersion.TextAlign = ContentAlignment.MiddleCenter;
            lblVersion.BackColor = Color.Transparent;
            logoPanel.Controls.Add(lblVersion);

            // Separator
            var sep = new Panel();
            sep.Dock = DockStyle.Top;
            sep.Height = 1;
            sep.BackColor = DesignSystem.Colors.Border;
            panelSidebar.Controls.Add(sep);

            var spacer = new Panel();
            spacer.Dock = DockStyle.Top;
            spacer.Height = 12;
            panelSidebar.Controls.Add(spacer);

            // Nav items (bottom-to-top because Dock=Top)
            AddNavButton("النسخ الاحتياطي", DesignSystem.Icons.Backup);
            AddNavButton("الإعدادات", DesignSystem.Icons.Settings);
            AddNavButton("المستخدمون", DesignSystem.Icons.Users);
            AddNavButton("التقارير", DesignSystem.Icons.Reports);
            AddNavButton("الإيرادات", DesignSystem.Icons.Income);
            AddNavButton("المصروفات", DesignSystem.Icons.Expenses);
            AddNavButton("المدفوعات", DesignSystem.Icons.Payments);
            AddNavButton("المخزون", DesignSystem.Icons.Inventory);
            AddNavButton("المشتريات", DesignSystem.Icons.Purchases);
            AddNavButton("المبيعات", DesignSystem.Icons.Sales);
            AddNavButton("المنتجات", DesignSystem.Icons.Products);
            AddNavButton("التصنيفات", DesignSystem.Icons.Categories);
            AddNavButton("الموردون", DesignSystem.Icons.Suppliers);
            AddNavButton("العملاء", DesignSystem.Icons.Customers);
            AddNavButton("لوحة التحكم", DesignSystem.Icons.Dashboard);

            // Footer
            var lblFooter = new Label();
            lblFooter.Text = "© 2026 جميع الحقوق محفوظة";
            lblFooter.Font = DesignSystem.Fonts.Tiny;
            lblFooter.ForeColor = DesignSystem.Colors.TextMuted;
            lblFooter.Dock = DockStyle.Bottom;
            lblFooter.Height = 30;
            lblFooter.TextAlign = ContentAlignment.MiddleCenter;
            lblFooter.BackColor = Color.Transparent;
            panelSidebar.Controls.Add(lblFooter);
        }

        private void AddNavButton(string text, string icon)
        {
            var btn = new NavButton();
            btn.Text = text;
            btn.IconText = icon;
            btn.Clicked += delegate { OnNavClicked(btn); };
            navButtons.Add(btn);

            // Insert at bottom of stack (since Dock=Top pushes to top)
            panelSidebar.Controls.Add(btn);
            btn.BringToFront();
        }

        private NavButton FindNavButton(string text)
        {
            foreach (var btn in navButtons)
                if (btn.Text == text) return btn;
            return null;
        }

        private void OnNavClicked(NavButton btn)
        {
            string key = btn.Text;
            if (!controlFactories.ContainsKey(key)) return;

            LoadControl(controlFactories[key](), key, btn);
        }

        private void LoadControl(UserControl control, string title, NavButton nav)
        {
            // Update nav active state
            if (activeNav != null) activeNav.IsActive = false;
            if (nav != null) { nav.IsActive = true; activeNav = nav; }

            // Update header
            lblHeaderTitle.Text = title;
            lblBreadcrumb.Text = "🏠 الرئيسية  ›  " + title;

            // Load content
            panelContent.SuspendLayout();
            panelContent.Controls.Clear();
            control.Dock = DockStyle.Fill;
            panelContent.Controls.Add(control);
            panelContent.ResumeLayout();
        }

        // ============ Global Search ============
        private void GlobalSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;

            string query = txtGlobalSearch.Text.Trim();
            if (string.IsNullOrEmpty(query)) return;

            ShowSearchResults(query);
        }

        private void ShowSearchResults(string query)
        {
            var results = new List<string>();

            try
            {
                // بحث في المنتجات
                var prodRepo = new Repositories.ProductRepository();
                var products = prodRepo.GetAll(query);
                foreach (var p in products)
                    results.Add("📱 منتج: " + p.Name + " (" + p.ProductCode + ")");

                // بحث في العملاء
                var custRepo = new Repositories.CustomerRepository();
                var customers = custRepo.GetAll(query);
                foreach (var c in customers)
                    results.Add("🧑‍💼 عميل: " + c.Name);

                // بحث في الموردين
                var supRepo = new Repositories.SupplierRepository();
                var suppliers = supRepo.GetAll(query);
                foreach (var s in suppliers)
                    results.Add("🏭 مورد: " + s.Name);
            }
            catch { }

            if (results.Count == 0)
            {
                UiHelper.ToastWarning("لا توجد نتائج للبحث: " + query);
                return;
            }

            string message = "نتائج البحث (" + results.Count + "):\n\n";
            int count = 0;
            foreach (var r in results)
            {
                if (count++ >= 15) { message += "..."; break; }
                message += r + "\n";
            }

            UiHelper.Info(message, "نتائج البحث");
        }

        // ============ Notifications ============
        private void ShowNotifications()
        {
            var alerts = new List<string>();

            try
            {
                // منتجات منخفضة المخزون
                var prodRepo = new Repositories.ProductRepository();
                var lowStock = prodRepo.GetLowStock();
                if (lowStock.Count > 0)
                    alerts.Add("⚠ " + lowStock.Count + " منتج بمخزون منخفض");

                // عملاء لديهم ديون
                var custRepo = new Repositories.CustomerRepository();
                var customers = custRepo.GetAll(null);
                int debtors = 0;
                foreach (var c in customers)
                    if (c.Balance > 0) debtors++;
                if (debtors > 0)
                    alerts.Add("💳 " + debtors + " عميل لديه دين");

                // موردون لديهم مستحقات
                var supRepo = new Repositories.SupplierRepository();
                var suppliers = supRepo.GetAll(null);
                int creditors = 0;
                foreach (var s in suppliers)
                    if (s.Balance > 0) creditors++;
                if (creditors > 0)
                    alerts.Add("💰 " + creditors + " مورد مستحق الدفع");
            }
            catch { }

            if (alerts.Count == 0)
            {
                UiHelper.ToastInfo("لا توجد تنبيهات حالياً");
                return;
            }

            string msg = "التنبيهات الحالية:\n\n";
            foreach (var a in alerts)
                msg += a + "\n";

            UiHelper.Info(msg, "التنبيهات");
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (clockTimer != null) clockTimer.Stop();
            base.OnFormClosed(e);
        }
    }
}