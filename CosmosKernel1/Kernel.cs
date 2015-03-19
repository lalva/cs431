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

        private FileDirectory files;
        private VariableStorage variables;

        protected override void BeforeRun()
        {
            UpdateTime();
            files = new FileDirectory();
            variables = new VariableStorage();
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        protected override void Run()
        {
            Console.Write("Input: ");
            string input = Console.ReadLine();

            ProcessInput(input);
        }

        public void ProcessInput(string input)
        {
            if (input.CompareTo("\n") == 0 || input.CompareTo("") == 0)
            {
                return;
            }
            string[] args = input.Split(' ');
            string command = args[0];
            string rest = input.Substring(command.Length).Trim();

            // SHOW DATE
            if (command.CompareTo("date") == 0)
            {
                Console.WriteLine(GetDate());
            }

            // SHOW TIME
            else if (command.CompareTo("time") == 0)
            {
                Console.WriteLine(GetTime());
            }

            // CREATE FILE
            else if (command.CompareTo("create") == 0)
            {
                CreateFile(rest);
            }

            // SHOW FILES
            else if (command.CompareTo("dir") == 0)
            {
                Console.WriteLine("Name\t\tExt\t\tCreated\t\t\t\t\tSize");
                for (int i = 0; i < files.Length(); i++)
                {
                    Console.WriteLine(files.GetFile(i).ToString());
                }
            }

            // SHOW VARIABLE
            else if (command.CompareTo("out") == 0)
            {
                if (variables.ContainsKey(rest))
                {
                    Console.WriteLine(rest + " = " + variables.GetValue(rest));
                }
                else
                {
                    Console.WriteLine("Variable not found.");
                }
            }

            // RUN .BAT
            else if (command.CompareTo("run") == 0)
            {
                RunBatchFile(rest);
            }

            // RUN .BAT FILES 
            else if (command.CompareTo("runall") == 0)
            {
                RunAllBatchFiles(rest.Split(' '));
            }

            // CREATE GLOBAL VARIABLE
            else if (command.CompareTo("shared") == 0)
            {
                SetVariable(rest);
            }

            // CREATE VARIABLE
            else if (input.IndexOf(" = ") != -1)
            {
                SetVariable(input);
            }

            // DEFAULT ACTION
            else
            {
                Console.WriteLine("Text typed: "+input);
            }
        }

        private void RunAllBatchFiles(string[] filenames)
        {
            File[] batchFiles = new File[filenames.Length];
            for (int i = 0; i < filenames.Length; i++)
            {
                batchFiles[i] = files.Find(filenames[i]);
                if (batchFiles[i] == null)
                {
                    Console.WriteLine(filenames[i] + " does not exist. Please make sure all files exist.");
                    return;
                }
                if (batchFiles[i].GetExtension().CompareTo("bat") != 0)
                {
                    Console.WriteLine(filenames[i] + " is not a .bat file. All files must be .bat files.");
                    return;
                }
            }
            ThreadManager tm = ThreadManager.GetInstance();
            tm.AddThreads(batchFiles);
            tm.Execute(this);
        }

        private void SetVariable(string expr)
        {
            string[] vars = expr.Split('=');
            if (vars.Length != 2)
            {
                Console.WriteLine("Incorrect use of variable assignment.");
                return;
            }
            string variable = vars[0].Trim();
            string expression = vars[1].Trim();
            string value = "";
            if (expression.Split(' ').Length == 1)
            {
                value = expression;
            }
            else
            {
                value = ExecuteExpression(expression);
            }
            if (value.IndexOf("Error:") != -1)
            {
                Console.WriteLine(value);
                return;
            }
            //Console.WriteLine("You have set "+variable + " = " + value);
            variables.Add(variable, value);
        }

        private void RunBatchFile(string filename)
        {
            string[] parts = filename.Split('.');
            string ext = parts[parts.Length - 1];
            if (ext.CompareTo("bat") == 0)
            {
                File file = files.Find(filename);
                if (file != null)
                {
                    string[] lines = file.GetContents().Split('\n');
                    for (int i = 0; i < lines.Length; i++)
                    {
                        ProcessInput(lines[i]);
                    }
                }
                else
                {
                    Console.WriteLine(filename + " does not exist.");
                }
            }
            else
            {
                Console.WriteLine("You can only run files with an extension of .bat");
            }
        }

        private void CreateFile(string filename)
        {
            if (filename.IndexOf(' ') != -1)
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
                    File file = new File(filename, contents, GetDate() + " " + GetTime());
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

        protected void UpdateTime()
        {
            hour = Cosmos.Hardware.RTC.Hour;
            minute = Cosmos.Hardware.RTC.Minute;
            second = Cosmos.Hardware.RTC.Second;
            month = Cosmos.Hardware.RTC.Month;
            dayOfMonth = Cosmos.Hardware.RTC.DayOfTheMonth;
            year = (Cosmos.Hardware.RTC.Century * 100) + Cosmos.Hardware.RTC.Year;
        }

        protected string GetTime()
        {
            UpdateTime();
            return hour.ToString() + ":" + minute.ToString() + ":" + second.ToString();
        }

        protected string GetDate()
        {
            UpdateTime();
            return month.ToString() + "/" + dayOfMonth.ToString() + "/" + year.ToString();
        }
    }

    public class ThreadManager
    {
        private static ThreadManager instance = null;
        private const int MAX_THREADS = 100;
        private int count;
        private Thread[] threads;

        private ThreadManager()
        {
            threads = new Thread[MAX_THREADS];
            count = 0;
        }

        public static ThreadManager GetInstance()
        {
            if (instance == null)
            {
                instance = new ThreadManager();
            }
            return instance;
        }

        public void AddThreads(File[] filenames)
        {
            for (int i = 0; i < filenames.Length; i++)
            {
                AddThread(filenames[i]);
            }
        }

        public int GetCount()
        {
            return count;
        }

        public void AddThread(File file)
        {
            Thread thread = new Thread(file);
            threads[count] = thread;
            count += 1;
        }

        public void Execute(Kernel kernel)
        {
            int skipped_threads = 0;
            while (skipped_threads < count)
            {
                skipped_threads = 0;
                for (int i = 0; i < count; i++)
                {
                    if (threads[i].IsCompleted())
                    {
                        skipped_threads++;
                        continue;
                    }
                    kernel.ProcessInput(threads[i].NextCommand());
                }
            }
        }
    }

    public class Thread
    {
        private File file;
        private string[] commands;
        private int iterator;

        public Thread(File file)
        {
            this.file = file;
            this.commands = file.GetContents().Split('\n');
            this.iterator = 0;
        }

        public string NextCommand()
        {
            return commands[iterator++];
        }

        public bool IsCompleted()
        {
            return iterator >= commands.Length;
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

        public string GetContents()
        {
            return contents;
        }

        public string GetName()
        {
            return name;
        }

        public string GetExtension()
        {
            return ext;
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
            int i = Index(filename);
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

        private int Index(string name)
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

        public File Find(string filename)
        {
            int i = Index(filename);
            if (i >= 0)
            {
                return files[i];
            }
            return null;
        }

        public string GetFilename(int i)
        {
            return filenames[i];
        }

        public File GetFile(int i)
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
            int i = Index(name);
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

        private int Index(string key)
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
            return Index(name) >= 0;
        }

        internal string GetValue(string name)
        {
            int num = Index(name);
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
