# ZED_streamer
This unity project reads in frames from a ZED mini stereo camera and renders them to a HTC VIVE. In addition it also measures pupil dilation caused by cogintive workload via the build in eye trackers of the HTC VIVE Pro Eye and streams the data via UDP streaming (which allows us then to easily record the data via ROS). 

The pupil dilation measurment is based on the approach in [this paper](https://ir.canterbury.ac.nz/bitstream/handle/10092/15404/icat-egve2017-HaoChen.pdf?sequence=2). Pupil dilation is not only based on cognitive workload but also the light that comes into your eyes. Pupil dilation due to cognitive workload is thus measures as 
```
  pd_cog = pd_raw - pd_brightness
```
where pd_raw is the raw pupil dilation value obtained by the SRanipal API and pd_brightness is the pupil dilation caused by brightness.

To obtain pd_brightness the following is done: First a calibration procedure is being conducted where the brightness of the current scene is being changed from very dark to very bright, over 19 different brightness levels. Here we measure the brightness coming from the VIVE screens using a simple LDR sensor that has been attached where we send the data via an Arduino to unity (using ardity). For each brightness level we then measure the raw pupil dilation value and the corresponding brightness using the LDR. This way we create a database which allows us to infer pd_brightness given the brightness of the current scene. 

**Note that:**
* You can change the duration and the overall scaling of the autoexposure in the inspector if desired. The longer the duration, the more accurate the averaging of pupil dilation and brightness values.
* You may need to adjust the COM port for the arduino in unity. In the scene hierarchy, go to "Eye Tracking"->"Pupil Dilation"-> "SerialController".
* To find suitable resistor values for the LDR sensor you can follow [this guide](https://markusthill.github.io/electronics/choosing-a-voltage-divider-resistor-for-a-ldr/). Simply run the calibration procedure and measure min and max R values. 
* The ZED manager script has been modified such that head tracking is deactivated as the position of the ZED mini in my user case is static. 
This results in a static screen with the rendered scene and a black background in unity, allowing the user to move their freely around.
* **You need to have a cuda capable GPU to run the the ZED SDK!** Also note that, even tho you have such a GPU, the ZED SDK might not detect it on Linux, I switched therefore to Windows.

**Assets being used for this project:**
* [SteamVR](https://valvesoftware.github.io/steamvr_unity_plugin/articles/Quickstart.html)
* [ZED SDK](https://www.stereolabs.com/developers/release/)
* [SRanipal](https://developer-express.vive.com/resources/vive-sense/eye-and-facial-tracking-sdk/)
* [Ardity](https://ardity.dwilches.com/)

This project was built in Unity 2019.4.28f1.
