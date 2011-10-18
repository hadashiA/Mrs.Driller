using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockController : MonoBehaviour {
    Vector2 playerPos {
        get { return PositionAt(state.playerRow, state.playerCol); }
    }

    public GameObject meterText;

    StageState state;
    GameObject player;
    Player playerBehaiviour;

    IEnumerator<float> playerDrop;
    IEnumerator<float> playerWalk;

    public void DigAt(int row, int col) {
        if (row < 0 || row >= state.numBlockRows ||
            col < 0 || col >= state.numBlockCols) {
            return;
        }

        GameObject block = state.blocks[row, col];
        if (block != null) {
            Destroy(block);
            state.blocks[row, col] = null;
        }
            
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
        // Player drop translate
        Vector2 playerSrc = player.transform.position;
        float playerSrcY  = playerSrc.y;
        float playerDestY = this.playerPos.y;

        if (playerSrcY > playerDestY) {
            if (this.playerDrop == null) 
                this.playerDrop = MoveTo(playerSrcY, playerDestY, state.gravity);
            
            if (this.playerDrop.MoveNext()) {
                player.transform.Translate(0, -this.playerDrop.Current, 0);
            } else {
                player.transform.position = new Vector2(playerSrc.x, playerDestY);
                this.playerDrop = null;
                this.playerBehaiviour.idle = true;
            }
        }

        // Player walk translate
        float playerSrcX  = playerSrc.x;
        float playerDestX = this.playerPos.x;

        if (playerSrcX != playerDestX) {
            if (this.playerWalk == null) 
                this.playerWalk = MoveTo(playerSrcX, playerDestX, state.walkSpeed);

            if (this.playerWalk.MoveNext()) {
                float sign = Mathf.Sign(playerDestX - playerSrcX);
                player.transform.Translate(this.playerWalk.Current * sign, 0, 0);
            } else {
                player.transform.position = new Vector2(playerDestX, playerSrc.y);
                this.playerWalk = null;
                this.playerBehaiviour.idle = true;
            }
        }

        // Blocks scroll translate
        float gravityPerFrame = state.gravity * Time.deltaTime;
        for (int row = 0; row < state.numBlockRows; row++) {
            for (int col = 0; col < state.numBlockCols; col++) {
                GameObject block = state.blocks[row, col];
                if (block == null) continue;

                float blockSrcY  = block.transform.position.y;
                float blockDestY = PositionAt(row, col).y;

                if (blockSrcY < blockDestY) {
                    if (blockSrcY + gravityPerFrame > blockDestY) {
                        block.transform.position = new Vector2(
                            block.transform.position.x, blockDestY
                        );
                        playerBehaiviour.idle = true;
                            
                    } else {
                        block.transform.Translate(new Vector2(0, gravityPerFrame));
                    }
                }
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

    IEnumerator<float> MoveTo(float moveFrom, float moveTo, float speed) {
        float speedPerFrame = speed * Time.deltaTime;
        float distance = Mathf.Abs(moveTo - moveFrom);
        float movedTotal = speedPerFrame;

        while (distance > movedTotal) {
            movedTotal += speedPerFrame;
            yield return speedPerFrame;
        }
    }
}
