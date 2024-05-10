using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableColor
{
    public float _r, _g, _b, _a;

    public Color GetColor() => new Color(_r, _g, _b, _a);
    public void SetColor(Color color)
    {
        _r = color.r;
        _g = color.g;
        _b = color.b;
        _a = color.a;
    }

    public SerializableColor() { _r = _g = _b = _a = 1f; }

    public SerializableColor(Color color) : this(color.r, color.g, color.b, color.a) { }

    public SerializableColor(float r, float g, float b, float a = 0f)
    {
        _r = r;
        _g = g;
        _b = b;
        _a = a;
    }
}
