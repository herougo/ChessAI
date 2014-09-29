using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessProgram
{
    public class Database
    {
        string name { get; set; }
        public string[] tag = new string[100];
        public int tag_length = 0;
        public string[,] database_array = new string[1000, 100];
        public int entry_length = 0;

        public Database(string new_name)
        {
            name = new_name;
            CreateDatabase();
        }

        private void CreateDatabase()
        {
            if (name != null)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + name + ".txt";
                
                // Delete the file if it exists. 
                if (!File.Exists(path))
                {
                    //Create the file. 
                    using (FileStream fs = File.Create(path))
                    {

                    }
                }
                else
                {
                    TxtToArray();
                }
            }
        }

        public void DeleteDatabase()
        {
            if (name != null)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + name + ".txt";

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        void TxtToArray()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + name + ".txt";
            int counter = 0;
            TextFunctions fn = new TextFunctions();

            // load file
            FileStream fs = new FileStream(path, FileMode.Open);
            StreamReader sr = new StreamReader(fs);

            string full_file = sr.ReadToEnd();

            sr.Close();
            fs.Close();

            // get tags
            tag_length = 0;
            while (fn.SearchString(full_file, "<#>"))
            {
                tag[tag_length] = full_file.Substring(0, fn.InString(full_file, "<#>")).Trim();

                full_file = full_file.Substring(fn.InString(full_file, "<#>") + 3);

                tag_length++;
            }

            // get data
            counter = 0;
            entry_length = 0;
            while (fn.SearchString(full_file, "<" + tag[counter] + ">"))
            {
                full_file = full_file.Substring(fn.InString(full_file,
                    "<" + tag[counter] + ">") + tag[counter].Length + 2);

                if (fn.SearchString(full_file, "<" + tag[(counter + 1) % tag_length] + ">"))
                {
                    database_array[entry_length, counter]
                        = full_file.Substring(0, fn.InString(full_file, "<" + tag[(counter + 1) % tag_length] + ">")).Trim();
                }
                else
                {
                    database_array[entry_length, counter] = full_file.Trim();
                    full_file = "";
                }

                counter = (counter + 1) % tag_length;

                if (counter == 0) entry_length++;
            }

            counter = 0;
        }

        void ArrayToTxt()
        {
            DeleteDatabase();
            CreateDatabase();
            
            string path = AppDomain.CurrentDomain.BaseDirectory + name + ".txt";
            FileStream fs = new FileStream(path, FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);

            // tag list
            for (int i = 0; i < tag_length; i++)
            {
                sw.WriteLine(tag[i] + "<#>");
            }
            sw.WriteLine("");

            // data
            for (int r = 0; r < entry_length; r++)
            {
                for (int c = 0; c < tag_length; c++)
                {
                    sw.WriteLine("<" + tag[c] + "> " + database_array[r, c]);
                }
                sw.WriteLine("");
            }

            sw.Close();
            fs.Close();
        }

        public void AddEntry(string[] entry)
        {
            if (entry.Length != tag_length)
            {
                return;
            }

            string path = AppDomain.CurrentDomain.BaseDirectory + name + ".txt";
            FileStream fs = new FileStream(path, FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);

            for (int t = 0; t < tag_length; t++)
            {
                database_array[entry_length, t] = entry[t];
                sw.WriteLine("<" + tag[t] + "> " + entry[t]);
            }
            sw.WriteLine("");

            sw.Close();
            fs.Close();

            entry_length++;
        }

        public void AddTag(string new_tag)
        {
            TxtToArray();
            
            for (int i = 0; i < entry_length; i++)
            {
                database_array[i, tag_length] = "";
            }
            tag[tag_length] = new_tag;
            tag_length++;

            // update txt
            ArrayToTxt();
        }

        public int TagToNum(string given_tag)
        {
            if (name != null && tag_length != 0)
            {
                for (int i = 0; i < tag_length; i++)
                {
                    if (tag[i] == given_tag)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public int Search(string tag, string tag_value)
        {
            if (name != null && tag_length != 0)
            {
                int tag_num = TagToNum(tag);

                for (int i = 0; i < entry_length; i++)
                {
                    if (database_array[i, tag_num] == tag_value)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }
    }
}
