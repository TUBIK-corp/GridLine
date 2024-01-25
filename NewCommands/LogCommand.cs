using LangLine;
using LangLine.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridLine_IDE.NewCommands
{
    public class LogCommand : IICommand
    {
        public LangLineCore Context { get; set; }


        public string CommandName { get; set; } = "LOG";

        private int _index = -1;


        public LogCommand(LangLineCore context, int index)
        {
            Context = context;
            _index = index;
        }

        public string TextTolog { get; set; }

        public void InterpreteArguments(string str_args)
        {
            TextTolog = str_args;
        }

        public void Execute()
        {
            Context.InterpreterModule.InvokeCustomEvent("Log", TextTolog);
        }

        public void StartCommand(string args)
        {
            InterpreteArguments(args);
            Execute();
        }
    }
}
