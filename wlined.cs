// wlined - wojtek line editor
// it is a try to create edlin-like clone in C#  
// (c) Wojciech Wawrzeczko 2022-2024
// License: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;


// (ab)using enum type (it could be bool as well)

enum Mode
{
    STOP, COMMAND
};


/* --------------------------------------------
 * EditorCommand - base for every command class
 * -------------------------------------------
*/

abstract class EditorCommand
{
    protected static Mode mode = Mode.COMMAND;

    private char _ckey;
    public char ckey
    {
        get { return _ckey; }

        set { _ckey = value; }
    }
   

    public EditorCommand(char c)
    {
        ckey = c;
    }

    protected EditorCommand()
        : this('*')
    { }

    public Mode ExecuteCommand(char cCommand, int iFirstLine, int iLastLine, List<String> l, string f)
    {

        return ((ckey == cCommand) ? (mode = ChangeText(iFirstLine, iLastLine, l, f)) : mode);

    }

    public abstract Mode ChangeText(int iFirstLine, int iLastline, List<String> l, string f);

}




/*-------------------------------------------
 * Lists lines of text
 *
 * 
 *-------------------------------------------
 */



class CmdListText : EditorCommand
{
    public CmdListText(char c) : base(c) { }


    public override Mode ChangeText(int iFirstLine, int iLastLine, List<String> l, string f)
    {

        if (iFirstLine >= l.Count)
        {
            Console.Error.WriteLine("ERROR: First line exceds number of lines");
        }

        for (int ix = iFirstLine; (ix <= iLastLine) && (ix < l.Count); ix++)
        {

            Console.WriteLine("" + ix + ":" + l[ix]);

        }

        return Mode.COMMAND;
    }

}

/*-------------------------------------------
 * Appends new lines to text
 *
 * 
 *-------------------------------------------
 */

class CmdEditText : EditorCommand
{
    public CmdEditText(char c) : base(c) { }

    public override Mode ChangeText(int iFirstLine, int iLastLine, List<String> l, string f)
    {
        List<String> w = new List<String>();
        string sLine = null;


        Console.Error.WriteLine("Appending at line {0}...End text with dot only!", iLastLine);


        while (true)
        {
            Console.Error.Write(":");
            sLine = Console.ReadLine();
            if (sLine == ".") break;
            w.Add(sLine);
        }

        for (; iLastLine > l.Count; l.Add("")) ;

        l.InsertRange(iLastLine, w);


        return Mode.COMMAND;
    }

}

/*-------------------------------------------
 * Saves text to file
 *
 * 
 *-------------------------------------------
 */


class CmdSaveText : EditorCommand
{
    public CmdSaveText(char c) : base(c) { }
    public override Mode ChangeText(int iFirstLine, int iLastLine, List<String> l, string f)
    {

        using (StreamWriter sw = new StreamWriter(f, false))
        {

            foreach (string line in l)
            {
                sw.WriteLine(line);
            }

            Console.Error.WriteLine("File: {0} was written.", f);

        }

        return Mode.COMMAND;
    }

}

/*-------------------------------------------
 * Saves text and sets STOP flag
 *
 * 
 *-------------------------------------------
 */

class CmdSaveAndExit : EditorCommand
{
    public CmdSaveAndExit(char c) : base(c) { }
    public override Mode ChangeText(int iFirstLine, int iLastLine, List<String> l, string f)
    {

        using (StreamWriter sw = new StreamWriter(f, false))
        {

            foreach (string line in l)
            {
                sw.WriteLine(line);
            }

        }

        Console.Error.WriteLine("File: {0} was saved. Bye", f);



        return Mode.STOP;
    }

}

/*-------------------------------------------
 * Sets STOP flag
 *
 * 
 *-------------------------------------------
 */


class CmdQuit : EditorCommand
{
    public CmdQuit(char c) : base(c) { }
    public override Mode ChangeText(int iFirstLine, int iLastLine, List<String> l, string f)
    {

        Console.Error.WriteLine("No changes made to file: {0}. Bye", f);

        return Mode.STOP;

    }

}


/*-------------------------------------------
 * Just dispaly help
 *
 * 
 *-------------------------------------------
 */


class CmdCallHelp : EditorCommand
{
    public CmdCallHelp(char c) : base(c) { }
    public override Mode ChangeText(int iFirstLine, int iLastLine, List<String> l, string f)
    {

        Console.WriteLine(texts.help);
        return Mode.COMMAND;

    }

}



/*-------------------------------------------
 * Delete line or lines
 *
 * 
 *-------------------------------------------
 */


class CmdDeleteLine : EditorCommand
{
    public CmdDeleteLine(char c) : base(c) { }
    public override Mode ChangeText(int iFirstLine, int iLastLine, List<String> l, string f)
    {
        if (iFirstLine == iLastLine)
        {
            if (iFirstLine < l.Count)
            {
                l.RemoveAt(iFirstLine);
                Console.Error.WriteLine("Line {0} was deleted!", iFirstLine);
            }
            else
            {
                Console.Error.WriteLine("ERROR: There is no {0} line in Text to delete", iFirstLine);
            }
        }
        else
        {
            int range = (iLastLine - iFirstLine + 1) > l.Count ? iLastLine - iFirstLine : iLastLine - iFirstLine + 1;

            if ((iFirstLine < l.Count) && (iLastLine <= l.Count))
            {
                l.RemoveRange(iFirstLine, range);
                Console.Error.WriteLine("Lines from {0} to {1} were deleted!", iFirstLine, iLastLine);
            }
            else
            {
                Console.Error.WriteLine("ERROR: Lines after {0} can't be deleted", l.Count - 1);
            }
        }


        return Mode.COMMAND;

    }

}


/*-------------------------------------------
 * Inserts file into text at given position
 *
 * 
 *-------------------------------------------
 */


class CmdInsertFile : EditorCommand
{
    public CmdInsertFile(char c) : base(c) { }
    public override Mode ChangeText(int iFirstLine, int iLastLine, List<String> l, string f)
    {

        List<String> w = new List<String>();
        string sLine = null;

        Console.Error.WriteLine("Enter filename to insert at line {0} or dot to cancel!", iLastLine);
        sLine = Console.ReadLine();

        if (sLine != ".")
        {

            using (TextReader tr = new StreamReader(sLine))
            {
                string sl = null;

                while ((sl = tr.ReadLine()) != null)
                {
                    w.Add(sl);
                }
                for (; iLastLine > l.Count; l.Add("")) ;
                l.InsertRange(iLastLine, w);

            }
        }



        return Mode.COMMAND;
    }

}

/*-------------------------------------------
 * Print text without line numbers (can be redirected to printer)
 *
 * 
 *-------------------------------------------
 */

class CmdPrintFile : EditorCommand
{
    public CmdPrintFile(char c) : base(c) { }
    public override Mode ChangeText(int iFirstLine, int iLastLine, List<String> l, string f)
    {

        for (int ix = iFirstLine; (ix <= iLastLine) && (ix < l.Count); ix++)
        {

            Console.WriteLine(l[ix]);
        }



        return Mode.COMMAND;

    }

}

/*-------------------------------------------
 * Allow edit line by hand
 *
 * 
 *-------------------------------------------
 */

class CmdEditLine : EditorCommand
{
    private StringBuilder input;
    private int iInputPosition = 0;
    private int iX = 0;
    private int iY = 0;
    private int iZ = 0;
    private int iT = 0;
    private int iLen;
    private char ch;
    private int iL;

    private void PrintStatus()
    {
        iLen = input.Length;


        if (iLen > iInputPosition) ch = input[iInputPosition];
        else
            ch = ' ';
        Console.SetCursorPosition(0, iY - 1);
        Console.Error.WriteLine("Editing line {0}, Cursor at [{1}+{2}]{3}/{4}, char={5}. <Enter> - accept changes, <Esc> - discard changes  ", iL, iX, iZ, iInputPosition, iLen, ch);
    }
    
    public CmdEditLine(char c) : base(c) { }

    public int getWidth()
    {
        int iW;
        if (input.Length - iZ < Console.WindowWidth)
              iW = input.Length - iZ;
          else
              iW = Console.WindowWidth;

        return iW;
    }

    public override Mode ChangeText(int iFirstLine, int iLastLine, List<String> l, string f)
    {
        //adding empty lines
        List<String> w = new List<String>();
        for (; iLastLine >= l.Count+w.Count; w.Add("")) ;
        l.InsertRange(l.Count, w);
        iL = iLastLine;

        input = new StringBuilder(l[iL]);
        iInputPosition = 0;

        iY = Console.CursorTop; // cursor begining position
        iX = Console.CursorLeft;
        iZ = 0; //frame position
        //int frameWidth = Console.WindowWidth;
        iT = (input.Length > Console.WindowWidth) ? Console.WindowWidth : input.Length;

        PrintStatus();

        Console.SetCursorPosition(iX, iY);
        Console.Error.WriteLine(input.ToString().Substring(0, iT));
        Console.SetCursorPosition(iX, iY);

        bool bEditLine = true;
        while (bEditLine)
        {
            var key = Console.ReadKey(intercept: true);

            switch (key.Key)
            {
                
                case ConsoleKey.Escape:
Console.Error.WriteLine();
                    bEditLine = false;
                    break;

                case ConsoleKey.Enter:
                    Console.Error.WriteLine();
                    bEditLine = false;
                    l[iLastLine] = input.ToString();

                    break;

                case ConsoleKey.LeftArrow:
                    if (iInputPosition > 0)
                    {
                        iInputPosition--;
                        if (iX > 0)
                        {
                            iX--;
                        }
                        else
                        {
                            iX = 0;
                            if (iZ > 0)
                            {
                                iZ--;
                                Console.SetCursorPosition(0, iY);
                                iT = getWidth();
                                Console.Error.Write(input.ToString().Substring(iZ, iT) + " ");
                            }
                        }
                        PrintStatus();

                        Console.SetCursorPosition(iX % Console.WindowWidth, iY);
                    }

                    break;
                case ConsoleKey.RightArrow:
                    if (iInputPosition < input.Length)
                    {
                        iInputPosition++;
                        if (iX < Console.WindowWidth - 1)
                        {
                            iX++;
                        }
                        else
                        {
                            iX = Console.WindowWidth - 1;
                            iZ++;
                            Console.SetCursorPosition(0, iY);

                            // three possibilities
                            // - input.Length < Console.WindowWidth
                            // - input.Length >= Console.WindowWidth
                            // - input.Length >= Console.WindowWidth but iZ+iX> input.Length % Console.WindowWidth


                            iT = getWidth();
                            Console.Error.Write(input.ToString().Substring(iZ, iT) + " ");

                        }
                        PrintStatus();


                        Console.SetCursorPosition(iX % Console.WindowWidth, iY);

                    }
                    break;

                case ConsoleKey.Backspace:
                    if (iInputPosition > 0)
                    {
                        input.Remove(iInputPosition - 1, 1);
                        iInputPosition--;


                        // If I eat one char input.Length = input.Length - 1

                        if (iX > 0)
                        {
                            iX--;
                        }
                        else if (iZ > 0)
                        {
                            iX = 0;
                            iZ--;

                        }


                        PrintStatus();

                        Console.SetCursorPosition(0, iY);
                        iT = getWidth();
                        Console.Error.Write(input.ToString().Substring(iZ, iT) + " ");

                        Console.SetCursorPosition(iX % Console.WindowWidth, iY);

                    }
                    break;
                case ConsoleKey.Delete:
                    if (iInputPosition < input.Length)
                    {
                        input.Remove(iInputPosition, 1);

                        PrintStatus();




                        //iT = (input.Length > Console.WindowWidth) ? Console.WindowWidth : input.Length;
                        iT = getWidth();
                        PrintStatus();
                        
                        
                        //if (iZ + iT > input.Length) iT--;
                        Console.Error.Write(input.ToString().Substring(iZ, iT) + " ");

                        Console.SetCursorPosition(iX % Console.WindowWidth, iY);

                    }

                    break;
                case ConsoleKey.UpArrow:
                case ConsoleKey.DownArrow:
                break;



                default:
                    if (key.KeyChar.ToString().Length !=1 ) break; 



                    input.Insert(iInputPosition, key.KeyChar);
                    iInputPosition++;
                    if (iX < Console.WindowWidth - 1)
                    {
                        iX++;
                    }
                    else
                    {
                        iX = Console.WindowWidth - 1;
                        iZ++;
                    }


                    Console.SetCursorPosition(0, iY);
                    //iT = (input.Length >= Console.WindowWidth) ? Console.WindowWidth - 1 : input.Length;
                    
                    //if (iZ + iT > input.Length) iT--;
                    iT = getWidth();
                    PrintStatus();


                    Console.Error.Write(input.ToString().Substring(iZ, iT) + " ");

                    Console.SetCursorPosition(iX % Console.WindowWidth, iY);



                    break;
            }



        }

        Console.Error.WriteLine();




        return Mode.COMMAND;

    }

}







/*-------------------------------------------
 * Asks for string to replace
 *
 * 
 *-------------------------------------------
 */

class CmdReplaceString : EditorCommand
{
    public CmdReplaceString(char c) : base(c) { }
    public override Mode ChangeText(int iFirstLine, int iLastLine, List<String> l, string f)
    {

        Console.Error.WriteLine("Enter text to be replaced or dot to cancel");
        string src = Console.ReadLine();

        if (src != ".")
        {
            Console.Error.WriteLine("Enter text replacing pevious string");
            string dest = Console.ReadLine();
            for (int ix = iFirstLine; (ix <= iLastLine) && (ix < l.Count); ix++)
            {
                String s = (String)l[ix];
                l[ix] = s.Replace(src, dest);
            }

        }


        return Mode.COMMAND;

    }

}

/*-------------------------------------------
 * Looks for string in Text and writing lines
 *
 * 
 *-------------------------------------------
 */


class CmdFindString : EditorCommand
{
    public CmdFindString(char c) : base(c) { }
    public override Mode ChangeText(int iFirstLine, int iLastLine, List<String> l, string f)
    {


        Console.Error.WriteLine("Enter text to find or dot to cancel");
        string src = Console.ReadLine();

        if (src != ".")
        {

            for (int ix = iFirstLine; (ix <= iLastLine) && (ix < l.Count); ix++)
            {
                String s = (String)l[ix];

                if (s.Contains(src)) Console.Error.WriteLine("" + ix + ":" + s);

            }

        }
        return Mode.COMMAND;

    }

}

/*-------------------------------------------
 *
 *
 * 
 *-------------------------------------------
 */


class CmdCopyToFile : EditorCommand
{
    public CmdCopyToFile(char c) : base(c) { }
    public override Mode ChangeText(int iFirstLine, int iLastLine, List<String> l, string f)
    {

        Console.Error.WriteLine("Enter filename to save lines from {0} to {1} or dot to cancel!", iFirstLine, iLastLine);
        String sLine = Console.ReadLine();

        if (sLine != ".")
        {
            using (StreamWriter sw = new StreamWriter(sLine, false))
            {
                for (int ix = iFirstLine; (ix <= iLastLine) && (ix < l.Count); ix++)
                {
                    sw.WriteLine(l[ix]);
                }

                sw.Close();

                Console.Error.WriteLine("Lines from {0} to {1} were written to {2}!", iFirstLine, iLastLine, sLine);


            }
        }
        return Mode.COMMAND;

    }

}

/*-------------------------------------------
 * wlined 
 *
 * 
 *-------------------------------------------
 */


class wlined
{
    delegate Mode ChangeText(char cCommand, int iFirstLine, int iLastLine, List<String> l, string f);

    List<String> StringList = new List<String>();
    String fname = null;
    private ChangeText ct;



    public static void Main(String[] argc)
    {
        Console.Error.WriteLine(texts.title);
        Console.Error.WriteLine(texts.copyright);
        Console.Error.WriteLine(texts.license);

        if (argc.Length == 1)
        {
            wlined app = new wlined();

            app.LoadFile(argc[0]);

            app.AddFunction(new CmdListText('L'));
            app.AddFunction(new CmdEditLine('E'));
            app.AddFunction(new CmdEditText('I'));
            app.AddFunction(new CmdReplaceString('R'));
            app.AddFunction(new CmdFindString('F'));
            app.AddFunction(new CmdCopyToFile('C'));
            app.AddFunction(new CmdPrintFile('P'));
            app.AddFunction(new CmdDeleteLine('D'));
            app.AddFunction(new CmdSaveText('W'));
            app.AddFunction(new CmdInsertFile('T'));
            app.AddFunction(new CmdCallHelp('H'));
            app.AddFunction(new CmdSaveAndExit('X'));
            app.AddFunction(new CmdQuit('Q'));

            app.MainLoop();
        }
        else
        {
            Console.Error.WriteLine(texts.help);
        }
    }




    public void AddFunction(EditorCommand f)
    {
        ct += f.ExecuteCommand;  // adds action to invocation list
    }




    public void MainLoop()
    {
        int iFirstLine;
        int iLastLine;
        String sLine;
        char cCommand;
        Mode iMode = Mode.COMMAND;

        while (iMode != Mode.STOP)
        {
            iFirstLine = 0;
            iLastLine = StringList.Count;
            cCommand = '*';

            Console.Error.Write("*"); // command line mode sign

            if (!string.IsNullOrEmpty(sLine = Console.ReadLine()))
            {
                parseCommand(sLine, ref cCommand, ref iFirstLine, ref iLastLine);

                if (StringList.Count < iFirstLine) StringList.Capacity = iFirstLine;

                iMode = ct(cCommand, iFirstLine, iLastLine, StringList, fname);  // calling sequence of all delegates, return value of the last sets iMode
            }
        }

    }


    public void LoadFile(string name)
    {
        fname = name;
        if (File.Exists(name))
        {
            using (TextReader tr = new StreamReader(name))
            {
                string ln = null;
                while ((ln = tr.ReadLine()) != null)
                {
                    this.StringList.Add(ln);
                }

            }
        }
    }




    public void parseCommand(String cmd, ref char cC, ref int iF, ref int iL)
    {

        bool bCheckNext = false;
        String[] asCmd = Regex.Split(cmd.ToUpper(), @"^(\d+)|,|([A-Z])");

        for (int ax = 0; ax < asCmd.Length; ax++)
        {
            if (asCmd[ax].Length == 0) continue;

            if (Char.IsLetter(asCmd[ax], 0))
            {
                cC = (asCmd[ax])[0];
                continue;
            }


            if (!bCheckNext)
            {
                bCheckNext = Int32.TryParse(asCmd[ax], out iF);
                iL = iF;
            }
            else
            {
                Int32.TryParse(asCmd[ax], out iL);
            }
        }


    }



}

/*----------------------------------------
 *
 *----------------------------------------
*/



class texts
{

    public const string title = "WLinEd - Wojtek Line Editor";
    public const string copyright = "(c) Wojciech Wawrzeczko 2022";
    public const string license = "MIT License";

    // using @ for verbatim-multiline string

    public const string help = @"Using:

wlined <filename.txt> - open filename.txt for editing

Command mode - every command must be accepted with Enter key
Some commands ask for aditional data (filename or string)

H - displays help;
L - lists lines of text;
P - displays lines without line numbers;
F - list lines containing given string;
I - inserts new lines of text;
D - deletes lines of text;
R - replaces first given string with second given string;
T - inserts another text file;
E - manual line edit;
W - (writes) saves the file;
X - saves the file and exit;
Q - quites wlined without saving.

Commands: L,P,F,D,R,I accepts line numbers:

For example:

1,6L - lists 1-6 lines of text file;
6I - inserts new line before 6th line;
6D - deletes 6th line;
1,6D - deletes lines from 1 to 6.

If you want to write a single period in a line, type it twice and replace with one period using R command.
";


}
