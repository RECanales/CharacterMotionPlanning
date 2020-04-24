using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planner : MonoBehaviour
{
	[Tooltip("Gameobject for character.")]
	public GameObject character;

	[Tooltip("Parent object of obstacles.")]
	public GameObject Obstacles;

	[Tooltip("Goal position for character.")]
	public Transform goalPos;

	[Tooltip("Radius of disc around character for collision checking.")]
	public float radius = 0.3f;

	[SerializeField]
	[Tooltip("Animation clips for actions the character can take.")]
	List<AnimationClip> motions = new List<AnimationClip>();

	[Tooltip("Records explored states and allows visualizing (limited).")]
	public bool debugMode = false;

	Animator animator;
	List<State> s_path = new List<State>();	// states in shortest path found
	List<Obstacle> obstacleObjects = new List<Obstacle>();	// the children of Obstacle
	List<Vector3> debug = new List<Vector3>();	// stores positions/orientations for explored states

	Transform startPos;	// the initial transform of the character
	Vector3 cached_start_position;
	Quaternion cached_start_rotation;

	// messy way of storing possible child states
	Dictionary<float, List<string>> graph;
	Dictionary<float, List<float>> multipliers;

	// various checks during playback
	int path_index = -1;
	bool jump_triggered = false;
	bool elapsed = false;
	bool show_exploration = false;
	bool trigger_replay = false;
	bool done = false;

	private void Start()
	{
		animator = character.GetComponent<Animator>();
		startPos = character.transform;

		// store the children of Obstacles into list
		InitObstacles();

		// I haven't figured out a better way to store the state machine given the motion clips I have
		/*
		graph = new Dictionary<float, List<string>>()
		{
			{ 0, new List<string> { "Idle_Ready", "Walk_Fwd"} },
			{ 0.5f, new List<string> { "Idle_Ready", "Walk_Fwd" } },
			{ 1, new List<string> { "Idle_Ready", "Walk_Fwd" } },
			{ 1.5f, new List<string> { "Walk_Fwd", "Jog_Fwd", "Jump_Fwd" } },
			{ 2, new List<string> { "Walk_Fwd", "Jog_Fwd", "Jump_Fwd" } },
			{ 3, new List<string> { "Walk_Fwd", "Jog_Fwd", "Jump_Fwd" } }
		};

		multipliers = new Dictionary<float, List<float>>()
		{
			{ 0, new List<float> { 0.5f, 1 } },
			{ 0.5f, new List<float> { 0.5f, 1 } },
			{ 1, new List<float> { 0.5f, 1, 1.5f } },
			{ 1.5f, new List<float> { 1, 1.5f } },
			{ 2, new List<float> { 1, 1.5f } },
			{ 3, new List<float> { 1.5f } }
		};*/

		graph = new Dictionary<float, List<string>>()
		{
			{ 0, new List<string> { "Idle_Ready", "Walk_Fwd", "Jog_Fwd" } },
			{ 0.5f, new List<string> { "Idle_Ready", "Walk_Fwd", "Jog_Fwd" } },
			{ 1, new List<string> { "Idle_Ready", "Walk_Fwd", "Jog_Fwd" } },
			{ 1.5f, new List<string> { "Walk_Fwd", "Jog_Fwd", "Jump_Fwd" } },
			{ 2, new List<string> { "Walk_Fwd", "Jog_Fwd", "Jump_Fwd" } },
			{ 3, new List<string> { "Walk_Fwd", "Jog_Fwd", "Jump_Fwd" } }
		};

		multipliers = new Dictionary<float, List<float>>()
		{
			{ 0, new List<float> { 0.5f, 1, 1.5f } },
			{ 0.5f, new List<float> { 0.5f, 1, 1.5f } },
			{ 1, new List<float> { 0.5f, 1, 1.5f } },
			{ 1.5f, new List<float> { 1, 1.5f } },
			{ 2, new List<float> { 1, 1.5f } },
			{ 3, new List<float> { 1.5f } }
		};
	}

	private void Update()
	{
		if (s_path.Count > 0)
		{
			if (debug.Count > 0 && Input.GetKeyDown(KeyCode.S))
			{
				if (!show_exploration)
					StartCoroutine("debugDraw");
				show_exploration = true;
			}

			if (!trigger_replay && Input.GetKeyDown(KeyCode.Space))
			{
				trigger_replay = true;
				Debug.Log("Replay triggered");
			}

			// play again
			if (trigger_replay && done)
			{
				path_index = -1;
				jump_triggered = false;
				elapsed = false;
				character.transform.position = cached_start_position;
				character.transform.rotation = cached_start_rotation;
				for (int i = 0; i < obstacleObjects.Count; ++i)
					obstacleObjects[i].GetComponent<Obstacle>().ResetPosition();
				StartCoroutine("WaitForClipEnd");
				trigger_replay = false;
				done = false;
			}

			if (path_index > -1 && path_index < s_path.Count)
				UpdateTransform();
		}
	}

	public void PlanPath()
	{
		print("Finding path...");

		// find shortest path
		s_path = FindPath();

		if (s_path.Count > 0)
		{
			cached_start_position = startPos.position;
			cached_start_rotation = startPos.rotation;

			for (int i = 0; i < obstacleObjects.Count; ++i)
				obstacleObjects[i].GetComponent<Obstacle>().StartPlaying();

			StartCoroutine("WaitForClipEnd");
			path_index = 0;
		}
	}

	public void Replay()
	{
		trigger_replay = true;
		Debug.Log("Replay triggered");
	}

	void UpdateTransform()
	{
		// updates position and rotation of character
		float goalY = s_path[path_index].GetOrientation();
		Quaternion newRotation = Quaternion.Euler(0, goalY, 0);
		character.transform.rotation = Quaternion.Slerp(character.transform.rotation, newRotation, Mathf.Clamp(s_path[path_index].p * 5f, 1, 6) * Time.deltaTime);

		Vector3 velocity = s_path[path_index].GetMotion().averageSpeed * s_path[path_index].p;
		Vector3 newPos = character.transform.position + newRotation * velocity;
		if (s_path[path_index].GetJump())
			character.transform.rotation = Quaternion.Slerp(character.transform.rotation, newRotation, 1);

		character.transform.position = Vector3.Lerp(character.transform.position, newPos, Time.deltaTime);
	}

	void InitObstacles()
	{
		if (!Obstacles || Obstacles.transform.childCount == 0)
			return;

		for (int i = 0; i < Obstacles.transform.childCount; ++i)
		{
			Obstacle _obstacle = Obstacles.transform.GetChild(i).gameObject.GetComponent<Obstacle>();
			_obstacle.Initiate();
			obstacleObjects.Add(_obstacle);
		}
	}

	List<State> FindPath()
	{
		State start = new State(new Vector2(startPos.position.x, startPos.position.z), startPos.rotation.eulerAngles.y, motions[1], 0);
		start.SetParams(new Vector2(0, 0));
		State goal = new State(new Vector2(goalPos.position.x, goalPos.position.z), 0, motions[1], 0);

		List<State> path = new List<State>();
		Dictionary<State, State> came_from = new Dictionary<State, State>();
		Dictionary<Vector3, State> closed_set = new Dictionary<Vector3, State>();

		PriorityQueue open_set = new PriorityQueue();
		open_set.Push(new Move(start, 0, Vector3.Magnitude(start.GetPosition() - goal.GetPosition())));

		bool reached_goal = false;
		int iterations = 0;
		while (!open_set.Empty() && iterations < 2000)
		{
			// get best state from queue
			Move m = open_set.Pop();
			closed_set[new Vector3(m.v.GetPosition().x, m.v.GetPosition().y, m.v.GetOrientation())] = m.v;	// add state to expanded state table

			if (debugMode)
				debug.Add(new Vector3(m.v.GetPosition().x, m.v.GetOrientation(), m.v.GetPosition().y));

			if (Vector3.Magnitude(m.v.GetPosition() - goal.GetPosition()) <= 0.5f)
			{
				Debug.Log("Goal reached");
				reached_goal = true;
				goal = m.v;
				break;
			}

			// get possible states
			List<string> allowable_names = graph[m.v.GetParams().y];
			List<float> mult = multipliers[m.v.GetParams().y];
			List<AnimationClip> allowable_motions = new List<AnimationClip>();
			foreach (AnimationClip clip in motions)
			{
				if (allowable_names.Contains(clip.name))
					allowable_motions.Add(clip);
			}

			List<State> child_states = m.v.GetChildStates(goal, allowable_motions, mult, obstacleObjects, radius);

			for (int i = 0; i < child_states.Count; ++i)
			{
				Vector2 new_pos = child_states[i].GetPosition();
				float new_theta = child_states[i].GetOrientation();
				//float clip_cost = Vector3.Magnitude(child_states[i].GetPosition() - m.v.GetPosition()); // does not let planning finish in time
				float curr_pathCost = m.pathCost + child_states[i].weight;

				int v_id = open_set.Get(child_states[i]);
				bool containsVert = v_id > -1 ? true : false;

				if (!closed_set.ContainsKey(new Vector3(new_pos.x, new_pos.y, new_theta)) && (!containsVert || curr_pathCost < open_set.GetMove(v_id).pathCost))
				{
					float next_cost = curr_pathCost + Vector3.Magnitude(new_pos - goal.GetPosition());
					Move nextMove = new Move(child_states[i], curr_pathCost, next_cost);

					if (containsVert)
						open_set.Remove(v_id);

					open_set.Push(nextMove);
					came_from[child_states[i]] = m.v;
				}
			}

			iterations++;
		}
		
		if (reached_goal)
		{
			while (goal != start)
			{
				// add state to path
				path.Add(goal);
				goal = came_from[goal];
			}

			path.Reverse();
			path.Add(new State(new Vector2(goalPos.position.x, goalPos.position.z), path[path.Count - 1].GetOrientation(), motions[1], 0));
			path[path.Count - 1].SetParams(Vector2.zero);

			// create cubes at each state
			for (int i = 0; i < path.Count; ++i)
			{
				GameObject test = GameObject.CreatePrimitive(PrimitiveType.Cube);
				test.transform.position = new Vector3(path[i].GetPosition().x, 1, path[i].GetPosition().y);
				test.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
				test.transform.rotation = Quaternion.Euler(0, path[i].GetOrientation(), 0);
				test.GetComponent<Renderer>().material.color = Color.red;
			}
		}

		Debug.Log("A* complete.");
		return path;
	}

	IEnumerator WaitForClipEnd()
	{
		path_index += 1;
		if (path_index < s_path.Count)
		{
			float wait_time = s_path[path_index].GetMotion().length;
			yield return new WaitForSeconds(wait_time);
			elapsed = true;
			
			if (path_index == s_path.Count - 1)
				done = true;
		}
	}

	public Vector2 GetInputParams()
	{
		if (s_path.Count > 0)
		{
			if (elapsed)
			{
				elapsed = false;
				jump_triggered = false;
				StartCoroutine("WaitForClipEnd");
			}

			if (path_index > -1 && path_index < s_path.Count)
			{
				if (s_path[path_index].GetJump() && !jump_triggered)
				{
					animator.SetTrigger("Jump_Fwd");
					jump_triggered = true;
					s_path[path_index].SetParams(new Vector2(0, 1));
				}

				if (s_path[path_index].GetJump() == false)
				{
					return s_path[path_index].GetParams();
				}
			}
		}

		return Vector2.zero;
	}

	Color MapToRGB(float v)
	{
		float a = (1 - v) / 0.2f;
		int x = (int)Mathf.Floor(a);
		int y = (int)Mathf.Floor(255 * (a - x));
		Color color = Color.white;

		switch (x)
		{
			case 0:
				color = new Color(255, y, 0);
				break;
			case 1:
				color = new Color(255 - y, 255, 0);
				break;
			case 2:
				color = new Color(0, 255, y);
				break;
			case 3:
				color = new Color(0, 255 - y, 255);
				break;
			case 4:
				color = new Color(y, 0, 255);
				break;
			case 5:
				color = new Color(255, 0, 255);
				break;
		}
		return color;
	}

	IEnumerator debugDraw()
	{
		if (debug.Count > 0)
		{
			for (int i = 0; i < debug.Count; ++i)
			{
				Quaternion debug_r = Quaternion.Euler(0, debug[i].y, 0);
				Vector3 _start = new Vector3(debug[i].x, 1, debug[i].z);
				Vector3 _end = _start + debug_r * new Vector3(0, 0, -0.2f);
				Debug.DrawLine(_start, _end, MapToRGB((float)i / (float)debug.Count), (float)debug.Count * 10);
				yield return new WaitForSeconds(0.05f);
			}
		}
	}
}
