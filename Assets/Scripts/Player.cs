using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

enum Direction {
    Left, Right, Up, Down
};

public class Player : MonoBehaviour {
    public bool idle  = true;

    Direction direction;
    float nextDigTime = 0;

    StageState state;
    BlockController blockController;

    // Use this for initialization
    void Start() {
        GameObject game = GameObject.Find("Game");
        this.state = game.GetComponent<StageState>();
        this.blockController = game.GetComponent<BlockController>();

        this.direction = Direction.Down;

        Drop();
    }
    
    // Update is called once per frame
    void Update() {
        Drop();
        
        // Dig
        if (Input.GetButton("Fire1") && Time.time > nextDigTime) {
            nextDigTime = Time.time + state.digTimeRate;
            Dig();
        }
        
        if (Input.GetKey(KeyCode.LeftArrow)) {
            this.direction = Direction.Left;
            WalkTo(Direction.Left);
            blockController.Point(state.playerRow, state.playerCol - 1);

        } else if (Input.GetKey(KeyCode.RightArrow)) {
            this.direction = Direction.Right;
            WalkTo(Direction.Right);
            blockController.Point(state.playerRow, state.playerCol + 1);

        } else if (Input.GetKey(KeyCode.UpArrow)) {
            this.direction = Direction.Up;
            blockController.Point(state.playerRow - 1, state.playerCol);

        } else if (Input.GetKey(KeyCode.DownArrow)) {
            this.direction = Direction.Down;
            blockController.Point(state.playerRow + 1, state.playerCol);
        }
    }

    void Drop() {
        if (this.idle) {
            int downRow = state.playerRow + 1;
            if (downRow < state.numBlockRows &&
                state.blocks[downRow, state.playerCol] == null) {
                state.playerRow = downRow;
                this.idle = false;
            }
        }
    }

    void Dig() {
        if (this.idle) {
            switch (this.direction) {
                case Direction.Down:
                    blockController.DigAt(state.playerRow + 1, state.playerCol);
                    break;
                case Direction.Up:
                    blockController.DigAt(state.playerRow - 1, state.playerCol);
                    break;
                case Direction.Left:
                    blockController.DigAt(state.playerRow, state.playerCol - 1);
                    break;
                case Direction.Right:
                    blockController.DigAt(state.playerRow, state.playerCol + 1);
                    break;
            }
        }
    }

    void WalkTo(Direction direction) {
        if (this.idle) {
            switch (direction) {
                case Direction.Left:
                    if (state.playerCol > 0 &&
                        state.blocks[state.playerRow, state.playerCol - 1] == null) {
                        state.playerCol--;
                        this.idle = false;
                    }
                    break;
                case Direction.Right:
                    if (state.playerCol < state.numBlockCols - 1 &&
                        state.blocks[state.playerRow, state.playerCol + 1] == null) {
                        state.playerCol++;
                        this.idle = false;
                    }
                    break;
            }
        }
    }
}
