using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EMEdge : System.IEquatable<EMEdge>
{
    // indicies to position in the shared vertex array
    public int A;

    public int B;

    public EMEdge(int a, int b)
    {
        A = a;
        B = b;
    }

    public EMEdge(EMEdge other)
    {
        A = other.A;
        B = other.B;
    }

    public bool Equals(EMEdge other)
    {
        return (A == other.A && B == other.B ) || (A == other.B && B == other.A);
    }

    public override bool Equals(object obj)
    {
        return obj is  EMEdge && Equals((EMEdge) obj);
    }

    public override int GetHashCode()
    {
        if(A < B)
            return 31 * A + 19 * B;
        else
            return 31 * B + 19 * A;
    }
}
