using Connection;
using System;
using System.Collections.Generic;
using System.IO;

namespace Connect_Four
{
    public interface ISavable
    {
        void Load();
        void Save();
    }

    [Serializable]
    public class SavableList<T> : List<T> , ISavable
    {
        private FileInfo fileInfo;

        public SavableList(FileInfo path)
        {
            fileInfo = path;
        }

        private string GetData()
        {
            string ret = "";
            try
            {
                var reader = new StreamReader(fileInfo.FullName);
                ret = reader.ReadToEnd();
                reader.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex);
            }
            
            return ret;
        }

        public void Load()
        {
            Clear();
            try
            {
                AddRange(JsonConvert.Deserialise<List<T>>(GetData()));
            }
            catch (Exception)
            {
            }
        }

        public void Save()
        {
            try
            {
                var writer = new StreamWriter(fileInfo.FullName, false);
                writer.Write(JsonConvert.Serialise(this));
                writer.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
