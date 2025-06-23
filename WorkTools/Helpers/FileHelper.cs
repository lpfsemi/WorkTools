using CsvHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.PlcOpen;

namespace WorkTools.Helpers
{
    public class FileHelper
    {
        public static void WriteDynaminCvs(string filePath, List<(string Name, List<dynamic>)> content)
        {
            List<List<dynamic>> strings = new List<List<dynamic>>();
            foreach (var item in content)
            {
                List<dynamic> str = new List<dynamic> { item.Name };
                str.AddRange(item.Item2.ToArray());
                strings.Add(str);
            }
            List<string> newStrings = new List<string>();
            int cols = strings.Count;
            int rows = strings[0].Count;

            foreach (var item in strings)
            {
                if (item.Count != rows)
                {
                    throw new InvalidOperationException("The length of column data is inconsistent.");
                }
            }

            for (int i = 0; i < rows; i++)
            {
                StringBuilder builder = new StringBuilder();
                builder.Clear();
                for (int j = 0; j < cols; j++)
                {
                    builder.Append(strings[j][i]);
                    builder.Append(',');
                }
                newStrings.Add(builder.ToString().Trim(','));
            }

            File.WriteAllLines(filePath, newStrings);
        }

        public static void WriteDynaminCvs<T>(string filePath, T content, int retries = 3)
        {
            for (int i = 0; i < retries; i++)
            {
                if (Write(filePath, content))
                {
                    break;
                }
            }

            static bool Write<T>(string filePath, T content)
            {
                try
                {
                    string json = JsonConvert.SerializeObject(content);
                    string csv = JsonToCSV(json);
                    File.WriteAllText(filePath, csv);

                    Span<string> strings = File.ReadAllLines(filePath);
                    var lines = strings[1..];
                    int count = lines.Length;
                    List<string[]> list = new(1200);
                    for (int i = 0; i < count; i++)
                    {
                        string[] value = lines[i].Split(",");
                        list.Add(value);
                    }

                    int cols = list.Count;
                    int rows = list[0].Length;

                    StringBuilder newRow = new();
                    List<string> newRows = new();

                    for (int row = 0; row < rows; row++)
                    {
                        newRow.Clear();
                        for (int col = 0; col < cols; col++)
                        {
                            newRow.Append(list[col][row]);
                            newRow.Append(',');
                        }
                        newRows.Add(newRow.ToString().Trim(','));
                    }
                    File.WriteAllLines(filePath, newRows);

                }
                catch (Exception)
                {
                    return false;
                }

                return true;
            }
        }

        public static string JsonToCSV(string jsonContent)
        {
            StringWriter csvString = new StringWriter();
            using (var csv = new CsvWriter(csvString, CultureInfo.CurrentCulture))
            {
                using (var dt = JsonStringToTable(jsonContent))
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        csv.WriteField(column.ColumnName);
                    }
                    csv.NextRecord();

                    foreach (DataRow row in dt.Rows)
                    {
                        for (var i = 0; i < dt.Columns.Count; i++)
                        {
                            csv.WriteField(row[i]);
                        }
                        csv.NextRecord();
                    }
                }
            }
            return csvString.ToString();
        }

        private static DataTable JsonStringToTable(string jsonContent)
        {
            return JsonConvert.DeserializeObject<DataTable>(jsonContent);
        }

        /// <summary>
        /// 加载要读取的参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T LoadParamConfig<T>()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Symbols.json");
            return LoadJson<T>(path);
        }

        public static void SaveCsv<T>(string filePath, IEnumerable<T> content)
        {
            SaveCsv<T>(filePath, content, 3);
        }

        #region private method
        private static IEnumerable<T> LoadCsv<T>(string fileName, int retries = 3)
        {
            IEnumerable<T> recipeRecoeds = null;
            for (int i = 0; i < retries; i++)
            {
                if (Read(fileName, out recipeRecoeds))
                {
                    break;
                }
            }

            return recipeRecoeds;

            static bool Read<T>(string fileName, out IEnumerable<T> recipeRecoeds)
            {
                try
                {
                    using var writer = new StreamReader(fileName);
                    using var csv = new CsvReader(writer, CultureInfo.CurrentCulture);
                    recipeRecoeds = csv.GetRecords<T>();
                }
                catch
                {
                    recipeRecoeds = default;
                    return false;
                }
                return true;
            }
        }

        private static void SaveCsv<T>(string filePath, IEnumerable<T> content, int retries = 3)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("filePath is null or empty");
            }

            for (int i = 0; i < retries; i++)
            {
                if (Save(filePath, content))
                {
                    break;
                }
            }

            static bool Save<T>(string filePath, IEnumerable<T> content)
            {
                try
                {
                    using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
                    using var csv = new CsvWriter(writer, CultureInfo.CurrentCulture);
                    csv.WriteRecords(content);
                }
                catch (Exception)
                {

                    return false;
                }
                return true;
            }
        }

        private static string LoadFile(string path, int retries = 3)
        {
            string value = string.Empty;
            for (int i = 0; i < retries; i++)
            {
                if (Read(path, out string str))
                {
                    value = str;
                    break;
                }
            }

            return value;

            static bool Read(string path, out string str)
            {
                try
                {
                    using var reader = new StreamReader(path);
                    str = reader.ReadToEnd();
                    return true;
                }
                catch (Exception)
                {
                    str = string.Empty;
                    return false;
                }
            }
        }

        private static bool SaveFile(string path, string license, int retries = 3)
        {
            bool success = false;
            for (int i = 0; i < retries; i++)
            {
                if (Save(path, license))
                {
                    success = true;
                    break;
                }
            }

            return success;

            static bool Save(string path, string license)
            {
                try
                {
                    using var writer = new StreamWriter(path, false, System.Text.Encoding.UTF8);
                    writer.Write(license);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        private static T LoadJson<T>(string path, int retries = 3)
        {
            T config = default;
            for (int i = 0; i < retries; i++)
            {
                if (Load<T>(path, ref config))
                {
                    break;
                }
            }

            return config;
            static bool Load<T>(string path, ref T config)
            {
                try
                {
                    string text = "";
                    using (var reader = new StreamReader(path))
                    {
                        text = reader.ReadToEnd();
                    }
                    config = JsonConvert.DeserializeObject<T>(text);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        private static bool SaveJson<T>(string path, T config, int retries = 3)
        {
            bool success = false;
            for (int i = 0; i < retries; i++)
            {
                if (Update<T>(path, config))
                {
                    success = true;
                    break;
                }
            }

            return success;
            static bool Update<T>(string path, T config)
            {
                try
                {
                    using var writer = new StreamWriter(path, false, System.Text.Encoding.UTF8);
                    string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                    writer.Write(json);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        #endregion
    }
}
