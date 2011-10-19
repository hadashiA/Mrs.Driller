using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Block : MonoBehaviour {
    public int color;
    public BlockGroup group;
    public Vector2 pos;

    BlockController blockController;

    IEnumerator<float> shake;

    public override string ToString() {
        return "color:" + this.color + " pos:" + pos;
    }

    // Use this for initialization
    void Start() {
        GameObject game = GameObject.Find("Game");
        this.blockController = game.GetComponent<BlockController>();
    }
    
    // Update is called once per frame
    void Update() {
        transform.position = blockController.ScreenPos(this.pos);
    }
}
