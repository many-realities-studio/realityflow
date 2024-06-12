using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.MixedReality.Toolkit.Data;
using RealityFlow.NodeGraph;
using UnityEngine;

public class GraphViewSource : DataSourceBase
{
    Graph graph;

    public Graph Graph { get => graph; set => graph = value; }

    public override object GetValueInternal(string resolvedKeyPath)
    {
        if (graph is null)
            return null;

        if (!MatchAndSplit(@"graph", ref resolvedKeyPath, out _))
            return null;

        if (MatchAndSplit(@"\.nodes\[(.*)\]", ref resolvedKeyPath, out Match indexMatch))
        {
            string indexString = indexMatch.Captures[0].Value;
            if (!MatchAndSplit(@"[0-9]+", ref indexString, out Match indexMatched))
            {
                Debug.LogWarning("encountered graph data binding path with non-integer index into nodes");
                return null;
            }

            int index = int.Parse(indexMatched.Value);
            NodeIndex nodeIndex = new(new(index));
            Node node = graph.GetNode(nodeIndex);

            if (MatchAndSplit(@"\.name", ref resolvedKeyPath, out _))
            {
                return node.Definition.Name;
            }

            return null;
        }

        return null;
    }

    readonly Dictionary<string, Regex> regexCache = new();
    bool MatchAndSplit(string regex, ref string path, out Match matched)
    {
        if (!regexCache.TryGetValue(regex, out Regex matcher))
        {
            matcher = new(regex);
            regexCache.Add(regex, matcher);
        }

        Match match = matcher.Match(path);
        if (!match.Success)
        {
            matched = Match.Empty;
            return false;
        }

        path = path[match.Length..];
        matched = match;
        return true;
    }

    public override void SetValueInternal(string resolvedKeyPath, object newValue)
    {
        throw new NotImplementedException();
        // base.SetValueInternal(resolvedKeyPath, newValue);
    }
}
