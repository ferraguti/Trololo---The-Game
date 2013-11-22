using UnityEngine;
using System.Collections;

public class HUDScore : MonoBehaviour 
{
    GUIText text;
    Player player;

    void Start()
    {
        text = GetComponent<GUIText>();
        player = GameObject.FindObjectOfType(typeof(Player)) as Player;
    }

    void Update()
    {
        text.text = "Score: " + player.score;
    }
}
