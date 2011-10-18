using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Block : MonoBehaviour {
    StageState state;

    IEnumerator<float> shake;

    public void Shake() {
        GameObject game = GameObject.Find("Game");
        this.state = game.GetComponent<StageState>();
    }

    // Use this for initialization
    void Start() {
    }
    
    // Update is called once per frame
    void Update() {
    }

    // IEnumerator<float> ShakeStart() {
    // }
}
