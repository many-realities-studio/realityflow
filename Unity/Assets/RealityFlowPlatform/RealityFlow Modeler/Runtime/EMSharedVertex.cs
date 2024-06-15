using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Stores the indicies of vertices that are unique, but share the same position as
/// others within the positions array.
/// </summary>
public class EMSharedVertex
{
    public int[] vertices;

    public EMSharedVertex(int[] vertices)
    {
        this.vertices = vertices;
    }

    public int Length
    {
        get { return vertices.Length; }
    }

    public void Add(int toAdd)
    {
        System.Array.Resize<int>(ref vertices, vertices.Length + 1);
        vertices[vertices.Length - 1] = toAdd;
    }

    public static EMSharedVertex[] GetSharedVertices(Vector3[] vertices)
    {
        Dictionary<Vector3, List<int>> dict = new Dictionary<Vector3, List<int>>();

        for (int i = 0; i < vertices.Length; i++)
        {
            List<int> v = new List<int>();
            if(dict.ContainsKey(vertices[i]))
            {
                dict[vertices[i]].Add(i);
            }
            else
            {
                dict.Add(vertices[i], new List<int>() { i });
            }
        }

        EMSharedVertex[] rv = new EMSharedVertex[dict.Count];

        int index = 0;
        foreach(KeyValuePair<Vector3, List<int>> k in dict)
        {
            rv[index] = new EMSharedVertex(k.Value.ToArray());
            index++;
        }

        return rv;
    }

    /// <summary>
    /// Creates a dictionary. The key is an index in the positions array and the value is the index in
    /// the SharedVertices array.
    /// </summary>
    public static Dictionary<int, int> CreateSharedVertexDict(EMSharedVertex[] sharedVerts)
    {
        Dictionary<int, int> dict = new Dictionary<int, int>();

        for(int i = 0; i < sharedVerts.Length; i++)
        {
            for(int j = 0; j < sharedVerts[i].Length; j++)
            {
                if(!dict.ContainsKey(sharedVerts[i].vertices[j]))
                    dict.Add(sharedVerts[i].vertices[j], i);
            }
        }

        return dict;
    }

    public static int GetSharedVertexIndexFromPosition(EditableMesh em, Vector3 pos)
    {
        for (int i = 0; i < em.sharedVertices.Length; i++)
        {
            int index = em.sharedVertices[i].vertices[0];
            if (em.positions[index] == pos)
                return i;
        }

        return -1;
    }

    /*
    public static EMSharedVertex[] RemoveVertices(Dictionary<int, int> dict, int[] remove)
    {
        var toRemove = new List<int>(remove);
        toRemove.Sort();

        // Mark vertices for removal
        foreach (int i in toRemove)
            dict[i] = -1;

        //EMSharedVertex[] sharedVertices = dict.Where(x => x.Value > -1);
    }
    */
}
