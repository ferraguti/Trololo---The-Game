using UnityEngine;
using System.Collections;

public class Saw : MonoBehaviour {
    public float speedMin = 0.15f;
    public float speedMax = 0.25f;

    private float speed;
    private int sign;

    void Start()
    {
        speed = Random.Range(speedMin, speedMax);
        sign = Random.Range(0, 1) == 0 ? 1 : -1;
    }

	void Update () 
    {
        transform.RotateAround(Vector3.forward, speed * sign);
	}
}
