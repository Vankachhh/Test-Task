using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{

    public float dirX, dirY; 
    public float speed;
    public Joystick joystick; 
    private Rigidbody2D rb;

    // ���� ������, ��� ������� ������ ��������� �������� ������ ��������� �� ��������� ���������, ��� � ������� �������� ����������

    public float minSpeedForAnimation = 0.01f; // ����� ��������, ����� �������� ������������ �� �����-��������
    private Vector2 lastPosition;
    public Animator animator;
    private bool isMoving;

    void Start()
    {
        lastPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        dirX = joystick.Horizontal * speed;
        dirY = joystick.Vertical * speed;

        // ��������� ��������, � �� ������� (����� ��� ������)
        float speedP = rb.velocity.magnitude;

        if (speedP > minSpeedForAnimation)
        {
            if (!isMoving)
            {
                isMoving = true;
                animator.SetBool("move", true);
            }
        }
        else
        {
            if (isMoving)
            {
                isMoving = false;
                animator.SetBool("move", false);
            }
        }
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(dirX, dirY);
    }
}
