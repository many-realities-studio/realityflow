using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RealityFlow.NodeGraph
{
    /// <summary>
    /// Represents every possible type in the NodeGraph type system.
    /// 
    /// An instance of this type and not a child of it denotes an empty type, which is invalid
    /// but useful as a default.
    /// 
    /// The ValueType class hierarchy is specifically designed to be serializable by unity using
    /// [SerializeReference], while also providing type safety to avoid constructing invalid graphs. 
    /// </summary>
    [Serializable]
    public class NodeValueType
    {
        public static PrimitiveType Bool = new(Primitive.Bool);
        public static PrimitiveType Int = new(Primitive.Int);
        public static PrimitiveType Float = new(Primitive.Float);
        public static PrimitiveType Vector2 = new(Primitive.Vector2);
        public static PrimitiveType Vector3 = new(Primitive.Vector3);
        public static PrimitiveType Quaternion = new(Primitive.Quaternion);
        public static PrimitiveType GameObject = new(Primitive.GameObject);
        public static PrimitiveType TemplateObject = new(Primitive.TemplateObject);
        public static PrimitiveType String = new(Primitive.String);
        public static VariableType Variable(string name) => new(name);
        public static GraphType Graph => new();
        public static AnyType Any => new();

        static NodeValueType()
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in asm.GetTypes())
                {
                    if (typeof(INodeFieldType).IsAssignableFrom(type))
                        FieldTypes
                }
            }
        }

        public readonly static INodeFieldType[] FieldTypes =
            AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.GetTypes())
            .Select(type => type.)
    }

    [Serializable]
    public enum Primitive
    {
        Bool,
        Int,
        Float,
        Vector2,
        Vector3,
        Quaternion,
        GameObject,
        TemplateObject,
        String,
    }

    [Serializable]
    public class PrimitiveType :
        NodeValueType, IEquatable<PrimitiveType>, INodeInputType,
        INodeOutputType, INodeFieldType, IGraphVariableType
    {
        public readonly Primitive kind;

        public PrimitiveType(Primitive kind)
        {
            this.kind = kind;
        }

        public bool Equals(PrimitiveType other)
        {
            return kind == other.kind;
        }
    }

    [Serializable]
    public class VariableType : NodeValueType, INodeInputType, INodeOutputType
    {
        public readonly string name;

        public VariableType(string name)
        {
            this.name = name;
        }
    }

    [Serializable]
    public class GraphType : NodeValueType, INodeFieldType, IEquatable<GraphType>
    {
        public bool Equals(GraphType other)
        {
            return true;
        }
    }

    [Serializable]
    public class AnyType : NodeValueType, INodeInputType, IEquatable<GraphType>
    {
        public bool Equals(GraphType other)
        {
            return true;
        }
    }

    public interface INodeInputType { }
    public interface INodeOutputType { }
    public interface INodeFieldType { }
    public interface IGraphVariableType { }
}