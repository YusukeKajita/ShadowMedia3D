using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Vector3f
{
    public float X { set; get; }
    public float Y { set; get; }
    public float Z { set; get; }

    public float Length
    {
        set
        {
            float length = this.Length;
            this.X = this.X * value / length;
            this.Y = this.Y * value / length;
            this.Z = this.Z * value / length;
        }
        get
        {
            return (float)Math.Pow((double)(this.X * this.X + this.Y * this.Y + this.Z * this.Z), (double)0.5);
        }
    }
    /// <summary>
    /// 0で初期化されます
    /// </summary>
    public Vector3f()
    {
        this.X = 0;
        this.Y = 0;
        this.Z = 0;
    }
    /// <summary>
    /// Vector 3f を初期化します
    /// </summary>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <param name="Z"></param>
    public Vector3f(float X, float Y, float Z)
    {
        this.X = X;
        this.Y = Y;
        this.Z = Z;
    }
    /// <summary>
    /// Vector 3f をコピーします
    /// </summary>
    /// <param name="vec3f"></param>
    public Vector3f(Vector3f vec3f)
    {
        this.X = vec3f.X;
        this.Y = vec3f.Y;
        this.Z = vec3f.Z;
    }
    public static Vector3f operator +(Vector3f z, Vector3f w)
    {
        return new Vector3f(z.X + w.X, z.Y + w.Y, z.Z + w.Z);
    }
    public static Vector3f operator +(Vector3f z, float w)
    {
        return new Vector3f(z.X + w, z.Y + w, z.Z + w);
    }
    public static Vector3f operator *(Vector3f z, float w)
    {
        return new Vector3f(z.X * w, z.Y * w, z.Z * w);
    }
    public static float operator *(Vector3f z, Vector3f w)
    {
        return z.X * w.X + z.Y * w.Y + z.Z * w.Z;
    }
    public static Vector3f operator +(float w, Vector3f z)
    {
        return new Vector3f(z.X + w, z.Y + w, z.Z + w);
    }

    public static Vector3f operator -(Vector3f z, Vector3f w)
    {
        return new Vector3f(z.X - w.X, z.Y - w.Y, z.Z - w.Z);
    }

}
