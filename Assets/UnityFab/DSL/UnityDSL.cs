using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DSLFunctionParser = System.Action<string, string>;
using DSLParameterParser = System.Action<string>;

[ExecuteInEditMode]
public class UnityDSL : MonoBehaviour
{
    const string FUNCTION_KEY_WORD = "module";

    public VariableParser variableParser;

    [SerializeField, HideInInspector] private string filename;
    [SerializeField, HideInInspector] private bool deletePreviousScene;

    public void ParseDSL()
    {
        TextAsset text = Resources.Load(filename) as TextAsset;
        if (text == null)
        {
            Debug.LogError("The file at \\Assets\\Resources\\" + filename + " could not be found.", text);
            return;
        }
        ParseDSL(text.text);
    }

    public void ParseDSL(string text)
    {
        // if (Instance == null) Instance = this;
        // GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // sphere.transform.localScale = Vector3.one * 1.3f;

        // CSG.Subtract(cube, sphere);

        text = text.Trim();

        if (!VerifyDSL(text)) return;

        if (deletePreviousScene && transform.childCount > 0) ClearDSL();
        else variableParser.Reset();

        // Trim all the commented lines
        string[] lines = text.Split('\n');
        List<string> uncommentedLines = new List<string>();
        foreach (string line in lines)
        {
            if (!line.StartsWith("#") && !line.StartsWith("//")) uncommentedLines.Add(line.Trim());
        }
        lines = uncommentedLines.ToArray();

        string allText = string.Join("", lines);
        string[] functions = DSLHelper.SplitAtLevel(allText.Trim(), ';');

        Debug.Log("Parsing code ...");
        // Then parse all of the modules (user-defined)
        bool successfulParsing = true;
        for (int i = 0; i < functions.Length; i++)
        {
            if (functions[i] == "") continue; // Skip any empty lines
            if (functions[i].StartsWith(FUNCTION_KEY_WORD))
            {
                string function = functions[i].Substring(FUNCTION_KEY_WORD.Length).Trim();
                successfulParsing = successfulParsing && variableParser.ParseFunction(function);
            }
        }
        if (successfulParsing)
        {
            Debug.Log("Finished parsing code!");

            Debug.Log("Evaluating code (intial run) ...");
            // Lastly parse all of the function calls (both primitive and user-defined)
            bool successfulEvaluation = true;
            for (int i = 0; i < functions.Length; i++)
            {
                if (functions[i] == "") continue; // Skip any empty lines
                if (!functions[i].StartsWith(FUNCTION_KEY_WORD))
                {
                    successfulEvaluation = successfulEvaluation &&
                        variableParser.EvaluateExpression(functions[i].Trim());
                }
            }
            if (successfulEvaluation)
            {
                Debug.Log("Evaluation passed!");
                variableParser.StartEvaluating();
            }
            else Debug.LogError("Evaluation failed!");
        }
        else Debug.LogError("Could not parse code!");
    }

    public void ClearDSL()
    {
        // Reset the variable parser so that functions/modules from previous file aren't stored
        variableParser.Reset();

        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }

    public bool VerifyDSL(string text)
    {
        if (text.Length == 0) return false;
        return true;
    }
}

public class Function
{
    public string parameters;
    public string[] expressions;
    public VariableParser parser;

    public Function(string _parameters, string[] _expressions, VariableParser _parser)
    {
        parameters = _parameters;
        expressions = _expressions;
        parser = _parser;
    }

    public bool Evaluate(string parameterValues, ref Variables functionVariables)
    {
        // Evaluate all the parameter values first
        string[] values = DSLHelper.SplitAtLevel(parameterValues, ',');
        for (int i = 0; i < values.Length; i++)
        {
            if (!parser.EvaluateVariable(values[i], ref functionVariables))
            {
                Debug.LogError("Could not parse the following function parameter: " + values[i]);
                return false;
            }
        }

        // Go through expressions in the function and evaluate each one sequentially
        for (int i = 0; i < expressions.Length; i++)
        {
            string expression = expressions[i];
            if (!parser.EvaluateExpression(expression, ref functionVariables))
            {
                Debug.LogError("Couldn't evaluate expression: " + expression);
                return false;
            };
        }

        return true;
    }
}
