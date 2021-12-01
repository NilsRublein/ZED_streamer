// Changes Brightness in the scene for X levels, measures pupil dilation and creates a baseline database for light dependent pupil dilation.
// Manipulating the post processing stack: https://docs.unity3d.com/Packages/com.unity.postprocessing@2.1/manual/Manipulating-the-Stack.html

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
using UnityEngine.Rendering.PostProcessing;
using System.Linq;
using System.IO;
public class CalibratePupilDilation : MonoBehaviour
{
    [Tooltip("Time per brightness level during calibration [s].")]
    public float period = 3.0f;
    [Tooltip("Auto exposure scale for calibration [int].")]
    public int exposure_scale = 1;
    [Tooltip("Filename of the pupil dilation databse.")]
    public string filename = "pd_database_name";
    [Tooltip("Desired location for the database file")]
    public string path = "D: /Unity stuff"; // Actually just saves it in the unity project folder

    public bool calibrated = false;
    public PostProcessVolume volume;
    public MyMessageListener listener;
    public VerboseData verboseData;

    private TimeSpan ts;
    private float ldr_val = 0.0f;
    private int brightness_lvl = 9; 
    private int n_brightness_lvl = 9;
    private bool start_calib = false;

    // Lists for creating the database
    List<float> ldr_list = new List<float>();
    List<int> brigtness_lvl_list = new List<int>();
    List<float> left_pd_list = new List<float>();
    List<float> right_pd_list = new List<float>();

    List<int> brigtness_lvl_list_ = new List<int>();
    List<double> ldr_avg_list = new List<double>();
    List<double> left_avg_pd_list = new List<double>();
    List<double> right_avg_pd_list = new List<double>();

    List<int> counts = new List<int>();

    AutoExposure autoExposure;
    public PD_database pd_database = new PD_database();
    System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch(); // Written this way due to ambiguous reference between UnityEngine.Debug & System.Diagnostics.Debug
    void Start()
    {
        if (!SRanipal_Eye_Framework.Instance.EnableEye)
        {
            enabled = false;
            return;
        }

        // Setup overwriting of auto exposure values
        autoExposure = ScriptableObject.CreateInstance<AutoExposure>();
        autoExposure.enabled.Override(true);
        autoExposure.minLuminance.Override(1f);
        autoExposure.maxLuminance.Override(1f);
        autoExposure.keyValue.Override(1f); // scaling factor
        volume = PostProcessManager.instance.QuickVolume(gameObject.layer, 100f, autoExposure);

        // Set initial auto exposure params for calibration
        Debug.Log("Setting initial auto exposure params for calibration");
        autoExposure.minLuminance.value = brightness_lvl;
        autoExposure.maxLuminance.value = brightness_lvl;
        autoExposure.keyValue.value = exposure_scale;
    }
    void Update()
    {
        // make sure the framework status is WORKING
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;

        // pass the verbose data
        // SRanipal_Eye.GetVerboseData(out verboseData); // v1
        SRanipal_Eye_v2.GetVerboseData(out verboseData); // v2

        if (Input.GetKeyDown("space"))
        {
            Debug.Log("Starting pupil dilation calibration.");
            timer.Start();
            ts = timer.Elapsed;

            // Re initialize values in case you want to calib again
            brightness_lvl = 9;
            autoExposure.minLuminance.value = brightness_lvl;
            autoExposure.maxLuminance.value = brightness_lvl;
            autoExposure.keyValue.value = exposure_scale;
            start_calib = true;
            InvokeRepeating("decreaseBrightness", period, period);
        }

        if (start_calib)
        {
            Calib();
        }
    }
    void OnDestroy()
    {
        RuntimeUtilities.DestroyVolume(volume, true, true);
    }

    // Get brightness value of LDR sensor
    public void Calib()
    {
        // Get ldr and pd measurements for each brightness lvl (which is updated via InvokeRepeating())
        if (brightness_lvl >= (-1 * n_brightness_lvl))
        {
            // Get LDR values and corresponding brightness lvl
            ldr_val = listener.recv_ldr;
            ldr_list.Add(ldr_val);
            brigtness_lvl_list.Add(brightness_lvl);

            // Get PD values
            left_pd_list.Add(verboseData.left.pupil_diameter_mm);
            right_pd_list.Add(verboseData.right.pupil_diameter_mm);
        }
        else
        {
            // May be used to inspect the data for debugging
            // Make sure to change list types in the WriteListToCsv() accordingly
            // WriteListToCsv("D: /Unity stuff", filename, brigtness_lvl_list, ldr_list, left_pd_list, right_pd_list); 

            // Stop changing brightness and reset to default values
            CancelInvoke();
            autoExposure.minLuminance.value = 0;
            autoExposure.maxLuminance.value = 0;
            autoExposure.keyValue.value = 1;
            
            start_calib = false; // Stop update
            Debug.Log("Finished Measuring PD values, start creating of database.");
            CreateDatabase();
            calibrated = true; // Used in pupil dilation and receiver scripts
            
            timer.Stop();
            Debug.Log("Calibration is done.");
            Debug.Log("Elapsed time: " + ts);
        }
    }
    void decreaseBrightness()
    {
        if (start_calib)
        {
            brightness_lvl--;
            autoExposure.minLuminance.value = brightness_lvl;
            Debug.Log("Changing brightness level to: " + brightness_lvl);
        }
    }
    public void CreateDatabase()
    {
        // Count how many values we have per brightness lvl
        int[] brigtness_lvl_ = brigtness_lvl_list.ToArray();
        for (int i = 9; i >= -9; i--)
        {
            var counter = brigtness_lvl_.Count(n => n == i);
            counts.Add(counter);
        }

        // Get avg values for the specified brightness lvl
        int idx = 0;
        for (int i = 0; i < counts.Count; i++)
        {
            double ldr_avg = ldr_list.GetRange(idx, counts[i]).Average();
            double left_pd_avg = left_pd_list.GetRange(idx, counts[i]).Where(c => c != -1).Average(); // Exclude -1 values (blinks, lost track of eyes etc.) from avg
            double right_pd_avg = right_pd_list.GetRange(idx, counts[i]).Where(c => c != -1).Average();

            // Append to list
            ldr_avg_list.Add(ldr_avg);
            left_avg_pd_list.Add(left_pd_avg);
            right_avg_pd_list.Add(right_pd_avg);
            brigtness_lvl_list_.Add(i);

            // Debug.Log("index: " + idx + " range: " + counts[i]);
            // Debug.Log("test: " + ldr_avg);

            // update idx for the next brightness lvl
            idx = idx + counts[i]; 
        }

        // May be used to inspect the data for debugging
        WriteListToCsv("D: /Unity stuff", filename, brigtness_lvl_list_, ldr_avg_list, left_avg_pd_list, right_avg_pd_list); 

        // Convert to arrays and add to class 
        double[] ldr_vals_ = ldr_avg_list.ToArray();
        double[] left_pd_ = left_avg_pd_list.ToArray();
        double[] right_pd_ = right_avg_pd_list.ToArray();

        pd_database.ldr_val = ldr_vals_;
        pd_database.brightness_lvl = brigtness_lvl_;
        pd_database.pd_left = left_pd_;
        pd_database.pd_right = right_pd_;
    }

    // Write the data from the lists to a csv file
    // Could be probably done better with a list of lists, however we have lists with different types (int, float) ...
    // Based on: https://forum.unity.com/threads/write-data-from-list-to-csv-file.643561/
    public void WriteListToCsv(string path, string filename, List<int> list1, List<double> list2, List<double> list3, List<double> list4)
    {
        //string filePath = path + "/" +  filename + ".csv"; // This seems add the path of where the unity project is located in by default??
        string filePath = filename + ".csv";
        StreamWriter writer = new StreamWriter(filePath); // pathto/filename.csv , make
        
        for (int i = 0; i < Mathf.Max(list1.Count, list2.Count, list3.Count, list4.Count); ++i)
        {
            if (i < list1.Count) writer.Write(list1[i]);
            writer.Write(","); // Go to next column
            if (i < list2.Count) writer.Write(list2[i]);
            writer.Write(",");
            if (i < list3.Count) writer.Write(list3[i]);
            writer.Write(","); 
            if (i < list4.Count) writer.Write(list4[i]);

            writer.Write(System.Environment.NewLine); // New row
        }

        writer.Flush();
        writer.Close();
    }
    public class PD_database
    {
        public double[] pd_left { get; set; }
        public double[] pd_right { get; set; }
        public double[] ldr_val { get; set; }
        public int[] brightness_lvl { get; set; }

    }

}


