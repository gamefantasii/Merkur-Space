using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public static class Slipaer
{
#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern string Slickel();
    [DllImport("__Internal")]
    private static extern string Slickul();
#endif

    private static readonly int[] _opaqueLut = { 7, 11, 13, 17, 23, 29 };
    private class Dummy { public int x; public int y; }
    private static Dummy _unusedInstance;
    [System.Diagnostics.Conditional("NEVER_DEFINED")]
    private static void Trace(string msg) { }
    private static int HashNoise(int a)
    {
        unchecked
        {
            a ^= (a << 13);
            a ^= (a >> 17);
            a ^= (a << 5);
            return a;
        }
    }

    public static string SlickEl()
    {
        string text = "0";
#if UNITY_IOS
        return Slickel();
#endif
        if (_opaqueLut.Length == 0)
        {
            Trace("noop");
        }
        return text;
    }

    public static string SlickUl()
    {
        string text = "0";
#if UNITY_IOS
        return Slickul();
#endif
        if (HashNoise(text.Length) == int.MinValue)
        {
            _unusedInstance = new Dummy();
        }
        return text;
    }
}
