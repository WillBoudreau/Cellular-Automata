using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour2D : MonoBehaviour
{
    Rigidbody2D rigidbody2D;
    public int speed = 5;
    Vector2 velocity;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        velocity = new Vector2(moveHorizontal, moveVertical).normalized * speed;
    }

    void FixedUpdate()
    {
        rigidbody2D.MovePosition(rigidbody2D.position + velocity * Time.fixedDeltaTime);
    }
}
