using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Color {
    Blue = 0, Green, Pink, Yellow
}

public class BlockGroup {
    HashSet<Block> blocks;

    BlockController blockController;

    public static HashSet<BlockGroup> SearchUnbalanceGroups(BlockGroup group) {
        HashSet<BlockGroup> result  = new HashSet<BlockGroup>();
        HashSet<BlockGroup> history = new HashSet<BlockGroup>();

        SearchUnbalanceGroupsRecursive(result, history, group);

        return result;
    }

    static void SearchUnbalanceGroupsRecursive(HashSet<BlockGroup> result,
                                               HashSet<BlockGroup> history,
                                               BlockGroup group) {
        result.Add(group);
    }

    public BlockGroup(BlockController blockController) {
        this.blockController = blockController;
        this.blocks = new HashSet<Block>();
    }

    public Grouping(Block block) {
        if (!Add(block)) return;

        // 上下左右
        foreach (Direction d in Enum.GetValues(typeof(Direction))) {
            Block nextBlock = NextBlock(d);
                
            if (nextBlock != null && nextBlock.color == block.color) 
                Grouping(nextBlock);
        }
    }

    public IEnumerator<Block> GetEnumerator() {
        foreach (Block block in this.blocks) {
            yield return block;
        }
    }

    bool Add(Block block) {
        block.group = this;
        return blocks.Add(block);
    }
}

public class Block : MonoBehaviour {
    public Color color;

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

    public void MoveNext() {
        if (this.shake != null && !this.shake.MoveNext()) {
            this.shake = null;
        } else if (this.drop != null) {
            this.drop.MoveNext();
        }
    }
    
    Block NextBlock(Direction d) {
        return blockController.BlockAtPos(this.pos + blockController.Offset[d]);
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

    IEnumerator GetDropEnumerator(float gravity) {
        while (true) {
            float gravityPerFrame = gravity * Time.deltaTime;
            this.pos.y += gravityPerFrame;
            yield return true;
        }
    }
}
