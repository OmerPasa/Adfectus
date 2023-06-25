using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Deb
{
    static bool spam_active = true;
    static bool spam_active1 = true;

    /// <summary>
    /// default debug log
    /// </summary>
    public static void u(string s)
    {
        Debug.Log(s);
    }

    /// <summary>
    /// For spamming debug logs in updates
    /// if active is false, it will not spam
    /// </summary>
    public static void ug(string s)
    {
        if (spam_active)
        {
            Debug.Log(s);
        }
    }

    /// <summary>
    /// alternative
    /// if active1 is false, it will not spam
    /// </summary>
    public static void ugg(string s)
    {
        if (spam_active1)
        {
            Debug.Log(s);
        }
    }

}
