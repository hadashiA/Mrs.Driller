using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockController : MonoBehaviour {
    StageState state;

    float scrolledY;

    GameObject player;

    public GameObject BlockAtPos(Vector2 pos) {
        int col = Mathf.FloorToInt(pos.x * state.blockSize);
        int row = -Mathf.FloorToInt((pos.y + scrolledY) * state.blockSize);

        if (col < 0 || col >= state.numBlockCols ||
            row < 0 || row >= state.numBlockRows) {
            return null;
        } else {
            return state.blocks[row, col];
        }
    }

    // public void DigAt(int row, int col) {
    //     if (ValidIndex(row, col)) {
    //         HashSet<GameObject> blocks = new HashSet<GameObject>();
    //         SetSameColorBlocks(blocks, row, col);
            
    //         foreach (GameObject block in blocks) {
    //             Destroy(block);
    //             state.blocks[row, col] = null;
    //         }

    //         for (int row = state.numBlockRows - 2; row >= 0; row--) {
    //             for (int col = 0; col < state.numBlockCols; col++) {
    //                 GameObject block = BlockAt(row, col);
    //                 if (block == null) continue;
                    
    //                 GameObject down  = BlockAt(row + 1, col);
    //                 if (down == null) {
    //                     GameObject left  = BlockAt(row, col - 1);
    //                     if (left != null && left.name == block.name)
    //                         continue;
                        
    //                     GameObject right = BlockAt(row, col + 1);
    //                     if (right != null && right.name == block.name)
    //                         continue;
                        
    //                     state.blocks[row, col] = null;
    //                     state.blocks[row + 1, col] = block;
    //                 }
    //             }
    //         }
    //     }
    // }

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
    }

    Vector2 PositionAt(int row, int col) {
        return new Vector2(
            col * state.blockSize,
            -((row + this.scrolledY) * state.blockSize)
        );
    }

    // void SetSameColorBlocks(GameObject parent, int row, int col) {
    //     GameObject block = BlockAt(row, col);
        
    //     if (block == null || !result.Add(block)) return;

    //     // 上下左右
    //     for (int rowOffset = -1; rowOffset <= 1; rowOffset++) {
    //         for (int colOffset = -1; colOffset <= 1; colOffset++) {
    //             if ((rowOffset != 0 && colOffset != 0) ||
    //                 (rowOffset == 0 && colOffset == 0)) continue;

    //             GameObject nextBlock = BlockAt(row + rowOffset, col + colOffset);
    //             if (nextBlock != null && nextBlock.name == block.name) {
    //                 SetSameColorBlocks(result, row + rowOffset, col + colOffset);
    //             }
    //         }
    //     }
    // }
}
