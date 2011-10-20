using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Direction {
    Left, Right, Down, Up
}

public class BlockController : MonoBehaviour {
    public float gravity = 5.0f;

    public float cameraFixed = 6.0f;

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
        new Dictionary<Direction, Vector2>() {
        { Direction.Left,  new Vector2(-1,  0) },
        { Direction.Right, new Vector2( 1,  0) },
        { Direction.Up,    new Vector2( 0, -1) },
        { Direction.Down,  new Vector2( 0,  1) }
    };

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

    public Block NextBlock(Vector2 pos, Direction d) {
        return BlockAtPos(pos + BlockController.Offset[d]);
    }

    public void RemoveAtPos(Vector2 pos) {
        Block block = BlockAtPos(pos);
        if (block == null)  return;
        
        HashSet<BlockGroup> upperGroups = block.group.SearchUpperGroups();

        foreach (Block member in block.group) {
            UnFixed(member);
            Destroy(member.gameObject);
        }

        foreach (BlockGroup upperGroup in upperGroups) {
            SetUnbalanceBlocks(upperGroup);
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
                block.color = (Block.Color)materialIndex;
                block.pos   = pos;

                this.blocks[row, col] = block;

                if (block.group == null) {
                    BlockGroup group = new BlockGroup(this);
                    group.Grouping(block);
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
                Debug.Log("Unfixed!!:" + block);
                UnFixed(block);
                block.DropStart(this.gravity);
            }

            Block underBlock = NextBlock(block.pos, Direction.Down);
            if (underBlock != null) {
                Fixed(block);
                continue;
            }

            Block leftBlock = NextBlock(block.pos, Direction.Left);
            if (leftBlock != null && leftBlock.color == block.color &&
                leftBlock.group != block.group) {
                Fixed(block);
                continue;
            }

            Block rightBlock = NextBlock(block.pos, Direction.Right);
            if (rightBlock != null && rightBlock.color == block.color &&
                rightBlock.group != block.group) {
                Fixed(block);
                continue;
            }
        }

        foreach (Block block in this.unbalanceBlocks) {
            if (!block.unbalance) {
                foreach (Block member in block.group) {
                    Fixed(member);
                }

                BlockGroup group = new BlockGroup(this);
                group.Grouping(block);
            }
        }

        this.unbalanceBlocks.RemoveAll(
            delegate(Block block) { return !block.unbalance; }
        );
    }
    
    // Update is called once per frame
    void LateUpdate() {
        Player playerBehaviour = player.GetComponent<Player>();
        player.transform.position = ScreenPos(playerBehaviour.pos);

        // Scroll
        if (this.cameraFixed < playerBehaviour.pos.y) {
            float cameraDiff = playerBehaviour.pos.y - this.cameraFixed;

            player.transform.Translate(0, cameraDiff, 0);
            foreach (GameObject blockObj in
                     GameObject.FindGameObjectsWithTag("Block")) {

                Block block = blockObj.GetComponent<Block>();
                blockObj.transform.position = ScreenPos(block.pos);
                blockObj.transform.Translate(0, cameraDiff, 0);

                // Debug
                Debug.DrawLine(
                    blockObj.transform.position,
                    blockObj.transform.position + new Vector3(-0.5f, -0.5f, 0),
                    (block.unbalance ? Color.red : Color.green)
                );

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
        this.blocks[row, col] = block;
    }

    void UnFixed(Block block) {
        int col = Col(block.pos.x);
        int row = Row(block.pos.y);

        this.blocks[row, col] = null;
        block.DropStart(this.gravity);
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
                return Mathf.FloorToInt(b.pos.y - a.pos.y);
            });

        Debug.Log(this.unbalanceBlocks.Count);
    }
}
