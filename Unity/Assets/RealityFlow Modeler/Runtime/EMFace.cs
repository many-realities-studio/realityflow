using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class Face stores data about the faces of a mesh
/// </summary>
public class EMFace
{
    [SerializeField]
    public int[] indicies { get; private set; }

    private int[] uniqueIndicies;

    private EMEdge[] edges;
    private EMEdge[] sharedEdges;

    public EMFace(int[] indices)
    {
        if (indices == null)
            throw new ArgumentNullException(nameof(indices));

        indicies = new int[indices.Length];

        for (int i = 0; i < indices.Length; i++)
        {
            this.indicies[i] = indices[i];
        }

        uniqueIndicies = null;
    }


    public int[] GetUniqueIndicies()
    {
        if(uniqueIndicies == null)
        {
            uniqueIndicies = indicies.Distinct().ToArray();
        }

        //Debug.Log(uniqueIndicies[0] + " " + uniqueIndicies[1] + " "  + uniqueIndicies[2] + " " + uniqueIndicies[3]);

        return uniqueIndicies;
    }

    public EMEdge[] GetExteriorEdges()
    {
        if (edges == null)
        {
            List<EMEdge> temp = new List<EMEdge>();
            HashSet<EMEdge> hs = new HashSet<EMEdge>();
            for (int i = 0; i < indicies.Length; i += 3)
            {
                EMEdge a = new EMEdge(indicies[i + 0], indicies[i + 1]);
                EMEdge b = new EMEdge(indicies[i + 1], indicies[i + 2]);
                EMEdge c = new EMEdge(indicies[i + 2], indicies[i + 0]);

                if (!hs.Add(a))
                    temp.Add(a);
                if (!hs.Add(b))
                    temp.Add(b);
                if (!hs.Add(c))
                    temp.Add(c);
            }
            hs.ExceptWith(temp);
            edges = hs.ToArray();
        }

        return edges;
    }

    public EMEdge[] GetExteriorEdgesWithSharedIndicies(EditableMesh mesh)
    {
        if (sharedEdges == null)
        {
           sharedEdges = GetExteriorEdges();
            for (int i = 0; i < sharedEdges.Length; i++)
            {
                EMEdge e = sharedEdges[i];
                e.A = mesh.sharedVertexLookup[e.A];
                e.B = mesh.sharedVertexLookup[e.B];
                sharedEdges[i] = e;
            }
        }

        return sharedEdges;
    }

    public int[] GetReducedIndicies()
    {
        GetUniqueIndicies();
        int[] pos = new int[indicies.Length];
        Dictionary<int, int> lookUp = new Dictionary<int, int>();

        int index = 0;
        for(int i = 0; i < indicies.Length; i++)
        {
            if(lookUp.ContainsKey(indicies[i]))
            {
                pos[i] = lookUp[indicies[i]];
            }
            else
            {
                lookUp.Add(indicies[i], index++);
                pos[i] = lookUp[indicies[i]];
            }
        }

        return pos;
    }

    public void DisplayIndicies()
    {
        Debug.Log(indicies[0] + " " + indicies[1] + " " + indicies[2]);
    }
}
