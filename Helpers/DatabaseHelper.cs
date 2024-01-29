using GridLine_IDE.Database;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace GridLine_IDE.Helpers
{
    public class DatabaseHelper
    {
        public static void AddProgram(string name, List<string> commands)
        {
            var program = new ProgramCode()
            {
                Name = name,
                Code = string.Join("\n", commands)
            };

            var insert = $"INSERT INTO Programs (Name, Code) VALUES ('{program.Name}', '{program.Code}')";

            SendNonQuery(insert);
        }

        public static List<ProgramCode> GetAllPrograms()
        {
            var select = $"SELECT * FROM Programs";
            var data = SendReadQuery<ProgramCode>(select);
            return data;
        }

        public static void UpdateProgramById(int id, List<string> commands)
        {
            var update = $"UPDATE Programs SET Code='{string.Join("\n", commands)}' WHERE ID='{id}'";
            SendNonQuery(update);
        }
        
        public static void UpdateProgramByName(string name, List<string> commands)
        {
            var update = $"UPDATE Programs SET Code='{string.Join("\n", commands)}' WHERE Name='{name}'";
            SendNonQuery(update);
        }


        public static List<ProgramCode> SelectData(Func<ProgramCode, bool> predicate)
        {
            var select = $"SELECT * FROM Programs";
            var data = SendReadQuery<ProgramCode>(select);
            return data.Where(predicate).ToList();
        }



        public static string AddTestProgram()
        {
            List<string> commands = new List<string>()
            {
                "RIGHT 2",
                "LEFT 2",
                "LOG HELLO",
            };

            var name = "TestProgram1";
            var program = new ProgramCode()
            {
                Name = name,
                Code = string.Join("\n", commands)
            };

            AddProgram(name, commands);
            var prog = SelectData(x => x.Name == name);

            return prog.First().Code;
        }

        public static string ConnectionString = string.Empty;
        public static void InitDB()
        {
            try
            {
                var folder = Environment.CurrentDirectory;
                ConnectionString = $"Data Source={System.IO.Path.Combine(folder, "database.db")}";
                var create = "CREATE TABLE IF NOT EXISTS Programs(ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, Name TEXT NOT NULL, Code TEXT NOT NULL)";

                SendNonQuery(create);
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static List<T> SendReadQuery<T>(string commandText) where T : new()
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand();
                command.Connection = connection;
                command.CommandText = commandText;

                return command.GetData<T>();
            }
        }

        private static void SendNonQuery(string commandText)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand();
                command.Connection = connection;
                command.CommandText = commandText;
                
                command.ExecuteNonQuery();
            }
        }
    }
}
