namespace SetIODemo
{
    partial class SetIODemo
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetIODemo));
            this.cbDeviceList = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.bnClose = new System.Windows.Forms.Button();
            this.bnOpen = new System.Windows.Forms.Button();
            this.bnEnum = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.bnSetLineMode = new System.Windows.Forms.Button();
            this.bnGetLineMode = new System.Windows.Forms.Button();
            this.cbLineMode = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.bnSetLineSel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.bnGetLineSel = new System.Windows.Forms.Button();
            this.cbLineSel = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbDeviceList
            // 
            this.cbDeviceList.AccessibleDescription = null;
            this.cbDeviceList.AccessibleName = null;
            resources.ApplyResources(this.cbDeviceList, "cbDeviceList");
            this.cbDeviceList.BackgroundImage = null;
            this.cbDeviceList.Font = null;
            this.cbDeviceList.FormattingEnabled = true;
            this.cbDeviceList.Name = "cbDeviceList";
            // 
            // groupBox1
            // 
            this.groupBox1.AccessibleDescription = null;
            this.groupBox1.AccessibleName = null;
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.BackgroundImage = null;
            this.groupBox1.Controls.Add(this.bnClose);
            this.groupBox1.Controls.Add(this.bnOpen);
            this.groupBox1.Controls.Add(this.bnEnum);
            this.groupBox1.Font = null;
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // bnClose
            // 
            this.bnClose.AccessibleDescription = null;
            this.bnClose.AccessibleName = null;
            resources.ApplyResources(this.bnClose, "bnClose");
            this.bnClose.BackgroundImage = null;
            this.bnClose.Font = null;
            this.bnClose.Name = "bnClose";
            this.bnClose.UseVisualStyleBackColor = true;
            this.bnClose.Click += new System.EventHandler(this.bnClose_Click);
            // 
            // bnOpen
            // 
            this.bnOpen.AccessibleDescription = null;
            this.bnOpen.AccessibleName = null;
            resources.ApplyResources(this.bnOpen, "bnOpen");
            this.bnOpen.BackgroundImage = null;
            this.bnOpen.Font = null;
            this.bnOpen.Name = "bnOpen";
            this.bnOpen.UseVisualStyleBackColor = true;
            this.bnOpen.Click += new System.EventHandler(this.bnOpen_Click);
            // 
            // bnEnum
            // 
            this.bnEnum.AccessibleDescription = null;
            this.bnEnum.AccessibleName = null;
            resources.ApplyResources(this.bnEnum, "bnEnum");
            this.bnEnum.BackgroundImage = null;
            this.bnEnum.Font = null;
            this.bnEnum.Name = "bnEnum";
            this.bnEnum.UseVisualStyleBackColor = true;
            this.bnEnum.Click += new System.EventHandler(this.bnEnum_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.AccessibleDescription = null;
            this.groupBox3.AccessibleName = null;
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.BackgroundImage = null;
            this.groupBox3.Controls.Add(this.bnSetLineMode);
            this.groupBox3.Controls.Add(this.bnGetLineMode);
            this.groupBox3.Controls.Add(this.cbLineMode);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.bnSetLineSel);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.bnGetLineSel);
            this.groupBox3.Controls.Add(this.cbLineSel);
            this.groupBox3.Font = null;
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // bnSetLineMode
            // 
            this.bnSetLineMode.AccessibleDescription = null;
            this.bnSetLineMode.AccessibleName = null;
            resources.ApplyResources(this.bnSetLineMode, "bnSetLineMode");
            this.bnSetLineMode.BackgroundImage = null;
            this.bnSetLineMode.Font = null;
            this.bnSetLineMode.Name = "bnSetLineMode";
            this.bnSetLineMode.UseVisualStyleBackColor = true;
            this.bnSetLineMode.Click += new System.EventHandler(this.bnSetLineMode_Click);
            // 
            // bnGetLineMode
            // 
            this.bnGetLineMode.AccessibleDescription = null;
            this.bnGetLineMode.AccessibleName = null;
            resources.ApplyResources(this.bnGetLineMode, "bnGetLineMode");
            this.bnGetLineMode.BackgroundImage = null;
            this.bnGetLineMode.Font = null;
            this.bnGetLineMode.Name = "bnGetLineMode";
            this.bnGetLineMode.UseVisualStyleBackColor = true;
            this.bnGetLineMode.Click += new System.EventHandler(this.bnGetLineMode_Click);
            // 
            // cbLineMode
            // 
            this.cbLineMode.AccessibleDescription = null;
            this.cbLineMode.AccessibleName = null;
            resources.ApplyResources(this.cbLineMode, "cbLineMode");
            this.cbLineMode.BackgroundImage = null;
            this.cbLineMode.Font = null;
            this.cbLineMode.FormattingEnabled = true;
            this.cbLineMode.Name = "cbLineMode";
            // 
            // label2
            // 
            this.label2.AccessibleDescription = null;
            this.label2.AccessibleName = null;
            resources.ApplyResources(this.label2, "label2");
            this.label2.Font = null;
            this.label2.Name = "label2";
            // 
            // bnSetLineSel
            // 
            this.bnSetLineSel.AccessibleDescription = null;
            this.bnSetLineSel.AccessibleName = null;
            resources.ApplyResources(this.bnSetLineSel, "bnSetLineSel");
            this.bnSetLineSel.BackgroundImage = null;
            this.bnSetLineSel.Font = null;
            this.bnSetLineSel.Name = "bnSetLineSel";
            this.bnSetLineSel.UseVisualStyleBackColor = true;
            this.bnSetLineSel.Click += new System.EventHandler(this.bnSetLineSel_Click);
            // 
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Font = null;
            this.label1.Name = "label1";
            // 
            // bnGetLineSel
            // 
            this.bnGetLineSel.AccessibleDescription = null;
            this.bnGetLineSel.AccessibleName = null;
            resources.ApplyResources(this.bnGetLineSel, "bnGetLineSel");
            this.bnGetLineSel.BackgroundImage = null;
            this.bnGetLineSel.Font = null;
            this.bnGetLineSel.Name = "bnGetLineSel";
            this.bnGetLineSel.UseVisualStyleBackColor = true;
            this.bnGetLineSel.Click += new System.EventHandler(this.bnGetLineSel_Click);
            // 
            // cbLineSel
            // 
            this.cbLineSel.AccessibleDescription = null;
            this.cbLineSel.AccessibleName = null;
            resources.ApplyResources(this.cbLineSel, "cbLineSel");
            this.cbLineSel.BackgroundImage = null;
            this.cbLineSel.Font = null;
            this.cbLineSel.FormattingEnabled = true;
            this.cbLineSel.Name = "cbLineSel";
            // 
            // SetIODemo
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cbDeviceList);
            this.Font = null;
            this.Icon = null;
            this.Name = "SetIODemo";
            this.groupBox1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cbDeviceList;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button bnClose;
        private System.Windows.Forms.Button bnOpen;
        private System.Windows.Forms.Button bnEnum;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bnGetLineSel;
        private System.Windows.Forms.ComboBox cbLineSel;
        private System.Windows.Forms.Button bnSetLineMode;
        private System.Windows.Forms.Button bnGetLineMode;
        private System.Windows.Forms.ComboBox cbLineMode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button bnSetLineSel;
    }
}

