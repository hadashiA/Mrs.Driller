using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Block : MonoBehaviour {
    public enum Type {
        Blue = 0, Green, Pink, Yellow,
        // Hard,
        // Air,
        Empty
    }

    public class Group {
        static int nextId = 0;

        public int id;

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

        public Group(BlockController blockController) {
            this.id = ++nextId;

            this.blockController = blockController;
            this.blocks = new HashSet<Block>();
        }

        public void Grouping(Block block) {
            if (block == null || block.type == Block.Type.Empty) return;

            if (!Add(block)) return;

            // 上下左右
            foreach (Direction d in System.Enum.GetValues(typeof(Direction))) {
                if (blockController.Collision(block.pos, d) == block.type) {
                    Grouping(blockController.NextBlock(block.pos, d));
                }
            }
        }

        public HashSet<Group> LookUpUpperGroups() {
            HashSet<Group> result = new HashSet<Group>();

            foreach (Block member in this.blocks) {
                if (member.pos.y < 1) continue;

                Block upperBlock = blockController.NextBlock(member.pos, Direction.Up);
                if (upperBlock != null && upperBlock.group != this) {
                    result.Add(upperBlock.group);
                }
            }

            return result;
        }

        public HashSet<Group> LookUpUnbalanceGroups() {
            HashSet<Group> result  = new HashSet<Group>();
            HashSet<Group> history = new HashSet<Group>();

            LookUpUnbalanceGroupsRecursive(result, history, this);

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

        void LookUpUnbalanceGroupsRecursive(HashSet<Group> result,
                                            HashSet<Group> history,
                                            Group group) {
            if (!history.Add(group)) return;

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
                        LookUpUnbalanceGroupsRecursive(result, history, underBlock.group);
                    }
                }
            }

            group.unbalance = true;
            result.Add(group);

            // // 自分に乗っているグループ調べる
            foreach (Group upperBlock in group.LookUpUpperGroups()) {
                LookUpUnbalanceGroupsRecursive(result, history, upperBlock);
            }
        }
    }

    public Material[] colorMaterials;
    public Material hardMaterial;

    public Vector2 pos;
    public Group group;

    public int stamina = 1;

    Type _type;
    public Type type {
        get {
            return this._type;
        }

        set {
            this._type = value;
            if (value == Type.Empty) {
                renderer.enabled = false;
            // } else if (value == Type.Hard) {
            //     renderer.material = this.hardMaterial;
            //     this.stamina = 5;
            } else {
                renderer.enabled = true;
                renderer.material = this.colorMaterials[(int)value];
            }
        }
    }

    public bool shaking {
        get { return this.shake != null;  }
    }

    public bool dropping {
        get { return this.drop != null; }
    }

    public bool blinking {
        get { return this.blink != null; }
    }

    public bool unbalance {
        get { return this.shaking || this.dropping; }
    }

    IEnumerator shake;
    IEnumerator drop;
    IEnumerator blink;

    public override string ToString() {
        return "type:" + this.type + " pos:" + pos;
    }

    public void ShakeStart(float shakeTime) {
        this.shake = GetShakeEnumerator(shakeTime);
    }

    public void DropStart(float gravity) {
        this.drop = GetDropEnumerator(gravity);
    }

    public void DropEnd() {
        this.drop = null;
    }

    public bool ShakeNext() {
        bool result = false;

        if (this.shake != null) {
            result = this.shake.MoveNext();
            if (!result) {
                this.shake = null;
            }                 
        }

        return result;
    }

    public bool DropNext() {
        if (this.drop != null) {
            return this.drop.MoveNext();
        } else {
            return false;
        }
    }
    
    public void BlinkStart(float blinkTime) {
        this.blink = GetBlinkEnumerator(blinkTime);
    }

    public bool BlinkNext() {
        bool result = false;

        if (this.blink != null) {
            result = this.blink.MoveNext();
            if (!result) {
                this.blink = null;
            }
        }

        return result;
    }

    IEnumerator GetShakeEnumerator(float shakeTime) {
        float beforeShake = Time.time;
        float beforeX = pos.x;

        while (Time.time - beforeShake < shakeTime) {
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

    IEnumerator GetBlinkEnumerator(float blinkTime) {
        float beforeBlink = Time.time;
        Color color = renderer.material.color;

        while (Time.time - beforeBlink < blinkTime) {
            float alpha = Mathf.Sin(Time.time * 1000.0f);
            color.a = alpha;
            renderer.material.color = color;

            yield return true;
        }
        color.a = 255;
        renderer.material.color = color;
    }
} 