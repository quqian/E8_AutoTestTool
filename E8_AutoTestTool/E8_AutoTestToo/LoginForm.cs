using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace A10_AutoTestTool
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            try
            {
                Server.Account = Server.GetMysqlUserInfo("user");
                Server.Password = Server.GetMysqlUserInfo("password");

                if (Server.Account == null)
                {
                    MessageBox.Show("用户信息读取失败", "异常提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
                textBox_UserName.Text = Server.ReadLastUserName(Server.lastLoginUserFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void skinButton_Login_Click(object sender, EventArgs e)
        {
            if (textBox_UserName.Text == null)
            {
                MessageBox.Show("用户名不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (textBox_LoginCode.Text == null)
            {
                MessageBox.Show("密码不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (Server.GetPasswordByUserName(textBox_UserName.Text) != textBox_LoginCode.Text)
            {
                MessageBox.Show("请检查用户名与密码是否正确", "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox_UserName.Text = "";
                textBox_LoginCode.Text = "";
                return;
            }

            Server.PresentAccount = textBox_UserName.Text;
            Server.WriteLastUserName(Server.lastLoginUserFile, Server.PresentAccount);
            this.Hide();
            MainForm fm = new MainForm();
            fm.Show();

        }

        private void textBox_LoginCode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                skinButton_Login_Click(sender, e);
            }
        }
    }
}












