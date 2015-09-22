namespace AmazonTest
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tp_unshipped_order = new System.Windows.Forms.TabPage();
            this.btn_export_eub_excel = new System.Windows.Forms.Button();
            this.btn_tag_observed = new System.Windows.Forms.Button();
            this.btn_get_order_report = new System.Windows.Forms.Button();
            this.dgv_unshipped_order = new System.Windows.Forms.DataGridView();
            this.tp_scheduled_task = new System.Windows.Forms.TabPage();
            this.btn_ship_order = new System.Windows.Forms.Button();
            this.btn_update_track_id = new System.Windows.Forms.Button();
            this.btn_tag_fake_shipped = new System.Windows.Forms.Button();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dgv_fake_unshipped_order = new System.Windows.Forms.DataGridView();
            this.dgv_observed_order = new System.Windows.Forms.DataGridView();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.dgv_ship_failed_order = new System.Windows.Forms.DataGridView();
            this.tabControl1.SuspendLayout();
            this.tp_unshipped_order.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_unshipped_order)).BeginInit();
            this.tp_scheduled_task.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_fake_unshipped_order)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_observed_order)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_ship_failed_order)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(144, 163);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(287, 163);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "End";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tp_unshipped_order);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tp_scheduled_task);
            this.tabControl1.Location = new System.Drawing.Point(36, 23);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1293, 743);
            this.tabControl1.TabIndex = 2;
            this.tabControl1.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl1_Selected);
            // 
            // tp_unshipped_order
            // 
            this.tp_unshipped_order.Controls.Add(this.btn_tag_fake_shipped);
            this.tp_unshipped_order.Controls.Add(this.btn_update_track_id);
            this.tp_unshipped_order.Controls.Add(this.btn_ship_order);
            this.tp_unshipped_order.Controls.Add(this.btn_export_eub_excel);
            this.tp_unshipped_order.Controls.Add(this.btn_tag_observed);
            this.tp_unshipped_order.Controls.Add(this.btn_get_order_report);
            this.tp_unshipped_order.Controls.Add(this.dgv_unshipped_order);
            this.tp_unshipped_order.Location = new System.Drawing.Point(4, 22);
            this.tp_unshipped_order.Name = "tp_unshipped_order";
            this.tp_unshipped_order.Padding = new System.Windows.Forms.Padding(3);
            this.tp_unshipped_order.Size = new System.Drawing.Size(1285, 717);
            this.tp_unshipped_order.TabIndex = 0;
            this.tp_unshipped_order.Text = "Unshipped Order";
            this.tp_unshipped_order.UseVisualStyleBackColor = true;
            // 
            // btn_export_eub_excel
            // 
            this.btn_export_eub_excel.Location = new System.Drawing.Point(472, 24);
            this.btn_export_eub_excel.Name = "btn_export_eub_excel";
            this.btn_export_eub_excel.Size = new System.Drawing.Size(174, 39);
            this.btn_export_eub_excel.TabIndex = 1;
            this.btn_export_eub_excel.Text = "3. Export EUB Excel";
            this.btn_export_eub_excel.UseVisualStyleBackColor = true;
            this.btn_export_eub_excel.Click += new System.EventHandler(this.btn_export_eub_excel_Click);
            // 
            // btn_tag_observed
            // 
            this.btn_tag_observed.Location = new System.Drawing.Point(290, 24);
            this.btn_tag_observed.Name = "btn_tag_observed";
            this.btn_tag_observed.Size = new System.Drawing.Size(125, 39);
            this.btn_tag_observed.TabIndex = 1;
            this.btn_tag_observed.Text = "2. Tag Observed";
            this.btn_tag_observed.UseVisualStyleBackColor = true;
            this.btn_tag_observed.Click += new System.EventHandler(this.btn_tag_observed_Click);
            // 
            // btn_get_order_report
            // 
            this.btn_get_order_report.Location = new System.Drawing.Point(71, 24);
            this.btn_get_order_report.Name = "btn_get_order_report";
            this.btn_get_order_report.Size = new System.Drawing.Size(160, 39);
            this.btn_get_order_report.TabIndex = 1;
            this.btn_get_order_report.Text = "1. Get Order Report";
            this.btn_get_order_report.UseVisualStyleBackColor = true;
            this.btn_get_order_report.Click += new System.EventHandler(this.btn_get_order_report_Click);
            // 
            // dgv_unshipped_order
            // 
            this.dgv_unshipped_order.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_unshipped_order.Location = new System.Drawing.Point(33, 80);
            this.dgv_unshipped_order.Name = "dgv_unshipped_order";
            this.dgv_unshipped_order.RowTemplate.Height = 23;
            this.dgv_unshipped_order.Size = new System.Drawing.Size(1216, 614);
            this.dgv_unshipped_order.TabIndex = 0;
            this.dgv_unshipped_order.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.dgv_unshipped_order_RowPostPaint);
            // 
            // tp_scheduled_task
            // 
            this.tp_scheduled_task.Controls.Add(this.button1);
            this.tp_scheduled_task.Controls.Add(this.button2);
            this.tp_scheduled_task.Location = new System.Drawing.Point(4, 22);
            this.tp_scheduled_task.Name = "tp_scheduled_task";
            this.tp_scheduled_task.Padding = new System.Windows.Forms.Padding(3);
            this.tp_scheduled_task.Size = new System.Drawing.Size(1285, 717);
            this.tp_scheduled_task.TabIndex = 1;
            this.tp_scheduled_task.Text = "Scheduled Task";
            this.tp_scheduled_task.UseVisualStyleBackColor = true;
            // 
            // btn_ship_order
            // 
            this.btn_ship_order.Location = new System.Drawing.Point(1096, 23);
            this.btn_ship_order.Name = "btn_ship_order";
            this.btn_ship_order.Size = new System.Drawing.Size(122, 39);
            this.btn_ship_order.TabIndex = 2;
            this.btn_ship_order.Text = "6. Confirm Order";
            this.btn_ship_order.UseVisualStyleBackColor = true;
            this.btn_ship_order.Click += new System.EventHandler(this.btn_ship_order_Click);
            // 
            // btn_update_track_id
            // 
            this.btn_update_track_id.Location = new System.Drawing.Point(697, 23);
            this.btn_update_track_id.Name = "btn_update_track_id";
            this.btn_update_track_id.Size = new System.Drawing.Size(153, 39);
            this.btn_update_track_id.TabIndex = 3;
            this.btn_update_track_id.Text = "4. Update Tracking ID";
            this.btn_update_track_id.UseVisualStyleBackColor = true;
            this.btn_update_track_id.Click += new System.EventHandler(this.btn_update_track_id_Click);
            // 
            // btn_tag_fake_shipped
            // 
            this.btn_tag_fake_shipped.Location = new System.Drawing.Point(908, 24);
            this.btn_tag_fake_shipped.Name = "btn_tag_fake_shipped";
            this.btn_tag_fake_shipped.Size = new System.Drawing.Size(138, 39);
            this.btn_tag_fake_shipped.TabIndex = 4;
            this.btn_tag_fake_shipped.Text = "5. Tag Fake Shipped";
            this.btn_tag_fake_shipped.UseVisualStyleBackColor = true;
            this.btn_tag_fake_shipped.Click += new System.EventHandler(this.btn_tag_fake_shipped_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dgv_observed_order);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1285, 717);
            this.tabPage1.TabIndex = 2;
            this.tabPage1.Text = "Observed Order";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dgv_fake_unshipped_order);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1285, 717);
            this.tabPage2.TabIndex = 3;
            this.tabPage2.Text = "Fake Shipped Order";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dgv_fake_unshipped_order
            // 
            this.dgv_fake_unshipped_order.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_fake_unshipped_order.Location = new System.Drawing.Point(32, 42);
            this.dgv_fake_unshipped_order.Name = "dgv_fake_unshipped_order";
            this.dgv_fake_unshipped_order.RowTemplate.Height = 23;
            this.dgv_fake_unshipped_order.Size = new System.Drawing.Size(1193, 638);
            this.dgv_fake_unshipped_order.TabIndex = 0;
            // 
            // dgv_observed_order
            // 
            this.dgv_observed_order.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_observed_order.Location = new System.Drawing.Point(50, 71);
            this.dgv_observed_order.Name = "dgv_observed_order";
            this.dgv_observed_order.RowTemplate.Height = 23;
            this.dgv_observed_order.Size = new System.Drawing.Size(1173, 606);
            this.dgv_observed_order.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.dgv_ship_failed_order);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(1285, 717);
            this.tabPage3.TabIndex = 4;
            this.tabPage3.Text = "Ship Failed Order";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // dgv_ship_failed_order
            // 
            this.dgv_ship_failed_order.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_ship_failed_order.Location = new System.Drawing.Point(47, 53);
            this.dgv_ship_failed_order.Name = "dgv_ship_failed_order";
            this.dgv_ship_failed_order.RowTemplate.Height = 23;
            this.dgv_ship_failed_order.Size = new System.Drawing.Size(1194, 617);
            this.dgv_ship_failed_order.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1353, 790);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "UprightBuz";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl1.ResumeLayout(false);
            this.tp_unshipped_order.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_unshipped_order)).EndInit();
            this.tp_scheduled_task.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_fake_unshipped_order)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_observed_order)).EndInit();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_ship_failed_order)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tp_unshipped_order;
        private System.Windows.Forms.DataGridView dgv_unshipped_order;
        private System.Windows.Forms.TabPage tp_scheduled_task;
        private System.Windows.Forms.Button btn_get_order_report;
        private System.Windows.Forms.Button btn_tag_observed;
        private System.Windows.Forms.Button btn_export_eub_excel;
        private System.Windows.Forms.Button btn_ship_order;
        private System.Windows.Forms.Button btn_update_track_id;
        private System.Windows.Forms.Button btn_tag_fake_shipped;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView dgv_fake_unshipped_order;
        private System.Windows.Forms.DataGridView dgv_observed_order;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.DataGridView dgv_ship_failed_order;
    }
}

