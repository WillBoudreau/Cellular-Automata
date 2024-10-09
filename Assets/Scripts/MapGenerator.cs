using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapGenerator : MonoBehaviour
{
    public int hight;
    public int width;
    public int SmoothingMap;

    public string seed;
    public bool randomSeed;

    [Range(0,100)]
    public int RandFillPercent;
    int[,] map;

    private void Start()
    {
        GenerateMap();

        SmoothingMap = 5;
    }
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            GenerateMap();
        }
    }
    void GenerateMap()
    {
        map = new int[width, hight];
        RandonFillMap();
        for(int i = 0; i < SmoothingMap; i++)
        {
            SmoothMap();
        }
    }
    void RandonFillMap()
    {
        if (randomSeed)
        {
            seed = Time.time.ToString();
        }
        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < hight; y++)
            {
                if(x == 0 || x == width-1|| y == 0 || y == hight -1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = (pseudoRandom.Next(0, 100) < RandFillPercent) ? 1: 0;
                }
            }
        }
    }
    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < hight; y++)
            {
                int neighborWallTiles = GetSurroundingWallCount(x, y);

                if(neighborWallTiles > 4)
                {
                    map[x, y] = 1;
                }
                else if (neighborWallTiles < 4)
                {
                    map[x, y] = 0;
                }
            }
        }
    }
    int GetSurroundingWallCount(int GridX, int GridY)
    {
        int WallCount = 0;
        for (int neighbourX = GridX - 1; neighbourX <= GridX + 1; neighbourX++)
        {
            for (int neighbourY = GridY - 1; neighbourY <= GridY + 1; neighbourY++)
            {
                if(neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < hight)
                {
                    if (neighbourX != GridX || neighbourY != GridY)
                    {
                        WallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    WallCount++;
                }
            }
        }
        return WallCount;
    }
    private void OnDrawGizmos()
    {
        if (map != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < hight; y++)
                {
                    Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
                    Vector3 pos = new Vector3(-width / 2 + x + 0.5f, 0, -hight / 2 + y + 0.5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }
}
