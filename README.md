# vivetrackeropenxr

According to [this post on the Steam forums](https://steamcommunity.com/app/250820/discussions/8/560232227703656040/) which references [this github project](https://github.com/danwillm/openxr-vive-tracker-ext/tree/main), pogo pins and usb input for the vive trackers in SteamVR has been enabled as of a couple months ago and I should be able to access them through OpenXR. This was corroborated by another user. 
----
I wanted to get this implemented in Unity, the scripts above are my results. 
1. To recreate, make a new Unity project, open the package manager and import the OpenXR plugin. Next, copy my assets and put them in your project.

2. Open Edit->Project Settings, under XR Plug-in Management toggle OpenXR on. It'll have a caution sign, click it to fix the PoseControl thing. Then under the OpenXR submenu, add HTC Vive Tracker OpenXR Profile as an interaction profile.

3. Create an empty game object in your scene and add the ViveTrackerManager script. Drag the input actions asset over and the mesh and material under ViveTracker3.obj, then click generate vive trackers. This should generate 13 vive trackers as a child (later todo is add the updated lsit of roles including wrists and ankles).

4. Upon clicking play, the active trackers should accept in position and rotation data, but not pogo pin/usb input.