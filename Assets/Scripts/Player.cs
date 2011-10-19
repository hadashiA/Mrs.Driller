using UnityEngine;
using System.Collections;
using System.Collections.Generic;

enum Direction {
    Left, Right, Up, Down
};

public class Player : MonoBehaviour {
    Direction direction;
    float nextDigTime = 0;

    StageState state;
    BlockController blockController;

    IEnumerator walk;
    IEnumerator drop;

    // Use this for initialization
    void Start() {
        GameObject game = GameObject.Find("Game");
        this.state = game.GetComponent<StageState>();
        this.blockController = game.GetComponent<BlockController>();

        this.direction = Direction.Down;

        this.drop = Drop();
    }
    
    // Update is called once per frame
    void Update() {
        // Drop
        if (this.drop != null && !this.drop.MoveNext()) {
            this.drop = null;
        }

        // Dig
        if (Input.GetButton("Fire1") && Time.time > nextDigTime) {
            nextDigTime = Time.time + state.digTimeRate;
            Dig();
        }
        
        if (this.walk == null) {
            if (Input.GetKey(KeyCode.LeftArrow)) {
                this.walk = WalkTo(Direction.Left);
                
            } else if (Input.GetKey(KeyCode.RightArrow)) {
                this.walk = WalkTo(Direction.Right);
                
            } else if (Input.GetKey(KeyCode.UpArrow)) {
                this.direction = Direction.Up;
                
            } else if (Input.GetKey(KeyCode.DownArrow)) {
                this.direction = Direction.Down;
            }

        } else if (!this.walk.MoveNext()) {
            this.walk = null;
        }
    }

    void Dig() {
        GameObject block = NextBlock(this.direction);
        blockController.Remove(block);
        if (this.direction == Direction.Down) {
            this.drop = Drop();
        }
    }

    GameObject NextBlock(Direction d) {
        Vector2 pos = transform.position;

        switch (d) {
            case Direction.Left:
                pos.x -= state.blockSize;
                break;
            case Direction.Right:
                pos.x += state.blockSize;
                break;
            case Direction.Up:
                pos.y += state.blockSize;
                break;
            case Direction.Down:
                pos.y -= state.blockSize;
                break;
        }

        return blockController.BlockAtPos(pos);
    }

    IEnumerator WalkTo(Direction d) {
        this.direction = d;

        if ((d == Direction.Left && NextBlock(Direction.Left) != null) ||
            (d == Direction.Right && NextBlock(Direction.Right) != null)) {
            yield break;
        }

        float walkFrom = transform.position.x;
        int sign = (d == Direction.Left ? -1 : 1);
        float walkTotal = 0;
        float distance = state.blockSize;

        while (walkTotal < distance * 0.9) {
            float speedPerFrame = state.walkSpeed * Time.deltaTime;
            transform.Translate(speedPerFrame * sign, 0, 0);
            walkTotal += speedPerFrame;
            yield return true;
        }
        
        transform.position =
            new Vector2(walkFrom + distance * sign, transform.position.y);
    }

    IEnumerator Drop() {
        GameObject downBlock = NextBlock(Direction.Down);

        while (downBlock == null) {
            float gravityPerFrame = state.gravity * Time.deltaTime;
            float nextY = transform.position.y - gravityPerFrame;

            downBlock = blockController.BlockAtPos(
                new Vector2(transform.position.x, nextY)
            );

            if (downBlock != null) {
                transform.position = new Vector2(
                    transform.position.x,
                    downBlock.transform.position.y + state.blockSize
                );
                yield break;
            }

            transform.Translate(0, -gravityPerFrame, 0);
            yield return true;
        }
    }
}
