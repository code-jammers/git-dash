using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitDash.Services;

public class VariableSubstitution
{
    public string Substitute(string pathLines, Dictionary<string, string> nameValues)
    {
        var substituted = "";
        foreach (var line in pathLines.Split(Environment.NewLine))
        {
            string substitutedLine = line;
            foreach (var (k, v) in nameValues)
            {
                if (line.Contains(v))
                {
                    substitutedLine = substitutedLine.Replace(v, "{" + k + "}");
                    break;
                }
            }
            substituted += (substituted.Length == 0) ? substitutedLine : Environment.NewLine + substitutedLine;
        }
        return substituted;
    }
}
