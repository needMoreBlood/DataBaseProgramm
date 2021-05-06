using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data.SQLite;

namespace bds
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        DataTable dTableTest30001 = new DataTable();
        DataTable dTableTest30002 = new DataTable();

        SQLiteConnection connection = new SQLiteConnection();

        private SQLiteCommand Test3000Cmd;

        private string startPath; // тут будем хранить путь к папке исмточнику
        bool startPathFlag = false;
        private string destPath; // тут будем хранить путь к папке назначения
        bool destPathFlag = false;
        bool triger = false;
        int time, ntime;
        string nameD, nameM;
        string nameOldD, nameOldM;
        string From, To;
        int countRows = 0;
        int timerTime = 7;//задать время, на котором будет происходить копирование
        SQLiteCommand m_sqlCmd = new SQLiteCommand();

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

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (startPathFlag == true && destPathFlag == true)//если указаны папки источник и назначение
            {
                Path();//указывается путь к сегодняшней базе
                if (triger == false)
                {
                    time = Convert.ToInt32(textBoxTimer.Text);
                    ntime = time;
                    triger = true;
                }
                if (triger == true && ntime != Convert.ToInt32(textBoxTimer.Text))//запускает таймер
                {
                    time = Convert.ToInt32(textBoxTimer.Text);
                    ntime = time;
                }

                try
                {
                    textBoxTimer.Enabled = false;
                    timer1.Start();
                    buttonStart.Enabled = false;
                    buttonStop.Enabled = true;
                    richTextBox1.Text += "Таймер запущен пользователем в: " + DateTime.Now + "\n";
                }
                catch
                {
                    richTextBox1.Text += "Ошибка в заданном времени таймера в: " + DateTime.Now + "\n";
                }


            }

            else
            {
                richTextBox1.Text += "Ошибка указания пути. Укажите корректный путь";
            }
            
        }


        private void buttonStop_Click(object sender, EventArgs e)
        {
            textBoxTimer.Enabled = true;
            timer1.Stop();
            buttonStop.Enabled = false;
            buttonStart.Enabled = true;
            richTextBox1.Text += "Таймер остановлен пользователем в: " + DateTime.Now + "\n";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            time = time - 1;
            label7.Text = time.ToString()+"сек";

            if (time == timerTime)
            {
                richTextBox1.Text += "time" + "\n";
                Test3000Up();
            }
            if (time == 0)
            {
                time = Convert.ToInt32(textBoxTimer.Text);
            }
        }

        void Test3000Up()
        {
            if (!File.Exists(To))
            {
                richTextBox1.Text += "Ошибка! База Test3000.sqlite3 сервера не существует или отсутствует по указанному пути" + "\n";
            }
            if (File.Exists(To))//(destPath))//ищект максимальную дату в локальной базе(Test3000ServerBasePath), подключается к серверу
                                //заполняет какую то новую базу dTableTest30001 
                                //SQLiteDataAdapter???
                                //вставляет только максимальную дату
            {
                connection = new SQLiteConnection(@"Data Source=" + To + ";" + "Version =3; ReadOnly=True;");
                connection.Open();
                string command1 = "SELECT  MAX (TIMESTAMP) FROM 'TEST'";
                SQLiteCommand sqlCommand = new SQLiteCommand(command1);                
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(command1, connection);
                richTextBox1.Text += "Запрос на поиск последней записи выполнен: \n";
                adapter.Fill(dTableTest30001);
                richTextBox1.Text += dTableTest30001.Rows[0][0] + " \n";
                connection.Close();
                                

            }
            if (!File.Exists(From))
            {
                richTextBox1.Text += "Ошибка! База Test3000.sqlite3 (исходная) не существует или отсутствует по указанному пути" + "\n";
            }
            if (File.Exists(From))//(startPath))//проверяет надичие основной бд
                                                      //выбирает данные, у которых дата больше последней даты из dTableTest30001
                                                      //копирует эти данные в dTableTest30002
                                                      //
            {
                richTextBox1.Text += "Есть подключение к основной базе Test3000.sqlite3" + "\n";
                connection = new SQLiteConnection(@"Data Source=" + From + ";" + "Version =3; ReadOnly=True;");
                string command2 = "SELECT  * FROM 'TEST' WHERE TIMESTAMP > '" + dTableTest30001.Rows[0][0] + "'";
                SQLiteCommand sqlCommand = connection.CreateCommand();
                connection.Open();
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(command2, connection);
                richTextBox1.Text += "Запрос на поиск расхождений в базах Test3000.sqlite3 выполнен \n";
                adapter.Fill(dTableTest30002);
                richTextBox1.Text += "Найдено новых строк: " + dTableTest30002.Rows.Count + "\n";/////////
                connection.Close();
            }

            Test3000Cmd = new SQLiteCommand();
            connection = new SQLiteConnection(string.Format("Data Source={0};", To));
            if (connection.State != ConnectionState.Open)
            {
                connection = new SQLiteConnection("Data Source=" + To + ";Version=3;");
                connection.Open();
                richTextBox1.Text += "Серверная база для записи Test3000.sqlite3 успешно открыта \n";
                Test3000Cmd.Connection = connection;
            }
            try
            {
                for (int i = 0; i < dTableTest30002.Rows.Count; i++)//из dTableTest30002 бд вставлет все столбцы в основную
                {
                    Test3000Cmd.CommandText = "INSERT INTO 'TEST' (TIMESTAMP, OPERATOR, NAME, MAINCOUNTER, RESULT, END, MARK) VALUES('"
                    + dTableTest30002.Rows[i][0] + "','" + dTableTest30002.Rows[i][1] + "','" + dTableTest30002.Rows[i][2] + "','" + dTableTest30002.Rows[i][3] + "','" + dTableTest30002.Rows[i][4] + "','" + dTableTest30002.Rows[i][5] +
                    "','" + null + "','" + null + "')" + "\n";
                    Test3000Cmd.ExecuteNonQuery();
                }
                richTextBox1.Text += "Базы Test3000.sqlite3 полностью синхронизированы в " + DateTime.Now + "\n";
                richTextBox1.Text += "\n";
            }
            catch (SQLiteException ex)
            {
                richTextBox1.Text += "Ошибка синхронизации баз Test3000.sqlite3 " + ex.Message + DateTime.Now + "\n";
            }
            connection.Close();
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
                                                                                                                                                                                                                // textBox1.Text = startPath + @"\" + nameD + "." + nameM + "." + DateTime.Parse(YYMMDD.ToString()).ToString("yyyy") + ".db";
             /////////////////////////////////////                                                                                                                                                                                                   /////////////////////////
            ///какое должно быть расширение? sqlite3 или db???????????????
            //////////////////////////////
            textBox2.Text = destPath + @"\" + nameM + "." + DateTime.Parse(YYMMDD.ToString()).ToString("yyyy") + @"\" + nameD + "." + nameM + "." + DateTime.Parse(YYMMDD.ToString()).ToString("yyyy") + ".sqlite3";
            //textBox2.Text = destPath + @"\" + nameD + "." + nameM + "." + DateTime.Parse(YYMMDD.ToString()).ToString("yyyy") + ".db";

            From = textBox1.Text;//файл источник
            To = textBox2.Text;//файл назначения
        }
    }
}
