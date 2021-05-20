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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace TestBaseCreate
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        int Time = 5; // время таймера, секунды
        private string startPath; // тут будем хранить путь к папке исмточнику
        bool startPathFlag = false;
        private string destPath; // тут будем хранить путь к папке назначения
        bool destPathFlag = false;
        string nameD, nameM;
        string From, To;

        private void button1_Click(object sender, EventArgs e)
        {
            if (startPathFlag == true && destPathFlag == true)//если указаны папки источник и назначение
            {
                Path();
                timer1.Start();
            }
        }

        void GO()
        {
            string from;
            //string to;
            DateTime SelectedStartDate = SelectMax(To);
            DateTime NowDate = DateTime.Now;



            //from = @"D:\Пркатика\From\" + DateTime.Parse(NowDate.ToString()).ToString("M.yyyy") + @"\" + DateTime.Parse(NowDate.ToString()).ToString("d.M.yyyy") + ".db"; //откуда копируем

            try
            {
                sql(From, SelectedStartDate);
                richTextBox1.Text += "База успешно обновлена в: " + DateTime.Now + ";" + "\n";
                richTextBox1.Text += "\n";
            }
            catch
            {
                richTextBox1.Text += "База оборудования не доступна " + DateTime.Now + ";" + "\n";
            }
        }

        DateTime SelectMax(string BasePath)
        {
            DataTable dTableSelectedMax = new DataTable();
            //string BasePath = Application.StartupPath + @"/TEST3000.db"; // откуда проверяем
            //string BasePath = @"D:\Пркатика\To-1\5.2021\20.5.2021.db";
            string commandText = "SELECT MAX(TIMESTAMP) FROM TEST1";
            if (!File.Exists(BasePath))
            {
                richTextBox1.Text += "Не удалось открыть базу сервера по указанному пути;" + "\n";
            }
            if (File.Exists(BasePath))
            {
                SQLiteConnection connect = new SQLiteConnection();
                connect = new SQLiteConnection(@"Data Source=" + BasePath + ";" + "Version =3; ReadOnly=true;");
                SQLiteCommand sqlCommand = connect.CreateCommand();
                connect.Open();
                richTextBox1.Text += "Серверная база открыта;" + "\n";
                SQLiteDataAdapter adapterTestServer = new SQLiteDataAdapter(commandText, connect);
                adapterTestServer.Fill(dTableSelectedMax);
                connect.Close();
                richTextBox1.Text += "Последняя запись серверной базы от: " + dTableSelectedMax.Rows[0][0] + ";" + "\n";
            }
            return Convert.ToDateTime(dTableSelectedMax.Rows[0][0]);
        }

        void sql(string BasePath, DateTime SelectedDate)
        {
            DataTable dTableTest = new DataTable();
            if (File.Exists(BasePath))
            {
                SQLiteConnection connect = new SQLiteConnection();
                connect = new SQLiteConnection(@"Data Source=" + BasePath + ";" + "Version =3; ReadOnly=true;");
                SQLiteCommand sqlCommand = new SQLiteCommand();
                if (connect.State != ConnectionState.Open)
                {
                    connect.Open();
                    sqlCommand.Connection = connect;
                }

                string commandText = "SELECT TIMESTAMP, OPERATOR, NAME, MAINCOUNTER, RESULT FROM TEST";
                SQLiteDataAdapter adapterTestServer = new SQLiteDataAdapter(commandText, connect);
                adapterTestServer.Fill(dTableTest);
                connect.Close();
            }

            string ServerBasePath = To;
            if (File.Exists(ServerBasePath))
            {
                SQLiteConnection connect = new SQLiteConnection();
                connect = new SQLiteConnection(@"Data Source=" + ServerBasePath + ";" + "Version =3; ReadOnly=false;");
                SQLiteCommand sqlCommand = new SQLiteCommand();

                if (connect.State != ConnectionState.Open)
                {
                    connect.Open();
                    sqlCommand.Connection = connect;
                }
                try
                {
                    int strok = 0;
                    string NAME;
                    string OPERATOR;
                    for (int i = 0; i < dTableTest.Rows.Count; i++)
                    {
                        if (Convert.ToDateTime(dTableTest.Rows[i][0]) > SelectedDate)
                        {
                            strok = strok + 1;
                            try
                            {
                                byte[] ByteName = (byte[])dTableTest.Rows[i][2];
                                NAME = Encoding.GetEncoding(1251).GetString(ByteName);
                                byte[] ByteOperator = (byte[])dTableTest.Rows[i][1];
                                OPERATOR = Encoding.GetEncoding(1251).GetString(ByteOperator);
                            }
                            catch
                            {
                                NAME = dTableTest.Rows[i][2].ToString();
                                OPERATOR = dTableTest.Rows[i][1].ToString();
                            }
                            sqlCommand.CommandText = "INSERT INTO 'TEST1' (TIMESTAMP, OPERATOR, NAME, MAINCOUNTER, RESULT) VALUES('"
                            + DateTime.Parse(dTableTest.Rows[i][0].ToString()).ToString("yyyy-MM-dd HH:mm:ss") + "','" + OPERATOR + "','" + NAME + "','" + dTableTest.Rows[i][3]
                            + "','" + dTableTest.Rows[i][4] + "')" + "\n";
                            sqlCommand.ExecuteNonQuery();
                        }
                    }
                    richTextBox1.Text += "Найдено новых строк: " + strok + ";" + "\n";
                }
                catch (SQLiteException ex)
                {

                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Time = Time - 1;
            label1.Text = Time.ToString() + " Сек";
            if (Time == 0)
            {
                GO();
                Time = 30;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void buttonTo_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();

            // если папка выбрана и нажата клавиша `OK` - значит можно получить путь к папке
            if (result == DialogResult.OK)
            {
                // запишем в нашу переменную путь к папке
                destPath = folderBrowserDialog1.SelectedPath;
                destPathFlag = true;
                richTextBox1.Text += "Выбрана конечная папка" + "\n";
                label5.Text = destPath;
            }
        }

        private void buttonFrom_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();

            // если папка выбрана и нажата клавиша `OK` - значит можно получить путь к папке
            if (result == DialogResult.OK)
            {
                // запишем в нашу переменную путь к папке
                startPath = folderBrowserDialog1.SelectedPath;
                startPathFlag = true;
                richTextBox1.Text += "Выбрана исходная папка" + "\n";
                label4.Text = startPath;
            }
        }

        void Path()//меняет формат времени, указывает путь
        {
            DateTime YYMMDD = DateTime.Now;
            if (Convert.ToInt32(DateTime.Parse(YYMMDD.ToString()).ToString("MM")) < 10)//если месяц, допустм, 8, то пишет не 08, а 8
            {
                nameM = DateTime.Parse(YYMMDD.ToString()).ToString("MM").Substring(1);
            }
            else
            {
                nameM = DateTime.Parse(YYMMDD.ToString()).ToString("MM");
            }

            if (Convert.ToInt32(DateTime.Parse(YYMMDD.ToString()).ToString("dd")) < 10)//аналогично месяцу
            {
                nameD = DateTime.Parse(YYMMDD.ToString()).ToString("dd").Substring(1);
            }
            else
            {
                nameD = DateTime.Parse(YYMMDD.ToString()).ToString("dd");
            }

            textBox1.Text = startPath + @"\" + nameM + "." + DateTime.Parse(YYMMDD.ToString()).ToString("yyyy") + @"\" + nameD + "." + nameM + "." + DateTime.Parse(YYMMDD.ToString()).ToString("yyyy") + ".db";//ищет базу данных этой даты - указывает путь

            textBox2.Text = destPath + @"\" + nameM + "." + DateTime.Parse(YYMMDD.ToString()).ToString("yyyy") + @"\" + nameD + "." + nameM + "." + DateTime.Parse(YYMMDD.ToString()).ToString("yyyy") + ".db";

            From = textBox1.Text;//файл источник
            To = textBox2.Text;//файл назначения
        }
    }
}
