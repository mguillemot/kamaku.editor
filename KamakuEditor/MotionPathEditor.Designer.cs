namespace Kamaku
{
    partial class MotionPathEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MotionPathEditor));
            this.surfaceControl1 = new SdlDotNet.Windows.SurfaceControl();
            this.button1 = new System.Windows.Forms.Button();
            this.buttonClear = new System.Windows.Forms.Button();
            this.listBoxNodes = new System.Windows.Forms.ListBox();
            this.surfaceControlSpeedGraph = new SdlDotNet.Windows.SurfaceControl();
            ((System.ComponentModel.ISupportInitialize)(this.surfaceControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.surfaceControlSpeedGraph)).BeginInit();
            this.SuspendLayout();
            // 
            // surfaceControl1
            // 
            this.surfaceControl1.AccessibleDescription = "SdlDotNet SurfaceControl";
            this.surfaceControl1.AccessibleName = "SurfaceControl";
            this.surfaceControl1.AccessibleRole = System.Windows.Forms.AccessibleRole.Graphic;
            this.surfaceControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.surfaceControl1.Image = ((System.Drawing.Image)(resources.GetObject("surfaceControl1.Image")));
            this.surfaceControl1.InitialImage = ((System.Drawing.Image)(resources.GetObject("surfaceControl1.InitialImage")));
            this.surfaceControl1.Location = new System.Drawing.Point(12, 12);
            this.surfaceControl1.Name = "surfaceControl1";
            this.surfaceControl1.Size = new System.Drawing.Size(340, 420);
            this.surfaceControl1.TabIndex = 0;
            this.surfaceControl1.TabStop = false;
            this.surfaceControl1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.surfaceControl1_MouseMove);
            this.surfaceControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.surfaceControl1_MouseUp);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(419, 24);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(97, 32);
            this.button1.TabIndex = 1;
            this.button1.Text = "Restart";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // buttonClear
            // 
            this.buttonClear.Location = new System.Drawing.Point(419, 78);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(97, 32);
            this.buttonClear.TabIndex = 2;
            this.buttonClear.Text = "Clear";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // listBoxNodes
            // 
            this.listBoxNodes.FormattingEnabled = true;
            this.listBoxNodes.Location = new System.Drawing.Point(381, 218);
            this.listBoxNodes.Name = "listBoxNodes";
            this.listBoxNodes.Size = new System.Drawing.Size(191, 212);
            this.listBoxNodes.TabIndex = 3;
            // 
            // surfaceControlSpeedGraph
            // 
            this.surfaceControlSpeedGraph.AccessibleDescription = "SdlDotNet SurfaceControl";
            this.surfaceControlSpeedGraph.AccessibleName = "SurfaceControl";
            this.surfaceControlSpeedGraph.AccessibleRole = System.Windows.Forms.AccessibleRole.Graphic;
            this.surfaceControlSpeedGraph.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.surfaceControlSpeedGraph.Image = ((System.Drawing.Image)(resources.GetObject("surfaceControlSpeedGraph.Image")));
            this.surfaceControlSpeedGraph.InitialImage = ((System.Drawing.Image)(resources.GetObject("surfaceControlSpeedGraph.InitialImage")));
            this.surfaceControlSpeedGraph.Location = new System.Drawing.Point(12, 446);
            this.surfaceControlSpeedGraph.Name = "surfaceControlSpeedGraph";
            this.surfaceControlSpeedGraph.Size = new System.Drawing.Size(558, 169);
            this.surfaceControlSpeedGraph.TabIndex = 4;
            this.surfaceControlSpeedGraph.TabStop = false;
            this.surfaceControlSpeedGraph.MouseMove += new System.Windows.Forms.MouseEventHandler(this.surfaceControlSpeedGraph_MouseMove);
            this.surfaceControlSpeedGraph.MouseUp += new System.Windows.Forms.MouseEventHandler(this.surfaceControlSpeedGraph_MouseUp);
            // 
            // MotionPathEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(595, 631);
            this.Controls.Add(this.surfaceControlSpeedGraph);
            this.Controls.Add(this.listBoxNodes);
            this.Controls.Add(this.buttonClear);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.surfaceControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MotionPathEditor";
            this.Text = "Kamaku MotionPath Editor";
            this.Load += new System.EventHandler(this.MotionPathEditor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.surfaceControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.surfaceControlSpeedGraph)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private SdlDotNet.Windows.SurfaceControl surfaceControl1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.ListBox listBoxNodes;
        private SdlDotNet.Windows.SurfaceControl surfaceControlSpeedGraph;
    }
}