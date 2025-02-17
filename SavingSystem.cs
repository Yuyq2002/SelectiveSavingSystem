using System.IO;
using UnityEngine;

namespace SavingSystem
{
    public class SavingSystem
    {
        public enum SavingMode
        {
            Text,
            Binary
        }

        /// <summary>
        /// Write a chunk of data to a file
        /// New file is created if the selected file does not exist
        /// Override all content if the file does exist
        /// </summary>
        /// <param name="file">File to write to/create</param>
        /// <param name="data">The data to write to file</param>
        public static void WriteToFile(string file, string data, SavingMode savingMode = SavingMode.Text)
        {
            WriteToPath(Path.Combine(Application.persistentDataPath, file), data, savingMode);
        }

        /// <summary>
        /// Write a chunk of data to file at the given path
        /// New file is created if the selected file does not exist
        /// Override all content if the file does exist
        /// </summary>
        /// <param name="path">Exact path where the file is located</param>
        /// <param name="data">The data to write to file at the path</param>
        public static void WriteToPath(string path, string data, SavingMode savingMode = SavingMode.Text)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(data);
            }
        }

        /// <summary>
        /// Read the content of the given file to a buffer
        /// </summary>
        /// <param name="file">The file to attempt to read</param>
        /// <param name="buffer">The buffer to output the content to</param>
        /// <returns>Returns true if the file exist</returns>
        public static bool ReadFromFile(string file, out string buffer)
        {
            if (!ReadFromPath(Application.persistentDataPath + '/' + file, out buffer))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Read the content of the file at the given path to a buffer
        /// </summary>
        /// <param name="file">The path to attempt to read</param>
        /// <param name="buffer">The buffer to output the content to</param>
        /// <returns>Returns true if the path contains the file exist</returns>
        public static bool ReadFromPath(string path, out string buffer)
        {
            if (!File.Exists(path))
            {
                //Debug.LogError("Cannot find file to read: " + path);
                buffer = null;
                return false;
            }

            using (StreamReader reader = new StreamReader(path))
            {
                buffer = reader.ReadToEnd();
                return true;
            }
        }
    }
}