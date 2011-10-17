using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

enum Direction {
    Left, Right, Up, Down
};

public class Player : MonoBehaviour {
    public float walkSpeed = 3.0f;

    public float digTimeRate = 0.5f;

    Direction direction;

    float nextDigTime = 0;

    bool _idle  = true;
    public bool idle {
        set { this._idle = value; }
    }

    StageState state;

    // Use this for initialization
    void Start() {
        GameObject game = GameObject.Find("Game");

        this.state = game.GetComponent<StageState>();
        this.direction = Direction.Down;

        Drop();
    }
    
    // Update is called once per frame
    void Update() {
        Drop();

        // Dig
        if (Input.GetButton("Fire1") && Time.time > nextDigTime) {
            nextDigTime = Time.time + digTimeRate;
            Dig();
        }

        // Walk
        if (this._idle) {
            if (Input.GetKey(KeyCode.LeftArrow)) {
                this.direction = Direction.Left;
                WalkTo(Direction.Left);
            } else if (Input.GetKey(KeyCode.RightArrow)) {
                this.direction = Direction.Right;
                WalkTo(Direction.Right);
            } else if (Input.GetKey(KeyCode.UpArrow)) {
                this.direction = Direction.Up;
            } else if (Input.GetKey(KeyCode.DownArrow)) {
                this.direction = Direction.Down;
            }
        }
    }

    void Drop() {
        if (this._idle) {
            int downRow = state.playerRow + 1;
            if (downRow < state.numBlockRows &&
                state.blocks[downRow, state.playerCol] == null) {
                state.playerRow = downRow;
                this._idle = false;
            }
        }
    }

    void Dig() {
    }

    void WalkTo(Direction direction) {
        if (this._idle) {
            switch (direction) {
                case Direction.Left:
                    if (state.playerCol > 0) {
                        state.playerCol--;
                        this._idle = false;
                    }
                    break;
                case Direction.Right:
                    if (state.playerCol < state.numBlockCols - 1) {
                        state.playerCol++;
                        this._idle = false;
                    }
                    break;
            }
        }
    }
}
