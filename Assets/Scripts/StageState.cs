using UnityEngine;
using System.Collections;

public class StageState : MonoBehaviour {
    public float gravity = 2.0f;
    public int cameraFixedRow = 5;
    public Vector2 screenTop = Vector2.zero;

    public int numBlockRows = 100;
    public int numBlockCols = 15;

    public GameObject playerPrefab;
    public GameObject blockPrefab;
    public Material[] blockMaterials;

    public float blockSize {
        get { return this.blockPrefab.transform.localScale.x; }
    }

    public int playerRow = 0;
    public int playerCol = 7;

    public GameObject[,] blocks;
}
