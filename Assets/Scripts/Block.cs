using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockGroup {
    HashSet<Block> blocks;

    public BlockGroup() {
    }

    public void Add(Block block) {
        blocks.Add(block);
    }
}

public class Block : MonoBehaviour {
    public int color;
    public BlockGroup group;
    public Vector2 pos;

    public float shakeTime = 0.5f;

    public bool dropStarted {
        get { return this.shake != null || this.drop != null; }
    }

    IEnumerator shake;
    IEnumerator drop;

    BlockController blockController;

    public override string ToString() {
        return "color:" + this.color + " pos:" + pos;
    }

    public void DropStart() {
        this.shake = GetShakeEnumerator();
    }

    // Use this for initialization
    void Start() {
        GameObject game = GameObject.Find("Game");
        this.blockController = game.GetComponent<BlockController>();
    }

    // Update is called once per frame
    void Update() {
        if (this.shake != null && !this.shake.MoveNext()) {
            this.shake = null;
            this.drop = GetDropEnumerator();
        } else if (this.drop != null && !this.drop.MoveNext()) {
            this.drop = null;
        }

        transform.position = blockController.ScreenPos(this.pos);
    }
    
    IEnumerator GetShakeEnumerator() {
        float beforeShake = Time.time;
        float beforeX = pos.x;

        while (true) {
            float total = Time.time - beforeShake;
            float progress = total / this.shakeTime;

            if (total > this.shakeTime) {
                pos.x = beforeX;
                blockController.UnFixed(this);
                yield break;
            } else {
                pos.x += 0.02f * (progress) * (progress > 0.5 ? -1 : 1);
                yield return true;
            }
        }
    }

    IEnumerator GetDropEnumerator() {
        float nextFoot = this.pos.y + 1;
        Block downBlock = blockController.BlockAtPos(this.pos.x, nextFoot);

        while (downBlock == null) {
            float gravityPerFrame = blockController.gravity * Time.deltaTime;

            nextFoot = this.pos.y + gravityPerFrame + 1;
            downBlock = blockController.BlockAtPos(this.pos.x, nextFoot);

            if (nextFoot % 1 > 0.5f) 
                blockController.Fixed(this);
            
            if (downBlock != null) {
                this.pos.y = downBlock.pos.y - 1;
                blockController.Fixed(this);
                yield break;
            }

            this.pos.y += gravityPerFrame;
            yield return true;
        }
    }
}
