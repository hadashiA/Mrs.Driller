using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {
    public float walkSpeed = 3.0f;
    public float digTimeRate = 0.5f;

    public Vector2 pos = new Vector2(7, 0);

    public float cameraFixed = 6.0f;

    // Direction direction;
    Direction _direction;
    Direction direction {
        get { return this._direction; }
        // Debug
        set {
            this._direction = value;
            Block nextBlock = blockController.BlockAtPos(NextPos(value));
            if (nextBlock != null) {
                foreach (Block b in nextBlock.group) {
                    Debug.DrawLine(transform.position, b.transform.position,
                                   Color.blue);
                }
            }
        }
    }

    float nextDigTime = 0;

    BlockController blockController;

    IEnumerator walk;
    IEnumerator drop;

    // Use this for initialization
    void Start() {
        GameObject game = GameObject.Find("Game");
        this.blockController = game.GetComponent<BlockController>();

        this.direction = Direction.Down;
        this.drop = GetDropEnumerator();
    }
    
    // Update is called once per frame
    void Update() {
        // Drop
        if (this.drop != null && !this.drop.MoveNext()) {
            this.drop = null;
        }

        // Dig
        if (Input.GetButton("Fire1") && Time.time > nextDigTime) {
            nextDigTime = Time.time + this.digTimeRate;
            Dig();
        }
        
        // Walk
        if (this.walk == null) {
            if (Input.GetKey(KeyCode.LeftArrow)) {
                this.walk = GetWalkEnumerator(Direction.Left);
                
            } else if (Input.GetKey(KeyCode.RightArrow)) {
                this.walk = GetWalkEnumerator(Direction.Right);
                
            } else if (Input.GetKey(KeyCode.UpArrow)) {
                this.direction = Direction.Up;
                
            } else if (Input.GetKey(KeyCode.DownArrow)) {
                this.direction = Direction.Down;
            }

        } else if (!this.walk.MoveNext()) {
            this.walk = null;
            if (NextBlock(Direction.Down) == null) 
                this.drop = GetDropEnumerator();
        }
    }

    void Dig() {
        blockController.RemoveAtPos(NextPos(this.direction));
        if (this.direction == Direction.Down) {
            this.drop = GetDropEnumerator();
        }
    }

    Vector2 NextPos(Direction d) {
        return this.pos + blockController.Offset[d];
    }

    Block NextBlock(Direction d) {
        return blockController.BlockAtPos(NextPos(d));
    }

    IEnumerator GetWalkEnumerator(Direction d) {
        this.direction = d;

        if ((d == Direction.Left && NextBlock(Direction.Left) != null) ||
            (d == Direction.Right && NextBlock(Direction.Right) != null)) {
            yield break;
        }

        float walkFrom = this.pos.x;
        int sign = (d == Direction.Left ? -1 : 1);
        float walkTotal = 0;

        while (walkTotal < 0.9f) {
            float speedPerFrame = this.walkSpeed * Time.deltaTime;
            this.pos.x += speedPerFrame * sign;
            walkTotal += speedPerFrame;
            yield return true;
        }
        
        this.pos.x = walkFrom + sign;
    }

    IEnumerator GetDropEnumerator() {
        Block downBlock = NextBlock(Direction.Down);

        while (downBlock == null) {
            float gravityPerFrame = blockController.gravity * Time.deltaTime;
            float nextFoot = this.pos.y + gravityPerFrame + 1;
            
            downBlock =
                blockController.BlockAtPos(new Vector2(this.pos.x, nextFoot));
            if (downBlock != null) {
                this.pos.y = downBlock.pos.y - 1;
                yield break;
            }

            this.pos.y += gravityPerFrame;
            yield return true;
        }
    }
}
