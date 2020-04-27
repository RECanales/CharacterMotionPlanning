using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateScene : MonoBehaviour
{
    public Camera ortho;
    public Transform obstacle_prefab;
    public Transform obstacle_parent;
    public UnityEngine.UI.InputField speed_input, range_input;
    bool selected = false;
    Transform selected_object;
    int obstacle_count = 0;
    float prevY = 0;
    float prevX = 0;
    bool rotate = false;
    bool scale = false;

    void Update()
    {
        float deltaX = Input.mousePosition.x - prevX;
        float deltaY = Input.mousePosition.y - prevY;

        if (rotate && Input.GetKeyUp(KeyCode.R))
        {
            selected = false;
            selected_object = null;
            rotate = false;
        }


        if (scale && Input.GetKeyUp(KeyCode.S))
        {
            selected = false;
            selected_object = null;
            scale = false;
        }

        if (!(speed_input.gameObject.activeSelf || range_input.gameObject.activeSelf) &&  Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = ortho.ScreenPointToRay(Input.mousePosition);

            if (ray.origin.x > -3.2f && Physics.Raycast(ray, out hit))
            {
                Transform objectHit = hit.transform;
                if (objectHit.name == "goal" || objectHit.name.Contains("obstacle"))
                {
                    selected_object = objectHit;
                    selected = true;
                }
            }
        }

        else if(Input.GetMouseButtonUp(0) && selected)
        {
            selected = false;
            selected_object = null;
        }

        if (selected)
        {
            if (Input.GetKey(KeyCode.R))
            {
                selected_object.Rotate(new Vector3(0, 0.3f * deltaY, 0));
                rotate = true;
            }

            else if(Input.GetKey(KeyCode.S))
            {
                Ray prev_point = ortho.ScreenPointToRay(new Vector2(prevX, prevY));
                Ray point = ortho.ScreenPointToRay(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
                Vector2 mouse_vec = new Vector2(deltaY, deltaX).normalized;
                Vector2 object_right = new Vector2(selected_object.right.x, selected_object.right.z).normalized;
                Vector2 object_fwd = new Vector2(selected_object.forward.x, selected_object.forward.z).normalized;
                float angleX = Vector2.Dot(mouse_vec, object_right);
                float angleY = Vector2.Dot(mouse_vec, object_fwd);
                
                /*
                float diff = Vector3.Magnitude(point.origin - selected_object.position) - Vector3.Magnitude(prev_point.origin - selected_object.position);
                if (diff < 0)
                {
                    angleX = -Mathf.Abs(angleX);
                    angleY = -Mathf.Abs(angleY);
                }*/

                float scaleX = Mathf.Clamp(selected_object.localScale.x + 0.1f * angleX, -15, 15);
                float scaleY = Mathf.Clamp(selected_object.localScale.z + 0.1f * angleY, -15, 15);

                Vector3 new_scale = new Vector3(scaleX, selected_object.localScale.y, scaleY);
                selected_object.localScale = new_scale;
                scale = true;
            }

            else
            {
                Ray ray = ortho.ScreenPointToRay(Input.mousePosition);
                selected_object.position = new Vector3(ray.origin.x, selected_object.position.y, ray.origin.z);
            }
        }
        prevX = Input.mousePosition.x;
        prevY = Input.mousePosition.y;
    }

    public void AddObstacle(bool moving)
    {
        if (obstacle_count > 15)
        {
            print("Only 15 obstacles allowed.");
            return;
        }

        Transform new_obstacle = Instantiate(obstacle_prefab, new Vector3(Random.Range(0, 1.5f), 0.2f, Random.Range(0, 1.5f)), Quaternion.identity);
        new_obstacle.parent = obstacle_parent;
        if (moving)
        {
            float speed = float.Parse(speed_input.text);
            float range = float.Parse(range_input.text);
            new_obstacle.GetComponent<Obstacle>().direction = Obstacle.Direction.Axis;
            if (speed > 0 && range > 0)
            {
                new_obstacle.GetComponent<Obstacle>().speed = speed;
                new_obstacle.GetComponent<Obstacle>().range = range;
            }

            else
                new_obstacle.GetComponent<Obstacle>().direction = Obstacle.Direction.Static;

            range_input.text = "";
            speed_input.text = "";
            speed_input.gameObject.SetActive(false);
            range_input.gameObject.SetActive(false);
        }

        
        obstacle_count++;
    }
}
