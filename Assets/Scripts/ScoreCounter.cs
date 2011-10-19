using UnityEngine;
using System.Collections;

public class ScoreCounter : MonoBehaviour {
    public GUIText meterText;

    Player player;

    // Use this for initialization
    void Start() {
        GameObject playerObj = GameObject.Find("Player");
        this.player = playerObj.GetComponent<Player>();
    }
    
    // Update is called once per frame
    void Update() {
        this.meterText.text = this.player.pos.y.ToString();
    }
}
