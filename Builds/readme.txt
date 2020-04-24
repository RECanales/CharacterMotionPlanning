Included in the build are three example scenes and a scene editor. The program opens with 
the first example scene. 
To play the animation, click the button "Find Path". This will perform the path search and
then automatically play the animation.
"Replay" will replay the motion (if a path was found).
"Next Scene" will move to the next example scene. This button is available until the last
example scene.
"Previous Scene" will move to the previous example scene. This button is available for each example
scene after the first. 
"Quit": close the program

"Create Scene" opens an empty scene and lets you move the goal position and add obstacles.

Creating a scene:
"Presets": goes back to the example scenes.
"Add Obstacle": lets the user add an obstacle.
- "Static": Inserts a non-moving obstacle
- "Moving": Inserts a moving obstacle
	- The first text box that shows after clicking "Moving" prompts for the range. This
	is how far the obstacle can move (should probably make it at most 15). Press enter
	to set the range.
	- The second text box prompts for the speed at which the obstacle moves. Press enter
	to set the speed. 
	- Each input must be positive.
	- This program could probably be broken with large values.
- "Done": initializes the scene for playback. The "Find Path" and "Replay" buttons will appear.
- "Start Over": go back to creating a new scene

To move the goal position around, click and drag the circle to the desired goal position.
After adding an obstacle, you can click and drag it to move it around. 
To scale an obstacle, click on an obstacle and drag while holding down the 'S' key.
To rotate an obstacle, click on an obstacle and drag while holding down the 'R' key.
See video for example of scene creation.
Note: The search is limited to a fixed amount of while loop iterations to avoid a very long wait.
So, a path will not always be found.