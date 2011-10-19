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

    BlockController blockController;

    IEnumerator drop;

    public override string ToString() {
        return "color:" + this.color + " pos:" + pos;
    }

    public void Drop() {
        this.drop = DropStart();

        Block upBlock = blockController.BlockAtPos(this.pos.x, this.pos.y - 1);
        if (upBlock != null) {
            upBlock.Drop();
        }
    }

    // Use this for initialization
    void Start() {
        GameObject game = GameObject.Find("Game");
        this.blockController = game.GetComponent<BlockController>();
    }
    
    // Update is called once per frame
    void Update() {
        if (this.drop != null && !this.drop.MoveNext()) 
            this.drop = null;

        transform.position = blockController.ScreenPos(this.pos);
    }

    IEnumerator DropStart() {
        float nextFoot = this.pos.y + 1;
        Block downBlock =
            blockController.BlockAtPos(this.pos.x, nextFoot);

        while (downBlock == null) {
            float gravityPerFrame = blockController.gravity * Time.deltaTime;

            nextFoot = this.pos.y + gravityPerFrame + 1;
            downBlock = blockController.BlockAtPos(this.pos.x, nextFoot);

            if (downBlock != null) {
                this.pos.y = downBlock.pos.y - 1;
                yield break;
            }

            this.pos.y += gravityPerFrame;
            yield return true;
        }
    }

}
