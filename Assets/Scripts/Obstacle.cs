using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public enum Direction { Static, Horizontal, Axis };
    public Direction direction = Direction.Static;
    public float speed = 3;
    public float range = 3;
    Vector3 original_position;
    Direction locked_direction;
    Rect box; // lower rectangle on obstacle
    float t = 0;
    bool play = false;

    public void Initiate()
    {
        original_position = gameObject.transform.position;
        locked_direction = direction;
        if (direction != Direction.Static && (speed <= 0 || range <= 0))
            locked_direction = Direction.Static;
        UpdateBox(0);
    }

    void Update()
    {
        if (play && locked_direction != Direction.Static)
        {
            gameObject.transform.position = original_position + UpdatePosition(t);
            t += speed * Time.deltaTime;
        }
    }

    public void ResetPosition()
    {
        t = 0;
        gameObject.transform.position = original_position;
    }

    public void StartPlaying()
    {
        play = true;
    }

    Vector3 UpdatePosition(float _t)
    {
        if (locked_direction == Direction.Horizontal)
            return new Vector3(range / 2 - Mathf.PingPong(_t, range), 0, 0);
        else if (locked_direction == Direction.Axis)
            return (range / 2 - Mathf.PingPong(_t, range)) * transform.right;
        else
            return Vector3.zero;
    }

    void UpdateBox(float _t)
    {
        Vector3 new_position = UpdatePosition(_t);

        float x1 = GetComponent<BoxCollider>().bounds.min.x + new_position.x;
        float y1 = GetComponent<BoxCollider>().bounds.min.z + new_position.z;
        float x2 = GetComponent<BoxCollider>().bounds.max.x + new_position.x;
        float y2 = GetComponent<BoxCollider>().bounds.max.z + new_position.z;

        box = new Rect(x1, y1, Mathf.Abs(x2 - x1), Mathf.Abs(y2 - y1));
    }

    bool CircleIntersect(Vector2 q, float radius)
    {
        float DeltaX = q.x - Mathf.Max(box.xMin, Mathf.Min(q.x, box.xMin + box.width));
        float DeltaY = q.y - Mathf.Max(box.yMin, Mathf.Min(q.y, box.yMin + box.height));
        if ((DeltaX * DeltaX + DeltaY * DeltaY) < (radius * radius))
            return true;
        return false;
    }

    public bool Collision(Vector2 p1, Vector2 p2, float radius, float start_time, float length, bool isJump)
    {
        if (locked_direction == Direction.Static)
        {
            if (CircleIntersect(p1, radius))
                return true;
            if (isJump)
                return false;
            return Intersect(p1, p2);
        }

        if (locked_direction == Direction.Horizontal && isJump && !CircleIntersect(p2, radius))
            return false;

        Vector2 diff = p2 - p1;
        for (float i = 0.0f; i <= 1; i += 0.05f)    // 20 samples
        {
            float new_time = start_time + i * length;
            Vector2 p = p1 + i * diff;    // step toward next position
            UpdateBox(new_time * speed);
            if (CircleIntersect(p, radius))
                return true;
        }

        return false;
    }

    bool Intersect(Vector2 p, Vector2 v2)
    {
        Vector2 r = v2 - p;

        Vector2 p1 = new Vector2(box.xMin, box.yMax);
        Vector2 p2 = new Vector2(box.xMax, box.yMax);
        Vector2 p3 = new Vector2(box.xMax, box.yMin);
        Vector2 p4 = new Vector2(box.xMin, box.yMin);
        List<Vector2> points = new List<Vector2>() { p1, p2, p3, p4 };
        List<Vector2> edges = new List<Vector2>() { p2 - p1, p3 - p2, p4 - p3, p1 - p4 };

        for (int j = 0; j < edges.Count; ++j)
        {
            Vector2 s = edges[j];
            float r_s = r[0] * s[1] - r[1] * s[0];
            Vector2 q = points[j];
            Vector2 diff = q - p;

            if (r_s != 0)
            {
                Vector2[] term = new Vector2[2] { s / r_s, r / r_s };
                float t = diff[0] * term[0][1] - diff[1] * term[0][0];
                float u = diff[0] * term[1][1] - diff[1] * term[1][0];

                if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
                    return true;
            }
        }

        return false;
    }
}
