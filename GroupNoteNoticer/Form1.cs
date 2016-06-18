using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LobiAPI;

namespace GroupNoteNoticer
{
    public partial class Form1 : Form
    {
        LobiAPI.BasicAPI api = new BasicAPI();
        bool enable = false;

        string group_id = "";
        string message = "";
        List<string> member_id_list = new List<string>();

        public Form1()
        {
            InitializeComponent();

            //グループid
            //送信するメッセージのタイプ(グループノート or メッセージ)
            //(メッセージ)
            //チェック間隔

            textBox3.PasswordChar = '●';
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            richTextBox1.Enabled = false;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            richTextBox1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (enable)
            {
                enable = false;
                timer1.Enabled = false;
                button1.Text = "スタート";
            }
            string mail = textBox2.Text;
            string password = textBox3.Text;

            if (mail == "")
            {
                MessageBox.Show("メールアドレスを入力してください。");
                return;
            }
            if (password == "")
            {
                MessageBox.Show("パスワードを入力してださい。");
                return;
            }

            if (radioButton3.Checked)//Lobi
            {
                if (!api.Login(mail, password))
                {
                    MessageBox.Show("ログインできませんでした。\nメールアドレスとパスワードを確認してください。");
                    return;
                }
            }
            else//Twitter
            {
                if (!api.TwitterLogin(mail, password))
                {
                    MessageBox.Show("ログインできませんでした。\nメールアドレスとパスワードを確認してください。");
                    return;
                }
            }
            try
            {
                if (radioButton1.Checked)
                {
                    message = api.GetGroup(textBox1.Text).description;
                }
                else
                {
                    if (richTextBox1.Text == "")
                    {
                        MessageBox.Show("メッセージを入力してください。");
                        return;
                    }
                    message = richTextBox1.Text;
                }

                LobiAPI.Json.User[] members = api.GetGroupMembers(textBox1.Text);
                foreach (LobiAPI.Json.User user in members)
                    member_id_list.Add(user.uid);
            }
            catch (System.Net.WebException ex)
            {
                if (ex.Status == System.Net.WebExceptionStatus.ProtocolError)
                    MessageBox.Show("グループが見つかりませんでした");
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show("エラーが発生しました。\n" + ex.Message);
                return;
            }

            group_id = textBox1.Text;

            enable = true;
            button1.Text = "ストップ";
            timer1.Interval = 3000;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();

            List<LobiAPI.Json.User> members = api.GetGroupMembers(group_id).ToList();
            List<string> remove = new List<string>();
            foreach (string uid in member_id_list)
                if (!members.Exists(d => d.uid.Equals(uid)))
                    remove.Add(uid);
            foreach (string uid_remove in remove)
                member_id_list.Remove(uid_remove);

            List<string> new_member = new List<string>();
            foreach (LobiAPI.Json.User user in members)
                if (!member_id_list.Contains(user.uid))
                    new_member.Add(user.uid);

            foreach (string uid in new_member)
            {
                LobiAPI.Json.MakePrivateGroupResult result = api.MakePrivateGroup(uid);
                api.MakeThread(result.uid, message, true);
                member_id_list.Add(uid);
            }
            

            if(enable)
                timer1.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string ok = "abcdefghijklmnopqrstuvwxyz0123456789";
            string test = textBox1.Text;
            if (test == "" || test.Length != 40)
            {
                MessageBox.Show("間違えの無いようにグループIDを入力してください。");
                return;
            }
            foreach (char c in ok)
                test = test.Replace(c.ToString(), "");
            if (test != "")
            {
                MessageBox.Show("間違えの無いようにグループIDを入力してください。");
                return;
            }
            try
            {
                LobiAPI.Json.Group group = api.GetGroup(textBox1.Text);
                richTextBox1.Text = group.description;
            }
            catch (System.Net.WebException ex)
            {
                if(ex.Status == System.Net.WebExceptionStatus.ProtocolError)
                    MessageBox.Show("グループが見つかりませんでした");
            }
            catch (Exception ex)
            {
                MessageBox.Show("エラーが発生しました。\n" + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}
