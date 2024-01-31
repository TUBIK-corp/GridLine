using LangLine;
using LangLine.Commands;
using LangLine.Interfaces;

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
            string res = "";
            foreach (var arg in str_args.Split(' '))
            {
                var argument = App.LangLineProgram.InterpreteArgument(arg);
                var text = argument.ToLangLineString();
                if(!arg.Equals(text))
                {
                    text = $"{arg}: {text}";
                }
                res = str_args.Replace(arg, text) + " ";
            }
            TextTolog = res;
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

    internal static class LogCommandExt
    {
        internal static string ToLangLineString(this object obj)
        {
            if (obj.GetType() == typeof(ProcedureCommand.ExecuteProcedure))
            {
                var target = ((ProcedureCommand.ExecuteProcedure)obj).Target as ProcedureCommand;

                var str = "";
                foreach (var command in target.Block)
                {
                    str += $"\n{command.Index}. {command.Line}";
                }
                return str + "\n";
            }
            else return obj.ToString();
        }
    }
}
