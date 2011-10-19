using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockGroup {
    static int nextId = 0;

    int id;

    HashSet<Block> blocks;

    public BlockGroup() {
        this.id = nextId++;
    }

    public void Add(Block block) {
        blocks.Add(block);
    }

    public static bool operator==(BlockGroup a, BlockGroup b) {
        return a.id == b.id;
    }
        
    public static bool operator!=(BlockGroup a, BlockGroup b) {
        return a.id != b.id;
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

    public bool unfixed {
        get { return this.drop != null; }
    }

    public bool dropStarted {
        get { return this.shaking || this.unfixed; }
    }

    public bool dropEnded {
        get { return !this.dropStarted; }
    }

    IEnumerator drop;
    IEnumerator shake;

    BlockController blockController;

    public override string ToString() {
        return "color:" + this.color + " pos:" + pos;
    }

    public void DropStart() {
        this.shake = GetShakeEnumerator();
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
            this.drop = GetDropEnumerator();
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
        float nextFoot = this.pos.y + 1;

        while (true) {
            float gravityPerFrame = blockController.gravity * Time.deltaTime;
            this.pos.y += gravityPerFrame;
            yield return true;
        }
    }
}
