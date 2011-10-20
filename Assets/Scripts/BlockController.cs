using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Direction {
    Left, Right, Down, Right
}

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
    List<Block> unbalanceBlocks;

    GameObject player;

    public static readonly Dictionary<Direction, Vector2> Offset =
        new Dictionary<direction, Vector2>() {
        { Direction.Left,  new Vector2(-1,  0) },
        { Direction.Right, new Vector2( 1,  0) },
        { Direction.Up,    new Vector2( 0,  1) },
        { Direction.Down,  new Vector2( 0, -1) },
    }

    public Block BlockAtPos(Vector2 pos) {
        int col = Col(pos.x);
        int row = Row(pos.y);

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

    public void RemoveAtPos(Vector2 pos) {
        Block block = BlockAtPos(pos);
        if (block == null)  return;
        
        foreach (Block member in block.group) {
            int col = Col(member.pos.x);
            int row = Row(member.pos.y);

            if (this.blocks[row, col] != null) {
                UnFixed(member);
                Destroy(member.gameObject);

                if (row > 0) {
                    Block upBlock = this.blocks[row - 1, col];
                    if (upBlock != null) {
                        SetUnbalanceBlocks(upBlock);
                    }
                }
            }
        }
    }

    void Awake() {
        this.blocks = new Block[this.numBlockRows, this.numBlockCols];
        this.unbalanceBlocks = new List<Block>();

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
                block.color = (Color)materialIndex;
                block.pos   = pos;

                this.blocks[row, col] = block;

                if (block.group == null) {
                    BlockGroup group = new BlockGroup();
                    Grouping(group, block);
                }
            }
        }
    }

    // Use this for initialization
    void Start() {
        this.player = GameObject.Find("Player");
    }

    void Update() {
        foreach (Block block in this.unbalanceBlocks) {
            block.MoveNext();

            if (!block.shaking) {
                UnFixed(block);
            }

            Block underBlock = BlockAtPos(block.pos + -Vector2.up);
            if (underBlock != null) {
                Fixed(block);
                continue;
            }

            Block leftBlock = BlockAtPos(block.pos + -Vector2.right);
            if (leftBlock != null) {
                Fixed(block);
                continue;
            }

            Block rightBlock = BlockAtPos(block.pos + Vector2.right);
            if (rightBlock != null) {
                Fixed(block);
                continue;
            }
        }

        BlockGroup group = new BlockGroup();
        group.Grouping(block);

        this.unbalanceBlocks.RemoveAll(
            delegate(Block block) { return !block.unbalance; }
        );
    }
    
    // Update is called once per frame
    void LateUpdate() {
        player.transform.position = ScreenPos(this.pos);

        // Scroll
        if (this.cameraFixed < this.pos.y) {
            float cameraDiff = this.pos.y - this.cameraFixed;
            transform.Translate(0, cameraDiff, 0);
            foreach (GameObject block in
                     GameObject.FindGameObjectsWithTag("Block")) {

                block.transform.position = blockController.ScreenPos(this.pos);
                block.transform.Translate(0, cameraDiff, 0);

                // Debug
                Block b = block.GetComponent<Block>();
                Debug.DrawLine(
                    block.transform.position,
                    block.transform.position + new Vector3(-0.5f, -0.5f, 0),
                    (b.unbalance ? Color.red : Color.green)
                );

            }
        }
    }

    Vector2 ScreenPos(Vector2 pos) {
        return new Vector2(pos.x, -pos.y);
    }

    int Row(float x) {
        return FloorToInt(x);
    }

    int Col(float y) {
        return FloorToInt(y);
    }

    void Fixed(Block block) {
        int col = Mathf.FloorToInt(block.pos.x);
        int row = Mathf.FloorToInt(block.pos.y);

        block.DropEnd();
        block.pos.y = row;
        this.blocks[row, col] = block;
    }

    void UnFixed(Block block) {
        int col = Mathf.FloorToInt(block.pos.x);
        int row = Mathf.FloorToInt(block.pos.y);

        this.blocks[row, col] = null;
        block.DropStart();
    }

    void SetUnbalanceBlocks(BlockGroup group) {
        HashSet<BlockGroup> unbalanceGroups =
            BlockGroup.SearchUnbalanceGroups(group);

        foreach (BlockGroup group in unbalanceGroups) {
            foreach (Block block in group) {
                this.unbalanceBlocks.Add(block);
                block.ShakeStart();
            }
        }

        this.unbalanceBlocks.Sort(delegate(Block a, Block b) {
                return FloorToInt(b.pos.y - a.pos.y);
            });
    }
}
