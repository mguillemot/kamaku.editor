namespace Kamaku
{
    partial class FormBulletEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBulletEditor));
            this.surface = new SdlDotNet.Windows.SurfaceControl();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxHitCount = new System.Windows.Forms.TextBox();
            this.richTextBoxBulletML = new System.Windows.Forms.RichTextBox();
            this.buttonValidate = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.surface)).BeginInit();
            this.SuspendLayout();
            // 
            // surface
            // 
            this.surface.AccessibleDescription = "SdlDotNet SurfaceControl";
            this.surface.AccessibleName = "SurfaceControl";
            this.surface.AccessibleRole = System.Windows.Forms.AccessibleRole.Graphic;
            this.surface.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.surface.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.surface.Image = ((System.Drawing.Image)(resources.GetObject("surface.Image")));
            this.surface.InitialImage = ((System.Drawing.Image)(resources.GetObject("surface.InitialImage")));
            this.surface.Location = new System.Drawing.Point(12, 12);
            this.surface.Name = "surface";
            this.surface.Size = new System.Drawing.Size(240, 320);
            this.surface.TabIndex = 0;
            this.surface.TabStop = false;
            this.surface.MouseLeave += new System.EventHandler(this.surface_MouseLeave);
            this.surface.MouseMove += new System.Windows.Forms.MouseEventHandler(this.surface_MouseMove);
            this.surface.MouseUp += new System.Windows.Forms.MouseEventHandler(this.surface_MouseUp);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 344);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Hit count:";
            // 
            // textBoxHitCount
            // 
            this.textBoxHitCount.Location = new System.Drawing.Point(68, 341);
            this.textBoxHitCount.Name = "textBoxHitCount";
            this.textBoxHitCount.ReadOnly = true;
            this.textBoxHitCount.Size = new System.Drawing.Size(50, 20);
            this.textBoxHitCount.TabIndex = 2;
            this.textBoxHitCount.Text = "0";
            this.textBoxHitCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxHitCount.WordWrap = false;
            // 
            // richTextBoxBulletML
            // 
            this.richTextBoxBulletML.DetectUrls = false;
            this.richTextBoxBulletML.Location = new System.Drawing.Point(261, 12);
            this.richTextBoxBulletML.Name = "richTextBoxBulletML";
            this.richTextBoxBulletML.Size = new System.Drawing.Size(400, 416);
            this.richTextBoxBulletML.TabIndex = 3;
            this.richTextBoxBulletML.Text = resources.GetString("richTextBoxBulletML.Text");
            // 
            // buttonValidate
            // 
            this.buttonValidate.Location = new System.Drawing.Point(89, 384);
            this.buttonValidate.Name = "buttonValidate";
            this.buttonValidate.Size = new System.Drawing.Size(75, 23);
            this.buttonValidate.TabIndex = 4;
            this.buttonValidate.Text = "Validate";
            this.buttonValidate.UseVisualStyleBackColor = true;
            this.buttonValidate.Click += new System.EventHandler(this.buttonValidate_Click);
            // 
            // FormBulletEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(669, 437);
            this.Controls.Add(this.buttonValidate);
            this.Controls.Add(this.richTextBoxBulletML);
            this.Controls.Add(this.textBoxHitCount);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.surface);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormBulletEditor";
            this.Text = "Kamaku Bullet Editor";
            this.Load += new System.EventHandler(this.FormBulletEditor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.surface)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SdlDotNet.Windows.SurfaceControl surface;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxHitCount;
        private System.Windows.Forms.RichTextBox richTextBoxBulletML;
        private System.Windows.Forms.Button buttonValidate;
    }
}

