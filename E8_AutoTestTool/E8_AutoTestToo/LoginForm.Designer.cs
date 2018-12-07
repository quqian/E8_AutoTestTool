namespace A10_AutoTestTool
{
    partial class LoginForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            this.tableLayoutPanel_LoginForm = new System.Windows.Forms.TableLayoutPanel();
            this.skinLabel1 = new CCWin.SkinControl.SkinLabel();
            this.skinLabel2 = new CCWin.SkinControl.SkinLabel();
            this.textBox_UserName = new System.Windows.Forms.TextBox();
            this.textBox_LoginCode = new System.Windows.Forms.TextBox();
            this.skinButton_Login = new CCWin.SkinControl.SkinButton();
            this.tableLayoutPanel_LoginForm.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel_LoginForm
            // 
            this.tableLayoutPanel_LoginForm.BackColor = System.Drawing.Color.LightSteelBlue;
            this.tableLayoutPanel_LoginForm.ColumnCount = 6;
            this.tableLayoutPanel_LoginForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.76923F));
            this.tableLayoutPanel_LoginForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49.23077F));
            this.tableLayoutPanel_LoginForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 228F));
            this.tableLayoutPanel_LoginForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutPanel_LoginForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
            this.tableLayoutPanel_LoginForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.tableLayoutPanel_LoginForm.Controls.Add(this.skinLabel1, 1, 1);
            this.tableLayoutPanel_LoginForm.Controls.Add(this.skinLabel2, 1, 2);
            this.tableLayoutPanel_LoginForm.Controls.Add(this.textBox_UserName, 2, 1);
            this.tableLayoutPanel_LoginForm.Controls.Add(this.textBox_LoginCode, 2, 2);
            this.tableLayoutPanel_LoginForm.Controls.Add(this.skinButton_Login, 2, 4);
            this.tableLayoutPanel_LoginForm.Location = new System.Drawing.Point(-122, 1);
            this.tableLayoutPanel_LoginForm.Name = "tableLayoutPanel_LoginForm";
            this.tableLayoutPanel_LoginForm.RowCount = 6;
            this.tableLayoutPanel_LoginForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel_LoginForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel_LoginForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel_LoginForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel_LoginForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel_LoginForm.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.tableLayoutPanel_LoginForm.Size = new System.Drawing.Size(794, 344);
            this.tableLayoutPanel_LoginForm.TabIndex = 3;
            // 
            // skinLabel1
            // 
            this.skinLabel1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.skinLabel1.AutoSize = true;
            this.skinLabel1.BackColor = System.Drawing.Color.Transparent;
            this.skinLabel1.BorderColor = System.Drawing.Color.White;
            this.skinLabel1.Font = new System.Drawing.Font("华文中宋", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel1.Location = new System.Drawing.Point(280, 66);
            this.skinLabel1.Name = "skinLabel1";
            this.skinLabel1.Size = new System.Drawing.Size(73, 21);
            this.skinLabel1.TabIndex = 0;
            this.skinLabel1.Text = "用户名:";
            // 
            // skinLabel2
            // 
            this.skinLabel2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.skinLabel2.AutoSize = true;
            this.skinLabel2.BackColor = System.Drawing.Color.Transparent;
            this.skinLabel2.BorderColor = System.Drawing.Color.White;
            this.skinLabel2.Font = new System.Drawing.Font("华文中宋", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel2.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.skinLabel2.ForeColorSuit = true;
            this.skinLabel2.Location = new System.Drawing.Point(299, 121);
            this.skinLabel2.Name = "skinLabel2";
            this.skinLabel2.Size = new System.Drawing.Size(54, 21);
            this.skinLabel2.TabIndex = 1;
            this.skinLabel2.Text = "密码:";
            this.skinLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBox_UserName
            // 
            this.textBox_UserName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBox_UserName.Font = new System.Drawing.Font("华文中宋", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_UserName.Location = new System.Drawing.Point(359, 60);
            this.textBox_UserName.MaxLength = 16;
            this.textBox_UserName.Name = "textBox_UserName";
            this.textBox_UserName.Size = new System.Drawing.Size(210, 33);
            this.textBox_UserName.TabIndex = 2;
            // 
            // textBox_LoginCode
            // 
            this.textBox_LoginCode.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBox_LoginCode.Font = new System.Drawing.Font("华文中宋", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_LoginCode.Location = new System.Drawing.Point(359, 115);
            this.textBox_LoginCode.MaxLength = 16;
            this.textBox_LoginCode.Name = "textBox_LoginCode";
            this.textBox_LoginCode.PasswordChar = '*';
            this.textBox_LoginCode.Size = new System.Drawing.Size(210, 33);
            this.textBox_LoginCode.TabIndex = 3;
            this.textBox_LoginCode.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_LoginCode_KeyPress);
            // 
            // skinButton_Login
            // 
            this.skinButton_Login.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.skinButton_Login.BackColor = System.Drawing.Color.Transparent;
            this.skinButton_Login.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.skinButton_Login.DownBack = null;
            this.skinButton_Login.Font = new System.Drawing.Font("华文中宋", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinButton_Login.Location = new System.Drawing.Point(359, 216);
            this.skinButton_Login.MouseBack = null;
            this.skinButton_Login.Name = "skinButton_Login";
            this.skinButton_Login.NormlBack = null;
            this.skinButton_Login.Size = new System.Drawing.Size(210, 43);
            this.skinButton_Login.TabIndex = 4;
            this.skinButton_Login.Text = "登录";
            this.skinButton_Login.UseVisualStyleBackColor = false;
            this.skinButton_Login.Click += new System.EventHandler(this.skinButton_Login_Click);
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(672, 345);
            this.Controls.Add(this.tableLayoutPanel_LoginForm);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LoginForm";
            this.Text = "用户登录";
            this.Load += new System.EventHandler(this.LoginForm_Load);
            this.tableLayoutPanel_LoginForm.ResumeLayout(false);
            this.tableLayoutPanel_LoginForm.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_LoginForm;
        private CCWin.SkinControl.SkinLabel skinLabel1;
        private CCWin.SkinControl.SkinLabel skinLabel2;
        private System.Windows.Forms.TextBox textBox_UserName;
        private System.Windows.Forms.TextBox textBox_LoginCode;
        private CCWin.SkinControl.SkinButton skinButton_Login;
    }
}