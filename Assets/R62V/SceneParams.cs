using System.Collections;
using System.Collections.Generic;

public static class SceneParams
{
    private static Dictionary<string, string> paramMap = new Dictionary<string, string>();
    public static void setParamValue(string key, string val)
    {
        if (paramMap.ContainsKey(key)) paramMap[key] = val;
        else paramMap.Add(key, val);
    }

    public static string getParamValue(string key)
    {
        if (paramMap.ContainsKey(key)) return paramMap[key];
        else return "";
    }
}
