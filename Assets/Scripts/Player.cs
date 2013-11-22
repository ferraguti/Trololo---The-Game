using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour 
{
    public float speed = .1f;
    public float jumpDistance = 7;


    bool inside = false;
    bool victory = false;   
    Vector3 initalPos, initRot;

    void Start()
    {
        initalPos = transform.position;
        initRot = transform.eulerAngles;
    }

	void Update () 
    {
        transform.Translate(Vector3.right * Time.deltaTime * speed);

        if(inside && Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
	}

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);

        if (other.CompareTag("wall"))
        {
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y + 180, 0);
        }
        else if(other.CompareTag("zone"))
        {
            //temp
            if(other.gameObject.name == "zone")
            {
                victory = true;
            }

            inside = true;
        }
        else if (other.CompareTag("death"))
        {
            Debug.Log("Death");
            transform.eulerAngles = initRot;
            transform.position = initalPos;

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

    void Jump()
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
        }

    }
}
