# VRChat Dynamics Cam
A small script to help test avatar dynamics in the unity editor.

If you run into any issues or have suggestions contact me on discord @SynLogic#5410

## Requirements
![Av3Emulator](https://github.com/lyuma/Av3Emulator) (Used for emulating the playable layers, contacts, and a lot of other really useful things!)

## Download
# Visit the ![releases](https://github.com/synlogic/VRChat-Dynamics-Cam/releases) page for the unitypackage download.

## How to use
As a side note the script disables extra cameras in the scene to avoid interferance with physbone interactions. However, this **doesn't** take affect when uploading an avatar.

***The game view camera moves to where your scene view camera was when you start game mode.***

1) ![Av3Emulator](https://github.com/lyuma/Av3Emulator) is required for simulating the contact recievers and senders.

2) Add the Dynamics Cam script to your main camera (or any camera with the tag: "Main Camera")

3) Right click on your avatar in the Hierchy and select "Set DynamicsCam Focus"

4) Edit the Sender prefab under SynLogic/Prefabs to your taste, this is where you can select your sender collision tags for your contacts.
 
5) Turn on gizmos in play view to visualize the collider sizes.

6) Adjust your scene camera to where you want the game camera to be and click play!


## Main Controls
**WASD** for forward movement, **Q and E** for vertical movement.

**Hold Right click** to rotate the camera.

**R** resets the camera to your set focus.

**F** selects the *root* of your set focus in the hierarchy.

**Hold MouseWheel click/Middle click** spawns a ContactSender prefab at the mouse location (if there is a reciever there) that stays until you let go of the button. 

<img src="https://user-images.githubusercontent.com/26206994/167501339-7fd9ce3c-397c-4d58-875c-bca276d50203.gif" width="500">


**Mouse Scroll Wheel** changes the size of the sphere colliders on the recievers.  ***This is useful for testing proximity contacts.***

<img src="https://user-images.githubusercontent.com/26206994/167501491-0842fa80-8fb8-4d90-a6e6-1961d6acd0fd.gif" width="500">


# Parameters

***Focus*** - 
The focusable object,  right click objects in the Hierarchy tab to set the focus easily. (Set DynamicsCam Focus)

![image](https://user-images.githubusercontent.com/26206994/167499483-f4a471f4-50bb-4821-883a-9b4a850d88c3.png)

***Focus Offset*** - Offset of camera (X,Y,Z) to the focus object when pressing R.

***Focus Head*** - Enable to focus on the head instead of root when pressing R.

***Sender Prefab*** - 
The sender object that is spawned when you middle click the mouse button.

***Min Radius Size*** - 
The minimum size the contact receiver collider radius will be able to be set to.

***Move Speed*** - Speed of the horizontal camera movement (WASD)

***Acceleration Speed** - Speed of acceleration of all camera movement.

***Shift Speed Multiplier*** - Increases the movespeed by the set value when holding down left shift.

***Rotate Speed*** - Speed of rotation when holding down right click.
