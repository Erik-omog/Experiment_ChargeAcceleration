using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float Basespeed;         // Base movement speed while not charging spin
    public float rotationSpeed;     // Speed
    public float spinPowerBase;     // Baseline charged force strength
    public float spinLevelFast;     // Treshhold for first level of charge
    public float spinLevelFaster;   // Treshhold for second level of charge

    private Rigidbody rb;           // Object's rigidbody
    private Vector3 mousePosition;  // Position of mouse on game screen
    private Vector3 moveDirection;  // Vector from the player position mouse position 
    private Renderer rend;          // The object's mesh
    private int spinPower = 1;      // A marker to indicate how long the charge has been held for based of distinct levels
    private float speed;            // Current speed of the object
    private float spinLevel;        // A marker to indicate how long the charge has been held for based on smaller increments

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();

        rb.sleepThreshold = 0.5f * Mathf.Pow(speed, 2);
        rend.material.color = Color.white;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // If in charge state decrease movement speed otherwise use base speed
        if (spinLevel > 30)
        {
            speed = Basespeed / 4;
        }
        else
        {
            speed = Basespeed;
        }

        // Give player controls based on arrow keys as long as they aren't in charged state
        // Possibly make bool to denote charged state?
        if (rb.velocity.magnitude < speed)
        {
            rb.freezeRotation = true;
            var x = Input.GetAxis("Horizontal") * Time.deltaTime * speed;
            var z = Input.GetAxis("Vertical") * Time.deltaTime * speed;
            moveDirection = new Vector3(x, 0, z);

            // Might want to use velocity vectors to control movement instead of translate 
            transform.Translate(moveDirection, Space.World);
            if (moveDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(moveDirection),
                    Time.deltaTime * rotationSpeed
                );
            }
        }


        // Button that controls the charge ability, right click by default
        bool down = Input.GetButtonDown("Rev");
        bool held = Input.GetButton("Rev");
        bool up = Input.GetButtonUp("Rev");

        // Orient the player in the direction they have mouse cursor when they are charging
        if (down || held)
        {
            Vector3 pointer = GetMousePosition();
            Vector3 difference = pointer - transform.position;
            if (moveDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(pointer),
                    Time.deltaTime * rotationSpeed
                );
            }

            else if (spinLevel > spinLevelFaster)
            {
                spinPower = 3;
                rend.material.color = Color.red;
            }
            else if (spinLevel > spinLevelFast)
            {
                spinPower = 2;
                rend.material.color = Color.yellow;
            }

            ++spinLevel;
        }
        // Get the direction the player wants to move then find how long they charged the shot for
        else if (up && spinPower > 1)
        {
            Vector3 pointer = GetMousePosition();
            Vector3 difference = pointer - transform.position;
            difference.Normalize();
            difference = Quaternion.Euler(0, 180, 0) * difference;
            rb.AddForce(difference * spinPowerBase * spinPower);
            rend.material.color = Color.white;
            spinLevel = 0;
            spinPower = 1;
        }
    }

    // Function to get mouse position based on a top down camera
    Vector3 GetMousePosition()
    {
        Plane p = new Plane(transform.up, transform.position);
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
        float d;
        if (p.Raycast(r, out d))
        {
            Vector3 v = r.GetPoint(d);
            return v;
        }

        throw new UnityException("Mouse position ray not intersecting launcher plane");
    }
}