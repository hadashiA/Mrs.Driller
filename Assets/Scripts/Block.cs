using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BlockGroup {
    public bool unbalance = false;

    HashSet<Block> blocks;

    BlockController blockController;

    public BlockGroup(BlockController blockController) {
        this.blockController = blockController;
        this.blocks = new HashSet<Block>();
    }

    public void Grouping(Block block) {
        if (!Add(block)) return;

        // 上下左右
        foreach (Direction d in Enum.GetValues(typeof(Direction))) {
            Block nextBlock = blockController.NextBlock(block.pos, d);
                
            if (nextBlock != null && nextBlock.color == block.color) 
                Grouping(nextBlock);
        }
    }

    public HashSet<BlockGroup> SearchUpperGroups() {
        HashSet<BlockGroup> result = new HashSet<BlockGroup>();

        foreach (Block member in this.blocks) {
            if (member.pos.y < 1) continue;

            Block upperBlock = blockController.NextBlock(member.pos, Direction.Up);
            if (upperBlock != null && upperBlock.group != this) {
                result.Add(upperBlock.group);
            }
        }

        return result;
    }

    public HashSet<BlockGroup> SearchUnbalanceGroups() {
        HashSet<BlockGroup> result  = new HashSet<BlockGroup>();
        HashSet<BlockGroup> history = new HashSet<BlockGroup>();

        SearchUnbalanceGroupsRecursive(result, history, this);

        return result;
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

    void SearchUnbalanceGroupsRecursive(HashSet<BlockGroup> result,
                                        HashSet<BlockGroup> history,
                                        BlockGroup group) {
        history.Add(group);

        // 自分が乗っかっているグループ調べる
        foreach (Block member in group.blocks) {
            if (member.pos.y > blockController.numBlockRows - 2) continue;

            Block underBlock = blockController.NextBlock(member.pos, Direction.Down);
            if (underBlock != null && underBlock.group != group) {
                if (history.Contains(underBlock.group)) {
                    if (!underBlock.group.unbalance) return;
                } else {
                    SearchUnbalanceGroupsRecursive(result, history, underBlock.group);
                }
            }
        }

        group.unbalance = true;
        result.Add(group);

        // // 自分に乗っているグループ調べる
        // foreach (BlockGroup upperBlock in group.SearchUpperGroups()) {
        //     SearchUnbalanceGroupsRecursive(result, history, upperBlock);
        // }
    }
}

public class Block : MonoBehaviour {
    public enum Color {
        Blue = 0, Green, Pink, Yellow
    }

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

    public void DropStart(float gravity) {
        this.drop = GetDropEnumerator(gravity);
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
