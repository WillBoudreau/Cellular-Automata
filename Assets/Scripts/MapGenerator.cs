using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapGenerator : MonoBehaviour
{
    public int hight;
    public int width;
    public int SmoothingMap;
    public int borderSize;
    public int Wallthreshholdsize;
    public int Roomthreshholdsize;

    public string seed;
    public bool randomSeed;

    [Range(0,100)]
    public int RandFillPercent;
    int[,] map;

    private void Start()
    {
        GenerateMap();
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

        ProcessMap();

        int[,] borderedMap = new int[width + borderSize * 2, hight + borderSize * 2];
        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if(x >= borderSize && x < width + borderSize && y >= borderSize && y < hight + borderSize)
                {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
                else 
                {
                    borderedMap[x, y] = 1;
                }
            }
        }

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(borderedMap, 1);
    }
    void ProcessMap()
    {
        List<List<Coord>> wallRegions = GetRegions(1);
        int wallThresholdSize = Wallthreshholdsize;
        foreach(List<Coord> wallRegion in wallRegions)
        {
            if(wallRegion.Count < wallThresholdSize)
            {
                foreach(Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }
        List<List<Coord>> roomRegions = GetRegions(0);
        int roomThresholdSize = Roomthreshholdsize;
        // List<Room> survivingRooms = new List<Room>();
        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
        }
    }
    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, hight];
        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y < hight; y++)
            {
                if(mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);
                    foreach(Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }
        return regions;
    }
    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, hight];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;
        while(queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);
            for(int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if(isInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if(mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }
        return tiles;
    }
    bool isInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < hight;
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
                if(isInMapRange(neighbourX, neighbourY))
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
    struct Coord
    {
        public int tileX;
        public int tileY;
        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }
    private void OnDrawGizmos()
    {
        //if (map != null)
        //{
        //    for (int x = 0; x < width; x++)
        //    {
        //        for (int y = 0; y < hight; y++)
        //        {
        //            Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
        //            Vector3 pos = new Vector3(-width / 2 + x + 0.5f, 0, -hight / 2 + y + 0.5f);
        //            Gizmos.DrawCube(pos, Vector3.one);
        //        }
        //    }
        //}
    }
}
