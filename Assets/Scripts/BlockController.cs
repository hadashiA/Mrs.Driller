using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Direction {
    Left, Right, Down, Up
}

public enum BlockColor {
    Blue = 0, Green, Pink, Yellow
}

public struct BlockData {
    public bool exists;
    public BlockColor color;
    public BlockGroup group;
}

public class BlockController : MonoBehaviour {
    Vector2 FRAME_OUT = new Vector2(-100, -100);

    public float gravity = 5.0f;

    public float blinkTime = 0.75f;

    public float cameraFixed = 6.0f;

    public int numBlockRows = 100;
    public int numBlockCols = 15;

    public GameObject blockPrefab;
    
    public float blockSize {
        get { return this.blockPrefab.transform.localScale.x; }
    }

    BlockData[,] fixedBlockData;

    List<Block> unbalanceBlocks;
    List<Block> fixedBlocksBuffer;
    int nextFixedBlocksBufferIndex = 0;

    GameObject player;

    public static readonly Dictionary<Direction, Vector2> Offset =
        new Dictionary<Direction, Vector2>() {
        { Direction.Left,  new Vector2(-1,  0) },
        { Direction.Right, new Vector2( 1,  0) },
        { Direction.Up,    new Vector2( 0, -1) },
        { Direction.Down,  new Vector2( 0,  1) }
    };

    public Block BlockAtPos(Vector2 pos) {
        int col = Col(pos.x);
        int row = Row(pos.y);

        BlockData data = this.fixedBlockData[row, col];
        if (data.exists) {
            Block block = this.fixedBlocksBuffer[this.nextFixedBlocksBufferIndex];
            block.data = (BlockData)data;
            block.pos = new Vector2(col, row);

            this.nextFixedBlocksBufferIndex =
                (this.nextFixedBlocksBufferIndex + 1) % this.fixedBlocksBuffer.Count;

            return block;

        } else {
            return null;
        }
    }

    public Block NextBlock(Vector2 pos, Direction direction) {
        return BlockAtPos(pos + Offset[direction]);
    }

    public bool Collision(Vector2 pos, Direction? direction = null) {
        if (direction != null) 
            pos += Offset[(Direction)direction];

        int row = Row(pos.y);
        int col = Row(pos.x);

        return CollisionAtIndex(row, col);
    }

    public bool Collision(float x, float y) {
        return Collision(new Vector2(x, y));
    }

    public void RemoveAtPos(Vector2 pos) {
        int col = Col(pos.x);
        int row = Row(pos.y);

        RemoveAtIndex(row, col);
    }

    void Awake() {
        int numBlocks = this.numBlockRows * this.numBlockCols;
        
        this.fixedBlocksBuffer = new List<Block>(numBlocks);
        this.unbalanceBlocks   = new List<Block>(numBlocks / 2);

        this.fixedBlockData = new BlockData[this.numBlockRows, this.numBlockCols];

        // random test data setting
        for (int row = 5; row < this.numBlockRows; row++) {
            for (int col = 0; col < this.numBlockCols; col++) {
                int colorCount = System.Enum.GetValues(typeof(BlockColor)).Length;
                int colorIndex = Random.Range(0, colorCount);

                BlockData data = new BlockData();
                data.color = (BlockColor)colorIndex;

                this.fixedBlockData[row, col] = data;
                
                GameObject blockObj = Instantiate(
                    this.blockPrefab, FRAME_OUT, Quaternion.identity
                ) as GameObject;
                
                Block block = blockObj.GetComponent<Block>();
                block.data = data;
                this.fixedBlocksBuffer.Add(block);
            }
        }
    }

    // Use this for initialization
    void Start() {
        this.player = GameObject.Find("Player");
    }

    void Update() {
        this.nextFixedBlocksBufferIndex = 0;

        foreach (Block fixedBlock in this.fixedBlocksBuffer) {
            fixedBlock.transform.position = FRAME_OUT;
        }

        for (int row = 0; row < this.numBlockRows; row++) {
            for (int col = 0; col < this.numBlockCols; col++) {
                BlockData data = this.fixedBlockData[row, col];
                if (!data.exists) continue;

                Block block = this.fixedBlocksBuffer[row * col];
                block.data = data;
                block.pos  = new Vector2(col, row);
            }
        }
    }
    
    // Update is called once per frame
    void LateUpdate() {
        Player playerBehaviour = player.GetComponent<Player>();
        player.transform.position = ScreenPos(playerBehaviour.pos);

        float cameraDiff = playerBehaviour.pos.y - this.cameraFixed;
        if (cameraDiff > 0) {
            player.transform.Translate(0, cameraDiff, 0);
        }

        foreach (GameObject blockObj in
                 GameObject.FindGameObjectsWithTag("Block")) {
            
            Block block = blockObj.GetComponent<Block>();

            blockObj.transform.position = ScreenPos(block.pos);
            
            if (cameraDiff > 0) {
                blockObj.transform.Translate(0, cameraDiff, 0);
            }
        }
    }

    bool CollisionAtIndex(int row, int col) {
        return this.fixedBlockData[row, col].exists;
    }

    void RemoveAtIndex(int row, int col) {
        BlockData blockData = this.fixedBlockData[row, col];
        if (!blockData.exists) return;

        this.fixedBlockData[row, col].exists = false;;

        // 上下左右同じ色一緒に消す
        for (int rowOffset = -1; rowOffset <= 1; rowOffset++) {
            for (int colOffset = -1; colOffset < colOffset; colOffset++) {
                if ((rowOffset != 0 && colOffset != 0) ||
                    (rowOffset == 0 && colOffset == 0)) continue;
                
                int nextRow = row + rowOffset;
                int nextCol = col + colOffset;

                BlockData nextBlockData = this.fixedBlockData[nextRow, nextCol];
                if (nextBlockData.exists && nextBlockData.color == blockData.color) {
                    RemoveAtIndex(nextRow, nextCol);
                }
            }
        }
    }

    Vector2 ScreenPos(Vector2 pos) {
        return new Vector2(pos.x, -pos.y);
    }

    int Row(float y) {
        return Mathf.FloorToInt(y);
    }

    int Col(float x) {
        return Mathf.FloorToInt(x);
    }

    void Fixed(Block block) {
        int col = Col(block.pos.x);
        int row = Row(block.pos.y);

        block.DropEnd();
        block.pos.y = row;

        this.unbalanceBlocks.Remove(block);
        this.fixedBlocksBuffer.Add(block);

        BlockData data = this.fixedBlockData[row, col];
        data.color = block.color;
        data.group = block.group;
        data.shake = null;
    }

    void UnFixed(int row, int col) {
        BlockData data = this.unfixedBlockData[row, col];
        Block block = this.unfixedBlocksBuffer[0];
        block.data = data;

        this.fixedBlocksBuffer.RemoveAt(0);
        this.unbalanceBlocks.Add(block);

        BlockGroup group = new BlockGroup(this);
        group.Grouping(block);

        foreach (Block member in group) {
            member.DropStart(this.gravity);
        }

        this.fixedBlockData[row, col] = null;
    }

    void SetUnbalanceBlocks(BlockGroup group) {
        HashSet<BlockGroup> unbalanceGroups = group.SearchUnbalanceGroups();
        foreach (BlockGroup g in unbalanceGroups) {
            foreach (Block member in g) {
                this.unbalanceBlocks.Add(member);
                member.ShakeStart();
            }
        }

        this.unbalanceBlocks.Sort(delegate(Block a, Block b) {
                // return Mathf.FloorToInt(b.pos.y - a.pos.y);
                return (a.pos.y < b.pos.y ? 1 : -1);
            });
    }
}
