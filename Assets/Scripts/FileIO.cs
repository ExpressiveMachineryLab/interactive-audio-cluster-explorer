using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public class FileIO
{
    public static List<string> LoadStringList(string fileName)
    {
        try
        {
            List<string> list = new List<string>();
            string line;
            StreamReader reader = new StreamReader(fileName, Encoding.Default);
            using (reader)
            {
                do
                {
                    line = reader.ReadLine();

                    if (line != null)
                    {
                        list.Add(line);
                    }
                }
                while (line != null);
                reader.Close();
                return list;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }

    public static void SaveStringList(string fileName, List<string> stringList)
    {
        try
        {
            StreamWriter writer = new StreamWriter(fileName);
            using (writer)
            {
                foreach(string line in stringList)
                {
                    writer.WriteLine(line);
                }
                writer.Close();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public static List<Vector3> LoadVector3List(string fileName)
    {
        try
        {
            List<Vector3> list = new List<Vector3>();
            string line;
            StreamReader reader = new StreamReader(fileName, Encoding.Default);
            using (reader)
            {
                do
                {
                    line = reader.ReadLine();

                    if (line != null)
                    {
                        string[] entries = line.Split('\t');
                        list.Add(new Vector3(Single.Parse(entries[0]), Single.Parse(entries[1]), Single.Parse(entries[2])));
                    }
                }
                while (line != null);
                reader.Close();
                return list;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }

    /// <summary>
    /// Loads the string dictionary. Strings are stored on file as value,key not key,value.
    /// </summary>
    /// <returns>The string dictionary.</returns>
    /// <param name="fileName">File name. Strings in filename are stored on file as value,key not key,value.</param>
    public static Dictionary<string, string> LoadStringDictionary(string fileName)
    {
        string line = "";
        try
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            StreamReader reader = new StreamReader(fileName, Encoding.Default);
            using (reader)
            {
                do
                {
                    line = reader.ReadLine();
                    string[] kvp;//NOTE! STRING IS STORED AS VALUE,KEY NOT KEY,VALUE!!!
                    if (line != null)
                    {
                        kvp = line.Split(',');
                        dictionary.Add(kvp[1], kvp[0]);
                    }
                }
                while (line != null);
                reader.Close();
                return dictionary;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message + ", Line: " + line);
            return null;
        }
    }
}
