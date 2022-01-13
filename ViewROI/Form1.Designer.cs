
namespace ViewROI
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
            this.hWindowControl1 = new HalconDotNet.HWindowControl();
            this.ExitApplButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButtonNone = new System.Windows.Forms.RadioButton();
            this.radioButtonZoom = new System.Windows.Forms.RadioButton();
            this.radioButtonMove = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.CircArcButton = new System.Windows.Forms.Button();
            this.LineButton = new System.Windows.Forms.Button();
            this.CircleButton = new System.Windows.Forms.Button();
            this.DelActROIButton = new System.Windows.Forms.Button();
            this.Rect2Button = new System.Windows.Forms.Button();
            this.Rect1Button = new System.Windows.Forms.Button();
            this.ResetButton = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // hWindowControl1
            // 
            this.hWindowControl1.BackColor = System.Drawing.Color.Black;
            this.hWindowControl1.BorderColor = System.Drawing.Color.Black;
            this.hWindowControl1.ImagePart = new System.Drawing.Rectangle(0, 0, 640, 480);
            this.hWindowControl1.Location = new System.Drawing.Point(-16, 12);
            this.hWindowControl1.Name = "hWindowControl1";
            this.hWindowControl1.Size = new System.Drawing.Size(541, 357);
            this.hWindowControl1.TabIndex = 0;
            this.hWindowControl1.WindowSize = new System.Drawing.Size(541, 357);
            // 
            // ExitApplButton
            // 
            this.ExitApplButton.Location = new System.Drawing.Point(755, 241);
            this.ExitApplButton.Name = "ExitApplButton";
            this.ExitApplButton.Size = new System.Drawing.Size(96, 43);
            this.ExitApplButton.TabIndex = 9;
            this.ExitApplButton.Text = "Exit Application";
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox2.Controls.Add(this.radioButtonNone);
            this.groupBox2.Controls.Add(this.radioButtonZoom);
            this.groupBox2.Controls.Add(this.radioButtonMove);
            this.groupBox2.Location = new System.Drawing.Point(595, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(154, 147);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "View Interaction";
            // 
            // radioButtonNone
            // 
            this.radioButtonNone.Checked = true;
            this.radioButtonNone.Location = new System.Drawing.Point(29, 112);
            this.radioButtonNone.Name = "radioButtonNone";
            this.radioButtonNone.Size = new System.Drawing.Size(96, 17);
            this.radioButtonNone.TabIndex = 2;
            this.radioButtonNone.TabStop = true;
            this.radioButtonNone.Text = "none";
            this.radioButtonNone.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radioButtonNone.CheckedChanged += new System.EventHandler(this.radioButtonNone_CheckedChanged);
            // 
            // radioButtonZoom
            // 
            this.radioButtonZoom.Location = new System.Drawing.Point(29, 69);
            this.radioButtonZoom.Name = "radioButtonZoom";
            this.radioButtonZoom.Size = new System.Drawing.Size(96, 26);
            this.radioButtonZoom.TabIndex = 1;
            this.radioButtonZoom.Text = "zoom";
            this.radioButtonZoom.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radioButtonZoom.CheckedChanged += new System.EventHandler(this.radioButtonZoom_CheckedChanged);
            // 
            // radioButtonMove
            // 
            this.radioButtonMove.Location = new System.Drawing.Point(29, 26);
            this.radioButtonMove.Name = "radioButtonMove";
            this.radioButtonMove.Size = new System.Drawing.Size(96, 26);
            this.radioButtonMove.TabIndex = 0;
            this.radioButtonMove.Text = "move";
            this.radioButtonMove.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.radioButtonMove.CheckedChanged += new System.EventHandler(this.radioButtonMove_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox1.Controls.Add(this.CircArcButton);
            this.groupBox1.Controls.Add(this.LineButton);
            this.groupBox1.Controls.Add(this.CircleButton);
            this.groupBox1.Controls.Add(this.DelActROIButton);
            this.groupBox1.Controls.Add(this.Rect2Button);
            this.groupBox1.Controls.Add(this.Rect1Button);
            this.groupBox1.Location = new System.Drawing.Point(595, 181);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(154, 259);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Create ROI";
            // 
            // CircArcButton
            // 
            this.CircArcButton.Location = new System.Drawing.Point(29, 129);
            this.CircArcButton.Name = "CircArcButton";
            this.CircArcButton.Size = new System.Drawing.Size(96, 35);
            this.CircArcButton.TabIndex = 10;
            this.CircArcButton.Text = "Circular Arc";
            this.CircArcButton.Click += new System.EventHandler(this.CircArcButton_Click);
            // 
            // LineButton
            // 
            this.LineButton.Location = new System.Drawing.Point(29, 164);
            this.LineButton.Name = "LineButton";
            this.LineButton.Size = new System.Drawing.Size(96, 34);
            this.LineButton.TabIndex = 9;
            this.LineButton.Text = "Line";
            this.LineButton.Click += new System.EventHandler(this.LineButton_Click);
            // 
            // CircleButton
            // 
            this.CircleButton.Location = new System.Drawing.Point(29, 95);
            this.CircleButton.Name = "CircleButton";
            this.CircleButton.Size = new System.Drawing.Size(96, 34);
            this.CircleButton.TabIndex = 8;
            this.CircleButton.Text = "Circle";
            this.CircleButton.Click += new System.EventHandler(this.CircleButton_Click);
            // 
            // DelActROIButton
            // 
            this.DelActROIButton.Location = new System.Drawing.Point(29, 215);
            this.DelActROIButton.Name = "DelActROIButton";
            this.DelActROIButton.Size = new System.Drawing.Size(96, 35);
            this.DelActROIButton.TabIndex = 7;
            this.DelActROIButton.Text = "Delete Active ROI";
            this.DelActROIButton.Click += new System.EventHandler(this.DelActROIButton_Click);
            // 
            // Rect2Button
            // 
            this.Rect2Button.Location = new System.Drawing.Point(29, 60);
            this.Rect2Button.Name = "Rect2Button";
            this.Rect2Button.Size = new System.Drawing.Size(96, 35);
            this.Rect2Button.TabIndex = 1;
            this.Rect2Button.Text = "Rectangle2";
            this.Rect2Button.Click += new System.EventHandler(this.Rect2Button_Click);
            // 
            // Rect1Button
            // 
            this.Rect1Button.Location = new System.Drawing.Point(29, 26);
            this.Rect1Button.Name = "Rect1Button";
            this.Rect1Button.Size = new System.Drawing.Size(96, 34);
            this.Rect1Button.TabIndex = 0;
            this.Rect1Button.Text = "Rectangle1";
            this.Rect1Button.Click += new System.EventHandler(this.Rect1Button_Click);
            // 
            // ResetButton
            // 
            this.ResetButton.Location = new System.Drawing.Point(755, 198);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(96, 43);
            this.ResetButton.TabIndex = 10;
            this.ResetButton.Text = "Reset All";
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1081, 530);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.ExitApplButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.ResetButton);
            this.Controls.Add(this.hWindowControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private HalconDotNet.HWindowControl hWindowControl1;
        private System.Windows.Forms.Button ExitApplButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButtonNone;
        private System.Windows.Forms.RadioButton radioButtonZoom;
        private System.Windows.Forms.RadioButton radioButtonMove;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button CircArcButton;
        private System.Windows.Forms.Button LineButton;
        private System.Windows.Forms.Button CircleButton;
        public System.Windows.Forms.Button DelActROIButton;
        public System.Windows.Forms.Button Rect2Button;
        public System.Windows.Forms.Button Rect1Button;
        public System.Windows.Forms.Button ResetButton;
    }
}

