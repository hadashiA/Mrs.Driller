using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockController : MonoBehaviour {
    StageState state;

    float scrolledY = 0;

    GameObject player;

    GameObject blockGroupPrefab;

    public GameObject BlockAtPos(Vector2 pos) {
        int col = Mathf.FloorToInt(pos.x * state.blockSize);
        int row = -Mathf.FloorToInt((pos.y - scrolledY) * state.blockSize);

        if (col < 0 || col >= state.numBlockCols ||
            row < 0 || row >= state.numBlockRows) {
            return null;
        } else {
            return state.blocks[row, col];
        }
    }

    public GameObject BlockAtPos(float x, float y) {
        return BlockAtPos(new Vector2(x, y));
    }

    public void Remove(GameObject block) {
        if (block == null) return;
        
        Vector2 pos = block.transform.position;
        int col = Mathf.FloorToInt(pos.x * state.blockSize);
        int row = -Mathf.FloorToInt((pos.y - scrolledY) * state.blockSize);

        Destroy(block);
        state.blocks[row, col] = null;

        // GameObject upBlock = this.blocks[row, col];
        // if (upBlock != null) {
        
        // }
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
                block.name = block.renderer.material.name;
                state.blocks[row, col] = block;
            }
        }

        this.player =
            Instantiate(
                state.playerPrefab,
                PositionAt(state.playerRow, state.playerCol),
                Quaternion.identity
            ) as GameObject;
    }
    
    void Update() {
        // Scroll blocks
        Vector2 playerPos = player.transform.position;
        if (playerPos.y < state.cameraFixedY) {
            float cameraDiff = state.cameraFixedY - playerPos.y;
            
            // Debug.Log("playerY:" + player.transform.position.y +
            //           " fixedY:" + state.cameraFixedY + 
            //           " cameraDiff" + cameraDiff);
            
            float nextScrolledY = this.scrolledY + cameraDiff;
            // if (nextScrolledY % state.blockSize > 0.9f) {
            //     nextScrolledY = Mathf.Floor(this.scrolledY) + state.blockSize;
            //     cameraDiff = nextScrolledY - this.scrolledY;
            // }

            player.transform.Translate(0, cameraDiff, 0);
            foreach (GameObject block in
                     GameObject.FindGameObjectsWithTag("Block")) {
                block.transform.Translate(0, cameraDiff, 0);
            } 

            this.scrolledY = nextScrolledY;
        }
    }

    Vector2 PositionAt(int row, int col) {
        return new Vector2(
            col * state.blockSize,
            -((row + this.scrolledY) * state.blockSize)
        );
    }

    void Grouping(GameObject parent, GameObject block) {
        if (block.transform.parent == parent.transform) return;
        
        block.transform.parent = parent.transform;
        Vector2 pos = block.transform.position;

        // 上下左右
        for (int rowOffset = -1; rowOffset <= 1; rowOffset++) {
            for (int colOffset = -1; colOffset <= 1; colOffset++) {
                if ((rowOffset != 0 && colOffset != 0) ||
                    (rowOffset == 0 && colOffset == 0)) continue;

                GameObject nextBlock = BlockAtPos(
                    pos.x + colOffset * state.blockSize,
                    pos.y + colOffset * state.blockSize
                );
                
                if (nextBlock != null && nextBlock.name == block.name) 
                    Grouping(parent, nextBlock);
            }
        }
    }
}
