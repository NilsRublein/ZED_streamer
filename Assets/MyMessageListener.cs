using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MyMessageListener : MonoBehaviour
{
    public float recv_ldr = 0.0f;
    [Tooltip("Print received msgs.")]
    public bool debug = false;

    // Use this for initialization
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
    }
    // Invoked when a line of data is received from the serial device.
    void OnMessageArrived(string msg)
    {
        if (debug)
        {
            Debug.Log("Arrived: " + msg);
        }

        recv_ldr = float.Parse(msg); // Convert from string to int
    }
    // Invoked when a connect/disconnect event occurs. The parameter 'success'
    // will be 'true' upon connection, and 'false' upon disconnection or
    // failure to connect.
    void OnConnectionEvent(bool success)
    {
        Debug.Log(success ? "Device connected" : "Device disconnected");
    }
}