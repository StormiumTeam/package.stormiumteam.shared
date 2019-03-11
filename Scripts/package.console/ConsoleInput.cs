using System;

namespace package.stormiumteam.console
{
    public class ConsoleInput
    {
        //public delegate void InputText( string strInput );
        public event Action<string> OnInputText;
        public string               prefix = string.Empty;
        public string               inputString;

        public void ClearLine()
        {
            Console.CursorLeft = 0;
            Console.Write(new string(' ', Console.BufferWidth));
            Console.CursorTop--;
            Console.CursorLeft = 0;
        }

        public void RedrawInputLine(bool force)
        {
            if (!force && inputString.Length == 0) return;

            if (Console.CursorLeft >= 0)
                ClearLine();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(prefix);
            Console.Write(inputString);
        }

        internal void OnBackspace()
        {
            if (inputString.Length < 1) return;

            inputString = inputString.Substring(0, inputString.Length - 1);
            RedrawInputLine(true);
        }

        internal void OnEscape()
        {
            ClearLine();
            inputString = "";
        }

        internal void OnEnter()
        {
            ClearLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("> " + inputString);

            var strtext = inputString;
            inputString = "";

            OnInputText?.Invoke(strtext);
        }

        public void Update()
        {
            if (!Console.KeyAvailable) return;
            var key = Console.ReadKey();

            if (key.Key == ConsoleKey.Enter)
            {
                OnEnter();
                return;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                OnBackspace();
                return;
            }

            if (key.Key == ConsoleKey.Escape)
            {
                OnEscape();
                return;
            }

            if (key.KeyChar != '\u0000')
            {
                inputString += key.KeyChar;
                RedrawInputLine(false);
                return;
            }
        }
    }
}