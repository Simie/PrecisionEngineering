using UnityEngine;

/// <summary>
/// https://github.com/StompyRobot/SRF/blob/master/Scripts/Extensions/Vector3Extensions.cs
/// </summary>
public static class Vector3Extensions
{
    public static Vector2 xy(this Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }

    public static Vector2 xz(this Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    /// <summary>
    /// Flattens the vector to ignore the y axis (x,z gameplay plane)
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector3 Flatten(this Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }

    public static Vector3 Average(Vector3 one, Vector3 two)
    {
        return (one + two)*0.5f;
    }

    public static Vector3 Average(Vector3 one, Vector3 two, Vector3 three)
    {
        return (one + two + three)/3f;
    }

    public static Vector3 Average(params Vector3[] vectors)
    {
        var total = Vector3.zero;

        for (var i = 0; i < vectors.Length; i++)
        {
            total += vectors[i];
        }

        return total/vectors.Length;
    }

    public static float Angle(Vector3 a1, Vector3 a2, Vector3 normal)
    {
        var angle = Vector3.Angle(a1, a2);

        var sign = Mathf.Sign(Vector3.Dot(normal, Vector3.Cross(a1, a2)));

        return angle*sign;
    }

    public static float DistanceSquared(Vector3 vector1, Vector3 vector2)
    {
        return (vector2 - vector1).sqrMagnitude;
    }

    public static Vector3 DirectionTo(this Vector3 t, Vector3 target)
    {
        var diff = target - t;
        diff.Normalize();
        return diff;
    }

    public static Vector3 Multiply(Vector3 one, Vector3 two)
    {
        var result = one;

        result.x *= two.x;
        result.y *= two.y;
        result.z *= two.z;

        return result;
    }

    /// <summary>
    /// http://stackoverflow.com/a/19684901/147003
    /// </summary>
    public static float GetClockwiseAngleBetween(Vector3 a, Vector3 b, Vector3 n)
    {
        // angle in [0,180]
        var angle = Vector3.Angle(a, b);
        var sign = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(a, b)));

        // angle in [-179,180]
        var signed_angle = angle*sign;

        // angle in [0,360] (not used but included here for completeness)
        var angle360 = (signed_angle + 180)%360;

        return angle360;
    }

    /// <summary>
    /// http://stackoverflow.com/a/19684901/147003
    /// </summary>
    public static float GetSignedAngleBetween(Vector3 a, Vector3 b, Vector3 n)
    {
        // angle in [0,180]
        var angle = Vector3.Angle(a, b);
        var sign = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(a, b)));

        // angle in [-179,180]
        var signed_angle = angle*sign;

        return signed_angle;
    }
}
