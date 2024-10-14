using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public MeshFilter walls;
    public MeshFilter cave;
    public bool Is2d;
    public SquareGrid squareGrid;
    List<Vector3> vertices;
    List<int> triangles;
    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    List<List<int>> outlines = new List<List<int>>();
    HashSet<int> checkedVertices = new HashSet<int>();
    public void GenerateMesh(int[,] map, float squareSize)
    {
        triangleDictionary.Clear();
        outlines.Clear();
        checkedVertices.Clear();

        squareGrid = new SquareGrid(map, squareSize);

        vertices = new List<Vector3>();
        triangles = new List<int>();

        for(int x = 0; x < squareGrid.squares.GetLength(0); x++)
            {
                for(int y = 0; y < squareGrid.squares.GetLength(1);y++)
                {
                    TriangulateSquare(squareGrid.squares[x, y]);
                }
            }
        Mesh mesh = new Mesh();
        cave.mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        if(!Is2d)
        {
            CreateWallMesh();
        }
    }
    void CreateWallMesh()
    {
        CalcvulateMeshOutlines();
        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        Mesh wallMesh = new Mesh();
        float wallHeight = 5;

        foreach(List<int> outline in outlines)
        {
            for(int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]);
                wallVertices.Add(vertices[outline[i + 1]]);
                wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight);
                wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * wallHeight);

                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);

            }
        }
        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        walls.mesh = wallMesh;

        MeshCollider wallCollider = walls.gameObject.AddComponent<MeshCollider>();
        wallCollider.sharedMesh = wallMesh;
    }
    void TriangulateSquare(Square square)
    {
        switch (square.Configuration)
        {
            //1 point
            case 0:
                break;
            case 1:
                MeshFromPoints(square.centerLeft,square.centerBottom,square.bottomLeft);
                break;
            case 2:
                MeshFromPoints(square.bottomRight,square.centerBottom,square.centerRight);
                break;
            case 4:
                MeshFromPoints(square.topRight,square.centerRight,square.centerTop);
                break;
            case 8:
                MeshFromPoints(square.topLeft,square.centerTop,square.centerLeft);
                break;
            //2 points
            case 3:
               MeshFromPoints(square.centerRight,square.bottomRight,square.bottomLeft,square.centerLeft);
                break;
            case 6:
                MeshFromPoints(square.centerTop,square.topRight,square.bottomRight,square.centerBottom);
                break;
            case 9:
                MeshFromPoints(square.topLeft,square.centerTop,square.centerBottom,square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft,square.topRight,square.centerRight,square.centerLeft);
                break;
            case 5:
                MeshFromPoints(square.centerTop,square.topRight,square.centerRight,square.centerBottom,square.bottomLeft,square.centerLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft,square.centerTop,square.centerRight,square.bottomRight,square.centerBottom,square.centerLeft);
                break;
            //3 points
            case 7:
                MeshFromPoints(square.centerTop,square.topRight,square.bottomRight,square.bottomLeft,square.centerLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft,square.centerTop,square.centerRight,square.bottomRight,square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft,square.topRight,square.centerRight,square.centerBottom,square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft,square.topRight,square.bottomRight,square.centerBottom,square.centerLeft);
                break;
            //4 points
            case 15:
                MeshFromPoints(square.topLeft,square.topRight,square.bottomRight,square.bottomLeft);
                checkedVertices.Add(square.topLeft.VertexIndex);
                checkedVertices.Add(square.topRight.VertexIndex);
                checkedVertices.Add(square.bottomRight.VertexIndex);
                checkedVertices.Add(square.bottomLeft.VertexIndex);
                break;
        }
    }
    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if(points.Length >= 3)
        {
            CreateTriangle(points[0], points[1], points[2]);
        }
        if(points.Length >= 4)
        {
            CreateTriangle(points[0], points[2], points[3]);
        }
        if(points.Length >= 5)
        {
            CreateTriangle(points[0], points[3], points[4]);
        }
        if(points.Length >= 6)
        {
            CreateTriangle(points[0], points[4], points[5]);
        }
    }
    void AssignVertices(Node[] points)
    {
        for(int i = 0; i < points.Length; i++)
        {
            points[i].VertexIndex = vertices.Count;
            vertices.Add(points[i].pos);
        }
    }
    void CalcvulateMeshOutlines()
    {
        for(int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
        {
            if(!checkedVertices.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if(newOutlineVertex != -1)
                {
                    checkedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }
    void FollowOutline(int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);
        if(nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }
    void CreateTriangle(Node a, Node b, Node c)
    {
        triangles.Add(a.VertexIndex);
        triangles.Add(b.VertexIndex);
        triangles.Add(c.VertexIndex);

        Triangle triangle = new Triangle(a.VertexIndex, b.VertexIndex, c.VertexIndex);
        
        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);
    }
    void AddTriangleToDictionary(int key, Triangle triangle)
    {
        if(triangleDictionary.ContainsKey(key))
        {
            triangleDictionary[key].Add(triangle);
        }
        else
        {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleDictionary.Add(key, triangleList);

        }
    }
    int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> triangleContainingVertex = triangleDictionary[vertexIndex];

        for(int i = 0; i < triangleContainingVertex.Count; i++)
        {
            Triangle triangle = triangleContainingVertex[i];
            for(int j  = 0; j < 3; j++)
            {
                int vertexB = triangle[j];
                if(vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                {
                    if( IsOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }
        return -1;
    }
    bool IsOutlineEdge(int a, int b)
    {
        List<Triangle> aTriangleList = triangleDictionary[a];
        int sharedTriangleCount = 0;
        
        for(int i = 0; i < aTriangleList.Count; i++)
        {
            if(aTriangleList[i].Contains(b))
            {
                sharedTriangleCount++;
                if(sharedTriangleCount > 1)
                {
                    break;
                }
            }
        }
        return sharedTriangleCount == 1;
    }
    struct Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;

        int[] vertices;

        public Triangle(int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;
            vertices = new int[3];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }

        public int this[int i]
        {
            get {return vertices[i];}
        }
        public bool Contains(int vertexIndex)
        {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }
    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(int[,] map, float squareSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for(int x = 0; x < nodeCountX; x ++)
            {
                for(int y = 0; y < nodeCountY; y ++)
                {
                    Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize / 2);
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, squareSize);
                }
            }
            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }
        }
    }
    public class Square
    {
        public ControlNode topLeft, topRight,bottomRight, bottomLeft;
        public Node centerTop, centerRight, centerBottom, centerLeft;
        public int Configuration;

        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _BottomLeft)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _BottomLeft;

            centerTop = topLeft.RightNode;
            centerRight = bottomRight.aboveNode;
            centerBottom = bottomLeft.RightNode;
            centerLeft = bottomLeft.aboveNode;

            if (topLeft.active)
            {
                Configuration += 8;
            }
            if (topRight.active)
            {
                Configuration += 4;
            }
            if (bottomRight.active)
            {
                Configuration += 2;
            }
            if (bottomLeft.active)
            {
                Configuration += 1;
            }
        }
    }
    public class Node
    {
        public Vector3 pos;
        public int VertexIndex = -1;

        public Node(Vector3 _position)
        {
            pos = _position;
        }
    }
    public class ControlNode : Node
    {
        public bool active;

        public Node aboveNode, RightNode;

        public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos)
        {
            active = _active;
            aboveNode = new Node(pos + Vector3.forward * squareSize / 2f);
            RightNode = new Node(pos + Vector3.right * squareSize / 2f);
        }
            
    }
}
