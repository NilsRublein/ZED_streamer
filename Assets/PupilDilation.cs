/* This script subtracts pupil dilation due to brightness from raw pupil dilation, 
 * and sends it via a udp streamer to a different machine.
 * 
 * Nils Rublein, University of Twente, 2021.
 * 
 * TODO:
 *  - Send data via udp streamer to ROS
 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
public class PupilDilation : MonoBehaviour
{
    [Tooltip("Print pupil dilation values.")]
    public bool debug = true;

    public CalibratePupilDilation pd_calib;
    public MyMessageListener listener;
    public VerboseData verboseData;

    public double raw_pd_left;
    public double raw_pd_right;
    public double brightness_pd_left;
    public double brightness_pd_right;
    public double pd_left;
    public double pd_right;

    List<double> ldr_diff = new List<double>();
    private void Awake()
    {
        pd_calib = GetComponent<CalibratePupilDilation>();
    }
    void Start()
        {
            if (!SRanipal_Eye_Framework.Instance.EnableEye)
            {
                enabled = false;
                return;
            }
        }
    void Update()
    {
        // make sure the framework status is WORKING
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;

        // pass the verbose data
        // SRanipal_Eye.GetVerboseData(out verboseData); // v1
        SRanipal_Eye_v2.GetVerboseData(out verboseData); // v2

        // read the necessary data
        // returns -1 if no valid data is found (e.g. when blinking)
        raw_pd_left = verboseData.left.pupil_diameter_mm;
        raw_pd_right = verboseData.right.pupil_diameter_mm;

        if (pd_calib.calibrated)
        {
            brightness_pd_left = GetBrightnessPD(pd_calib.pd_database.pd_left);
            brightness_pd_right = GetBrightnessPD(pd_calib.pd_database.pd_right);

            pd_left = GetCogPD(raw_pd_left, brightness_pd_left);
            pd_right = GetCogPD(raw_pd_right, brightness_pd_right);

            if (debug)
            {
                Debug.Log("LEFT raw pd: " + raw_pd_left + ", bright pd: " + brightness_pd_left + ", cog pd: " + pd_left);
                Debug.Log("RIGHT raw pd: " + raw_pd_right + ", bright pd: " + brightness_pd_right + ", cog pd: " + pd_right);
                Debug.Log("LDR: " + listener.recv_ldr);
            }
        }
    }

    // Get pd due to brightness from database
    public double GetBrightnessPD(double[] pd_db)
    {
        double diff;
        double brightness_pd;
        double[] ldr_db = pd_calib.pd_database.ldr_val;
        float ldr_val = listener.recv_ldr;

        // Get absolute difference for each value in our database and add to a list
        for (int i = 0; i < ldr_db.Count(); i++)
        {
            diff = Math.Abs((ldr_val - ldr_db[i]));
            ldr_diff.Add(diff);
        }

        // Get the idx for the smallest difference and get the corresponding pd value
        int idx = ldr_diff.FindIndex(a => a == ldr_diff.Min()); 
        brightness_pd = pd_db[idx];
        ldr_diff.Clear(); 

        return brightness_pd;
    }
    public double GetCogPD(double raw_pd, double brightness_pd)
    {
        double cog_pd;

        // If the raw pd value is not valid (-1), set cog pd value also to not valid
        if (raw_pd == -1)
        {
            cog_pd = -1;
            brightness_pd = -1;
        }
        else
        {
            cog_pd = raw_pd - brightness_pd;
        }

        return cog_pd;
    }
}



