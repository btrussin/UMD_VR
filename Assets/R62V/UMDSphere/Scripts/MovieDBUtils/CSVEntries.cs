using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class CSVEntries {

    List<string> m_fields = new List<string>();
    List<Dictionary<string, string>> m_entries = new List<Dictionary<string, string> >();

    void addField(string field)
    {
        m_fields.Add(field);
    }

    void addEntry(Dictionary<string, string> entry)
    {
        m_entries.Add(entry);
    }

    public void printEntries(string field = "")
    {
        bool printAll = field.Length == 0;

        int num = 0;

        foreach( Dictionary<string, string> currMap in m_entries )
        {
            Debug.Log("Entry: " + num + "\n");

            string currVal;

            foreach( string currKey in currMap.Keys )
            {
                if( printAll || field.CompareTo(currKey) == 0 )
                {
                    currMap.TryGetValue(currKey, out currVal);
                    Debug.Log("\t" + currKey + ": " + currVal + "\n");
                }
            }

        }
    }

    public static CSVEntries parseCSVFromText(char[] chars, List<string> fieldsToSkip = null, int maxEntries = -1)
    {
        CSVEntries result = new CSVEntries();

        bool inFieldEntry = false;

        int currPtr = 0;
        int endPtr = chars.Length;

        List<string> fieldsInFile = new List<string>();

        bool inHeader = true;

        string currField;

        bool openQuote = false;

        char currChar, nextChar;

        HashSet<string> tabooFields = new HashSet<string>();

        if( fieldsToSkip != null && fieldsToSkip.Count > 0 )
        {
            foreach( string f in fieldsToSkip )
            {
                tabooFields.Add(f);
            }
        }

        while(inHeader)
        {
            currField = "";

            inFieldEntry = true;

            openQuote = false;

            while( currPtr < endPtr && inFieldEntry )
            {
                currChar = chars[currPtr];
                nextChar = chars[currPtr + 1];

                switch((int)currChar)
                {
                    case 0xA:
                    case 0xD:
                        if (!openQuote) inHeader = false;
                        goto case ',';
                    case ',':
                        if (openQuote) currField += currChar;
                        else inFieldEntry = false;
                        break;
                    case '"':
                        if (nextChar == '"')
                        {
                            if (openQuote) currField += '"';
                            currPtr++;
                        }
                        else openQuote = !openQuote;
                        break;
                    default:
                        currField += currChar;
                        break;
                }

                currPtr++;
            }

            fieldsInFile.Add(currField);
        }
        
        foreach(string s in fieldsInFile)
        {
            if (tabooFields.Contains(s)) continue;
            result.addField(s);
        }

        int numEntriesAdded = 0;
        int maxEntriesToParse = maxEntries > 0 ? maxEntries : int.MaxValue;

        while( currPtr < endPtr && numEntriesAdded < maxEntriesToParse )
        {
            // grab all the line feed and carriage returns
            while( ( chars[currPtr] == 0xA || chars[currPtr] == 0xD ) && currPtr < endPtr )
            {
                currPtr++;
            }

            Dictionary<string, string> currEntry = new Dictionary<string, string>();

            foreach( string field in fieldsInFile )
            {
                // iterate over each line
                inFieldEntry = true;
                openQuote = false;
                currField = "";

                while( currPtr < endPtr && inFieldEntry )
                {
                    currChar = chars[currPtr];
                    nextChar = chars[currPtr + 1];

                    switch ((int)currChar)
                    {
                        case 0xA:
                        case 0xD:
                        case ',':
                            if (openQuote) currField += currChar;
                            else inFieldEntry = false;
                            break;
                        case '"':
                            if (nextChar == '"')
                            {
                                if (openQuote) currField += '"';
                                currPtr++;
                            }
                            else openQuote = !openQuote;
                            break;
                        default:
                            currField += currChar;
                            break;
                    }

                    currPtr++;
                }

                if (tabooFields.Contains(field)) continue;
                currEntry.Add(field, currField);
            }

            result.addEntry(currEntry);
        } 

        return result;
    }

    public static void SaveOutputData(List<string> selectInformation, long startTime)
    {
        long endTime = DateTime.Now.ToFileTime();

        string path = "Form_Results/CSVFormData_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + ".csv";
        Debug.Log(selectInformation.Count);

        using (var w = new StreamWriter(path))
        {
            for (int i = 0; i < selectInformation.Count; i++)
            {
                var first = selectInformation[i];
                string line = string.Format("{0},{1}", "Checked Option", first); //using string interpolation
                w.WriteLine(line);
                w.Flush();
            }

            DateTime startDate = DateTime.FromFileTime(startTime);
            DateTime endDate = DateTime.FromFileTime(endTime);

            string startToEnd = string.Format("{0},{1}", startDate, endDate);
            w.WriteLine(startToEnd);
            w.Flush();

            string lastLine = string.Format("{0},{1}", "Time Elapsed", endDate - startDate);
            w.WriteLine(lastLine);
            w.Flush();
        }
    }



}
