using System;
using System.Collections.Generic;

public class Move : IComparable<Move>
{
	public State v;
	public float pathCost, cost;

	public Move(State _v, float g, float f) { v = _v; pathCost = g; cost = f; }
	public int CompareTo(Move other)
	{
		float thisDist = cost;
		float thatDist = other.cost;
		if (thisDist < thatDist)
			return -1;
		if (thisDist > thatDist)
			return 1;

		return 0;
	}
}

public class PriorityQueue
{
	private List<Move> data;
	public PriorityQueue() { data = new List<Move>(); }
	public Move Pop()
	{
		data.Sort();
		Move value = data[0];
		data.RemoveAt(0);
		return value; 
	}

	public void Push(Move value)
	{
		data.Add(value);
	}

	public int Get(State value)
	{
		int index = -1;
		for (int i = 0; i < data.Count; ++i)
		{
			if(data[i].v.Equal(value))
			{
				index = i;
				break;
			}
		}
		return index;
	}

	public void Remove(int id)
	{
		data.RemoveAt(id);
	}

	public Move GetMove(int id)
	{
		if (id > -1 && id < data.Count)
			return data[id];
		return null;
	}

	public bool Empty()
	{
		return data.Count == 0 ? true : false;
	}
}
