using GridLine_IDE.Models;
using LangLine;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
        public static LangLineCore LangLineProgram = new LangLineCore();
        public static MovementField MainGrid;
        public static int ExecutionInterval = 20;
    }
}
