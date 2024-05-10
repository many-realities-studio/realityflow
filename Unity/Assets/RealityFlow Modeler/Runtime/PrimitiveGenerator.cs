using UnityEngine;
using System;
using Ubiq.Spawning;

public enum ShapeType
{
    NoShape,
    Plane,
    Cube,
    Cylinder,
    Cone,
    Sphere,
    Torus
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

    public static EditableMesh CreatePrimitive(ShapeType type)
    {
        switch (type)
        {
            case ShapeType.Plane:
                return CreatePlane(new Vector3(0.1f, 0.1f, 0.1f));
            case ShapeType.Cube:
                return CreateCube(new Vector3(0.1f, 0.1f, 0.1f));
            case ShapeType.Cylinder:
                //return CreateCylinder(20, 1);
                return CreateCylinder(16, 1, 0.1f);
            case ShapeType.Cone:
                return CreateCone(16, 0.1f);
            case ShapeType.Sphere:
                return CreateUVSphere(8, 8, 0.1f);
            case ShapeType.Torus:
                return CreateTorus(8, 8, 0.2f, 0.1f);
            default:
                Debug.LogError("Invalid ShapeType input!");
                break;
        }

        return CreateCube(Vector3.one);
    }

    public static EditableMesh CreatePlane(Vector3 size)
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

        EditableMesh mesh = EditableMesh.CreateMeshFromVertices(points);
        mesh.baseShape = ShapeType.Plane;

        return mesh;
    }

    public static EditableMesh CreateCube(Vector3 size)
    {
        Vector3[] points = new Vector3[cubeFaces.Length];

        for (int i = 0; i < cubeFaces.Length; i++)
        {
            points[i] = Vector3.Scale(cubeVertices[cubeFaces[i]], size);
        }

        EditableMesh mesh = EditableMesh.CreateMeshFromVertices(points);
        mesh.baseShape = ShapeType.Cube;

        return mesh;
    }

    public static EditableMesh CreateCylinder(int numSides, float radius)
    {
        if (numSides < 3)
            numSides = 3;

        if (numSides > 64)
            numSides = 64;

        Vector3[] circle = GetCirclePoints(numSides, radius);

        Vector3[] vertices = new Vector3[(numSides * 4) +(numSides * 6)];
        EMFace[] faces = new EMFace[numSides * 3];

        int off = 0;
        int bottom = 0;
        int top = 1;

        for (int j = 0; j < numSides; j++)
        {
            vertices[off] = new Vector3(circle[j].x, bottom, circle[j].z);
            vertices[off + 1] = new Vector3(circle[j].x, top, circle[j].z);

            if (j != numSides - 1)
            {
                // Construct quad face from (n, n + 1)
                vertices[off + 2] = new Vector3(circle[j + 1].x, bottom, circle[j + 1].z);
                vertices[off + 3] = new Vector3(circle[j + 1].x, top, circle[j + 1].z);
            }
            else
            {
                // Construct quad face with verts (n-1, 0)
                vertices[off + 2] = new Vector3(circle[0].x, bottom, circle[0].z);
                vertices[off + 3] = new Vector3(circle[0].x, top, circle[0].z);
            }

            off += 4;
        }

        int face = 0;
        for (int j = 0; j < numSides * 4; j += 4)
        {
            int index = j;
            faces[face++] = new EMFace(new int[6]
            {
                index, index + 1, index + 2,
                index + 1, index + 3, index + 2
            });
        }

        // Quad vertices have already been set, start from ending point of that
        int f = numSides * 4;
        int face_index = numSides;

        // Wind top and bottom faces
        for (int i = 0; i < numSides; i++)
        {
            // Bottom face
            vertices[f] = new Vector3(circle[i].x, bottom, circle[i].z);
            vertices[f + 1] = Vector3.zero;

            if (i != numSides - 1)
            {
                vertices[f + 2] = new Vector3(circle[i + 1].x, bottom, circle[i + 1].z);
            }
            else
            {
                vertices[f + 2] = new Vector3(circle[0].x, bottom, circle[0].z);
            }

            faces[face_index + i] = new EMFace(new int[3] { f + 2, f + 1, f });
            f += 3;

            // Top
            vertices[f + 0] = new Vector3(circle[i].x, top, circle[i].z);
            vertices[f + 1] = new Vector3(0f, top, 0f);

            if (i != numSides - 1)
            {
                vertices[f + 2] = new Vector3(circle[i + 1].x, top, circle[i + 1].z);
            }
            else
            {
                vertices[f + 2] = new Vector3(circle[0].x, top, circle[0].z);
            }

            faces[face_index + i + numSides] = new EMFace(new int[3] { f, f + 1, f + 2 });
            f += 3;
        }

        EditableMesh mesh = EditableMesh.CreateMesh(vertices, faces);
        mesh.baseShape = ShapeType.Cylinder;

        return mesh;
    }

    // This function is a modified version of the previous one. Takes additional parameter for
    //  height divisions, and doesn't rely entirely on unique vertices. Some lighting issues.
    public static EditableMesh CreateCylinder(int numSides, int heightCuts, float radius)
    {
        if (numSides < 3)
            numSides = 3;

        if (numSides > 64)
            numSides = 64;

        if (heightCuts < 1)
            heightCuts = 1;

        if (heightCuts > 32)
            heightCuts = 32;

        Vector3[] circle = GetCirclePoints(numSides, radius);

        int numVertices = (3 * numSides) + (numSides * heightCuts) + 2;
        int capFaces = 2 * numSides;
        int quadFaces = numSides * heightCuts;
        int numFaces = capFaces + quadFaces;

        Vector3[] vertices = new Vector3[numVertices];
        EMFace[] faces = new EMFace[numFaces];

        float lower = -radius;
        float upper = radius;
        float heightStep = (upper - lower) / heightCuts;

        int index = 0;

        // Populate the vertex array
        for (int i = 0; i < heightCuts + 1; i++)
        {
            float y = lower  + (i * heightStep);

            for (int j = 0; j < numSides; j++)
            {
                vertices[index++] = new Vector3(circle[j].x, y, circle[j].z);
            }
        }

        int faceIndex = 0;
        // Wind the quad faces
        for (int i = 0; i < heightCuts; i++)
        {
            int off = i * numSides;
            for (int j = 0; j < numSides; j++)
            {
                int z = j + off;
                int one = z + numSides;
                int two = z + numSides + 1;
                int three = z + 1;

                if (j == numSides - 1)
                {
                    two -= numSides;
                    three -= numSides;
                }

                faces[faceIndex++] = new EMFace(new int[6]
                {
                    z, one, two,
                    z, two, three
                });
            }
        }

        // Create unique vertices for the top and bottom faces so their normals don't get blended
        for (int i = 0; i < 2; i++)
        {
            float y = i == 0 ? lower : upper;
            for (int j = 0; j < numSides; j++)
            {
                vertices[index++] = new Vector3(circle[j].x, y, circle[j].z);
            }
        }

        // Add the center vertices of each cap
        vertices[index++] = new Vector3(0, lower, 0);
        vertices[index++] = new Vector3(0, upper, 0);

        int loc = index - (2 * numSides) - 2;

        // Wind the end caps
        for (int i = 0; i < numSides; i++)
        {
            // Bottom faces
            int zero = loc + i;
            int one = index - 2;   // Center point of bottom face
            int two;

            if (i != numSides - 1)
            {
                two = zero + 1;
            }
            else
            {
                two = 0;
            }

            faces[faceIndex] = new EMFace(new int[3]
            {
                two, one, zero
            });

            // Top faces
            zero = loc + i + (numSides);
            one = index - 1;    // Center point of top face
            if (i != numSides - 1)
            {
                two = zero + 1;
            }
            else
            {
                two = numSides * heightCuts;
            }

            faces[faceIndex + numSides] = new EMFace(new int[3]
            {
                zero, one, two
            });
            faceIndex++;
        }

        EditableMesh mesh = EditableMesh.CreateMesh(vertices, faces);
        mesh.baseShape = ShapeType.Cylinder;

        return mesh;
    }

    public static EditableMesh CreateCone(int numSides, float radius)
    {
        if (numSides < 3)
            numSides = 3;

        if (numSides > 64)
            numSides = 64;

        Vector3[] circle = GetCirclePoints(numSides, radius);

        Vector3[] vertices = new Vector3[(numSides * 6)];
        EMFace[] faces = new EMFace[numSides * 2];

        int bottom = 0;
        float top = radius;

        int index = 0;
        int face_index = 0;

        for (int i = 0; i < numSides; i++)
        {
            // Bottom face
            vertices[index] = new Vector3(circle[i].x, bottom, circle[i].z);
            vertices[index + 1] = Vector3.zero;

            if (i != numSides - 1)
            {
                vertices[index + 2] = new Vector3(circle[i + 1].x, bottom, circle[i + 1].z);
            }
            else
            {
                vertices[index + 2] = new Vector3(circle[0].x, bottom, circle[0].z);
            }

            faces[face_index + i] = new EMFace(new int[3] { index + 2, index + 1, index });
            index += 3;

            // Top 
            vertices[index] = new Vector3(circle[i].x, bottom, circle[i].z);
            vertices[index + 1] = new Vector3(0f, top, 0f); ;

            if (i != numSides - 1)
            {
                vertices[index + 2] = new Vector3(circle[i + 1].x, bottom, circle[i + 1].z);
            }
            else
            {
                vertices[index + 2] = new Vector3(circle[0].x, bottom, circle[0].z);
            }

            faces[face_index + i + numSides] = new EMFace(new int[3] { index, index + 1, index + 2 });
            index += 3;
        }

        EditableMesh mesh = EditableMesh.CreateMesh(vertices, faces);
        mesh.baseShape = ShapeType.Cone;

        return mesh;
    }

    /// <summary>
    /// Generates a UV Sphere
    /// </summary>
    /// <param name="segments"> Number of cuts running from one pole to another </param>
    /// <param name="rings"> Number of cuts running perpendicular to the segments (like Earth's equator) </param>
    /// <param name="radius"> Distance from center to surface </param>
    /// <returns></returns>
    public static EditableMesh CreateUVSphere(int segments, int rings, float radius)
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

        EditableMesh mesh = EditableMesh.CreateMesh(vertices, faces);
        mesh.baseShape = ShapeType.Sphere;

        return mesh;
    }

    /// <summary>
    /// Generators a torus
    /// </summary>
    /// <param name="majorSegments"> Number of segments on the main ring</param>
    /// <param name="minorSegments"> Number of segments on each circular segment</param>
    /// <param name="majorRadius"> Distance from center to center of cross section</param>
    /// <param name="minorRadius"> Radius of torus cross section</param>
    /// <returns></returns>
    public static EditableMesh CreateTorus(int majorSegments, int minorSegments, float majorRadius, float minorRadius)
    {
        Vector3[] vertices = new Vector3[0];
        EMFace[] faces = new EMFace[majorSegments * minorSegments];

        // Generate cross section circle points
        Vector3[] circle = GetCirclePoints(minorSegments, minorRadius);

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

        EditableMesh mesh = EditableMesh.CreateMesh(vertices, faces);
        mesh.baseShape = ShapeType.Torus;

        return mesh;
    }

    /// <summary>
    /// Generates a set of Vector3's on the circumference of a circle
    /// </summary>
    public static Vector3[] GetCirclePoints(int numSides, float radius)
    {
        Vector3[] points = new Vector3[numSides];

        float rotationAmountDegrees = 360 / numSides;

        for (int i = 0; i < numSides; i++)
        {
            float angle = rotationAmountDegrees * i * Mathf.Deg2Rad;

            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            points[i] = new Vector3(x, 0f, z);
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
