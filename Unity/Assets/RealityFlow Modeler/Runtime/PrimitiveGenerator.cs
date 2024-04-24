using UnityEngine;
using System;

public enum ShapeType
{
    NoShape,
    Plane,
    Cube,
    Wedge,
    Cylinder,
    Cone,
    Sphere,
    Torus,
    Pipe
};

public struct PrimitiveCreationParams
{
    public ShapeType shapeType;
}

/// <summary>
/// Class PrimitiveGenerator provides methods to generate primitives
/// </summary>
public static class PrimitiveGenerator
{
    static Vector3[] cubeVertices = new Vector3[]
    {
        // Bottom
        new Vector3(-.5f, -.5f, .5f),   // 0
        new Vector3(.5f, -.5f, .5f),    // 1
        new Vector3(.5f, -.5f, -.5f),   // 2
        new Vector3(-.5f, -.5f, -.5f),  // 3

        // Top
        new Vector3(-.5f, .5f, .5f),    // 4
        new Vector3(.5f, .5f, .5f),     // 5
        new Vector3(.5f, .5f, -.5f),    // 6
        new Vector3(-.5f, .5f, -.5f)    // 7
    };

    static int[] cubeFaces = new int[]
    {
        3, 2, 0, 1, 0, 1, 4, 5, 1, 2, 5, 6, 2, 3, 6, 7, 3, 0, 7, 4, 4, 5, 7, 6
    };

    public static PrimitiveData CreatePrimitive(ShapeType type)
    {
        switch (type)
        {
            case ShapeType.Plane:
                return CreatePlane(new Vector3(0.1f, 0.1f, 0.1f));
            case ShapeType.Cube:
                return CreateCube(new Vector3(0.1f, 0.1f, 0.1f));
            case ShapeType.Wedge:
                return CreateWedge(new Vector3(0.1f, 0.1f, 0.1f));
            case ShapeType.Cylinder:
                return CreateCylinder(16, 1, 0.1f);
            case ShapeType.Cone:
                return CreateCone(16, 0.1f);
            case ShapeType.Sphere:
                return CreateUVSphere(8, 8, 0.1f);
            case ShapeType.Torus:
                return CreateTorus(8, 8, 0.2f, 0.1f);
            case ShapeType.Pipe:
                return CreatePipe(8, 1, 0.1f, 0.1f, 0.05f);
            default:
                Debug.LogError("Invalid ShapeType input!");
                break;
        }

        return CreateCube(Vector3.one);
    }

    public static PrimitiveData CreatePlane(Vector3 size)
    {
        Vector3[] points = new Vector3[4];

        points[0] = new Vector3(-.5f, 0, .5f);
        points[1] = new Vector3(.5f, 0, .5f);
        points[2] = new Vector3(-.5f, 0, -.5f);
        points[3] = new Vector3(.5f, 0, -.5f);

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = Vector3.Scale(points[i], size);
        }

        EMFace[] f = new EMFace[points.Length / 4];

        for (int i = 0; i < points.Length; i += 4)
        {
            f[i/4] = new EMFace(new int[6]
            {
                i + 0, i + 1, i + 2,
                i + 1, i + 3, i + 2
            });
        }

        PrimitiveData data = new PrimitiveData(ShapeType.Plane, points, f);

        return data;
    }

    public static PrimitiveData CreateCube(Vector3 size)
    {
        Vector3[] points = new Vector3[cubeFaces.Length];

        for (int i = 0; i < cubeFaces.Length; i++)
        {
            points[i] = Vector3.Scale(cubeVertices[cubeFaces[i]], size);
        }

        EMFace[] f = new EMFace[points.Length / 4];
        for (int i = 0; i < points.Length; i += 4)
        {
            f[i/4] = new EMFace(new int[6]
            {
                i + 0, i + 1, i + 2,
                i + 1, i + 3, i + 2
            });
        }

        PrimitiveData data = new PrimitiveData(ShapeType.Cube, points, f);
        return data;
    }

    public static PrimitiveData CreateWedge(Vector3 size)
    {
        Vector3[] points = new Vector3[6];

        points[0] = new Vector3(-.5f, 0, .5f);
        points[1] = new Vector3(.5f, 0, .5f);
        points[2] = new Vector3(-.5f, 0, -.5f);
        points[3] = new Vector3(.5f, 0, -.5f);
        points[4] = new Vector3(-0.5f, 0.5f, 0.5f);
        points[5] = new Vector3(-0.5f, 0.5f, -0.5f);

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = Vector3.Scale(points[i], size);
        }

        Vector3[] verts = new Vector3[18]
        {
            points[0], points[1], points[2], points[3],   // Bottom
            points[4], points[5], points[0], points[2],   // Front
            points[4], points[1], points[5], points[3],   // Rear
            points[5], points[3], points[2],              // Side 1
            points[4], points[0], points[1]               // Side 2
        };

        EMFace[] f = new EMFace[5];

        f[0] = new EMFace(new int[6] { 0, 2, 1, 1, 2, 3 });
        f[1] = new EMFace(new int[6] { 4, 5, 6, 5, 7, 6 });
        f[2] = new EMFace(new int[6] { 8, 9, 10, 9, 11, 10 });
        f[3] = new EMFace(new int[3] { 12, 13, 14 });
        f[4] = new EMFace(new int[3] { 15, 16, 17 });
        
        PrimitiveData data = new PrimitiveData(ShapeType.Wedge, verts, f);
        return data;
    }

    public static PrimitiveData CreateCylinder(int numSides, int heightCuts, float radius)
    {
        if (numSides < 3)
            numSides = 3;

        if (numSides > 64)
            numSides = 64;

        if (heightCuts < 0)
            heightCuts = 0;

        if (heightCuts > 32)
            heightCuts = 32;

        Vector2[] circle = GetCirclePoints(numSides, radius);

        int numQuadFaces = numSides * (heightCuts + 1);
        Vector3[] vertices = new Vector3[(numQuadFaces * 4) + (numSides * 6)];
        EMFace[] faces = new EMFace[(numSides * 2) + (numSides * (heightCuts + 1))];

        int off = 0;

        float lower = -radius;
        float upper = radius;
        float step = (upper - lower) / (heightCuts + 1);

        for (int i = 0; i < heightCuts + 1; i++)
        {
            float bottom = lower + (i * step);
            float top = lower + ((i + 1) * step);

            for (int j = 0; j < numSides; j++)
            {
                vertices[off] = new Vector3(circle[j].x, bottom, circle[j].y);
                vertices[off + 1] = new Vector3(circle[j].x, top, circle[j].y);

                if (j != numSides - 1)
                {
                    // Construct quad face from (n, n + 1)
                    vertices[off + 2] = new Vector3(circle[j + 1].x, bottom, circle[j + 1].y);
                    vertices[off + 3] = new Vector3(circle[j + 1].x, top, circle[j + 1].y);
                }
                else
                {
                    // Construct quad face with verts (n-1, 0)
                    vertices[off + 2] = new Vector3(circle[0].x, bottom, circle[0].y);
                    vertices[off + 3] = new Vector3(circle[0].x, top, circle[0].y);
                }

                off += 4;
            }
        }

        int face = 0;
        for (int i = 0; i < heightCuts + 1; i++)
        {
            for (int j = 0; j < numSides * 4; j += 4)
            {
                int index = (i * (numSides * 4)) + j;
                faces[face++] = new EMFace(new int[6]
                {
                index, index + 1, index + 2,
                index + 1, index + 3, index + 2
                });
            }
        }

        // Quad vertices have already been set, start from ending point of that
        int f = numQuadFaces * 4;
        int face_index = numQuadFaces;

        // Wind top and bottom faces
        for (int i = 0; i < numSides; i++)
        {
            // Bottom face
            vertices[f] = new Vector3(circle[i].x, lower, circle[i].y);
            vertices[f + 1] = new Vector3(0.0f, lower, 0.0f);

            if (i != numSides - 1)
            {
                vertices[f + 2] = new Vector3(circle[i + 1].x, lower, circle[i + 1].y);
            }
            else
            {
                vertices[f + 2] = new Vector3(circle[0].x, lower, circle[0].y);
            }

            faces[face_index + i] = new EMFace(new int[3] { f + 2, f + 1, f });
            f += 3;

            // Top
            vertices[f + 0] = new Vector3(circle[i].x, upper, circle[i].y);
            vertices[f + 1] = new Vector3(0f, upper, 0f);

            if (i != numSides - 1)
            {
                vertices[f + 2] = new Vector3(circle[i + 1].x, upper, circle[i + 1].y);
            }
            else
            {
                vertices[f + 2] = new Vector3(circle[0].x, upper, circle[0].y);
            }

            faces[face_index + i + numSides] = new EMFace(new int[3] { f, f + 1, f + 2 });
            f += 3;
        }

        PrimitiveData data = new PrimitiveData(ShapeType.Cylinder, vertices, faces);

        return data;
    }

    public static PrimitiveData CreateCone(int numSides, float radius)
    {
        if (numSides < 3)
            numSides = 3;

        if (numSides > 64)
            numSides = 64;

        Vector2[] circle = GetCirclePoints(numSides, radius);

        Vector3[] vertices = new Vector3[(numSides * 6)];
        EMFace[] faces = new EMFace[numSides * 2];

        int bottom = 0;
        float top = radius;

        int index = 0;
        int face_index = 0;

        for (int i = 0; i < numSides; i++)
        {
            // Bottom face
            vertices[index] = new Vector3(circle[i].x, bottom, circle[i].y);
            vertices[index + 1] = Vector3.zero;

            if (i != numSides - 1)
            {
                vertices[index + 2] = new Vector3(circle[i + 1].x, bottom, circle[i + 1].y);
            }
            else
            {
                vertices[index + 2] = new Vector3(circle[0].x, bottom, circle[0].y);
            }

            faces[face_index + i] = new EMFace(new int[3] { index + 2, index + 1, index });
            index += 3;

            // Top 
            vertices[index] = new Vector3(circle[i].x, bottom, circle[i].y);
            vertices[index + 1] = new Vector3(0f, top, 0f); ;

            if (i != numSides - 1)
            {
                vertices[index + 2] = new Vector3(circle[i + 1].x, bottom, circle[i + 1].y);
            }
            else
            {
                vertices[index + 2] = new Vector3(circle[0].x, bottom, circle[0].y);
            }

            faces[face_index + i + numSides] = new EMFace(new int[3] { index, index + 1, index + 2 });
            index += 3;
        }

        PrimitiveData data = new PrimitiveData(ShapeType.Cone, vertices, faces);

        return data;
    }

    /// <summary>
    /// Generates a UV Sphere
    /// </summary>
    /// <param name="segments"> Number of cuts running from one pole to another </param>
    /// <param name="rings"> Number of cuts running perpendicular to the segments (like Earth's equator) </param>
    /// <param name="radius"> Distance from center to surface </param>
    /// <returns></returns>
    public static PrimitiveData CreateUVSphere(int segments, int rings, float radius)
    {
        Vector3[] vertices = new Vector3[(segments * (rings - 2) * 4) + (segments * 6)];
        EMFace[] faces = new EMFace[segments * rings];

        int numVertices = segments * (rings - 1) + 2;
        Vector3[] sphereVertices = new Vector3[numVertices];

        double deltaAngle = Math.PI / segments;
        double deltaAngle2 = 2 * Math.PI / rings;

        int off = 0;

        // Calculate Vertices of sphere
        for (int i = 1; i < rings; i++)
        {
            double phi = deltaAngle * i;
            double xz = Math.Sin(phi) * radius;
            for (int j = 0; j < segments; j++)
            {
                double theta = j * deltaAngle2;
                double x = xz * Math.Cos(theta);
                double y = Math.Cos(phi) * radius;
                double z = xz * Math.Sin(theta);
                sphereVertices[i + j + off] = new Vector3((float)x, (float)y, (float)z);
            }

            off += segments -1;
        }

        sphereVertices[0] = new Vector3(0, radius, 0);
        sphereVertices[numVertices - 1] = new Vector3(0, -radius, 0);

        off = 0;
        int numQuadFaces = segments * (rings - 2);
        int f = 0;
        int face_index = numQuadFaces;

        // Calculate and wind quad faces
        for (int i = 0; i < segments - 2; i++)
        {
            for (int j = 0; j < rings; j++)
            {
                int index = (segments * i) + j + 1;
                vertices[off] = sphereVertices[index];
                vertices[off + 1] = sphereVertices[index + segments];
                if (j != rings - 1)
                {
                    vertices[off + 2] = sphereVertices[index + 1];
                    vertices[off + 3] = sphereVertices[index + segments + 1];
                }
                else
                {
                    vertices[off + 2] = sphereVertices[index - segments + 1];
                    vertices[off + 3] = sphereVertices[index + 1];
                }

                faces[f] = new EMFace(new int[6]
                {
                    off, off + 3, off + 1,
                    off, off + 2, off + 3
                });

                f++;
                off += 4;
            }
        }


        // Calculate triangular cap vertices
        for (int i = 0; i < segments; i++)
        {
            // Bottom
            vertices[off] = sphereVertices[i + 1];
            vertices[off + 1] = sphereVertices[0];

            if (i != segments - 1)
            {
                vertices[off + 2] = sphereVertices[i + 2];
            }
            else
            {
                vertices[off + 2] = sphereVertices[1];
            }

            faces[face_index + i] = new EMFace(new int[3] { off, off + 1, off + 2 });
            off += 3;

            // Top
            vertices[off] = sphereVertices[numVertices - segments + i - 1];
            vertices[off + 1] = sphereVertices[numVertices - 1];

            if (i != segments - 1)
            {
                vertices[off + 2] = sphereVertices[numVertices - segments + i];
            }
            else
            {
                vertices[off + 2] = sphereVertices[numVertices - segments - 1];
            }

            faces[face_index + i + segments] = new EMFace(new int[3] { off + 2, off + 1, off });
            off += 3;
        }

         PrimitiveData data = new PrimitiveData(ShapeType.Sphere, vertices, faces);

        return data;
    }

    /// <summary>
    /// Generators a torus
    /// </summary>
    /// <param name="majorSegments"> Number of segments on the main ring</param>
    /// <param name="minorSegments"> Number of segments on each circular segment</param>
    /// <param name="majorRadius"> Distance from center to center of cross section</param>
    /// <param name="minorRadius"> Radius of torus cross section</param>
    /// <returns></returns>
    public static PrimitiveData CreateTorus(int majorSegments, int minorSegments, float majorRadius, float minorRadius)
    {
        Vector3[] vertices = new Vector3[0];
        EMFace[] faces = new EMFace[majorSegments * minorSegments];

        // Generate cross section circle points
        Vector3[] circle = GetCirclePoints3D(minorSegments, minorRadius);

        RotateCirclePointsX(ref circle);

        // Offset the points of the circle so it's at the major radius
        for (int i = 0; i < minorSegments; i++)
        {
            circle[i].x += majorRadius;
        }

        vertices = RotateCircularCrossSection(circle, majorSegments);

        // Wind faces into quads
        // each segment along the major has minorSegment vertices
        int face_index = 0;
        int zero, one, two, three;
        for(int i = 0; i < vertices.Length - minorSegments; i++)
        {
            zero = i;
            two = i + minorSegments;
            
            if((i + 1) % minorSegments == 0)
            {
                one = i - minorSegments + 1;
                three = one + minorSegments;
            }
            else
            {
                one = i + 1;
                three = two + 1;
            }

            faces[face_index++] = new EMFace(new int[6]
            {
                zero, one, two,
                one, three, two
            });
        }

        // Wind the last segment
        int temp = 0;
        for(int i = vertices.Length - minorSegments; i < vertices.Length; i++)
        {
            zero = i;
            two = temp++;

            if(i == vertices.Length - 1)
            {
                one = i - minorSegments + 1;
                three = temp - minorSegments;
            }
            else
            {
                one = i + 1;
                three = two + 1;
            }

            faces[face_index++] = new EMFace(new int[6]
            {
                zero, one, two,
                one, three, two
            });
        }

        PrimitiveData data = new PrimitiveData(ShapeType.Torus, vertices, faces);

        return data;
    }

    public static PrimitiveData CreatePipe(int numSides, int heightCuts, float height, float radius, float thickness)
    {
        int numQuadFaces = (numSides * 2) + (2 * numSides * (heightCuts + 1)); // (num top and bottom faces + num of side faces)
        int numVertices = numQuadFaces * 6; // Every face is a quad

        Vector2[] innerCircle = GetCirclePoints(numSides, radius);
        Vector2[] outerCircle = GetCirclePoints(numSides, radius + thickness);

        float lower = height * -0.5f;
        float upper = height * 0.5f;
        float step = height / (heightCuts + 1);

        EMFace[] f = new EMFace[numQuadFaces];
        Vector3[] points = new Vector3[numVertices];

        int zero, one, two, three;
        int index = 0;

        int f_index = 0;
        int n = 0;
        // Wind the exterior and interior side faces
        for(int i = 0; i < heightCuts + 1; i++)
        {
            float y1 = lower + (i * step);
            float y2 = lower + ((i + 1) * step);
            for(int j = 0; j < numSides; j++)
            {
                n = index + 4;
                // Inner
                points[index] = new Vector3(innerCircle[j].x, y1, innerCircle[j].y);
                points[index + 1] = new Vector3(innerCircle[j].x, y2, innerCircle[j].y);

                // Outer
                points[n] = new Vector3(outerCircle[j].x, y1, outerCircle[j].y);
                points[n + 1] = new Vector3(outerCircle[j].x, y2, outerCircle[j].y);

                if(j !=  numSides - 1)
                {
                    points[index + 2] = new Vector3(innerCircle[j + 1].x, y1, innerCircle[j + 1].y);
                    points[index + 3] = new Vector3(innerCircle[j + 1].x, y2, innerCircle[j + 1].y);

                    points[n + 2] = new Vector3(outerCircle[j + 1].x, y1, outerCircle[j + 1].y);
                    points[n + 3] = new Vector3(outerCircle[j + 1].x, y2, outerCircle[j + 1].y);

                }
                else
                {
                    points[index + 2] = new Vector3(innerCircle[0].x, y1, innerCircle[0].y);
                    points[index + 3] = new Vector3(innerCircle[0].x, y2, innerCircle[0].y);

                    points[n + 2] = new Vector3(outerCircle[0].x, y1, outerCircle[0].y);
                    points[n + 3] = new Vector3(outerCircle[0].x, y2, outerCircle[0].y);
                }

                // Inner
                f[f_index++] = new EMFace(new int[6]{
                    index, index + 2, index + 1,
                    index + 1, index + 2, index + 3
                });

                // Outer
                f[f_index++] = new EMFace(new int[6]{
                    n, n + 1, n + 2,
                    n + 1, n + 3, n + 2
                });

                index += 8;
            }
        }

        // Build the top and bottom faces
        for(int i = 0; i < numSides; i ++)
        {
            n = index + 4;
            // Bottom faces
            points[index] = new Vector3(innerCircle[i].x, lower, innerCircle[i].y);
            points[index + 1] = new Vector3(outerCircle[i].x, lower, outerCircle[i].y);

            // Top faces
            points[n] = new Vector3(innerCircle[i].x, upper, innerCircle[i].y);
            points[n + 1] = new Vector3(outerCircle[i].x, upper, outerCircle[i].y);

            if(i != numSides - 1)
            {
                points[index + 2] = new Vector3(innerCircle[i + 1].x, lower, innerCircle[i + 1].y);
                points[index + 3] = new Vector3(outerCircle[i + 1].x, lower, outerCircle[i + 1].y);

                points[n + 2] = new Vector3(innerCircle[i + 1].x, upper, innerCircle[i + 1].y);
                points[n + 3] = new Vector3(outerCircle[i + 1].x, upper, outerCircle[i + 1].y);
            }
            else
            {
                points[index + 2] = new Vector3(innerCircle[0].x, lower, innerCircle[0].y);
                points[index + 3] = new Vector3(outerCircle[0].x, lower, outerCircle[0].y);

                points[n + 2] = new Vector3(innerCircle[0].x, upper, innerCircle[0].y);
                points[n + 3] = new Vector3(outerCircle[0].x, upper, outerCircle[0].y);
            }

            // Bottom face, reverse the winding order
            f[f_index++] = new EMFace(new int[6] {
                index, index + 1, index + 3,
                index, index + 3, index + 2
            });

            // Top face
            f[f_index++] = new EMFace(new int[6] {
                n, n + 3, n + 1,
                n, n + 2, n + 3
            });

            index += 8;
        }

        PrimitiveData data = new PrimitiveData(ShapeType.Pipe, points, f);
        return data;
    }

    /// <summary>
    /// Generates a set of Vector3's on the circumference of a circle
    /// </summary>
    public static Vector2[] GetCirclePoints(int numSides, float radius)
    {
        Vector2[] points = new Vector2[numSides];

        float rotationAmountDegrees = 360 / numSides;

        for (int i = 0; i < numSides; i++)
        {
            float angle = rotationAmountDegrees * i * Mathf.Deg2Rad;

            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            points[i] = new Vector2(x, z);
        }


        return points;
    }

    public static Vector3[] GetCirclePoints3D(int numSides, float radius)
    {
        Vector3[] points = new Vector3[numSides];

        float rotationAmountDegrees = 360 / numSides;

        for (int i = 0; i < numSides; i++)
        {
            float angle = rotationAmountDegrees * i * Mathf.Deg2Rad;

            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            points[i] = new Vector3(x, 0.0f, z);
        }


        return points;
    }

    /// <summary>
    /// Rotates an array of vector3 90 degrees around the X axis
    /// </summary>
    /// <param name="points"> output param: array of points </param>
    public static void RotateCirclePointsX(ref Vector3[] points)
    {
        int length = points.Length;

        for(int i = 0; i < length; i++)
        {
            points[i] = Quaternion.Euler(90, 0, 0) * points[i];
        }
    }

    public static Vector3[] RotateCircularCrossSection(Vector3[] points, int segments)
    {
        Vector3[] vertices = new Vector3[points.Length * segments];

        int index = 0;
        float rotationAmountDegress = 360 / segments;

        for(int i = 0; i < segments; i++)
        {
            float angle = rotationAmountDegress * i;
            for(int j = 0; j < points.Length; j++)
            {
                vertices[index++] = Quaternion.Euler(0, angle, 0) * points[j];
            }
        }

        return vertices;
    }
}
