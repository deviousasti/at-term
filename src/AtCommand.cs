using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace AtTerm
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

        public static bool IsValidCommand(string command) => !String.IsNullOrWhiteSpace(command);

        public static AtCommand Parse(string raw)
        {
            var line = raw.Trim();
            var isAT = line.StartsWith("AT+");
            var args = (isAT ? line.Substring(3) : line).Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
            var command = args.FirstOrDefault();


            return new AtCommand(args.FirstOrDefault(), args.LastOrDefault(), isAT);
        }

        public override string ToString() => Command;

        public static string MTKCheckSum(string cmd)
        {
            cmd = cmd.Trim('$');
            // Compute the checksum by XORing all the character values in the string.
            var checksum = 0;
            for (var i = 0; i < cmd.Length; i++)
            {
                checksum = checksum ^ cmd[i];
            }

            // Convert it to hexadecimal (base-16, upper case, most significant nybble first).
            var hexsum = checksum.ToString("X").PadLeft(2, '0');
            return hexsum;
        }

        public static string Qualify(string commandText)
        {
            return String.IsNullOrWhiteSpace(commandText) ? "AT" :
                commandText.StartsWith(">") ? commandText.Substring(1) :
                commandText.StartsWith("$") ? $"{commandText}*{MTKCheckSum(commandText)}" :
                commandText.StartsWith("AT+") ? commandText :
                commandText.StartsWith("0x") ? GetHexString(commandText) :
                commandText.StartsWith(Base64Prefix) ? commandText :
                $"AT+{commandText}";
        }

        public const string Base64Prefix = "base64:";

        public static string GetHexString(string commandText)
        {
            var chars =
                commandText
                .Split(' ')
                .Select(hex => hex.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase) ? hex.Substring(2) : hex)
                .SelectMany(block => Enumerable.Range(0, block.Length / 2).Select(i => block.Substring(i * 2, 2)))
                .Select(hex => (success: byte.TryParse(hex, NumberStyles.HexNumber, null, out var value), value))
                .Where(result => result.success)
                .Select(result => result.value)
                .ToArray();

            return Base64Prefix + Convert.ToBase64String(chars);
        }

        public static string Unqualify(string commandText)
        {
            return commandText.StartsWith("AT+") ? commandText.Substring(3) : ">" + commandText;
        }
    }

}
