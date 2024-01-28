using GridLine_IDE.Database;
using GridLine_IDE.Helpers;
using GridLine_IDE.Models;
using LangLine;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GridLine_IDE
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static LangLineCore LangLineProgram;
        public static MovementField MainGrid;
        public static int ExecutionInterval = 20;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //DLLCleaner();
            DatabaseHelper.InitDB();
            LangLineProgram  = new LangLineCore();
        }

        private void DLLCleaner()
        {
            var executionPath = Environment.CurrentDirectory;
            var dllsPath = Path.Combine(executionPath, "lib");
            if (!Directory.Exists(dllsPath))
                Directory.CreateDirectory(dllsPath);

            foreach (var dll in new DirectoryInfo(executionPath).GetFiles().Where(
                x => ((FileInfo)x).Extension == ".dll" || ((FileInfo)x).Extension == ".xml"))
            {
                try
                {
                    if (dll.Name.Split('.')[0] == "e_sqlite3")
                        continue;
                    dll.MoveTo(Path.Combine(dllsPath, dll.Name));
                } catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
