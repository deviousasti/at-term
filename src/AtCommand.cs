using System;
using System.Linq;

namespace AtTerm
{

    public partial class TermViewModel
    {
        public class AtCommand
        {
            public AtCommand(string command, string args, bool isAT = false)
            {
                Command = command;
                Args = args;
                IsAT = isAT;
            }

            public string Command { get; }
            public string Args { get; }
            public bool IsAT { get; }

            public bool IsValid => !String.IsNullOrWhiteSpace(Command);


            public static AtCommand Parse(string raw)
            {
                var line = raw.Trim();
                var isAT = line.StartsWith("AT+");
                var args = (isAT ? line.Substring(3) : line).Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                var command = args.FirstOrDefault();


                return new AtCommand(args.FirstOrDefault(), args.LastOrDefault(), isAT);
            }

            public override string ToString() => Command;

        }
    }
}
