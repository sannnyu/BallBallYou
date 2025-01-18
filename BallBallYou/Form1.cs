using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Data.Entity.Core.Metadata.Edm;

namespace BallBallYou
{
    public partial class Form1 : Form
    {
        string RadioType = "发放";//默认发球
        private SQLiteConnection sqliteConnection;

        private void CreateDatabaseAndTable()
        {
            try
            {
                // 检查数据库文件是否存在，如果不存在则创建
                if (!System.IO.File.Exists("BallRecords.db"))
                {
                    SQLiteConnection.CreateFile("BallRecords.db");
                }

                // 初始化并打开数据库连接
                sqliteConnection = new SQLiteConnection("Data Source=BallRecords.db;Version=3;");
                sqliteConnection.Open();

                // 创建BallRecords表，如果不存在
                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS BallRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    readDate TEXT NOT NULL,
                    readTime TEXT NOT NULL,
                    readType TEXT NOT NULL,
                    BallsCode TEXT NOT NULL
                )";

                SQLiteCommand command = new SQLiteCommand(createTableQuery, sqliteConnection);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                sqliteConnection.Close();
            }
        }

        // 从数据库加载数据到DataGridView
        private void LoadData()
        {
            // 插入数据后重新加载数据
            try
            {
                if (sqliteConnection.State != ConnectionState.Open)
                {
                    sqliteConnection.Open();
                }

                // 获取选择的日期
                string selectedDate = DateTime.Now.ToString("yyyy-MM-dd");

                // 查询选定日期的记录
                string query = "SELECT readDate, readTime, readType, BallsCode FROM BallRecords WHERE readDate = @selectedDate ORDER BY readTime DESC";
                SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(query, sqliteConnection);
                dataAdapter.SelectCommand.Parameters.AddWithValue("@selectedDate", selectedDate);

                // 使用DataTable来存储查询结果
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);

                // 将DataTable绑定到DataGridView
                dataGridView1.DataSource = dataTable;
                // 设置列标题
                dataGridView1.Columns["readDate"].HeaderText = "读取日期";
                dataGridView1.Columns["readTime"].HeaderText = "读取时间";
                dataGridView1.Columns["readType"].HeaderText = "类型";
                dataGridView1.Columns["BallsCode"].HeaderText = "球卡编码";
                label_CountGo.Text = dataTable.Select("readType = '发放'").Length.ToString();
                labelBack.Text = dataTable.Select("readType = '回收'").Length.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                sqliteConnection.Close();
            }
        }

        public Form1()
        {
            InitializeComponent();

            // 连接到SQLite数据库
            string connectionString = "Data Source=BallRecords.db;Version=3;";
            sqliteConnection = new SQLiteConnection(connectionString);
            CreateDatabaseAndTable();
            // 加载数据到DataGridView
            LoadData();
            radioButton1.Checked = true;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            else 
            {
                string ballsCode = textBox1.Text;
                if (RadioType != "领用")
                {
                    if (sqliteConnection.State != ConnectionState.Open)
                    {
                        sqliteConnection.Open();
                    }         
                    // 查询该球卡编码在今天是否已经发放
                    string query = "SELECT COUNT(*) FROM BallRecords WHERE BallsCode = @ballsCode";
                    SQLiteCommand command = new SQLiteCommand(query, sqliteConnection);
                    command.Parameters.AddWithValue("@ballsCode", ballsCode);

                    // 执行查询并检查结果
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if (count > 0)
                    {
                        // 如果记录数大于0，说明已经发放/回收/领用过
                    }
                    else
                    {
                        MessageBox.Show("该球卡未领用");
                        textBox1.Clear();
                        return;
                    }

                }
                //查今天是否领用了
                if (IsAlreadyIssuedToday(ballsCode, RadioType))
                {

                    MessageBox.Show("该球卡今天已发放或已回收，或领用未发放。");
                    textBox1.Clear();
                    return;
                }
                else
                {
                    string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                    string currentTime = DateTime.Now.ToString("HH:mm:ss");

                    AddRecord(currentDate, currentTime, RadioType, ballsCode);
                }
                textBox1.Clear();
            }
            
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                RadioType = "回收";
                textBox1.Clear();
                textBox1.Focus();
            }

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                RadioType = "发放";
                textBox1.Clear();
                textBox1.Focus();
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                RadioType = "领用";
                textBox1.Clear();
                textBox1.Focus();
            }
        }

        private void AddRecord(string readDate, string readTime, string readType, string ballsCode)
        {
            try
            {
                if (sqliteConnection.State != ConnectionState.Open)
                {
                    sqliteConnection.Open();
                }

                string insertQuery = "INSERT INTO BallRecords (readDate, readTime, readType, BallsCode) VALUES (@readDate, @readTime, @readType, @ballsCode)";
                SQLiteCommand command = new SQLiteCommand(insertQuery, sqliteConnection);

                command.Parameters.AddWithValue("@readDate", readDate);
                command.Parameters.AddWithValue("@readTime", readTime);
                command.Parameters.AddWithValue("@readType", readType);
                command.Parameters.AddWithValue("@ballsCode", ballsCode);

                command.ExecuteNonQuery();

                // 插入数据后重新加载数据
                try
                {
                    if (sqliteConnection.State != ConnectionState.Open)
                    {
                        sqliteConnection.Open();
                    }

                    // 获取选择的日期
                    string selectedDate = DateTime.Now.ToString("yyyy-MM-dd");

                    // 查询选定日期的记录
                    string query = "SELECT readDate, readTime, readType, BallsCode FROM BallRecords WHERE readDate = @selectedDate ORDER BY readTime DESC";
                    SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(query, sqliteConnection);
                    dataAdapter.SelectCommand.Parameters.AddWithValue("@selectedDate", selectedDate);

                    // 使用DataTable来存储查询结果
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    // 将DataTable绑定到DataGridView
                    dataGridView1.DataSource = dataTable;
                    // 设置列标题
                    dataGridView1.Columns["readDate"].HeaderText = "读取日期";
                    dataGridView1.Columns["readTime"].HeaderText = "读取时间";
                    dataGridView1.Columns["readType"].HeaderText = "类型";
                    dataGridView1.Columns["BallsCode"].HeaderText = "球卡编码";
                    label_CountGo.Text = dataTable.Select("readType = '发放'").Length.ToString();
                    labelBack.Text = dataTable.Select("readType = '回收'").Length.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                    textBox1.Clear();
                }
                finally
                {
                    sqliteConnection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                textBox1.Clear();
            }
            finally
            {
                sqliteConnection.Close();
            }
        }

        private bool IsAlreadyIssuedToday(string ballsCode, string readType)
        {
            try
            {
                if (sqliteConnection.State != ConnectionState.Open)
                {
                    sqliteConnection.Open();
                }

                // 获取当前日期
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

                // 查询该球卡编码在今天是否已经发放
                string query = "SELECT COUNT(*) FROM BallRecords WHERE BallsCode = @ballsCode AND readDate = @currentDate AND readType = @readType";
                SQLiteCommand command = new SQLiteCommand(query, sqliteConnection);
                command.Parameters.AddWithValue("@ballsCode", ballsCode);
                command.Parameters.AddWithValue("@currentDate", currentDate);
                command.Parameters.AddWithValue("@readType", readType);
                
                // 执行查询并检查结果
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0; // 如果记录数大于0，说明已经发放/回收/领用过
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                textBox1.Clear();
                return false;
            }
            finally
            {
                sqliteConnection.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Random random = new Random();
            int randomNumber = random.Next(100000000, 1000000000); // 生成一个 9 位数
            textBox1.Text = "2222222";
            KeyEventArgs key = new KeyEventArgs(Keys.Enter);
            textBox1_KeyDown(this, key);
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (sqliteConnection.State != ConnectionState.Open)
                {
                    sqliteConnection.Open();
                }

                // 获取选择的日期
                string selectedDate = dateTimePicker1.Value.ToString("yyyy-MM-dd");

                // 查询选定日期的记录
                string query = "SELECT readDate, readTime, readType, BallsCode FROM BallRecords WHERE readDate = @selectedDate ORDER BY readTime DESC";
                SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(query, sqliteConnection);
                dataAdapter.SelectCommand.Parameters.AddWithValue("@selectedDate", selectedDate);

                // 使用DataTable来存储查询结果
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);

                // 将DataTable绑定到DataGridView
                dataGridView1.DataSource = dataTable;
                // 设置列标题
                dataGridView1.Columns["readDate"].HeaderText = "读取日期";
                dataGridView1.Columns["readTime"].HeaderText = "读取时间";
                dataGridView1.Columns["readType"].HeaderText = "类型";
                dataGridView1.Columns["BallsCode"].HeaderText = "球卡编码";
                label_CountGo.Text = dataTable.Select("readType = '发放'").Length.ToString();
                labelBack.Text = dataTable.Select("readType = '回收'").Length.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                textBox1.Clear();
            }
            finally
            {
                sqliteConnection.Close();
            }
        }
    }
}
