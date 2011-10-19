using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockController : MonoBehaviour {
    public float gravity = 5.0f;
    public float cameraFixedY = -5.0f;

    public int numBlockRows = 100;
    public int numBlockCols = 15;

    public GameObject blockPrefab;
    public Material[] blockMaterials;

    public float blockSize {
        get { return this.blockPrefab.transform.localScale.x; }
    }

    Block[,] blocks;
    List<Block> dropBlocks;

    public Vector2 ScreenPos(Vector2 pos) {
        return new Vector2(
            pos.x, -(pos.y)
        );
    }

    public Block BlockAtPos(Vector2 pos) {
        int col = Mathf.FloorToInt(pos.x);
        int row = Mathf.FloorToInt(pos.y);

        if (col < 0 || col >= this.numBlockCols ||
            row < 0 || row >= this.numBlockRows) {
            return null;
        } else {
            return this.blocks[row, col];
        }
    }

    public Block BlockAtPos(float x, float y) {
        return BlockAtPos(new Vector2(x, y));
    }

    public void Remove(Block block) {
        if (block != null) {
            int col = Mathf.FloorToInt(block.pos.x);
            int row = Mathf.FloorToInt(block.pos.y);
            if (this.blocks[row, col] != null) {
                UnFixed(block);
                Destroy(block.gameObject);

                if (row > 0) {
                    Block upBlock = this.blocks[row - 1, col];
                    if (upBlock != null) {
                        FindDropBlocks(upBlock);
                        Debug.Log(this.dropBlocks.Count);
                    }
                }
            }            
        }
    }

    public void Fixed(Block block) {
        int col = Mathf.FloorToInt(block.pos.x);
        int row = Mathf.FloorToInt(block.pos.y);

        if (this.blocks[row, col] == null) {
            this.blocks[row, col] = block;
        }
    }

    public void UnFixed(Block block) {
        int col = Mathf.FloorToInt(block.pos.x);
        int row = Mathf.FloorToInt(block.pos.y);

        this.blocks[row, col] = null;
    }

    // Use this for initialization
    void Start() {
        this.blocks = new Block[this.numBlockRows, this.numBlockCols];
        this.dropBlocks = new List<Block>();

        // random test data setting
        for (int row = 5; row < this.numBlockRows; row++) {
            for (int col = 0; col < this.numBlockCols; col++) {
                Vector2 pos = new Vector2(col, row);

                GameObject blockObj = Instantiate(
                    this.blockPrefab, ScreenPos(pos), Quaternion.identity
                ) as GameObject;

                int materialIndex = Random.Range(0, this.blockMaterials.Length);
                blockObj.renderer.material = this.blockMaterials[materialIndex];
                
                Block block = blockObj.GetComponent<Block>();
                block.color = materialIndex;
                block.pos   = pos;

                this.blocks[row, col] = block;
            }
        }
    }

    void Update() {
    }
    
    void FindDropBlocks(Block block) {
        this.dropBlocks.Add(block);

        int col = Mathf.FloorToInt(block.pos.x);
        int row = Mathf.FloorToInt(block.pos.y);

        if (row > 0) {
            Block upBlock = this.blocks[row - 1, col];
            if (upBlock != null) FindDropBlocks(upBlock);
        }
    }

    void Grouping(BlockGroup group, Block block) {
        if (block.group == group) return;
        
        block.group = group;

        // 上下左右
        for (int rowOffset = -1; rowOffset <= 1; rowOffset++) {
            for (int colOffset = -1; colOffset <= 1; colOffset++) {
                if ((rowOffset != 0 && colOffset != 0) ||
                    (rowOffset == 0 && colOffset == 0)) continue;

                Block nextBlock = BlockAtPos(
                    block.pos.x + colOffset,
                    block.pos.y + rowOffset
                );
                
                if (nextBlock != null && nextBlock.color == block.color) 
                    Grouping(group, nextBlock);
            }
        }
    }
}
