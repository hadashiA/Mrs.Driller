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

    BlockController blockController;

    IEnumerator shake;
    IEnumerator drop;

    public override string ToString() {
        return "color:" + this.color + " pos:" + pos;
    }

    public void Drop() {
        this.shake = ShakeStart();
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
            this.drop = DropStart();
        }

        if (this.drop != null && !this.drop.MoveNext()) {
            this.drop = null;
        }

        transform.position = blockController.ScreenPos(this.pos);
    }

    IEnumerator DropStart() {
        float nextFoot = this.pos.y + 1;
        Block downBlock = blockController.BlockAtPos(this.pos.x, nextFoot);

        while (downBlock == null) {
            float gravityPerFrame = blockController.gravity * Time.deltaTime;

            nextFoot = this.pos.y + gravityPerFrame + 1;
            downBlock = blockController.BlockAtPos(this.pos.x, nextFoot);

            if (this.pos.y % 1 > 0.5f) {
                blockController.Fixed(this);
            }

            if (downBlock != null) {
                this.pos.y = downBlock.pos.y - 1;
                yield break;
            }

            this.pos.y += gravityPerFrame;
            yield return true;
        }
    }

    IEnumerator ShakeStart() {
        float beforeShake = Time.time;
        float beforeX = pos.x;

        while (true) {
            float total = Time.time - beforeShake;
            float progress = total / this.shakeTime;
            Debug.Log(progress);

            if (total > this.shakeTime) {
                pos.x = beforeX;
                yield break;
            } else {
                pos.x += 0.02f * (progress) * (progress > 0.5 ? -1 : 1);
                yield return true;
            }
        }
        // yield return new WaitForSeconds(this.shakeTime);
    }
}
