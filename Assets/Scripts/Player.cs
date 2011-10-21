using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {
    public float walkSpeed = 3.0f;
    public float walkToUpperWait = 0.5f;

    public float digTimeRate = 0.5f;
    
    public Vector2 pos = new Vector2(7, 0);

    // Direction direction;
    Direction direction;
    
    float nextDigTime = 0;

    BlockController blockController;

    IEnumerator walk;
    IEnumerator drop;

    bool walkButtonOn = false;

    public void DropStart() {
        this.drop = GetDropEnumerator();
    }

    // Use this for initialization
    void Start() {
        GameObject game = GameObject.Find("Game");
        this.blockController = game.GetComponent<BlockController>();

        this.direction = Direction.Down;
        DropStart();
    }
    
    // Update is called once per frame
    void Update() {
        // Drop
        if (this.drop != null && !this.drop.MoveNext()) 
            this.drop = null;
        
        // Dig
        if (Input.GetButton("Fire1") && Time.time > nextDigTime) {
            nextDigTime = Time.time + this.digTimeRate;
            this.walkButtonOn = false;
            Dig();
        }
        
        // Walk
        if (this.walk != null && !this.walk.MoveNext()) {
            this.walk = null;
            if (!blockController.Collision(this.pos, Direction.Down)) 
                DropStart();
        }

        this.walkButtonOn = false;
        if (Input.GetKey(KeyCode.LeftArrow)) {
            this.walkButtonOn = true;
            if (this.walk == null)
                this.walk = GetWalkEnumerator(Direction.Left);
            
        } else if (Input.GetKey(KeyCode.RightArrow)) {
            this.walkButtonOn = true;
            if (this.walk == null) 
                this.walk = GetWalkEnumerator(Direction.Right);
            
        } else if (Input.GetKey(KeyCode.UpArrow)) {
            this.direction = Direction.Up;
            
        } else if (Input.GetKey(KeyCode.DownArrow)) {
            this.direction = Direction.Down;
        }
    }

    void Dig() {
        blockController.RemoveAtPos(
            this.pos + BlockController.Offset[this.direction]
        );
        
        if (this.direction == Direction.Down) {
            DropStart();
        }
    }

    IEnumerator GetWalkEnumerator(Direction d) {
        this.direction = d;

        switch (d) {
            case Direction.Left:
                if (this.pos.x < 1) {
                    yield break;
                }
                break;

            case Direction.Right:
                if (this.pos.x > blockController.numBlockCols - 2) {
                    yield break;
                }
                break;

            default:
                yield break;
        }

        Vector2 offset = Vector2.zero;
        if (this.drop != null) 
            offset.y += 1;
        
        if (!blockController.Collision(this.pos + offset, d)) {
            // いちだんうえにあがれるか
            if (!blockController.Collision(this.pos + offset, Direction.Up) &&
                !blockController.Collision(
                    this.pos + offset + blockController.Offset[d], Direction.Up
                )) {

                float beforeWait = Time.time;
                while (Time.time - beforeWait < this.walkToUpperWait) {
                    if (!walkButtonOn) yield break;
                    yield return true;
                }
                this.pos.y -= 1;
                
            } else {
                yield break;
            }
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
        while (!blockController.Collision(this.pos, Direction.Down)) {
            float gravityPerFrame = blockController.gravity * Time.deltaTime;
            this.pos.y += gravityPerFrame;

            if (blockController.Collision(this.pos, Direction.Down)) 
                break;

            yield return true;
        }
        this.pos.y = downBlock.pos.y - 1;
    }
}
