using UnityEngine;
using System.Collections;

public class HUDTime : MonoBehaviour {
    GUIText text;
    Player player;

    void Start()
    {
        text = GetComponent<GUIText>();
        player = GameObject.FindObjectOfType(typeof(Player)) as Player;

        if (player.mode != MODE.ZEN)
            Destroy(gameObject);
    }

	void Update () 
    {
        text.text = "Time: " + (Time.timeSinceLevelLoad - player.reducedTime).ToString("F2");
	}
}
