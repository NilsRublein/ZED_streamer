# ZED_streamer
This unity project reads in frames from a ZED mini stereo camera and renders them to a HTC VIVE. 
The ZED manager script has been modified such that head tracking is deactivated as the position of the ZED mini is static. 
This results in a static screen with a black background in unity, allowing the user to move their freely around.

This project uses Unity 2019.4.28f1.

# TODO
- Add SRanipal to measure pupil dilation. 
- Add udp streamer to record pupil dilation via ROS. 
