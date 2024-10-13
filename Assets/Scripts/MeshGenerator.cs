using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public SquareGrid squareGrid;
    List<Vector3> vertices;
    List<int> triangles;
    public void GenerateMesh(int[,] map, float squareSize)
    {
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
        GetComponent<MeshFilter>().mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();


    }
    void TriangulateSquare(Square square)
    {
        switch (square.Configuration)
        {
            //1 point
            case 0:
                break;
            case 1:
                MeshFromPoints(square.centerBottom,square.bottomLeft,square.centerLeft);
                break;
            case 2:
                MeshFromPoints(square.centerRight,square.bottomRight,square.centerBottom);
                break;
            case 4:
                MeshFromPoints(square.centerTop,square.topRight,square.centerRight);
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
            points[i].VerticalIndex = vertices.Count;
            vertices.Add(points[i].pos);
        }
    }
    void CreateTriangle(Node a, Node b, Node c)
    {
        triangles.Add(a.VerticalIndex);
        triangles.Add(b.VerticalIndex);
        triangles.Add(c.VerticalIndex);
    }
    private void OnDrawGizmos()
    {
        // if(squareGrid != null)
        // {
        //     for(int x = 0; x < squareGrid.squares.GetLength(0); x++)
        //     {
        //         for(int y = 0; y < squareGrid.squares.GetLength(1);y++)
        //         {
        //             Gizmos.color = (squareGrid.squares[x, y].topLeft.active) ? Color.black : Color.white;
        //             Gizmos.DrawCube(squareGrid.squares[x, y].topLeft.pos, Vector3.one * .4f);

        //             Gizmos.color = (squareGrid.squares[x, y].topRight.active) ? Color.black : Color.white;
        //             Gizmos.DrawCube(squareGrid.squares[x, y].topRight.pos, Vector3.one * .4f);

        //             Gizmos.color = (squareGrid.squares[x, y].bottomRight.active) ? Color.black : Color.white;
        //             Gizmos.DrawCube(squareGrid.squares[x, y].bottomRight.pos, Vector3.one * .4f);

        //             Gizmos.color = (squareGrid.squares[x, y].bottomLeft.active) ? Color.black : Color.white;
        //             Gizmos.DrawCube(squareGrid.squares[x, y].bottomLeft.pos, Vector3.one * .4f);

        //             Gizmos.color = Color.gray;
        //             Gizmos.DrawCube(squareGrid.squares[x, y].centerTop.pos, Vector3.one * .15f);
        //             Gizmos.DrawCube(squareGrid.squares[x, y].centerRight.pos, Vector3.one * .15f);
        //             Gizmos.DrawCube(squareGrid.squares[x, y].centerBottom.pos, Vector3.one * .15f);
        //             Gizmos.DrawCube(squareGrid.squares[x, y].centerLeft.pos, Vector3.one * .15f);
        //         }
        //     }
        // }
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
        public int VerticalIndex = -1;

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
