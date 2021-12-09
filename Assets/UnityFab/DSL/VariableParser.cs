using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// TODO: need some method for keeping track of time-based functions
//       so that we can re-evaluate those variables on every call of FixedUpdate()

public class VariableParser : MonoBehaviour
{
    [Header("Extra Primitives")]
    public GameObject m_PlanePrimitive;
    public GameObject m_PyramidPrimitive;
    public GameObject m_ConePrimitive;
    public GameObject m_WedgePrimitive;

    [SerializeField, HideInInspector] private Variables globalVariables = new Variables();
    [SerializeField, HideInInspector] private Dictionary<string, Function> m_Functions = new Dictionary<string, Function>();
    [SerializeField, HideInInspector] private List<RigidbodyProperties> m_Rigidbodies = new List<RigidbodyProperties>();

    private float m_Time = 0;
    private float m_TimeScale = 1;

    private bool canEvaluate = false;

    private void LateUpdate()
    {
        if (!canEvaluate) return;

        m_Time += Time.deltaTime * m_TimeScale;

        globalVariables.AddValue("t", m_Time);
        globalVariables.AddValue("time", m_Time);
        foreach (RigidbodyProperties rigidbody in m_Rigidbodies)
        {
            if (!UpdateRigidbody(rigidbody))
            {
                Debug.LogError("Could not update rigidbody at time step: t = " + m_Time);
                canEvaluate = false;
            }
        }
    }

    // Called for every rigidbody on every call of FixedUpdate()
    public bool UpdateRigidbody(RigidbodyProperties rigidbody)
    {
        rigidbody.variables.AddValue("t", m_Time);
        rigidbody.variables.AddValue("time", m_Time);

        // Iterate through all the time-based variables
        foreach (KeyValuePair<string, string> var in rigidbody.variables.timeBasedVars)
        {
            string varName = var.Key;
            string varValue = var.Value;
            if (!EvaluateVariable(varName, varValue, ref rigidbody.variables))
            {
                Debug.LogError("Could not parse time-based variable: " + varName + " = " + varValue);
                return false;
            }
        }

        // Update the rigidbody properties tied to time-based variables
        for (int i = 0; i < rigidbody.timeParameters.Length; i++)
        {
            string[] parameter = rigidbody.timeParameters[i].Split('=');
            switch (parameter[0].Trim())
            {
                case "m":
                case "mass":
                    if (!TryParseFloat(parameter[1].Trim(), rigidbody.variables, out rigidbody.mass))
                    {
                        Debug.LogError("Couldn't parse mass parameter of rigidbody: " + parameter[1].Trim());
                        return false;
                    }
                    rigidbody.transform.GetComponent<Rigidbody>().mass = Mathf.Max(rigidbody.mass, 0.0001f);
                    break;
                case "bounce":
                case "bounciness":
                    if (!TryParseFloat(parameter[1].Trim(), rigidbody.variables, out rigidbody.bounce))
                    {
                        Debug.LogError("Couldn't parse bounce parameter of rigidbody: " + parameter[1].Trim());
                        return false;
                    }
                    rigidbody.transform.GetComponent<MeshCollider>().sharedMaterial.bounciness = Mathf.Clamp01(rigidbody.bounce);
                    break;
                case "up":
                case "norm":
                case "normal":
                    if (!TryParseVector(parameter[1].Trim(), rigidbody.variables, out rigidbody.normal))
                    {
                        Debug.LogError("Couldn't parse normal parameter of rigidbody: " + parameter[1].Trim());
                        return false;
                    }
                    rigidbody.transform.up = rigidbody.normal;
                    break;
                case "pos":
                case "position":
                    if (!TryParseVector(parameter[1].Trim(), rigidbody.variables, out rigidbody.position))
                    {
                        Debug.LogError("Couldn't parse position parameter of rigidbody: " + parameter[1].Trim());
                        return false;
                    }
                    rigidbody.transform.localPosition = rigidbody.position;
                    break;
                case "rot":
                case "rotation":
                    if (!TryParseVector(parameter[1].Trim(), rigidbody.variables, out rigidbody.rotation))
                    {
                        Debug.LogError("Couldn't parse rotation parameter of rigidbody: " + parameter[1].Trim());
                        return false;
                    }
                    rigidbody.transform.localEulerAngles = rigidbody.rotation;
                    break;
                case "size":
                case "scale":
                    if (!TryParseVector(parameter[1].Trim(), rigidbody.variables, out rigidbody.scale))
                    {
                        float size;
                        if (!TryParseFloat(parameter[1].Trim(), rigidbody.variables, out size))
                        {
                            Debug.LogError("Couldn't parse scale parameter of rigidbody: " + parameter[1].Trim());
                            return false;
                        }
                        rigidbody.scale = size * Vector3.one;
                    }
                    rigidbody.transform.localScale = rigidbody.scale;
                    break;
                case "f":
                case "force":
                    if (!TryParseVector(parameter[1].Trim(), rigidbody.variables, out rigidbody.force))
                    {
                        Debug.LogError("Couldn't parse force parameter of rigidbody: " + parameter[1].Trim());
                        return false;
                    }
                    rigidbody.transform.GetComponent<Rigidbody>().AddForce(rigidbody.force, ForceMode.Impulse);
                    break;
                case "color":
                    Vector3 colorVector;
                    if (!TryParseVector(parameter[1].Trim(), rigidbody.variables, out colorVector))
                    {
                        Debug.LogError("Couldn't parse color parameter of rigidbody: " + parameter[1].Trim());
                        return false;
                    }
                    rigidbody.color = new Color(colorVector.x, colorVector.y, colorVector.z);
                    rigidbody.transform.GetComponent<Renderer>().sharedMaterial.color = rigidbody.color;
                    break;
            }
        }

        return true;
    }

    public void StartEvaluating() => canEvaluate = true;

    public void Reset()
    {
        canEvaluate = false;

        // Reset the global variables 
        globalVariables.Clear();
        m_Functions.Clear();
        m_Rigidbodies.Clear();

        // Reset time-based variables
        globalVariables.AddTimeBasedVar("t", "time");
        globalVariables.AddTimeBasedVar("time", "time");

        // Reset boolean values
        globalVariables.AddValue("true", true);
        globalVariables.AddValue("false", false);

        // Reset float values
        m_Time = 0;
        globalVariables.AddValue("t", m_Time);
        globalVariables.AddValue("time", m_Time);
        globalVariables.AddValue("g", -9.8f);
        globalVariables.AddValue("dt", Time.deltaTime);
        globalVariables.AddValue("deltatime", Time.deltaTime);
        globalVariables.AddValue("pi", Mathf.PI);
        globalVariables.AddValue("tau", 2 * Mathf.PI);

        // Reset vector values
        globalVariables.AddValue("up", transform.up);
        globalVariables.AddValue("right", transform.right);
        globalVariables.AddValue("left", -transform.right);
        globalVariables.AddValue("down", -transform.up);

        // Reset color values (vector3)
        foreach (KeyValuePair<string, Vector3> entry in DSLHelper.ColorsMap)
            globalVariables.AddValue(entry.Key, entry.Value);
    }

    public bool EvaluateExpression(string expression)
    {
        return EvaluateExpression(expression, ref globalVariables);
    }

    public bool EvaluateExpression(string expression, ref Variables functionVariables)
    {
        switch (DSLHelper.GetExpressionType(expression))
        {
            case DSLExpressionType.loop:
                if (!EvaluateLoop(expression, ref functionVariables))
                {
                    Debug.LogError("Couldn't evaluate loop from " + expression);
                    return false;
                }
                else return true;
            case DSLExpressionType.function:
                if (!EvaluateFunction(expression, ref functionVariables))
                {
                    Debug.LogError("Couldn't evaluate function from " + expression);
                    return false;
                }
                else return true;
            case DSLExpressionType.conditional:
                if (!EvaluateConditional(expression, ref functionVariables))
                {
                    Debug.LogError("Couldn't evaluate conditional from " + expression);
                    return false;
                }
                else return true;
            case DSLExpressionType.variable:
                if (!EvaluateVariable(expression, ref functionVariables))
                {
                    Debug.LogError("Couldn't parse variable from " + expression);
                    return false;
                }
                else return true;
        }
        return false;
    }

    public bool EvaluateLoop(string loop, ref Variables loopVariables)
    {
        string loopName = DSLHelper.GetLoopName(loop);
        string[] loopExpressions = DSLHelper.GetFunctionExpressions(loop);
        string loopValues = DSLHelper.GetLoopValues(loop);
        Vector3 rangeValues;
        if (!TryParseVector(loopValues, loopVariables, out rangeValues))
        {
            Debug.LogError("Couldn't parse loop range from " + loopValues);
            return false;
        }
        Variables savedVariables = new Variables(loopVariables);
        for (float j = rangeValues.x; j <= rangeValues.y; j += rangeValues.z)
        {
            loopVariables.AddValue(loopName, j); // Set the loop variable
            for (int i = 0; i < loopExpressions.Length; i++)
            {
                if (!EvaluateExpression(loopExpressions[i], ref loopVariables))
                {
                    Debug.LogError("Couldn't parse expression in loop: " + loopExpressions[i]);
                    return false;
                }
            }
            loopVariables = new Variables(savedVariables); // Reset the variables used in the loop
        }
        return true;
    }

    public bool EvaluateFunction(string function, ref Variables functionVariables)
    {
        string functionName = DSLHelper.GetFunctionName(function);
        print("Evaluating function " + functionName);
        string functionParameters = DSLHelper.GetFunctionParameters(function);
        if (DSLHelper.RigidbodyNames.Contains(functionName))
        {
            if (!TryParseRigidbody(functionName, functionParameters, ref functionVariables))
            {
                Debug.LogError("Couldn't parse rigidbody");
                return false;
            }
        }
        else if (m_Functions.ContainsKey(functionName))
        {
            if (!m_Functions[functionName].Evaluate(functionParameters, ref functionVariables))
            {
                Debug.LogError("Couldn't parse function " + functionName);
                return false;
            }
        }
        return true;
    }


    public bool EvaluateConditional(string function, ref Variables functionVariables)
    {
        string conditional = DSLHelper.GetConditional(function);

        bool passedConditional = false;
        if (!TryParseBool(conditional, functionVariables, out passedConditional))
        {
            Debug.LogError("Couldn't evaluate conditional " + conditional);
            return false;
        };

        if (passedConditional) // If we pass the conditional check, then run the contained expressions
        {
            foreach (string expression in DSLHelper.GetFunctionExpressions(function))
            {
                if (!EvaluateExpression(expression, ref functionVariables))
                {
                    Debug.LogError("Couldn't evaluate expression in conditional: " + expression);
                    return false;
                }
            }
        }
        return true;
    }

    public bool EvaluateVariable(string variable, ref Variables otherVariables)
    {
        string variableName = DSLHelper.GetVariableName(variable);
        string variableValue = DSLHelper.GetVariableValue(variable);
        // DSLVariableType variableType = DSLHelper.GetVariableType(variableValue, otherVariables, globalVariables);
        return EvaluateVariable(variableName, variableValue, ref otherVariables);
    }

    public bool EvaluateVariable(string variableName, string variableValue, ref Variables otherVariables)
    {
        // Check if this variable is a time-based variable, if so we add this variable to the list of time-based ones
        if (!otherVariables.HasTimeBasedVar(variableName) && otherVariables.IsTimeBased(variableValue))
        {
            otherVariables.AddTimeBasedVar(variableName, variableValue);
        }

        Vector3 vectorValue;
        if (TryParseVector(variableValue, otherVariables, out vectorValue))
        {
            otherVariables.AddValue(variableName, vectorValue);
            return true;
        }

        float floatValue;
        if (TryParseFloat(variableValue, otherVariables, out floatValue))
        {
            otherVariables.AddValue(variableName, floatValue);
            return true;
        }

        bool boolValue;
        if (TryParseBool(variableValue, otherVariables, out boolValue))
        {
            otherVariables.AddValue(variableName, boolValue);
            return true;
        }

        return false;
    }

    /**
        Functions will always follow the same template:
            Func(args)
        We get the function name by looking at text before the first '('
        and then get the args by looking at what is inside of the '()'
    **/
    public bool ParseFunction(string function)
    {
        // Verify function syntax
        if (!VerifyFunction(function))
        {
            Debug.LogError("Can't parse function");
            return false;
        }

        string functionName = DSLHelper.GetFunctionName(function);
        string functionParameters = DSLHelper.GetFunctionParameters(function);
        string[] functionExpressions = DSLHelper.GetFunctionExpressions(function);
        m_Functions.Add(functionName, new Function(functionParameters, functionExpressions, this));
        return true;
    }

    private bool VerifyFunction(string function)
    {
        return true;
    }

    private bool TryParseBool(string input, Variables variables, out bool value)
    {
        input = DSLHelper.TryTrimOutsideParen(input);
        value = false;

        if (globalVariables.HasBoolValue(input))
        {
            globalVariables.GetValue(input, out value);
            return true;
        }
        if (variables.HasBoolValue(input))
        {
            variables.GetValue(input, out value);
            return true;
        }

        bool firstBool, secondBool;
        float firstFloat, secondFloat;
        Vector3 firstVector, secondVector;

        // COMPARISON EXPRESSIONS  ==, >=, <=, <, >
        if (DSLHelper.ContainsAtLevel(input, "=="))
        {
            int splitIndex = DSLHelper.GetIndexOfAtLevel(input, "==");
            if (TryParseBool(input.Substring(0, splitIndex), variables, out firstBool))
            {
                if (!TryParseBool(input.Substring(splitIndex + 2, input.Length - splitIndex - 2), variables, out secondBool)) return false;
                value = firstBool == secondBool;
                return true;
            }
            else if (TryParseFloat(input.Substring(0, splitIndex), variables, out firstFloat))
            {
                if (!TryParseFloat(input.Substring(splitIndex + 2, input.Length - splitIndex - 2), variables, out secondFloat)) return false;
                value = firstFloat == secondFloat;
                return true;
            }
            else if (TryParseVector(input.Substring(0, splitIndex), variables, out firstVector))
            {
                if (!TryParseVector(input.Substring(splitIndex + 2, input.Length - splitIndex - 2), variables, out secondVector)) return false;
                value = firstVector == secondVector;
                return true;
            }
            return false;
        }
        if (DSLHelper.ContainsAtLevel(input, ">="))
        {
            int splitIndex = DSLHelper.GetIndexOfAtLevel(input, ">=");
            if (!TryParseFloat(input.Substring(0, splitIndex), variables, out firstFloat)) return false;
            if (!TryParseFloat(input.Substring(splitIndex + 2, input.Length - splitIndex - 2), variables, out secondFloat)) return false;
            value = firstFloat >= secondFloat;
            return true;
        }
        if (DSLHelper.ContainsAtLevel(input, "<="))
        {
            int splitIndex = DSLHelper.GetIndexOfAtLevel(input, "<=");
            if (!TryParseFloat(input.Substring(0, splitIndex), variables, out firstFloat)) return false;
            if (!TryParseFloat(input.Substring(splitIndex + 2, input.Length - splitIndex - 2), variables, out secondFloat)) return false;
            value = firstFloat <= secondFloat;
            return true;
        }
        if (DSLHelper.ContainsAtLevel(input, ">"))
        {
            int splitIndex = DSLHelper.GetIndexOfAtLevel(input, ">");
            if (!TryParseFloat(input.Substring(0, splitIndex), variables, out firstFloat)) return false;
            if (!TryParseFloat(input.Substring(splitIndex + 1, input.Length - splitIndex - 1), variables, out secondFloat)) return false;
            value = firstFloat > secondFloat;
            return true;
        }
        if (DSLHelper.ContainsAtLevel(input, "<"))
        {
            int splitIndex = DSLHelper.GetIndexOfAtLevel(input, "<");
            if (!TryParseFloat(input.Substring(0, splitIndex), variables, out firstFloat)) return false;
            if (!TryParseFloat(input.Substring(splitIndex + 1, input.Length - splitIndex - 1), variables, out secondFloat)) return false;
            value = firstFloat < secondFloat;
            return true;
        }

        // BOOLEAN EXPRESSIONS: &&, ||, !,
        if (DSLHelper.ContainsAtLevel(input, "&&"))
        {
            int splitIndex = DSLHelper.GetIndexOfAtLevel(input, "&&");
            if (!TryParseBool(input.Substring(0, splitIndex), variables, out firstBool)) return false;
            if (!TryParseBool(input.Substring(splitIndex + 2, input.Length - splitIndex - 2), variables, out secondBool)) return false;
            value = firstBool && secondBool;
            return true;
        }
        if (DSLHelper.ContainsAtLevel(input, "||"))
        {
            int splitIndex = DSLHelper.GetIndexOfAtLevel(input, "||");
            if (!TryParseBool(input.Substring(0, splitIndex), variables, out firstBool)) return false;
            if (!TryParseBool(input.Substring(splitIndex + 2, input.Length - splitIndex - 2), variables, out secondBool)) return false;
            value = firstBool || secondBool;
            return true;
        }
        if (DSLHelper.ContainsAtLevel(input, "!"))
        {
            int splitIndex = DSLHelper.GetIndexOfAtLevel(input, "!");
            if (!TryParseBool(input.Substring(splitIndex + 1), variables, out value)) return false;
            value = !value;
            return true;
        }

        return bool.TryParse(input, out value);
    }

    private bool TryParseFloat(string input, Variables variables, out float value)
    {
        input = DSLHelper.TryTrimOutsideParen(input);
        value = 0;


        if (globalVariables.HasFloatValue(input))
        {
            globalVariables.GetValue(input, out value);
            return true;
        }
        if (variables.HasFloatValue(input))
        {
            variables.GetValue(input, out value);
            return true;
        }

        float firstFloat;
        float secondFloat;
        Vector3 firstVector;
        Vector3 secondVector;
        if (DSLHelper.ContainsAtLevel(input, "+"))
        {
            // print("Running + on " + input);
            int splitIndex = DSLHelper.GetIndexOfAtLevel(input, "+");
            if (!TryParseFloat(input.Substring(splitIndex + 1, input.Length - splitIndex - 1), variables, out secondFloat)) return false;
            if (splitIndex == 0) try { value = 1 * secondFloat; } catch { return false; }
            else
            {
                if (!TryParseFloat(input.Substring(0, splitIndex), variables, out firstFloat)) return false;
                try { value = firstFloat + secondFloat; } catch { return false; }
            }
            return true;
        }
        if (DSLHelper.ContainsAtLevel(input, "-"))
        {
            // print("Running - on " + input);
            int splitIndex = DSLHelper.GetIndexOfAtLevel(input, "-");
            if (!TryParseFloat(input.Substring(splitIndex + 1, input.Length - splitIndex - 1), variables, out secondFloat)) return false;
            if (splitIndex == 0) try { value = -1 * secondFloat; } catch { return false; }
            else
            {
                if (!TryParseFloat(input.Substring(0, splitIndex), variables, out firstFloat)) return false;
                try { value = firstFloat - secondFloat; } catch { return false; }
            }
            return true;
        }
        if (DSLHelper.ContainsAtLevel(input, "*"))
        {
            // print("Running * on " + input);
            int splitIndex = DSLHelper.GetIndexOfAtLevel(input, "*");
            if (!TryParseFloat(input.Substring(0, splitIndex), variables, out firstFloat)) return false;
            if (!TryParseFloat(input.Substring(splitIndex + 1, input.Length - splitIndex - 1), variables, out secondFloat)) return false;
            try { value = firstFloat * secondFloat; } catch { return false; }
            return true;
        }
        if (DSLHelper.ContainsAtLevel(input, "/"))
        {
            // print("Running // on " + input);
            int splitIndex = DSLHelper.GetIndexOfAtLevel(input, "/");
            if (!TryParseFloat(input.Substring(0, splitIndex), variables, out firstFloat)) return false;
            if (!TryParseFloat(input.Substring(splitIndex + 1, input.Length - splitIndex - 1), variables, out secondFloat)) return false;
            try { value = firstFloat / secondFloat; } catch { return false; }
            return true;
        }
        if (DSLHelper.ContainsAtLevel(input, "%"))
        {
            // print("Running % on " + input);
            int splitIndex = DSLHelper.GetIndexOfAtLevel(input, "%");
            if (!TryParseFloat(input.Substring(0, splitIndex), variables, out firstFloat)) return false;
            if (!TryParseFloat(input.Substring(splitIndex + 1, input.Length - splitIndex - 1), variables, out secondFloat)) return false;
            value = firstFloat % secondFloat;
            return true;
        }
        if (DSLHelper.ContainsAtLevel(input, "sin"))
        {
            // print("Running sin() on " + input);
            int startingParen = DSLHelper.GetIndexOfAtLevel(input, "(");
            int endingParen = DSLHelper.GetIndexOfMatchingParen(input, startingParen);
            if (!TryParseFloat(input.Substring(startingParen + 1, endingParen - startingParen - 1).Trim(), variables, out value)) return false;
            value = Mathf.Sin(value);
            return true;
        }
        if (DSLHelper.ContainsAtLevel(input, "cos"))
        {
            // print("Running cos() on " + input);
            int startingParen = DSLHelper.GetIndexOfAtLevel(input, "(");
            int endingParen = DSLHelper.GetIndexOfMatchingParen(input, startingParen);
            if (!TryParseFloat(input.Substring(startingParen + 1, endingParen - startingParen - 1).Trim(), variables, out value)) return false;
            value = Mathf.Cos(value);
            return true;
        }
        if (DSLHelper.ContainsAtLevel(input, "round"))
        {
            // print("Running round() on " + input);
            int startingParen = DSLHelper.GetIndexOfAtLevel(input, "(");
            int endingParen = DSLHelper.GetIndexOfMatchingParen(input, startingParen);
            if (!TryParseFloat(input.Substring(startingParen + 1, endingParen - startingParen - 1).Trim(), variables, out value)) return false;
            value = Mathf.Round(value);
            return true;
        }
        if (DSLHelper.ContainsAtLevel(input, "abs"))
        {
            // print("Running abs() on " + variable);
            int startingParen = DSLHelper.GetIndexOfAtLevel(input, "(");
            int endingParen = DSLHelper.GetIndexOfMatchingParen(input, startingParen);
            if (!TryParseFloat(input.Substring(startingParen + 1, endingParen - startingParen - 1).Trim(), variables, out value)) return false;
            value = Mathf.Abs(value);
            return true;
        }
        if (DSLHelper.ContainsAtLevel(input, "dot"))
        {
            // print("Running dot() on " + input);
            int startingParen = DSLHelper.GetIndexOfAtLevel(input, "(");
            int endingParen = DSLHelper.GetIndexOfMatchingParen(input, startingParen);
            input = input.Substring(startingParen + 1, endingParen - startingParen - 1);
            int splitIndex = DSLHelper.GetIndexOfAtLevel(input, ",");
            if (!TryParseVector(input.Substring(0, splitIndex), variables, out firstVector)) return false;
            if (!TryParseVector(input.Substring(splitIndex + 1, input.Length - splitIndex - 1), variables, out secondVector)) return false;
            value = Vector3.Dot(firstVector, secondVector);
            return true;
        }

        return float.TryParse(input, out value);
    }

    // Vector inputs are of the form [a, b, c]
    private bool TryParseVector(string input, Variables variables, out Vector3 value)
    {
        input = DSLHelper.TryTrimOutsideParen(input);
        value = Vector3.zero;

        if (globalVariables.HasVectorValue(input))
        {
            globalVariables.GetValue(input, out value);
            return true;
        }

        if (variables.HasVectorValue(input))
        {
            variables.GetValue(input, out value);
            return true;
        }

        float firstFloat;
        float secondFloat;
        Vector3 firstVector;
        Vector3 secondVector;
        if (DSLHelper.ContainsAtLevel(input, "+"))
        {
            // print("Running + on " + input);
            int splitIndex = DSLHelper.GetIndexOfAtLevel(input, "+");
            if (!TryParseVector(input.Substring(0, splitIndex), variables, out firstVector)) return false;
            if (!TryParseVector(input.Substring(splitIndex + 1, input.Length - splitIndex - 1), variables, out secondVector)) return false;
            value = firstVector + secondVector;
            return true;
        }
        if (DSLHelper.ContainsAtLevel(input, "-"))
        {
            // print("Running - on " + input);
            int splitIndex = DSLHelper.GetIndexOfAtLevel(input, "-");
            if (!TryParseVector(input.Substring(0, splitIndex), variables, out firstVector)) return false;
            if (!TryParseVector(input.Substring(splitIndex + 1, input.Length - splitIndex - 1), variables, out secondVector)) return false;
            value = firstVector - secondVector;
            return true;
        }
        if (DSLHelper.ContainsAtLevel(input, "*"))
        {
            // print("Running * on " + input);
            int splitIndex = DSLHelper.GetIndexOfAtLevel(input, "*");
            if (TryParseVector(input.Substring(0, splitIndex), variables, out firstVector))
            {
                if (!TryParseFloat(input.Substring(splitIndex + 1, input.Length - splitIndex - 1), variables, out secondFloat)) return false;
                value = firstVector * secondFloat;
                return true;
            }
            else if (TryParseFloat(input.Substring(0, splitIndex), variables, out firstFloat))
            {
                if (!TryParseVector(input.Substring(splitIndex + 1, input.Length - splitIndex - 1), variables, out secondVector)) return false;
                value = firstFloat * secondVector;
                return true;
            }
            return false;
        }
        if (DSLHelper.ContainsAtLevel(input, "/"))
        {
            // print("Running // on " + input);
            int splitIndex = DSLHelper.GetIndexOfAtLevel(input, "/");
            if (!TryParseVector(input.Substring(0, splitIndex), variables, out firstVector)) return false;
            if (!TryParseFloat(input.Substring(splitIndex + 1, input.Length - splitIndex - 1), variables, out secondFloat)) return false;
            value = firstVector / secondFloat;
            return true;
        }
        if (DSLHelper.ContainsAtLevel(input, "cross"))
        {
            // print("Running dot() on " + input);
            int startingParen = DSLHelper.GetIndexOfAtLevel(input, "(");
            int endingParen = DSLHelper.GetIndexOfMatchingParen(input, startingParen);
            input = input.Substring(startingParen + 1, endingParen - startingParen - 1);
            int splitIndex = DSLHelper.GetIndexOfAtLevel(input, ",");
            if (!TryParseVector(input.Substring(0, splitIndex), variables, out firstVector)) return false;
            if (!TryParseVector(input.Substring(splitIndex + 1, input.Length - splitIndex - 1), variables, out secondVector)) return false;
            value = Vector3.Cross(firstVector, secondVector);
            return true;
        }

        // Try manually parsing the vector
        if (input.Length < 3) return false;
        string[] values;
        if (input.Contains(",")) values = input.Substring(1, input.Length - 2).Split(',');
        else values = input.Substring(1, input.Length - 2).Split(' ');
        if (values.Length < 3) return false;

        float x, y, z;
        if (!TryParseFloat(values[0], variables, out x)) { Debug.LogError("Couldn't parse float from " + values[0]); return false; }
        if (!TryParseFloat(values[1], variables, out y)) { Debug.LogError("Couldn't parse float from " + values[1]); return false; }
        if (!TryParseFloat(values[2], variables, out z)) { Debug.LogError("Couldn't parse float from " + values[2]); return false; }
        value = new Vector3(x, y, z);
        return true;
    }

    // Rigidbodies are the primitives of this dsl and can either
    // be a cube, sphere, cylinder, or capsule
    public bool TryParseRigidbody(string rigidType, string rigidParameters, ref Variables rigidVariables)
    {
        RigidbodyProperties properties = new RigidbodyProperties();
        List<string> timeParams = new List<string>();
        // Get the parameters if any are defined
        if (rigidParameters.Length > 0)
        {
            string[] parameters = DSLHelper.SplitAtLevel(rigidParameters, ',');
            properties.parameters = parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (rigidVariables.IsTimeBased(parameters[i])) timeParams.Add(parameters[i]);
                string[] parameter = parameters[i].Split('=');
                switch (parameter[0].Trim())
                {
                    case "lock":
                    case "locked":
                        if (!TryParseBool(parameter[1].Trim(), rigidVariables, out properties.locked))
                        {
                            Debug.LogError("Couldn't parse locked parameter of rigidbody: " + parameter[1].Trim());
                            return false;
                        }
                        break;
                    case "m":
                    case "mass":
                        if (!TryParseFloat(parameter[1].Trim(), rigidVariables, out properties.mass))
                        {
                            Debug.LogError("Couldn't parse mass parameter of rigidbody: " + parameter[1].Trim());
                            return false;
                        }
                        properties.mass = Mathf.Max(properties.mass, 0.0001f);
                        break;
                    case "bounce":
                    case "bounciness":
                        if (!TryParseFloat(parameter[1].Trim(), rigidVariables, out properties.bounce))
                        {
                            Debug.LogError("Couldn't parse bounce parameter of rigidbody: " + parameter[1].Trim());
                            return false;
                        }
                        properties.bounce = Mathf.Clamp01(properties.bounce);
                        break;
                    case "pos":
                    case "position":
                        if (!TryParseVector(parameter[1].Trim(), rigidVariables, out properties.position))
                        {
                            Debug.LogError("Couldn't parse position parameter of rigidbody: " + parameter[1].Trim());
                            return false;
                        }
                        break;
                    case "up":
                    case "norm":
                    case "normal":
                        if (!TryParseVector(parameter[1].Trim(), rigidVariables, out properties.normal))
                        {
                            Debug.LogError("Couldn't parse normal parameter of rigidbody: " + parameter[1].Trim());
                            return false;
                        }
                        break;
                    case "rot":
                    case "rotation":
                        if (!TryParseVector(parameter[1].Trim(), rigidVariables, out properties.rotation))
                        {
                            Debug.LogError("Couldn't parse rotation parameter of rigidbody: " + parameter[1].Trim());
                            return false;
                        }
                        break;
                    case "size":
                    case "scale":
                        if (!TryParseVector(parameter[1].Trim(), rigidVariables, out properties.scale))
                        {
                            float size;
                            if (!TryParseFloat(parameter[1].Trim(), rigidVariables, out size))
                            {
                                Debug.LogError("Couldn't parse scale parameter of rigidbody: " + parameter[1].Trim());
                                return false;
                            }
                            properties.scale = size * Vector3.one;
                        }
                        break;
                    case "force":
                        if (!TryParseVector(parameter[1].Trim(), rigidVariables, out properties.force))
                        {
                            Debug.LogError("Couldn't parse force parameter of rigidbody: " + parameter[1].Trim());
                            return false;
                        }
                        break;
                    case "color":
                        Vector3 colorVector;
                        if (!TryParseVector(parameter[1].Trim(), rigidVariables, out colorVector))
                        {
                            Debug.LogError("Couldn't parse color parameter of rigidbody: " + parameter[1].Trim());
                            return false;
                        }
                        properties.color = new Color(colorVector.x, colorVector.y, colorVector.z);
                        break;
                }
            }
        }
        properties.timeParameters = timeParams.ToArray();

        GameObject newRigidBody;
        switch (rigidType)
        {
            case "cube":
                newRigidBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
            case "sphere":
                newRigidBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                break;
            case "capsule":
                newRigidBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                break;
            case "cylinder":
                newRigidBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                break;
            case "cone":
                newRigidBody = Instantiate(m_ConePrimitive);
                break;
            case "plane":
                newRigidBody = Instantiate(m_PlanePrimitive);
                properties.locked = true;
                break;
            case "pyramid":
                newRigidBody = Instantiate(m_PyramidPrimitive);
                break;
            case "wedge":
                newRigidBody = Instantiate(m_WedgePrimitive);
                break;
            default:
                newRigidBody = new GameObject();
                break;
                // case "custom":
                //     Debug.Log("Creating a custom rigidbody is not yet supported!");
                //     return;
                // default:
                //     Debug.LogError("Can't create rigidbody");
                //     return;
        }

        // Initialize the expected parameters (default values)
        properties.transform = newRigidBody.transform;
        properties.variables = rigidVariables;

        // print("Creating a rigidbody of type " + rigidType + " with the following properties:\n" + properties);

        // All primitives come with a collider default to their primitive type (sphere comes with sphere collider)
        // These primitive colliders don't scale well, so replace them with mesh colliders even though it's less performant
        Collider preexistingCollider = newRigidBody.GetComponent<Collider>();
        if (preexistingCollider) DestroyImmediate(preexistingCollider);

        MeshCollider meshCollider = newRigidBody.AddComponent<MeshCollider>();
        if (!properties.locked) meshCollider.convex = true;
        meshCollider.sharedMaterial = new PhysicMaterial();
        meshCollider.sharedMaterial.staticFriction = 0;
        meshCollider.sharedMaterial.dynamicFriction = 0;
        meshCollider.sharedMaterial.bounciness = properties.bounce;
        meshCollider.sharedMaterial.bounceCombine = PhysicMaterialCombine.Maximum;

        // Apply transform properties to the new gameobject
        newRigidBody.transform.parent = transform;
        newRigidBody.transform.position = properties.position;
        newRigidBody.transform.rotation = Quaternion.Euler(properties.rotation);
        newRigidBody.transform.up = properties.normal;
        newRigidBody.transform.localScale = properties.scale;

        // Apply renderer properties
        Renderer renderer = newRigidBody.GetComponent<Renderer>();
        Material mat = new Material(renderer.sharedMaterial);
        mat.color = properties.color;
        renderer.sharedMaterial = mat;

        if (!properties.locked)
        {
            // Apply the Rigidbody (physics) properties
            Rigidbody rigid = newRigidBody.AddComponent<Rigidbody>();
            // rigid.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rigid.mass = properties.mass;
            rigid.AddForce(properties.force, ForceMode.Impulse);
            // rigid.isKinematic = true;
            // rigid.constraints = RigidbodyConstraints.FreezeAll;
        }

        m_Rigidbodies.Add(properties);

        return true;
    }
}

public class Variables
{
    public Dictionary<string, bool> boolVars;
    public Dictionary<string, float> floatVars;
    public Dictionary<string, Vector3> vectorVars;
    public Dictionary<string, string> timeBasedVars;

    public Variables()
    {
        boolVars = new Dictionary<string, bool>();
        floatVars = new Dictionary<string, float>();
        vectorVars = new Dictionary<string, Vector3>();
        timeBasedVars = new Dictionary<string, string>();
        AddTimeBasedVar("t", "time");
        AddTimeBasedVar("time", "time");
    }

    public Variables(Variables _variables)
    {
        boolVars = new Dictionary<string, bool>(_variables.boolVars);
        floatVars = new Dictionary<string, float>(_variables.floatVars);
        vectorVars = new Dictionary<string, Vector3>(_variables.vectorVars);
        timeBasedVars = new Dictionary<string, string>(_variables.timeBasedVars);
    }

    public void GetValue(string var, out bool value) { value = boolVars[var]; }
    public void GetValue(string var, out float value) { value = floatVars[var]; }
    public void GetValue(string var, out Vector3 value) { value = vectorVars[var]; }

    public void AddValue(string var, bool value) { boolVars[var] = value; }
    public void AddValue(string var, float value) { floatVars[var] = value; }
    public void AddValue(string var, Vector3 value) { vectorVars[var] = value; }

    public bool HasBoolValue(string var) { return boolVars.ContainsKey(var); }
    public bool HasFloatValue(string var) { return floatVars.ContainsKey(var); }
    public bool HasVectorValue(string var) { return vectorVars.ContainsKey(var); }

    public bool HasTimeBasedVar(string varName) { return timeBasedVars.ContainsKey(varName); }
    public void AddTimeBasedVar(string varName, string varValue) { timeBasedVars[varName] = varValue; }
    public bool IsTimeBased(string variable)
    {
        foreach (KeyValuePair<string, string> var in timeBasedVars)
            if (DSLHelper.ContainsVariable(variable, var.Key)) return true;
        return false;
    }

    public void Clear()
    {
        boolVars.Clear();
        floatVars.Clear();
        vectorVars.Clear();
        timeBasedVars.Clear();
    }
}

public class RigidbodyProperties
{
    public Variables variables;
    public string[] parameters;
    public string[] timeParameters;
    public Transform transform;
    public bool locked = false;
    public float mass = 1;
    public float bounce = 0;
    public Vector3 position = Vector3.zero;
    public Vector3 rotation = Vector3.zero;
    public Vector3 normal = Vector3.up;
    public Vector3 scale = Vector3.one;
    public Vector3 force = Vector3.zero;
    public Color color = Color.white;

    public override string ToString()
    {
        return "Rigidbody Properties {\n\tTransform: " + transform.name + "\n\tLocked: " + locked +
        "\n\tMass: " + mass + "\n\tBounciness: " + bounce + "\n\tPosition: " + position +
        "\n\tRotation: " + rotation + "\n\tScale: " + scale +
        "\n\tForce: " + force + "\n\tColor: " + color + "\n}";
    }
}
