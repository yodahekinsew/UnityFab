using System;
using System.Linq;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;

public enum DSLExpressionType
{
    variable,
    function,
    conditional,
    loop
}

public enum DSLVariableType
{
    boolean,
    number,
    vector
}

public class DSLHelper : MonoBehaviour
{
    public static DSLExpressionType GetExpressionType(string input)
    {
        if (input.StartsWith("for")) return DSLExpressionType.loop;
        if (input.StartsWith("if")) return DSLExpressionType.conditional;
        if (Regex.IsMatch(input, "^[^=]*\\(.*\\)")) return DSLExpressionType.function;
        return DSLExpressionType.variable;
    }

    public static DSLVariableType GetVariableType(string input)
    {
        if (input.Contains("[")) return DSLVariableType.vector;
        return DSLVariableType.number;
    }

    public static DSLVariableType GetVariableType(string input, params Variables[] variableSets)
    {
        // DSLVariableType type = DSLVariableType.number;
        // foreach (Variables variables in variableSets)
        // {
        //     if (variables.HasValue(input, out type)) return type;
        // }
        return GetVariableType(input);
    }

    public static DSLVariableType GetVariableTypeFromDeclaration(string typestr)
    {
        switch (typestr)
        {
            case "vec":
            case "vec3":
            case "vector":
            case "vector3":
                return DSLVariableType.vector;
            case "bool":
            case "boolean":
                return DSLVariableType.boolean;
            default:
                return DSLVariableType.number;
        }
    }

    public static bool ContainsVariable(string str, string variable)
    {
        return Regex.IsMatch(str, "(?:^|\\W)" + variable + "(?:$|\\W)");
    }

    public static string TryTrimOutsideParen(string str)
    {
        str = str.Trim();
        if (str.Length > 2 && str[0] == '(' && str[str.Length - 1] == ')')
            str = str.Substring(1, str.Length - 2);
        return str;
    }

    public static int GetIndexOfMatchingParen(string str, int startingParenIndex)
    {
        char openingParen = str[startingParenIndex];
        char closingParen = ' ';
        if (openingParen == '(') closingParen = ')';
        else if (openingParen == '{') closingParen = '}';
        else if (openingParen == '[') closingParen = ']';
        else
        {
            Debug.LogError("Valid parenthesis was not given.");
            return -1;
        }

        Dictionary<char, int> parenCounts = new Dictionary<char, int> { };
        parenCounts.Add(openingParen, 0);
        parenCounts.Add(closingParen, 0);

        for (int i = startingParenIndex; i < str.Length; i++)
        {
            if (parenCounts.ContainsKey(str[i])) parenCounts[str[i]]++;
            if (parenCounts[openingParen] == parenCounts[closingParen]) return i;
        }

        Debug.LogError("No matching parenthesis");
        return -1;
    }

    // Splits string array as normal and gets rid of empty characters.
    public static string[] Split(string str, char delimiter)
    {
        return str.Split(delimiter).Where(x => x.Length != 0).ToArray();
    }

    // Splits at the string at the given parenthesis/brackets level
    public static string[] SplitAtLevel(string str, char delimiter)
    {
        List<string> split = new List<string>();
        int startIndex = 0;
        Dictionary<char, int> parenCounts = new Dictionary<char, int> {
            {'(',0}, {'{',0}, {'[',0}, {')',0}, {'}',0}, {']',0}
        };
        for (int i = 0; i < str.Length; i++)
        {
            if (parenCounts.ContainsKey(str[i])) parenCounts[str[i]]++;
            if (str[i] == delimiter)
            {
                if (parenCounts['('] != parenCounts[')'] ||
                    parenCounts['{'] != parenCounts['}'] ||
                    parenCounts['['] != parenCounts[']']) continue;
                if (startIndex != i)
                {
                    split.Add(str.Substring(startIndex, i - startIndex).Trim());
                    startIndex = i + 1;
                }
            }
        }
        if (startIndex < str.Length - 1) split.Add(str.Substring(startIndex, str.Length - startIndex).Trim());
        return split.ToArray();
    }

    // Splits at the string at the given parenthesis/brackets level
    public static int GetIndexOfAtLevel(string str, string search)
    {
        Dictionary<char, int> parenCounts = new Dictionary<char, int> {
            {'(',0}, {'{',0}, {'[',0}, {')',0}, {'}',0}, {']',0}
        };
        for (int i = 0; i < str.Length; i++)
        {
            if (parenCounts['('] == parenCounts[')'] &&
                parenCounts['{'] == parenCounts['}'] &&
                parenCounts['['] == parenCounts[']'] &&
                (i + search.Length) < str.Length &&
                str.Substring(i, search.Length) == search) return i;
            if (parenCounts.ContainsKey(str[i])) parenCounts[str[i]]++;
        }
        return -1;
    }

    // Checks if the input str contains the search string at the given
    // parenthesis/brackets level
    public static bool ContainsAtLevel(string str, string value)
    {
        Dictionary<char, int> parenCounts = new Dictionary<char, int> {
            {'(',0}, {'{',0}, {'[',0}, {')',0}, {'}',0}, {']',0}
        };
        string search = "";
        for (int i = 0; i < str.Length; i++)
        {
            if (parenCounts.ContainsKey(str[i])) parenCounts[str[i]]++;
            if (parenCounts['('] == parenCounts[')'] &&
                parenCounts['{'] == parenCounts['}'] &&
                parenCounts['['] == parenCounts[']']) search += str[i];
        }
        return search.Contains(value);
    }

    public static string GetFunctionName(string function)
    {
        int startingParen = function.IndexOf('(');
        return function.Substring(0, startingParen).Trim();
    }

    public static string GetFunctionParameters(string function)
    {
        int startingParen = function.IndexOf('(');
        int endingParen = GetIndexOfMatchingParen(function, startingParen);
        return function.Substring(startingParen + 1, endingParen - startingParen - 1).Trim();
    }

    public static string[] GetFunctionExpressions(string function)
    {
        print(function);
        // If the function doesn't have brackets {}, then assume it's a single expression
        int startingBracket = function.IndexOf('{');
        if (startingBracket == -1)
        {
            int startingParen = function.IndexOf('(');
            int endingParen = GetIndexOfMatchingParen(function, startingParen);
            return new string[] { function.Substring(endingParen + 1).Trim() };
        }
        int endingBracket = GetIndexOfMatchingParen(function, startingBracket);
        return SplitAtLevel(function.Substring(startingBracket + 1, endingBracket - startingBracket - 1).Trim(), ';');
    }

    public static string GetVariableName(string variable)
    {
        int equalSign = variable.IndexOf('=');
        return variable.Substring(0, equalSign).Trim();
    }

    public static string GetVariableValue(string variable)
    {
        int equalSign = variable.IndexOf('=');
        string value = variable.Substring(equalSign + 1).Trim();
        if (value.EndsWith(";") || value.EndsWith(",")) return value.Substring(0, value.Length - 1);
        return value;
    }

    public static string GetLoopName(string loop)
    {
        int startingParen = loop.IndexOf('(');
        int equalSign = loop.IndexOf('=', startingParen);
        return loop.Substring(startingParen + 1, equalSign - startingParen - 1).Trim();
    }

    /** Returns a transformed version of the loop value from
            for (x = [0:10:1])  to  (0,10,1)
        so it can be easily processed as a Vector3
    **/
    public static string GetLoopValues(string loop)
    {
        int startingParen = loop.IndexOf('(');
        int endingParen = GetIndexOfMatchingParen(loop, startingParen);
        string loopParams = loop.Substring(startingParen + 1, endingParen - startingParen - 1).Split('=')[1];
        string[] values = loopParams.Substring(1, loopParams.Length - 2).Split(':');
        return "[" + string.Join(", ", values) + "]";
    }

    public static string GetConditional(string conditional)
    {
        int startingParen = conditional.IndexOf('(');
        int endingParen = GetIndexOfMatchingParen(conditional, startingParen);
        return conditional.Substring(startingParen + 1, endingParen - startingParen - 1).Trim();
    }

    // Map of colors to their Color classes
    public static Dictionary<string, Vector3> ColorsMap = new Dictionary<string, Vector3>()
        {
            { "white", new Vector3(1, 1, 1) },
            { "black", new Vector3(0, 0, 0) },
            { "brown", new Vector3(.3f, .15f, 0) },
            { "blue", new Vector3(0, 0, 1) },
            { "red", new Vector3(1, 0, 0) },
            { "orange", new Vector3(1, .5f, 0)},
            { "yellow", new Vector3(1, 0.92f, 0.016f) },
            { "green", new Vector3(0, 1, 0) },
            { "magenta", new Vector3(1, 0, 1) },
            { "cyan", new Vector3(0, 1, 1) },
            { "grey", new Vector3(.5f, .5f, .5f) },
            { "gray", new Vector3(.5f, .5f, .5f) },
        };

    public static HashSet<string> RigidbodyNames = new HashSet<string>() {
        "cube",
        "sphere",
        "capsule",
        "cylinder",
        "cone",
        "plane",
        "pyramid",
        "wedge"
    };
    // static Dictionary<Type, Func<String, String>> m_VariableParseFunctions = new Dictionary<Type, Func<String, String>> {
    //         { typeof(float), ParseFloat },
    //         { typeof(Vector3), ParseVector },
    //     };
}
