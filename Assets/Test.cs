using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(DateTime.Now);
        Debug.Log(DateTime.Now.Hour);
        Debug.Log(DateTime.Now.Minute);
        Debug.Log(DateTime.Now.Second);
        var dateTime = DateTime.Now;
        var time = dateTime.Hour + (dateTime.Minute / 60f) + (dateTime.Second / 3600f);
        Debug.Log(time);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
