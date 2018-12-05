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

    /// <summary>
    /// Impliments a list that can save and load.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class SavableList<T> : List<T> , ISavable
    {
        private FileInfo fileInfo;

        public SavableList(FileInfo path)
        {
            fileInfo = path;
        }

        /// <summary>
        /// Retrieves raw data from the save file.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Loads data from the save file.
        /// </summary>
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

        /// <summary>
        /// Saves data to the save file.
        /// </summary>
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
