using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace SpawnCreator
{
    public partial class AccountCreator : Form
    {
        // This code allow the user to minimize the form by clicking the taskbar icon
        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_MINIMIZEBOX = 0x00020000;
                var cp = base.CreateParams;
                cp.Style |= WS_MINIMIZEBOX;
                return cp;
            }
        }

        public AccountCreator()
        {
            InitializeComponent();
        }

        private readonly Form_MainMenu form_MM;
        public AccountCreator(Form_MainMenu form_MainMenu)
        {
            InitializeComponent();
            form_MM = form_MainMenu;
        }
        
        MySqlConnection connection = new MySqlConnection();

        public void GetMySqlConnection()
        {
            //string connStr = string.Format("Server={0};Port={1};UID={2};Pwd={3};",
            //    form_MM.GetHost(), form_MM.GetPort(), form_MM.GetUser(), form_MM.GetPass());
            string connStr = 
                $"Server={ form_MM.GetHost() };Port={ form_MM.GetPort() };Uid={ form_MM.GetUser() };Pwd={ form_MM.GetPass() };";

            MySqlConnection _connection = new MySqlConnection(connStr);
            connection = _connection;
        }

        public void ShowExistingAccounts()
        {
            GetMySqlConnection();

            //string query = $"SELECT id, username, email FROM { form_MM.GetAuthDB() }.account;";

            //MySqlCommand _command = new MySqlCommand(query, connection);

            //try
            //{
            //    connection.Open();

            //    MySqlDataAdapter sda = new MySqlDataAdapter();
            //    sda.SelectCommand = _command;
            //    DataTable dbdataset = new DataTable();
            //    sda.Fill(dbdataset);
            //    BindingSource bsource = new BindingSource();

            //    bsource.DataSource = dbdataset;
            //    dataGridView1.DataSource = bsource;
            //    sda.Update(dbdataset);

            //    _command.Connection.Close();
            //}
            //catch (MySqlException ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}

            
            string query = $"SELECT id, username, email FROM { form_MM.GetAuthDB() }.account;";
            try
            {
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    MySqlDataAdapter sda = new MySqlDataAdapter();
                    sda.SelectCommand = cmd;
                    DataTable dbdataset = new DataTable();
                    sda.Fill(dbdataset);
                    dataGridView1.DataSource = dbdataset;
                    dataGridView1.Columns[0].Width = 50; // Entry
                    dataGridView1.Columns[1].Width = 160; // Name
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Error No. " + ex.Number + ": " + ex.Message, "SpawnCreator", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static string stringSqlShare;
        private void GenerateSQL()
        {
            string BuildSQL = $"INSERT INTO { form_MM.GetAuthDB() }.account (username, sha_pass_hash, expansion, email) " + "\n" +
                $"VALUES (UPPER('{ textBox_username.Text }'), (SHA1(CONCAT(UPPER('{ textBox_username.Text }'), ':', UPPER('{ textBox_pass.Text }')))), { textBox_Expansion.Text }, '{ textBox_email.Text }'); " + "\n" +
                $"INSERT INTO { form_MM.GetAuthDB() }.account_access (id, gmlevel, RealmID) " + "\n" +
                $"VALUES ((SELECT id FROM { form_MM.GetAuthDB() }.account WHERE username = '{ textBox_username.Text }'), { textBox_Account_Access_Level.Text }, { textBox_realmID.Text }); \n";
            stringSqlShare = BuildSQL;
        }

        //private void GenerateSQL()
        //{
        //    string BuildSQL = $"INSERT INTO { form_MM.GetAuthDB() }.account (username, sha_pass_hash, expansion, email) " + "\n" +
        //        $"VALUES (UPPER(@username), (SHA1(CONCAT(UPPER(@username), ':', UPPER(@password)))), @expansion, @email); " + "\n" +
        //        $"INSERT INTO { form_MM.GetAuthDB() }.account_access (id, gmlevel, RealmID) " + "\n" +
        //        $"VALUES ((SELECT id FROM { form_MM.GetAuthDB() }.account WHERE username = @username), @accesslvl, @realmID); \n";
        //    stringSqlShare = BuildSQL;
        //}

        private bool _mouseDown;
        private Point lastLocation;

        Form_MainMenu mainmenu = new Form_MainMenu();

        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseDown = true;
            lastLocation = e.Location;
        }

        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseDown)
            {
                Location = new Point(
                    (Location.X - lastLocation.X) + e.X, (Location.Y - lastLocation.Y) + e.Y);

                Update();
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void panel2_MouseUp(object sender, MouseEventArgs e)
        {
            _mouseDown = false;
        }

        public bool IsProcessOpen(string name = "mysqld")
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Contains(name))
                {
                    label_mysql_status2.Text = "Connected!";
                    label_mysql_status2.ForeColor = Color.LawnGreen;
                    button_Execute_Query.Visible = true;
                    button1.Visible = true; // Refresh button
                    dataGridView1.Enabled = true;
                    return true;
                }
            }

            label_mysql_status2.Text = "Connection Lost - MySQL is not running";
            label_mysql_status2.ForeColor = Color.Red;
            button_Execute_Query.Visible = false;
            button1.Visible = false; // Refresh button
            dataGridView1.Enabled = false;
            return false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            IsProcessOpen();
        }

        private void AccountCreator_Load(object sender, EventArgs e)
        {
            //timer7.Start(); // Refresh accounts
            timer1.Start();
            comboBox_Account_Access_level.SelectedIndex = 3; // 3 (Admin)
            comboBox_Expansion.SelectedIndex = 2; // 2 (WOTLK)
            timer6.Start();

            if (form_MM.CB_NoMySQL.Checked)
            {
                label_mysql_status2.Visible = false;
                label85.Visible = false;
                timer1.Enabled = false;
                button_Execute_Query.Visible = false;
                button1.Visible = false; // refresh
                dataGridView1.Enabled = false;
            }
            else
                ShowExistingAccounts();
        }

        // Copy to Clipboard - Button
        private void label86_Click(object sender, EventArgs e)
        {
            GenerateSQL();

            if (textBox_username.Text == "")
            {
                MessageBox.Show("Username should not be empty", "Error");
                return;
            }
            if (textBox_pass.Text == "")
            {
                MessageBox.Show("Password should not be empty", "Error");
                return;
            }
            Clipboard.SetText(stringSqlShare);
            //label87.Visible = true;
            timer4.Start();
        }

        private void label86_MouseEnter(object sender, EventArgs e)
        {
            panel7.BackColor = Color.Firebrick;
        }

        private void label86_MouseLeave(object sender, EventArgs e)
        {
            panel7.BackColor = Color.FromArgb(58, 89, 114);
        }

        private void label83_MouseEnter(object sender, EventArgs e)
        {
            panel5.BackColor = Color.Firebrick;
        }

        private void label83_MouseLeave(object sender, EventArgs e)
        {
            panel5.BackColor = Color.FromArgb(58, 89, 114);
        }

        private void comboBox_Account_Access_level_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox_Account_Access_Level.Text = comboBox_Account_Access_level.Text;

            if (comboBox_Account_Access_level.Text      == "0 (Player)") textBox_Account_Access_Level.Text    = "0";
            else if (comboBox_Account_Access_level.Text == "1 (GM)") textBox_Account_Access_Level.Text        = "1";
            else if (comboBox_Account_Access_level.Text == "2 (Moderator)") textBox_Account_Access_Level.Text = "2";
            else if (comboBox_Account_Access_level.Text == "3 (Admin)") textBox_Account_Access_Level.Text     = "3";
            else if (comboBox_Account_Access_level.Text == "4 (Console)") textBox_Account_Access_Level.Text   = "4";
        }

        private void button_Execute_Query_Click(object sender, EventArgs e)
        {
            GenerateSQL();

            if (textBox_username.Text == "")
            {
                MessageBox.Show("Username should not be empty", "Error");
                return;
            }
            if (textBox_pass.Text == "")
            {
                MessageBox.Show("Password should not be empty", "Error");
                return;
            }

            GetMySqlConnection();

            try
            {
                //string connStr = string.Format("Server={0};Port={1};UID={2};Pwd={3};",
                //form_MM.GetHost(), form_MM.GetPort(), form_MM.GetUser(), form_MM.GetPass());

                //using (var con = new MySqlConnection(connStr))
                //{
                //    connection.Open();
                //    using (MySqlCommand cmd = new MySqlCommand(stringSqlShare, connection))
                //    {
                //        cmd.Parameters.AddWithValue("@username", textBox_username.Text);
                //        cmd.Parameters.AddWithValue("@password", textBox_pass.Text);
                //        cmd.Parameters.AddWithValue("@expansion", textBox_Expansion.Text);
                //        cmd.Parameters.AddWithValue("@email", textBox_email.Text);
                //        cmd.Parameters.AddWithValue("@accesslvl", textBox_Account_Access_Level.Text);
                //        cmd.Parameters.AddWithValue("@realmID", textBox_realmID.Text);
                //        cmd.ExecuteNonQuery();
                //        label_Executed_Successfully.Visible = true;
                //        button1_Click_1(sender, e);
                //    }
                //}

                connection.Open();
                MySqlCommand command = new MySqlCommand(stringSqlShare, connection);

                command.ExecuteNonQuery();
                label_Executed_Successfully.Visible = true;
                button1_Click_1(sender, e);
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message, "SpawnCreator", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        private void textBox_username_TextChanged(object sender, EventArgs e)
        {
            label_Executed_Successfully.Visible = false;
        }

        private void textBox_pass_TextChanged(object sender, EventArgs e)
        {
            label_Executed_Successfully.Visible = false;
        }

        private void textBox_Account_Access_Level_TextChanged(object sender, EventArgs e)
        {
            label_Executed_Successfully.Visible = false;
        }

        private void label83_Click(object sender, EventArgs e)
        {
            GenerateSQL();

            if (textBox_username.Text == "")
            {
                MessageBox.Show("Username should not be empty", "Error");
                return;
            }
            if (textBox_pass.Text == "")
            {
                MessageBox.Show("Password should not be empty", "Error");
                return;
            }
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "sql files (*.sql)|*.sql";
                sfd.FilterIndex = 2;
                sfd.FileName = "Account_" + textBox_username.Text;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(sfd.FileName, stringSqlShare);
                    //label88.Visible = true;
                    //label87.Visible = false;
                    timer2.Start();
                }
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            label88.Visible = true;
            timer2.Stop();

            timer3.Start();
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            label88.Visible = false;
            timer3.Stop();
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            label87.Visible = true;
            timer4.Stop();

            timer5.Start();
        }

        private void timer5_Tick(object sender, EventArgs e)
        {
            label87.Visible = false;
            timer5.Stop();
        }

        int i = 1;
        DateTime dt = new DateTime();
        private void timer6_Tick(object sender, EventArgs e)
        {
            label_stopwatch.Text = dt.AddSeconds(i).ToString("HH:mm:ss");
            i++;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://emucraft.com");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
            BackToMainMenu backtomainmenu = new BackToMainMenu(form_MM);
            backtomainmenu.Show();

        }

        private void label2_MouseEnter(object sender, EventArgs e)
        {
            label2.BackColor = Color.Firebrick;
        }

        private void label2_MouseLeave(object sender, EventArgs e)
        {
            label2.BackColor = Color.FromArgb(58, 89, 114);
        }

        private void label1_MouseEnter(object sender, EventArgs e)
        {
            label1.BackColor = Color.Firebrick;
        }

        private void label1_MouseLeave(object sender, EventArgs e)
        {
            label1.BackColor = Color.FromArgb(58, 89, 114);
        }

        private void comboBox_Expansion_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox_Expansion.Text = comboBox_Expansion.Text;

            if (comboBox_Expansion.Text      == "0 (Vanilla)") textBox_Expansion.Text   = "0";
            else if (comboBox_Expansion.Text == "1 (TBC)") textBox_Expansion.Text       = "1";
            else if (comboBox_Expansion.Text == "2 (WOTLK)") textBox_Expansion.Text     = "2";

            else if (comboBox_Expansion.Text == "3 (Cataclysm)") textBox_Expansion.Text = "3";
            else if (comboBox_Expansion.Text == "4 (MoP)") textBox_Expansion.Text       = "4";
            else if (comboBox_Expansion.Text == "5 (WoD)") textBox_Expansion.Text       = "5";
        }

        private void textBox_realmID_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox_realmID_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '-'))
            {
                e.Handled = true;
            }

            // only allow one minus
            if ((e.KeyChar == '-') && ((sender as TextBox).Text.IndexOf('-') > -1))
            {
                e.Handled = true;
            }
        }

        private void label78_MouseEnter(object sender, EventArgs e)
        {
            label78.BackColor = Color.Firebrick;
        }

        private void label78_MouseLeave(object sender, EventArgs e)
        {
            label78.BackColor = Color.FromArgb(58, 89, 114);
        }

        private void label78_Click(object sender, EventArgs e)
        {
            Close();
            BackToMainMenu backtomainmenu = new BackToMainMenu(form_MM);
            backtomainmenu.Show();
        }

        private void panel7_MouseEnter(object sender, EventArgs e)
        {
            panel7.BackColor = Color.Firebrick;
        }

        private void panel7_MouseLeave(object sender, EventArgs e)
        {
            panel7.BackColor = Color.FromArgb(58, 89, 114);
        }

        private void panel5_MouseEnter(object sender, EventArgs e)
        {
            panel5.BackColor = Color.Firebrick;
        }

        private void panel5_MouseLeave(object sender, EventArgs e)
        {
            panel5.BackColor = Color.FromArgb(58, 89, 114);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            ShowExistingAccounts();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    panel1.BackColor = Color.CornflowerBlue;

                    label3.ForeColor = Color.Black;
                    label4.ForeColor = Color.Black;
                    label6.ForeColor = Color.Black;
                    label5.ForeColor = Color.Black;
                    label7.ForeColor = Color.Black;
                    label8.ForeColor = Color.Black;
                    break;
                case 1:
                    panel1.BackColor = Color.Black;

                    label3.ForeColor = Color.White;
                    label4.ForeColor = Color.White;
                    label6.ForeColor = Color.White;
                    label5.ForeColor = Color.White;
                    label7.ForeColor = Color.White;
                    label8.ForeColor = Color.White;
                    break;
            }
        }

        private void timer7_Tick(object sender, EventArgs e)
        {
            ShowExistingAccounts();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F2)
            {
                //Application.Exit();
                
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
