﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Sys = Cosmos.System;

namespace CosmosProj1
{
    public class Kernel : Sys.Kernel
    {
        public Date SYSTEM_DATE;
        public List<File> FILESYS;
        public List<Variable> GLOBAL_VARS;

        protected override void BeforeRun()
        {
            SYSTEM_DATE = new Date();
            FILESYS = new List<File>();
            GLOBAL_VARS = new List<Variable>();
            Console.WriteLine(" _|_|_|_|            _|    _|        _|_|      _|_|_|");
            Console.WriteLine(" _|        _|  _|_|      _|_|_|_|  _|    _|  _|");
            Console.WriteLine(" _|_|_|    _|_|      _|    _|      _|    _|    _|_|");
            Console.WriteLine(" _|        _|        _|    _|      _|    _|        _|");
            Console.WriteLine(" _|        _|        _|      _|_|    _|_|    _|_|_|");
        }

        protected override void Run()
        {
            //Print out a marker and get input
            Console.Write("usr:/ > ");
            String input = Console.ReadLine().Trim();
            //Cut the input into a function call and arguments
            int endIndex = 0;
            if (input.IndexOf(' ') < 0)
            {
                endIndex = input.Length - 1;
            }
            else
            {
                endIndex = input.IndexOf(' ');
            }
            String func = input.Substring(0, endIndex + 1).Trim();
            String args = input.Substring(func.Length).Trim();
            execute(func, args);
        }

        public void execute(String func, String args)
        {
            //Based on the function, call the appropriate built in method with arguments
            if (func == "help")
            {
                help();
            }
            else if (func == "time")
            {
                time(args);
            }
            else if (func == "date")
            {
                SYSTEM_DATE.update();
                date(args);
            }
            else if (func == "cls")
            {
                clearScreen();
            }
            else if (func == "create")
            {
                create(args);
            }
            else if (func == "dir")
            {
                dir(args);
            }
            else if (func == "out")
            {
                output(args);
            }
            else if (func == "vars")
            {
                vars();
            }
            else if (func == "run")
            {
                int res = run(args);
                Console.WriteLine("Run returned code: " + res);
            }
            else if (func == "rm")
            {
                rm(args);
            }
            else if (func == "clr")
            {
                clr(args);
            }
            else
            {
                String input = func + args;
                if (input.Contains("="))
                {
                    parseInput(input);
                }
                else
                {
                    Console.WriteLine(input);
                }
            }
        }

        //DOS time and date functions, functional except for setting time/date (prints out new time/date, but doesn't set it)
        public void time(String args)
        {
            //For the "/t" argument, print out the time in a human readable format and exit
            if (args == "/t" || args == "/T")
            {
                String r = "";
                if (Cosmos.Hardware.RTC.Hour >= 12)
                {
                    r = byteToString((byte)(Cosmos.Hardware.RTC.Hour - 12)) + ':' + byteToString(Cosmos.Hardware.RTC.Minute) + " PM";
                }
                else
                {
                    r = byteToString(Cosmos.Hardware.RTC.Hour) + ':' + byteToString(Cosmos.Hardware.RTC.Minute) + " AM";
                }
                Console.WriteLine(r);
                return;
            }
            //If no arguments were given, print out the time before continuing
            if (args.Length == 0)
            {
                Console.WriteLine("The current time is: " + byteToString(Cosmos.Hardware.RTC.Hour) + ':'
                    + byteToString(Cosmos.Hardware.RTC.Minute) + ':' + byteToString(Cosmos.Hardware.RTC.Second));
            }
            //Switch on args' length to determine what to do
            bool locker = true;
            while (locker)
            {
                switch (args.Length)
                {
                    case 0:
                        Console.WriteLine("Enter the new time: (HH:MM:SS)");
                        args = Console.ReadLine();
                        if (args.Length == 0)
                        {
                            return;
                        }
                        else if (args.Length != 8)
                        {
                            Console.WriteLine("The system cannot accept the time entered.");
                            args = "";
                            break;
                        }
                        break;
                    case 8:
                        char[] temp = args.ToCharArray();
                        bool error = false;
                        for (int i = 0; i < temp.Length; i++)
                        {
                            if (!isValidChar(temp[i], true))
                            {
                                error = true;
                                break;
                            }
                        }
                        if (!error && temp[2] == ':' && temp[5] == ':')
                        {
                            //Set the new time according to what the user input
                            byte hour = (byte)(Int32.Parse(args.Substring(0, 2)));
                            byte min = (byte)(Int32.Parse(args.Substring(3, 2)));
                            byte sec = (byte)(Int32.Parse(args.Substring(6, 2)));
                            //User entered invalid time check
                            if (hour < 0 || hour > 23 || min < 0 || min > 59 || sec < 0 || sec > 59)
                            {
                                Console.WriteLine("The system cannot accept the time entered.");
                                args = "";
                                break;
                            }
                            else
                            {
                                Console.WriteLine("The new time is: " + byteToString(hour) + ':' + byteToString(min) + ':' + byteToString(sec));
                                //Cosmos.Hardware.RTC.Hour = hour;
                                //Cosmos.Hardware.RTC.Minute = min;
                                //Cosmos.Hardware.RTC.Second = sec;
                                locker = false;
                            }
                        }
                        else
                        {
                            Console.WriteLine("The system cannot accept the time entered.");
                            args = "";
                        }
                        break;
                    default:
                        Console.WriteLine("The system cannot accept the time entered.");
                        args = "";
                        break;
                }
            }
            return;
        }

        public void date(String args)
        {
            //Print out the current date if the arguments are "/t", "/T", or none
            if (args.Length == 0)
            {
                Console.WriteLine("The current date is: " + SYSTEM_DATE.getLongDate());
            }
            //If the "/t" argument is present, exits
            else if (args == "/t" || args == "/T")
            {
                Console.WriteLine(SYSTEM_DATE.getLongDate());
                return;
            }
            //For all other arguments, switch on the input
            bool locker = true;
            while (locker)
            {
                switch (args.Length)
                {
                    case 0:
                        Console.WriteLine("Enter the new date: (mm-dd-yy)");
                        args = Console.ReadLine();
                        if (args.Length == 0)
                        {
                            return;
                        }
                        else if (args.Length != 8)
                        {
                            Console.WriteLine("The system cannot accept the date entered.");
                            args = "";
                            return;
                        }
                        break;
                    case 8:
                        char[] temp = args.ToCharArray();
                        bool error = false;
                        for(int i = 0; i < temp.Length; i++)
                        {
                            if (!isValidChar(temp[i], false))
                            {
                                error = true;
                                break;
                            }
                        }
                        if (!error && temp[2] == '-' && temp[5] == '-')
                        {
                            //Set the new date according to what the user input
                            byte mon = (byte)(Int32.Parse(args.Substring(0, 2)));
                            byte day = (byte)(Int32.Parse(args.Substring(3, 2)));
                            byte cent = Cosmos.Hardware.RTC.Century;
                            byte yr = (byte)(Int32.Parse(args.Substring(6, 2)));
                            //User entered invalid date check
                            if (mon < 1 || mon > 12 || day < 1 || day > 31 || yr < 0 || yr > 99 
                                || (day > 29 && mon == 2 && yr%4==0) || (day > 28 && mon == 2)
                                || (day > 30 && (mon == 4 || mon == 6 || mon == 9 || mon == 11))) {
                                Console.WriteLine("The system cannot accept the date entered.");
                                args = "";
                                break;
                            }
                            else
                            {
                                SYSTEM_DATE.update(day, mon, cent, yr);
                                Console.WriteLine("The new date is: " + SYSTEM_DATE.getLongDate());
                                locker = false;
                            }
                        }
                        else
                        {
                            Console.WriteLine("The system cannot accept the date entered.");
                            args = "";
                        }
                        break;
                    default:
                        Console.WriteLine("The system cannot accept the date entered.");
                        args = "";
                        break;
                }
            }
            return;
        }

        public void clearScreen()
        {
            Console.Clear();
        }

        public void help()
        {
            Console.WriteLine("Current commands: ");
            Console.WriteLine("time");
            Console.WriteLine("date");
            Console.WriteLine("cls");
            Console.WriteLine("create");
            Console.WriteLine("dir");
            Console.WriteLine("out");
            Console.WriteLine("vars");
            Console.WriteLine("run");
            Console.WriteLine("rm");
            Console.WriteLine("clr");
            Console.WriteLine("help");
            Console.WriteLine("Global variables may be input with: ");
            Console.WriteLine("<VARNAME> = <ARITHMETIC EXPR>");
            Console.WriteLine("<VARNAME> = <STRING EXPR");
        }

        public void create(String args)
        {
            if (args.IndexOf('.') < 0)
            {
                Console.WriteLine("Usage: create <Filename>.<Ext>");
                return;
            }
            else if (fileExists(args))
            {
                Console.WriteLine("Error: file already exists. Overwrite? (Y/N)");
                while (true)
                {
                    String input = Console.ReadLine();
                    if (input.ToLower() == "n")
                    {
                        return;
                    }
                    else if (input.ToLower() == "y")
                    {
                        rm(args);
                        break;
                    }
                }
            }
            File f = new File(args);
            FILESYS.Add(f);
            int i = 1;
            while (true)
            {
                Console.Write(i + "> ");
                String input = Console.ReadLine().Trim();
                if (input == "save")
                {
                    break;
                }
                f.writeLine(input);
                i++;
            }
            Console.WriteLine("*** File Saved ***");
        }

        public void dir(String args)
        {
            if (args.Length != 0)
            {
                Console.WriteLine("No arguments allowed. Stop that. You butt.");
                return;
            }
            Console.WriteLine("Filename             Extension  Date       Size");
            Console.WriteLine("---------------------------------------------------");
            File[] temp = new File[FILESYS.Count];
            FILESYS.CopyTo(temp);
            for(int i = 0; i < FILESYS.Count; i++)
            {
                File f = temp[i];
                Console.Write(f.getFileName().PadRight(20) + ' ');
                Console.Write(f.getExtension().PadRight(10) + ' ');
                Console.Write(f.getCreationDate().PadRight(10) + ' ');
                Console.Write(f.getByteSize() + 'B');
                Console.WriteLine();
            }
            Console.WriteLine("Total Files: " + FILESYS.Count);
        }

        public void output(String args)
        {
            if (args.Split(' ').Length - 1 > 0)
            {
                Console.WriteLine("Usage: out <VARNAME>");
                return;
            }
            Variable[] temp = new Variable[GLOBAL_VARS.Count];
            GLOBAL_VARS.CopyTo(temp);
            String output = "";
            for(int i = 0; i < GLOBAL_VARS.Count; i++)
            {
                if (temp[i].getName() == args)
                {
                    output += temp[i].toString();
                }
            }
            if (output.Length == 0)
            {
                output = "No such variable exists.";
            }
            Console.WriteLine(output);
        }

        public void vars()
        {
            Variable[] temp = new Variable[GLOBAL_VARS.Count];
            GLOBAL_VARS.CopyTo(temp);
            Console.WriteLine("Variable Name   Value");
            Console.WriteLine("---------------------------");
            for (int i = 0; i < GLOBAL_VARS.Count; i++)
            {
                Console.WriteLine(temp[i].getName().PadRight(15) + ' ' + temp[i].toString());
            }
        }

        public int run(String args)
        {
            Console.WriteLine("Not supported yet!");
            return -1;
        }

        public void parseInput(String input)
        {
            //Cut the input into a function call and arguments
            int endIndex = 0;
            if (input.IndexOf('=') < 0)
            {
                Console.WriteLine("Invalid expression.");
                Console.WriteLine("Usage: <VARNAME> = <EXPR>");
                return;
            }
            else
            {
                endIndex = input.IndexOf('=');
            }
            String varname = input.Substring(0, endIndex);
            String expr = input.Substring(varname.Length + 1).Trim();
            varname = varname.Trim();
            if (varname.Length == 0 || expr.Length == 0)
            {
                Console.WriteLine("Error: Variable name doesn't exist or expression doesn't exist.");
                Console.WriteLine("Usage: <VARNAME> = <EXPR>");
                return;
            }
            String val = "\"\""; 
            //TODOODODODODODODOODODODODOODODOODODOODODOODODODOODODODOODODODODOO


            //NIKKO I NEED A METHOD HERE THAT TAKES IN AN EXPRESSION expr
            //AND IT SPITS OUT A STRING


            //string variables are enclosed with surrounding double quotes
            //int variables are default

            //IT WILL DEFAULT TO BEING AN EMPTY STRING VARIABLE BY THE WAY



            //put the variable into the list of globals
            Variable v = null;
            if (val.Contains("\""))
            {
                v = new Variable(varname, val.Substring(1, val.Length - 2));
            }
            else
            {
                v = new Variable(varname, Int32.Parse(val));
            }
            clr(varname);
            GLOBAL_VARS.Add(v);
        }

        public void rm(String args)
        {
            if (args.IndexOf('.') < 0 || args.Split(' ').Length - 1 > 0)
            {
                Console.WriteLine("Usage: rm <Filename>.<Ext>");
                return;
            }
            if (fileExists(args))
            {
                FILESYS.RemoveAt(getFileIndex(args));
            }
            else
            {
                Console.WriteLine("File doesn't exist!");
            }
        }

        public void clr(String args)
        {
            if (args.Split(' ').Length - 1 > 0)
            {
                Console.WriteLine("Usage: clr <VARNAME>");
                return;
            }
            Variable[] temp = new Variable[GLOBAL_VARS.Count];
            GLOBAL_VARS.CopyTo(temp);
            for (int i = 0; i < GLOBAL_VARS.Count; i++)
            {
                if (temp[i].getName() == args)
                {
                    GLOBAL_VARS.RemoveAt(i);
                    return;
                }
            }
            Console.WriteLine("No such variable exists.");
        }

        //Displays bytes as pairs of two digits 0-9. Used only for printing the time.
        private String byteToString(byte s)
        {
            String r = s.ToString();
            if (s < 10)
            {
                r = '0' + r;
            }
            return r;
        }

        //Returns if a character is valid or not: 0-9, :, - are valid. : is valid if timeOrDate is true, - if it is false
        private bool isValidChar(char c, bool timeOrDate)
        {
            switch (c)
            {
                case '0':
                    return true;
                case '1':
                    return true;
                case '2':
                    return true;
                case '3':
                    return true;
                case '4':
                    return true;
                case '5':
                    return true;
                case '6':
                    return true;
                case '7':
                    return true;
                case '8':
                    return true;
                case '9':
                    return true;
                case ':':
                    return timeOrDate;
                case '-':
                    return !timeOrDate;
                default:
                    return false;
            }
        }

        //Helper for determining if a duplicate file is being made
        private bool fileExists(String fname)
        {
            File[] temp = new File[FILESYS.Count];
            FILESYS.CopyTo(temp);
            for (int i = 0; i < FILESYS.Count; i++)
            {
                if (temp[i].getFileName() == fname)
                {
                    return true;
                }
            }
            return false;
        }

        //Helper for finding files
        private int getFileIndex(String fname)
        {
            File[] temp = new File[FILESYS.Count];
            FILESYS.CopyTo(temp);
            for (int i = 0; i < FILESYS.Count; i++)
            {
                if (temp[i].getFileName() == fname)
                {
                    return i;
                }
            }
            return -1;
        }

        private Variable getVar(String n)
        {
            Variable[] temp = new Variable[GLOBAL_VARS.Count];
            GLOBAL_VARS.CopyTo(temp);
            for (int i = 0; i < GLOBAL_VARS.Count; i++)
            {
                if (temp[i].getName() == n)
                {
                    return temp[i];
                }
            }
            return null;
        }

        /*private String stringToOther(String expr)
        {
            //Check if it can be converted to either double or int,
            //if it can, return as is
            double num;
            int num2;
            if (Int32.TryParse(expr, out num2))
                return expr;
            //Return the string with quotes around it
            return "\"" + expr + "\"";
        }*/
    }
}
