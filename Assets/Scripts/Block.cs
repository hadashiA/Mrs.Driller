using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockGroup {
    static int nextId = 0;

    // int id = 0;

    HashSet<Block> blocks;

    public BlockGroup() {
        // this.id = nextId++;
        this.blocks = new HashSet<Block>();
    }

    public bool Add(Block block) {
        block.group = this;
        return blocks.Add(block);
    }

    public IEnumerator<Block> GetEnumerator() {
        foreach (Block block in this.blocks) {
            yield return block;
        }
    }
}

public class Block : MonoBehaviour {
    public int color;

    public BlockGroup group;

    public Vector2 pos;

    public float shakeTime = 0.5f;

    public bool shaking {
        get { return this.shake != null;  }
    }

    public bool dropping {
        get { return this.drop != null; }
    }

    public bool unbalance {
        get { return this.shaking || this.dropping; }
    }

    IEnumerator drop;
    IEnumerator shake;

    BlockController blockController;

    public override string ToString() {
        return "color:" + this.color + " pos:" + pos;
    }

    public void ShakeStart() {
        this.shake = GetShakeEnumerator();
    }

    public void DropStart() {
        this.drop = GetDropEnumerator();
    }

    public void DropEnd() {
        this.drop = null;
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
        } else if (this.drop != null) {
            this.drop.MoveNext();
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
                yield break;
            } else {
                pos.x += 0.02f * (progress) * (progress > 0.5 ? -1 : 1);
                yield return true;
            }
        }
    }

    IEnumerator GetDropEnumerator() {
        while (true) {
            float gravityPerFrame = blockController.gravity * Time.deltaTime;
            this.pos.y += gravityPerFrame;
            yield return true;
        }
    }
}
