using UnityEngine;
using System.Collections;

public class ScoreCounter : MonoBehaviour {
    public GUIText meterText;

    StageState state;

    // Use this for initialization
    void Start() {
        GameObject game = GameObject.Find("Game");
        this.state = game.GetComponent<StageState>();
    }
    
    // Update is called once per frame
    void Update() {
        this.meterText.text = state.playerRow.ToString();
    }
}
