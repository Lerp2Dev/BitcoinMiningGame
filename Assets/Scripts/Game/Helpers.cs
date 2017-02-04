using UnityEngine;
using System.Collections;

public enum RectSides { Left, Top, Right, Bottom }

public static class Helpers
{

    //List

    public static T[] Populate<T>(this T[] arr, T value)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = value;
        }
        return arr;
    }

    //Rect

    public static Rect Add(this Rect r1, Rect r2)
    {
        return new Rect(r1.xMin + r2.xMin, r1.yMin + r2.yMin, r1.width + r2.width, r1.height + r2.height);
    }

    public static Rect Change(this Rect r, float value, RectSides sides)
    {
        float new_left = 0, new_top = 0, new_right = 0, new_bottom = 0;
        if (sides == RectSides.Left) new_left = value;
        if (sides == RectSides.Top) new_top = value;
        if (sides == RectSides.Right) new_right = value;
        if (sides == RectSides.Bottom) new_bottom = value;
        return new Rect(r.xMin + new_left, r.yMin + new_top, r.width + new_right, r.height + new_bottom);
    }

}
