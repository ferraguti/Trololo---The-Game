﻿using UnityEngine;
using System.Collections;

public enum MODE
{
    NORMAL, ZEN
}

public enum INPUT
{
    KEYBOARD, MIKE
}


public class Player : MonoBehaviour 
{
    public float speed = .1f;
    public float jumpDistance = 7;
    public MODE mode;
    public INPUT input;
    public bool twoPlayer = false;
    public int score = 0;
    public float reducedTime = 0;

	private short moveDirection = 1;

    bool inside = false;
    bool victory = false;   
    Vector3 initalPos, initRot;
    
	CharacterController charController;

    void Start()
    {
		charController = gameObject.GetComponent<CharacterController>();

        initalPos = transform.position;
        initRot = transform.eulerAngles;

        if(mode == MODE.ZEN)
        {
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("death"))
                Destroy(g);
        }
    }

	void FixedUpdate()
	{
		charController.Move(new Vector3(moveDirection, 0, 0) * Time.deltaTime * speed);

		if(inside && Input.GetKeyDown(KeyCode.Space))
		{
			Jump();
		}
	}

    void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("wall"))
        {
			moveDirection *= -1;
        }
        else if(other.CompareTag("zone"))
        {
            ////temp
            //if(other.gameObject.name == "zone")
            //{
            //    victory = true;
            //}

            inside = true;
        }
        else if (other.CompareTag("death"))
        {
            Debug.Log("Death");
            transform.eulerAngles = initRot;
            transform.position = initalPos;
            score = 0;
            reducedTime = Time.timeSinceLevelLoad;
        }
    }
	
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("zone"))
        {
            inside = false;
            victory = false;
        }

    }

   public void Jump()
    {
        if(victory)
        {
            Debug.Log("YOU WIN");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                Application.Quit();
        }
        else
        {
            inside = false;
            transform.Translate(Vector3.up * jumpDistance, Space.World);
            score += 10000000;
        }

    }
}
