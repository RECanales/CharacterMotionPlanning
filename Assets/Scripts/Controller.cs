using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public GameObject character;
    public float damping = 0.15f;
    Animator animator;

    void Start()
    {
        animator = character.GetComponent<Animator>();
    }

    void Update()
    {
        int m_HashHorizontalPara = Animator.StringToHash("Horizontal");
        int m_HashVerticalPara = Animator.StringToHash("Vertical");

        Vector2 input = GameObject.FindGameObjectWithTag("Animate").GetComponent<Planner>().GetInputParams();
        animator.SetFloat(m_HashHorizontalPara, input.x, damping, Time.deltaTime);
        animator.SetFloat(m_HashVerticalPara, input.y, damping, Time.deltaTime);
    }
}
