using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AccountingSystem.Helpers;
using AccountingSystem.Services;
using AccountingSystem.Theme;

namespace AccountingSystem.UserControls
{
    public class ReportsControl : UserControl
    {
        private ComboBox cboReportType;
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private DoubleBufferedGrid dgv;
        private Label lblSummary;
        private ReportService service = new ReportService();

        public ReportsControl()
        {
            RightToLeft = RightToLeft.Yes;
            
            
            
            BackColor = ThemeManager.Background;
            Dock = DockStyle.Fill;
            Build();
        }

        private void Build()
        {
            var toolbar = new Panel();
            toolbar.RightToLeft = RightToLeft.Yes;
            toolbar.Dock = DockStyle.Top;
            toolbar.Height = 100;
            toolbar.BackColor = ThemeManager.Panel;
            toolbar.Padding = new Padding(20);
            Controls.Add(toolbar);

            int y = 15;

            AddLabel(toolbar, "نوع التقرير:", 15, y);
            cboReportType = new ComboBox();
            cboReportType.Location = new Point(15, y + 22);
            cboReportType.Width = 220;
            cboReportType.DropDownStyle = ComboBoxStyle.DropDownList;
            ThemeManager.ApplyComboBox(cboReportType);
            cboReportType.Items.Add("تقرير المبيعات");
            cboReportType.Items.Add("تقرير المشتريات");
            cboReportType.Items.Add("تقرير الأرباح");
            cboReportType.Items.Add("المنتجات الأكثر مبيعاً");
            cboReportType.SelectedIndex = 0;
            toolbar.Controls.Add(cboReportType);

            AddLabel(toolbar, "من تاريخ:", 255, y);
            dtpFrom = new DateTimePicker();
            dtpFrom.Location = new Point(255, y + 22);
            dtpFrom.Width = 150;
            dtpFrom.Format = DateTimePickerFormat.Short;
            dtpFrom.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            toolbar.Controls.Add(dtpFrom);

            AddLabel(toolbar, "إلى تاريخ:", 425, y);
            dtpTo = new DateTimePicker();
            dtpTo.Location = new Point(425, y + 22);
            dtpTo.Width = 150;
            dtpTo.Format = DateTimePickerFormat.Short;
            dtpTo.Value = DateTime.Now;
            toolbar.Controls.Add(dtpTo);

            var btnRun = new Button();
            btnRun.Text = "▶ تشغيل التقرير";
            btnRun.Location = new Point(595, y + 20);
            btnRun.Width = 180;
            btnRun.Height = 34;
            ThemeManager.ApplyPrimaryButton(btnRun);
            btnRun.Click += delegate { RunReport(); };
            toolbar.Controls.Add(btnRun);

            var btnExport = new Button();
            btnExport.Text = "📊 تصدير Excel";
            btnExport.Location = new Point(785, y + 20);
            btnExport.Width = 180;
            btnExport.Height = 34;
            ThemeManager.ApplyGhostButton(btnExport);
            btnExport.Click += delegate { ExportToExcel(); };
            toolbar.Controls.Add(btnExport);

            lblSummary = new Label();
            lblSummary.RightToLeft = RightToLeft.Yes;
            lblSummary.Dock = DockStyle.Top;
            lblSummary.Height = 40;
            lblSummary.Font = ThemeManager.FontBold;
            lblSummary.ForeColor = ThemeManager.Text;
            lblSummary.Padding = new Padding(20, 10, 20, 0);
            lblSummary.
            RightToLeft = RightToLeft.Yes;
            
            
            BackColor = ThemeManager.Background;
            Controls.Add(lblSummary);
            lblSummary.BringToFront();
            toolbar.BringToFront();

            dgv = new DoubleBufferedGrid();
            dgv.RightToLeft = RightToLeft.Yes;
            dgv.Dock = DockStyle.Fill;
            ThemeManager.ApplyGrid(dgv);
            Controls.Add(dgv);
            dgv.BringToFront();
        }

        private void AddLabel(Control p, string text, int x, int y)
        {
            var lbl = new Label();
            lbl.Text = text;
            lbl.Location = new Point(x, y);
            lbl.AutoSize = true;
            lbl.ForeColor = ThemeManager.Muted;
            lbl.Font = ThemeManager.FontSmall;
            p.Controls.Add(lbl);
        }

        private void RunReport()
        {
            try
            {
                DateTime from = dtpFrom.Value.Date;
                DateTime to = dtpTo.Value.Date.AddDays(1).AddSeconds(-1);

                DataTable dt = null;
                string type = cboReportType.SelectedItem.ToString();

                if (type == "تقرير المبيعات")
                    dt = service.GetSalesReport(from, to);
                else if (type == "تقرير المشتريات")
                    dt = service.GetPurchasesReport(from, to);
                else if (type == "تقرير الأرباح")
                    dt = service.GetProfitReport(from, to);
                else if (type == "المنتجات الأكثر مبيعاً")
                    dt = service.GetTopSellingProducts(from, to, 20);

                if (dt == null) return;

                dgv.Columns.Clear();
                dgv.AutoGenerateColumns = true;
                dgv.DataSource = dt;
                dgv.AutoGenerateColumns = false;

                lblSummary.Text = "عدد السجلات: " + dt.Rows.Count +
                    "   |   الفترة: " + from.ToString("yyyy-MM-dd") +
                    " إلى " + to.ToString("yyyy-MM-dd");
            }
            catch (Exception ex)
            {
                UiHelper.Error("فشل إنشاء التقرير: " + ex.Message);
            }
        }

        private void ExportToExcel()
        {
            try
            {
                if (dgv.Rows.Count == 0)
                {
                    UiHelper.Warn("لا توجد بيانات للتصدير.");
                    return;
                }

                var saveDialog = new SaveFileDialog();
                saveDialog.Filter = "CSV Files (*.csv)|*.csv";
                saveDialog.FileName = "Report_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";

                if (saveDialog.ShowDialog() != DialogResult.OK) return;

                using (var writer = new System.IO.StreamWriter(saveDialog.FileName, false, System.Text.Encoding.UTF8))
                {
                    for (int i = 0; i < dgv.Columns.Count; i++)
                    {
                        if (i > 0) writer.Write(",");
                        writer.Write("\"" + dgv.Columns[i].HeaderText + "\"");
                    }
                    writer.WriteLine();

                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        if (row.IsNewRow) continue;
                        for (int i = 0; i < dgv.Columns.Count; i++)
                        {
                            if (i > 0) writer.Write(",");
                            var val = row.Cells[i].Value;
                            writer.Write("\"" + (val != null ? val.ToString() : "") + "\"");
                        }
                        writer.WriteLine();
                    }
                }

                UiHelper.Info("تم التصدير بنجاح إلى:\n" + saveDialog.FileName);
            }
            catch (Exception ex)
            {
                UiHelper.Error("فشل التصدير: " + ex.Message);
            }
        }
    }
}