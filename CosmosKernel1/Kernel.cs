using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace CosmosKernel1
{
    public class Kernel : Sys.Kernel
    {
        private int month;
        private int dayOfMonth;
        private int year;
        private int hour;
        private int minute;
        private int second;

        /*
        private int monthOffset;
        private int dayOffset;
        private int yearOffset;
        private int hourOffset;
        private int minuteOffset;
        private int secondOffset;
        */

        private FileDirectory files;
        private VariableStorage variables;

        protected override void BeforeRun()
        {
            updateTime();
            /*monthOffset = 0;
            dayOffset = 0;
            yearOffset = 0;
            hourOffset = 0;
            minuteOffset = 0;
            secondOffset = 0;*/
            files = new FileDirectory();
            variables = new VariableStorage();
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        protected override void Run()
        {
            Console.Write("Input: ");
            string input = Console.ReadLine();

            // SHOW DATE
            if (input.IndexOf("date") == 0)
            {
                /*if (input.IndexOf("date ") == 0)
                {
                    setDate(input.Substring(5, input.Length - 4));
                }*/
                Console.WriteLine(getDate());
            }

            // SHOW TIME
            else if (input.IndexOf("time") == 0)
            {
                /*if (input.IndexOf("time ") == 0)
                {
                    setTime(input.Substring(5, input.Length - 4));
                }*/
                Console.WriteLine(getTime());
            }

            // CREATE FILE
            else if (input.IndexOf("create ") == 0)
            {
                string filename = input.Substring(7, input.Length - 6);
                if (filename.IndexOf(' ') != -1 )
                {
                    Console.WriteLine("Spaces are not allowed in the filename");
                    return;
                }
                string contents = "";
                Console.WriteLine("The following lines will be saved to your file once you enter 'save'.");
                while (true)
                {
                    string file_input = Console.ReadLine();
                    if (file_input.CompareTo("save") == 0)
                    {
                        File file = new File(filename, contents, getDate() + " " + getTime());
                        files.Add(filename, file);
                        Console.WriteLine(filename + " has been saved.");
                        return;
                    }
                    else
                    {
                        contents += file_input + "\n";
                    }
                }
            }

            // SHOW FILES
            else if (input.IndexOf("dir") == 0)
            {
                Console.WriteLine("Name\t\tExt\t\tCreated\t\t\t\tSize");
                for (int i = 0; i < files.Length(); i++)
                {
                    Console.WriteLine(files.getFile(i).ToString());
                }
            }

            // SHOW VARIABLE
            else if (input.IndexOf("out ") == 0)
            {
                string key = input.Substring(4, input.Length - 3);
                if (variables.ContainsKey(key))
                {
                    Console.WriteLine(key + " = " + variables.GetValue(key));
                }
                else
                {
                    Console.WriteLine("Variable not found.");
                }
            }

            // CREATE VARIABLE
            else if (input.IndexOf(" = ") > 0)
            {
                string[] vars = input.Split('=');
                if (vars.Length != 2)
                {
                    Console.WriteLine("Incorrect use of variable assignment.");
                    return;
                }
                string variable = vars[0].Trim();
                string expression = vars[1].Trim();
                string value = ExecuteExpression(expression);
                if (value.IndexOf("Error:") != -1)
                {
                    Console.WriteLine(value);
                    return;
                }
                Console.WriteLine(variable + " = " + value);
                variables.Add(variable, value);
            }

            // DEFAULT ACTION
            else
            {
                Console.Write("Text typed: ");
                Console.WriteLine(input);
            }
        }

        private string ExecuteExpression(string expression)
        {
            string[] operands = new string[1];
            
            char[] operators = new char[] {'+', '-', '*', '/', '&', '|', '^'};
            char op = '\n';
            for (int i = 0; i < operators.Length; i++)
            {
                char o = operators[i];
                if (expression.IndexOf(o) >= 0)
                {
                    op = o;
                    break;
                }
            }
            operands = expression.Split(op);
            if (operands.Length != 2)
            {
                return "Error: Only one operator allowed.";
            }

            operands[0] = operands[0].Trim();
            operands[1] = operands[1].Trim();
            if (variables.GetValue(operands[0]).Length > 0)
            {
                operands[0] = variables.GetValue(operands[0]);
            }
            if (variables.GetValue(operands[1]).Length > 0)
            {
                operands[1] = variables.GetValue(operands[1]);
            }
            if (op == '+')
            {
                int operand1 = Int32.Parse(operands[0]);
                int operand2 = Int32.Parse(operands[1]);
                return "" + (operand1 + operand2);
            } 
            else if (op == '-') 
            {
                int operand1 = Int32.Parse(operands[0]);
                int operand2 = Int32.Parse(operands[1]);
                return "" + (operand1 - operand2);
            }
            else if (op == '*') 
            {
                int operand1 = Int32.Parse(operands[0]);
                int operand2 = Int32.Parse(operands[1]);
                return "" + (operand1 * operand2);
            }
            else if (op == '/')
            {
                int operand1 = Int32.Parse(operands[0]);
                int operand2 = Int32.Parse(operands[1]);
                return "" + (operand1 / operand2);
            }
            else if (op == '&')
            {
                bool operand1 = operands[0].ToLower().CompareTo("true") == 0 ? true : false;
                bool operand2 = operands[1].ToLower().CompareTo("true") == 0 ? true : false;
                return "" + (operand1 & operand2);
            }
            else if (op == '|')
            {
                bool operand1 = operands[0].ToLower().CompareTo("true") == 0 ? true : false;
                bool operand2 = operands[1].ToLower().CompareTo("true") == 0 ? true : false;
                return "" + (operand1 | operand2);
            }
            else if (op == '^')
            {
                bool operand1 = operands[0].ToLower().CompareTo("true") == 0 ? true : false;
                bool operand2 = operands[1].ToLower().CompareTo("true") == 0 ? true : false;
                return "" + (operand1 ^ operand2);
            }
            return "Error: Invalid operator";
        }
        /*
        protected void setTime(string userTime)
        {
            string[] times = userTime.Split(':');
            int userHour = Int32.Parse(times[0]);
            int userMinute = Int32.Parse(times[1]);
            int userSecond = Int32.Parse(times[2]);
            updateTime();
            hourOffset = hour - userHour;
            minuteOffset = minute - userMinute;
            secondOffset = second - userSecond;
        }
        protected void setDate(string userDate)
        {
            string[] dates = userDate.Split('/');
            int userMonth = Int32.Parse(dates[0]);
            int userDay = Int32.Parse(dates[1]);
            int userYear = Int32.Parse(dates[2]);
            updateTime();
            monthOffset = month - userMonth;
            dayOffset = dayOfMonth - userDay;
            yearOffset = year - userYear;
        }
        */
        protected void updateTime()
        {
            hour = Cosmos.Hardware.RTC.Hour;
            minute = Cosmos.Hardware.RTC.Minute;
            second = Cosmos.Hardware.RTC.Second;
            month = Cosmos.Hardware.RTC.Month;
            dayOfMonth = Cosmos.Hardware.RTC.DayOfTheMonth;
            year = (Cosmos.Hardware.RTC.Century * 100) + Cosmos.Hardware.RTC.Year;
        }

        protected string getTime()
        {
            updateTime();
            return (hour/* - hourOffset*/).ToString() + ":" + (minute/* - minuteOffset*/).ToString() + ":" + (second/* - secondOffset*/).ToString();
        }

        protected string getDate()
        {
            updateTime();
            return (month/* - monthOffset*/).ToString() + "/" + (dayOfMonth/* - dayOffset*/).ToString() + "/" + (year/* - yearOffset*/).ToString();
        }
    }

    public class File
    {
        private string name;
        private string ext;
        private string created_at;
        private string contents;

        public File(string filename, string data, string timestamp)
        {
            string[] name_ext = filename.Split('.');
            ext = name_ext[name_ext.Length - 1];
            name = filename.Substring(0, filename.Length - ext.Length - 1);
            contents = data;
            created_at = timestamp;
        }

        override public string ToString()
        {
            return name + "\t\t" + ext + "\t\t" + created_at + "\t\t" + (contents.Length * 2) + " Bytes";
        }
    }

    public class FileDirectory
    {
        private string[] filenames;
        private File[] files;
        private int count;
        private const int MAX_FILES = 50;

        public FileDirectory()
        {
            filenames = new string[MAX_FILES];
            files = new File[MAX_FILES];
            count = 0;
        }

        public void Add(string filename, File file)
        {
            int i = index(filename);
            if (i >= 0)
            {
                files[i] = file;
            }
            else
            {
                filenames[count] = filename;
                files[count] = file;
                count = count + 1;
            }
        }

        private int index(string name)
        {
            for (int i = 0; i < count; i++)
            {
                if (filenames[i].CompareTo(name) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public int Length()
        {
            return count;
        }

        public string getFilename(int i)
        {
            return filenames[i];
        }

        public File getFile(int i)
        {
            return files[i];
        }
    }

    public class VariableStorage
    {
        private string[] variables;
        private string[] values;
        private int count;
        private const int MAX_VARIABLES = 100;

        public VariableStorage()
        {
            variables = new string[MAX_VARIABLES];
            values = new string[MAX_VARIABLES];
            count = 0;
        }

        public void Add(string name, string val)
        {
            int i = index(name);
            if (i >= 0)
            {
                values[i] = val;
            }
            else
            {
                variables[count] = name;
                values[count] = val;
                count = count + 1;
            }
        }

        private int index(string key)
        {
            for (int i = 0; i < count; i++)
            {
                if (variables[i].CompareTo(key) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool ContainsKey(string name)
        {
            return index(name) >= 0;
        }

        internal string GetValue(string name)
        {
            int num = index(name);
            if (num < 0)
            {
                return "";
            }
            else
            {
                return values[num];
            }
        }
    }
}
