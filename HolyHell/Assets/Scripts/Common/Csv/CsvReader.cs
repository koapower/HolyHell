using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

public class CsvReader
{
    //static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };

    /// <summary>
    /// Reads a CSV file located in Resources folder.
    /// </summary>
    public static List<T> ReadFromFile<T>(string file) where T : new()
    {
        string[] lines = ReadAllLines(file);
        lines = FilterLines(lines);
        return ProcessLines<T>(lines);
    }

    public static List<T> ReadFromString<T>(string str) where T : new()
    {
        var lines = Regex.Split(str, LINE_SPLIT_RE);
        lines = MergeQuotedLines(lines);
        lines = FilterLines(lines);
        return ProcessLines<T>(lines);
    }

    private static string[] ReadAllLines(string file)
    {
        if (!File.Exists(file))
        {
            throw new FileNotFoundException(Path.GetFileName(file) + " does not exist!, please check file path!\npath: " + file);
        }

        string tempFileName = Path.GetTempFileName();
        File.Copy(file, tempFileName, overwrite: true);
        string[] rawLines = File.ReadAllLines(tempFileName);
        return MergeQuotedLines(rawLines);
    }

    /// <summary>
    /// Merges lines that are split inside quoted fields into a single logical line.
    /// Per CSV spec, newlines within double quotes are valid characters and should not be treated as row separators.
    /// Tracks quote state character-by-character to correctly handle multiple open/close pairs on the same line
    /// (e.g. closing one quoted field and opening another: ...value","next...).
    /// </summary>
    private static string[] MergeQuotedLines(string[] rawLines)
    {
        List<string> merged = new List<string>();
        StringBuilder current = null;
        bool insideQuotes = false;

        foreach (string line in rawLines)
        {
            if (current != null)
            {
                // Rejoin with \n to preserve the original newline within the quoted field
                current.Append('\n');
            }
            else
            {
                current = new StringBuilder();
            }

            current.Append(line);

            // Walk through the line character by character to track quote state
            foreach (char c in line)
            {
                if (c == '"')
                {
                    insideQuotes = !insideQuotes;
                }
            }

            if (!insideQuotes)
            {
                // All quotes are closed -> this is a complete logical line
                merged.Add(current.ToString());
                current = null;
            }
            // Otherwise, insideQuotes is still true -> keep accumulating the next line
        }

        // If there is still an unclosed field at the end, add it anyway to avoid data loss
        if (current != null)
        {
            merged.Add(current.ToString());
        }

        return merged.ToArray();
    }

    private static string[] FilterLines(string[] lines)
    {
        // Filter out empty lines and lines containing only commas or whitespace
        string[] filteredLines = lines
            .Where(line => !string.IsNullOrWhiteSpace(line) && !Regex.IsMatch(line, @"^[,\s]*$"))
            .ToArray();

        return filteredLines;
    }

    private static List<T> ProcessLines<T>(string[] lines) where T : new()
    {
        List<T> list = new List<T>();
        if (lines.Length <= 1) return list;

        string[] headers = ParseLine(lines[0]);
        Dictionary<string, FieldInfo> fieldInfoMap = GetFieldInfoMap<T>();
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = ParseLine(lines[i]);
            list.Add(ToObject<T>(headers, values, fieldInfoMap));
        }

        return list;
    }

    private static string[] ParseLine(string line)
    {
        List<string> list = new List<string>();
        bool flag = false;
        StringBuilder stringBuilder = new StringBuilder();
        foreach (char c in line)
        {
            switch (c)
            {
                case '"':
                    flag = !flag;
                    continue;
                case ',':
                    if (!flag)
                    {
                        list.Add(stringBuilder.ToString());
                        stringBuilder.Clear();
                        continue;
                    }

                    break;
            }

            stringBuilder.Append(c);
        }

        list.Add(stringBuilder.ToString());
        return list.ToArray();
    }

    private static Dictionary<string, FieldInfo> GetFieldInfoMap<T>() where T : new()
    {
        return (from f in typeof(T).GetFields()
                where f.GetCustomAttribute<CsvIgnoreAttribute>() == null
                select f).ToDictionary((FieldInfo f) => f.GetCustomAttribute<ColumnAttribute>()?.name ?? f.Name, (FieldInfo f) => f);
    }

    private static T ToObject<T>(string[] headers, string[] values, Dictionary<string, FieldInfo> map, T target = default(T)) where T : new()
    {
        T val = target;
        if (val == null)
        {
            target = new T();
        }

        for (int i = 0; i < headers.Length; i++)
        {
            if (map.ContainsKey(headers[i]))
            {
                FieldInfo fieldInfo = map[headers[i]];
                var stringValue = i >= values.Length ? string.Empty : values[i];
                try
                {
                    object value;
                    if (string.IsNullOrWhiteSpace(stringValue))
                    {
                        value = GetDefaultValue(fieldInfo.FieldType);
                    }
                    else
                    {
                        if (fieldInfo.FieldType == typeof(bool))
                        {
                            if (stringValue == "1") value = true;
                            else if (stringValue == "0") value = false;
                            else value = Convert.ChangeType(stringValue, fieldInfo.FieldType);
                        }
                        else if (fieldInfo.FieldType.IsEnum)
                        {
                            value = Enum.Parse(fieldInfo.FieldType, stringValue, true); // ignore case
                        }
                        else
                        {
                            value = Convert.ChangeType(values[i], fieldInfo.FieldType);
                        }
                    }

                    fieldInfo.SetValue(target, value);
                }
                catch (Exception)
                {
                    throw new InvalidCastException(string.Format("{0}: Column [{1}] Data [{2}] is not type of [{3}]，please correct the csv data!", "CsvUtility", headers[i], stringValue, fieldInfo.FieldType));
                }
            }
        }

        return target;
    }

    private static object GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}
