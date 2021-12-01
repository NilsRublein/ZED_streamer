using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;
public class UdpStreamer : MonoBehaviour
{
    [Tooltip("Port to send UDP messages to")]
    public int port = 9052;
    [Tooltip("IP adres to send UDP messages to")]
    public string destination_ip = "127.0.0.1";
    public bool Sending = true;
    [Tooltip("Sender name, used for debug messages.")]
    public string sender_name = "Pupil Dilation UDP Sender";
    [Tooltip("Print received msgs")]
    public bool debug = false;

    public PupilDilation pd_data;
    public CalibratePupilDilation pd_calib;
    UDPSender<EyeGazeSerializer> sender;
    EyeGazeSerializer data = new EyeGazeSerializer();

    private void Awake()
    {
        pd_data = GetComponent<PupilDilation>();
        pd_calib = GetComponent<CalibratePupilDilation>();
    }
    void Start()
    {
        sender = new UDPSender<EyeGazeSerializer>(port, destination_ip, sender_name);
    }
    void Update()
    {
        if (Sending)
        {
            if (pd_calib.calibrated) 
            {
                data.raw_pd_left = pd_data.raw_pd_left;
                data.raw_pd_right = pd_data.raw_pd_right;
                data.brightness_pd_left = pd_data.brightness_pd_left;
                data.brightness_pd_right = pd_data.brightness_pd_right;
                data.pd_left = pd_data.pd_left;
                data.pd_right = pd_data.pd_right;
                data.eyegaze_enabled = true;

                if (debug)
                {
                    Debug.Log("Sending left raw pd: " + data.raw_pd_left);
                    Debug.Log("Sending right raw pd: " + data.raw_pd_right);
                    Debug.Log("Sending left brightness pd: " + data.brightness_pd_left);
                    Debug.Log("Sending right brightness pd: " + data.brightness_pd_right);
                    Debug.Log("Sending left cog pd: " + data.pd_left);
                    Debug.Log("Sending right cog pd: " + data.pd_right);
                }
            }
        }
        else
        {
            data.eyegaze_enabled = false;
        }
        sender.Send(data);
    }
}

[MessagePackObject(keyAsPropertyName: true)]
public class EyeGazeSerializer
{
    public bool eyegaze_enabled { get; set; }
    public double raw_pd_left { get; set; }
    public double raw_pd_right { get; set; }
    public double brightness_pd_left;
    public double brightness_pd_right;
    public double pd_left;
    public double pd_right;
}
