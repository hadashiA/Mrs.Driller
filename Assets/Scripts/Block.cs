using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BlockGroup {
    public int Count {
        get { return this.blocks.Count; }
    }

    public bool unbalance = false;

    public bool blinking {
        get { return this.blink != null; }
    }

    HashSet<Block> blocks;

    BlockController blockController;

    IEnumerator blink;

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

    public void BlinkStart() {
        this.blink = GetBlinkEnumerator();
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
                    foreach (Block underMember in underBlock.group) {
                        Block underUnderBlock = blockController.NextBlock(
                            underMember.pos, Direction.Down
                        );
                        if (underUnderBlock != null &&
                            !history.Contains(underUnderBlock.group)) {
                            return;
                        }
                    }
                    SearchUnbalanceGroupsRecursive(result, history, underBlock.group);
                }
            }
        }

        group.unbalance = true;
        result.Add(group);

        // // 自分に乗っているグループ調べる
        foreach (BlockGroup upperBlock in group.SearchUpperGroups()) {
            SearchUnbalanceGroupsRecursive(result, history, upperBlock);
        }
    }

    IEnumerator GetBlinkEnumerator() {
        float beforeBlink = Time.time;

        while (Time.time - beforeBlink < blockController.blinkTime) {
            float alpha = Mathf.Sin(Time.time * 100.0f);
            foreach (Block member in this.blocks) {
                Color color = member.renderer.material.color;
                color.a = alpha;
                member.renderer.material.color = color;
            }
            yield return true;
        }

        foreach (Block member in this.blocks) {
            Color color = member.renderer.material.color;
            color.a = 0;
            member.renderer.material.color = color;
        }
    }
}

public class Block : MonoBehaviour {
    public float shakeTime = 0.5f;

    public BlockData data {
        set {
            this.color = value.color;
            this.group = value.group;
        }
    }

    public Vector2 pos;

    public BlockGroup group;

    BlockColor _color;
    public BlockColor color {
        get {
            return this._color;
        }
        set {
            this._color = value;
            renderer.material = this.blockMaterials[(int)this._color];
        }
    }

    public bool shaking {
        get { return this.shake != null;  }
    }

    public bool dropping {
        get { return this.drop != null; }
    }

    public bool unbalance {
        get { return this.shaking || this.dropping; }
    }

    IEnumerator shake;
    IEnumerator drop;

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

        while (Time.time - beforeShake < this.shakeTime) {
            pos.x += Mathf.Sin(Time.time * 50.0f) / 30.0f;
            yield return true;
        }
        pos.x = beforeX;
    }

    IEnumerator GetDropEnumerator(float gravity) {
        while (true) {
            float gravityPerFrame = gravity * Time.deltaTime;
            this.pos.y += gravityPerFrame;
            yield return true;
        }
    }
}
