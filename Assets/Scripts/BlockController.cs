using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockController : MonoBehaviour {
    public Vector2 playerPos {
        get { return PositionAt(state.playerRow, state.playerCol); }
    }

    public GameObject meterText;

    StageState state;
    GameObject player;
    Player playerBehaiviour;

    public void DigAt(int row, int col) {
    }

    // Use this for initialization
    void Start() {
        this.state = GetComponent<StageState>();

        state.blocks = new GameObject[state.numBlockRows, state.numBlockCols];

        // random test data setting
        for (int row = 5; row < state.numBlockRows; row++) {
            for (int col = 0; col < state.numBlockCols; col++) {
                GameObject block = Instantiate(
                    state.blockPrefab, PositionAt(row, col), Quaternion.identity
                ) as GameObject;
                
                int materialIndex = Random.Range(0, state.blockMaterials.Length);
                block.renderer.material = state.blockMaterials[materialIndex];
                state.blocks[row, col] = block;
            }
        }

        this.player =
            Instantiate(state.playerPrefab, this.playerPos, Quaternion.identity)
              as GameObject;
        this.playerBehaiviour = player.GetComponent<Player>();
    }
    
    void Update() {
        float gravityPerFrame = state.gravity * Time.deltaTime;

        // Player drop translate
        float playerY = player.transform.position.y;
        float moveToY = this.playerPos.y;
        if (moveToY < playerY) {
            if (playerY - gravityPerFrame < moveToY * 0.9f) {
                transform.position = new Vector2(
                    player.transform.position.x, moveToY
                );
                playerBehaiviour.idle = true;
                
            } else {
                player.transform.Translate(new Vector2(0, -gravityPerFrame));
            }
        }

        // Blocks scroll translate
        for (int row = 0; row < state.numBlockRows; row++) {
            for (int col = 0; col < state.numBlockCols; col++) {
                GameObject block = state.blocks[row, col];
                if (block == null) continue;

                float blockY = block.transform.position.y;
                moveToY = PositionAt(row, col).y;

                if (moveToY > blockY) {
                    if (blockY + gravityPerFrame > moveToY * 0.9f) {
                        block.transform.position = new Vector2(
                            block.transform.position.x, moveToY
                        );
                        playerBehaiviour.idle = true;
                            
                    } else {
                        block.transform.Translate(new Vector2(0, gravityPerFrame));
                    }
                }
            }
        }

        // Player walking translate
        float playerX = player.transform.position.x;
        float moveToX = this.playerPos.x;
        if (moveToX != playerX) {
            float distance = moveToX - playerX;
            float walkPerFrame =
                playerBehaiviour.walkSpeed * Time.deltaTime * Mathf.Sign(distance);
            float nextX = playerX + walkPerFrame;
            
            if ((distance > 0 && nextX > moveToX * 0.9f) ||
                (distance < 0 && nextX < moveToX * 0.9f)) {
                transform.position = new Vector2(
                    this.playerPos.x, transform.position.y
                );
                playerBehaiviour.idle = true;
                
            } else {
                transform.Translate(new Vector2(walkPerFrame, 0));
            }
        }
    }

    Vector2 PositionAt(int row, int col) {
        float x = col * state.blockSize;
        float y = -(row * state.blockSize);

        int cameraDiff = state.playerRow - state.cameraFixedRow;
        if (cameraDiff > 0) {
            y += (state.playerRow - cameraDiff) * state.blockSize;
        }
        return state.screenTop + new Vector2(x, y);
    }
}
