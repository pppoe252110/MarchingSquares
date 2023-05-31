using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{

    //Basic blocks
    public GameObject moonrockTile;
    public GameObject moonstoneTile;
    public GameObject mudTile;
    public GameObject moonsandTile;

    //Ores
    public GameObject tileGold;
    public GameObject tileCopper;
    public GameObject tileSilver;

    public int width;
    public float heightMultiplier;
    public int heightAddition;

    public float smoothness;

    public float seed;

    public float chanceGold;
    public float chanceCopper;
    public float chanceSilver;

    void Start()
    {
        seed = Random.Range(-10000, 10000);
        Generate();
    }

    public void Generate()
    {
        for (int i = 0; i < width; i++)
        {
            int h = Mathf.RoundToInt(Mathf.PerlinNoise(seed, (i + transform.position.x) / smoothness) * heightMultiplier) + heightAddition;

            for (int j = 0; j < h; j++)
            {
                GameObject selectedTile;

                //Layers of tiles here:
                if (j < h - 8)
                {
                    selectedTile = moonstoneTile;
                }
                else if (j < h - 4)
                {
                    selectedTile = mudTile;
                }
                else if (j < h - 2)
                {
                    selectedTile = moonsandTile;
                }
                else
                {
                    selectedTile = moonrockTile;
                }

                //Spawn new tiles and add to Generate gameobject
                GameObject newTile = Instantiate(selectedTile, Vector3.zero, Quaternion.identity) as GameObject;

                newTile.transform.parent = this.gameObject.transform;
                newTile.transform.localPosition = new Vector3(i, j);
            }
        }



        GenOre();
    }

    public void GenOre()
    {
        foreach (GameObject t in GameObject.FindGameObjectsWithTag("Blocks/Stone"))
        {
            if (t.transform.parent == this.gameObject.transform)
            {
                float r = Random.Range(0f, 100f);
                GameObject selectedTile = null;

                if (r < chanceGold)
                {
                    selectedTile = tileGold;
                }
                else if (r < chanceSilver)
                {
                    selectedTile = tileSilver;
                }
                else if (r < chanceCopper)
                {
                    selectedTile = tileCopper;
                }

                if (selectedTile != null)
                {
                    GameObject newResourceTile = Instantiate(selectedTile, t.transform.position, Quaternion.identity) as GameObject;
                    newResourceTile.transform.parent = transform;
                    Destroy(t);
                }
            }
        }
    }
}