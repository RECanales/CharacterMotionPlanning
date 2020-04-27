using System;
using System.Collections.Generic;
using UnityEngine;

public class State
{
	Vector2 position, blend;
	float theta, time;
	bool jump = false;
	AnimationClip motion_clip;
	public float p = 1;
	public float weight = 1;

	public State(Vector2 _pos, float _theta, AnimationClip _clip, float _time) { 
		position = _pos; 
		theta = _theta;
		motion_clip = _clip;
		time = _time;
	}

	public Vector2 GetPosition() { return position; }
	public float GetTime() { return time; }
	public float GetOrientation() { return theta; }
	public Vector2 GetParams() { return blend; }
	public void SetParams(Vector2 _blend) { blend = _blend; }
	public void SetJump(bool _jump) { jump = _jump; }
	public bool GetJump() { return jump; }
	public AnimationClip GetMotion() { return motion_clip; }

	public bool Equal(State s)
	{
		return position == s.GetPosition() && theta == s.GetOrientation();
	}

	bool Collision(List<Obstacle> obstacles, State s2, float radius, float length)   // disc collision
	{
		foreach (Obstacle obstacle in obstacles)
		{
			if (obstacle.Collision(position, s2.GetPosition(), radius, time, length, s2.GetJump()))
				return true;
		}

		return false;
	}

	public List<State> GetChildStates(State goal, List<AnimationClip> motions, List<float> mult, List<Obstacle> obstacles, float radius)
	{
		List<State> possible_states = new List<State>();
		Vector3 curr_pos = new Vector3(position.x, 0, position.y);

		foreach (AnimationClip clip in motions)
		{
			float rotation = -180;
			while (rotation < 180)
			{
				float new_euler = theta + rotation;
				Quaternion newRotation = Quaternion.Euler(0, new_euler, 0);

				if (clip.name == "Walk_Fwd")
				{
					foreach (float f in mult)
					{
						Vector3 child_pos = curr_pos + newRotation * (clip.length * clip.averageSpeed * f);
						State new_state = new State(new Vector2(child_pos.x, child_pos.z), new_euler, clip, clip.length + time) { p = f };
						new_state.SetParams(new Vector2(0, f));

						if (!Collision(obstacles, new_state, radius, clip.length))
							possible_states.Add(new_state);
					}
				}

				else
				{
					Vector3 child_pos = curr_pos + newRotation * (clip.length * clip.averageSpeed);
					State new_state = new State(new Vector2(child_pos.x, child_pos.z), new_euler, clip, clip.length + time);
					if (clip.name == "Jog_Fwd")
					{
						new_state.SetParams(new Vector2(0, 2f));
						new_state.weight = 1.5f;
						if (Vector3.Magnitude(new_state.GetPosition() - goal.GetPosition()) <= 1.5f)
							new_state.weight = 2f;
					}
					else if (motion_clip.name != "Idle_Ready")    // jump
					{
						new_state.SetParams(new Vector2(0, 3f));
						new_state.SetJump(true);
						new_state.weight = 2.3f;
						if (Vector3.Magnitude(new_state.GetPosition() - goal.GetPosition()) <= 1.5f)
							new_state.weight = 3f;
					}

					if (!Collision(obstacles, new_state, radius, clip.length))
						possible_states.Add(new_state);
				}

				rotation += 15;
			}
		}

		return possible_states;
	}
}

