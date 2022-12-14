using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GridSystem : MonoBehaviour
{
    public int width;
    public Transform canvasAnchor;
    public float cellSize = 1f;
    public int value = 0;
    public Material material;
    public Sprite sprite;
    public Transform playerTransform;
    public SerializedObject serializedObject;
    public List<Node> path;
    public List<Node> debugingPathList;
    public Node[,] gridArray;

    private int height;
    public int Height { get { return height; } }

    /*need to make debug variable to private*/
    [HideInInspector]
    public bool isImageOn = false;
    public bool IsImageOn { get { return isImageOn; } }
    [HideInInspector]
    public bool isLineOn = true;
    public bool IsLineOn { get { return IsLineOn; } }
    [HideInInspector]
    public bool isDebugOn;
    public bool IsDebugOn { get { return IsDebugOn; } }


    private float screenWidth;
    private float screenHeight;
    private Vector2 playerTransformInScreenPoint;
    private int squareSize;
    private TextMeshProUGUI[,] textMeshPro;
    private Image[,] imgArray;
    private AStarAlgorithm aStar;
    private int debugIndex;
    private bool clicked;


    private void Start()
    {
        /* init Screen vrariables*/
        var aspectRatio = GetScreenAspectRatio();
        height = Mathf.FloorToInt(width / aspectRatio) + 1;
        squareSize = Mathf.FloorToInt(screenWidth / width);

        /* init grid variables*/
        gridArray = new Node[width, height];
        canvasAnchor.position = new Vector3(0, 0);
        textMeshPro = new TextMeshProUGUI[width, height];
        imgArray = new Image[width, height];
        CreateGridArray();

        /* init Astar variables*/
        aStar = new AStarAlgorithm(this);
        InitGridArray(0, 0, 0, 0);
        if (isDebugOn)
        {
            debugingPathList = new List<Node>();
        }
    }
    private void Update()
    {
        CheckPlayerTransformInValidPosition();
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            StartPathFinding();
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            SetOrRedoObstacle();
        }
        if (clicked == true && isDebugOn && isImageOn)
        {
            TracePathWithImg(debugIndex);
            debugIndex++;
            if (debugIndex == debugingPathList.Count - 1)
            {
                clicked = false;
            }
        }
        if (clicked == true && isDebugOn && isLineOn /*&& Input.GetKeyDown(KeyCode.Space)*/)
        {
            TracePathWithTMP(debugIndex);
            debugIndex++;
            if (debugIndex == debugingPathList.Count - 1)
            {
                clicked = false;
            }
        }

    }
    private void ResetGridImageColor()
    {
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                if (imgArray[x, y].color != Color.white && imgArray[x, y].color != Color.gray)
                {
                    imgArray[x, y].color = Color.white;
                }
            }
        }
    }
    private void ResetGridFontColor()
    {
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                if (textMeshPro[x, y].color != Color.white && textMeshPro[x, y].color != Color.gray)
                {
                    textMeshPro[x, y].color = Color.white;
                }
            }
        }
    }
    private float GetScreenAspectRatio()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        return screenWidth / screenHeight;
    }
    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt(worldPosition.x / squareSize);
        y = Mathf.FloorToInt(worldPosition.y / squareSize);
    }
    private void CheckPlayerTransformInValidPosition()
    {
        int x, y;
        GetXY(Camera.main.WorldToScreenPoint(playerTransform.position), out x, out y);
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            if (x < 0) { x = 0; }
            if (x >= width) { x = width - 1; }
            if (y < 0) { y = 0; }
            if (y >= height) { y = height - 1; }
            playerTransform.position = Camera.main.ScreenToWorldPoint(new Vector3(x, y, 0) * squareSize + new Vector3(squareSize/2, squareSize/2, 0));
        }
        else
        {
            return;
        }
    }
    private void StartPathFinding()
    {
        var mousePosition = Input.mousePosition;
        int startX, startY, destinationX, destinationY;
        if (isDebugOn && isImageOn)
        {
            ResetGridImageColor();
        }
        if (isDebugOn && isLineOn)
        {
            ResetGridFontColor();
        }
        debugingPathList = null;
        debugingPathList = new List<Node>();
        aStar.GetStartPosition(out startX, out startY);
        aStar.GetDestination(mousePosition, out destinationX, out destinationY);
        InitGridArray(startX, startY, destinationX, destinationY);
        aStar.FindTheWay(startX, startY, destinationX, destinationY);
        //aStar.FindTheWayUsingQueue(startX, startY, destinationX, destinationY);
        clicked = true;
        debugIndex = 0;
    }
    private void SetOrRedoObstacle()
    {
        var mousePosition = Input.mousePosition;
        int x, y;
        GetXY(mousePosition, out x, out y);
        if (x < 0 || x > width || y < 0 || y > height)
        {
            Debug.LogError("You are trying to set obstacle at outside of bounds");
            return;
        }
        var obstacleNode = gridArray[x, y];
        if (obstacleNode.traversable == false)
        {
            RedoObstacle(obstacleNode);
        }
        else
        {
            SetObstacle(obstacleNode);
        }
    }
    private void CreateGridArray()
    {
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                if (isDebugOn == true)
                {
                    GameObject gameObject = new GameObject("(" + x + ", " + y + ")");
                    gameObject.transform.SetParent(canvasAnchor);
                    gameObject.transform.localPosition = new Vector2(x * cellSize + cellSize / 2f, y * cellSize + cellSize / 2f) * squareSize;
                    if (isLineOn)
                    {
                        CreateTextMeshPro(gameObject, x, y);
                    }
                    if (isImageOn == true)
                    {
                        CreateImage(gameObject, x, y);
                    }
                }
            }
        }
    }
    private void InitGridArray(int startX, int startY, int destinationX, int destinationY)
    {
        for (int currentX = 0; currentX < gridArray.GetLength(0); currentX++)
        {
            for (int currentY = 0; currentY < gridArray.GetLength(1); currentY++)
            {
                if (gridArray[currentX, currentY] == null)
                {
                    gridArray[currentX, currentY] = new Node(startX, startY, currentX, currentY, destinationX, destinationY);
                }
                else if (gridArray[currentX, currentY].traversable != false)
                {
                    gridArray[currentX, currentY] = new Node(startX, startY, currentX, currentY, destinationX, destinationY);
                }
            }
        }
    }
    private void CreateTextMeshPro(GameObject gameObject, int x, int y)
    {
        textMeshPro[x, y] = gameObject.AddComponent<TextMeshProUGUI>();
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(40f, 10f);
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 150);
        textMeshPro[x, y].text = "(" + x + "," + y + ")";
        
        /*Font size update depends on Screen width*/
        const int ratio = 60;
        int fontSize = Mathf.FloorToInt(Screen.width / ratio);
        if (fontSize <= 5)
        {
            Debug.LogError("FontSize is too small. please decrease width size");
        }
        textMeshPro[x, y].fontSize = fontSize;
        textMeshPro[x, y].alignment = TextAlignmentOptions.Center;
        textMeshPro[x, y].canvas.sortingLayerName = "foreground";
        textMeshPro[x, y].canvas.sortingOrder = 1000;
        DrawSquare(gameObject, x, y);

    }
    private void DrawSquare(GameObject gameObject, int x, int y)
    {
        SetLineRenderer(gameObject, GetVectorPositionArray(x, y), x, y);
    }
    private Vector3[] GetVectorPositionArray(int x, int y)
    {
        Vector3[] vectors = new Vector3[4];
        vectors[0] = Camera.main.ScreenToWorldPoint(new Vector3(x * squareSize, y * squareSize, 10f));
        vectors[1] = Camera.main.ScreenToWorldPoint(new Vector3(x * squareSize, (y + 1) * squareSize, 10f));
        vectors[2] = Camera.main.ScreenToWorldPoint(new Vector3((x + 1) * squareSize, (y + 1) * squareSize, 10f));
        vectors[3] = Camera.main.ScreenToWorldPoint(new Vector3((x + 1) * squareSize, y * squareSize, 10f));
        return vectors;
    }
    private void SetLineRenderer(GameObject gameObject, Vector3[] vectors, int x, int y)
    {
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.widthMultiplier = 0.1f;
        lineRenderer.loop = true;
        lineRenderer.positionCount = vectors.Length;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.SetPositions(vectors);
        lineRenderer.material = material;
    }
    private void CreateImage(GameObject gameObject, int x, int y)
    {
        imgArray[x, y] = gameObject.AddComponent<Image>();
        imgArray[x, y].sprite = sprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(squareSize, squareSize);
    }
    public void RenderingDebugImagesPath(Node startNode)
    {
        imgArray[startNode.currentX, startNode.currentY].color = Color.blue;
        foreach (Node shortestNode in path)
        {
            imgArray[shortestNode.currentX, shortestNode.currentY].color = Color.black;
            if (path.LastIndexOf(shortestNode) == path.Count - 1)
            {
                imgArray[shortestNode.currentX, shortestNode.currentY].color = Color.red;
            }
        }
    }

    public void RenderingDebugTMPsPath(Node startNode)
    {
        textMeshPro[startNode.currentX, startNode.currentY].color = Color.blue;
        foreach (Node shortestNode in path)
        {
            textMeshPro[shortestNode.currentX, shortestNode.currentY].color = Color.black;
            if (path.LastIndexOf(shortestNode) == path.Count - 1)
            {
                textMeshPro[shortestNode.currentX, shortestNode.currentY].color = Color.red;
            }
        }
    }
    public void SetObstacle(Node obstacleNode)
    {
        if (isDebugOn && isImageOn)
        {
            imgArray[obstacleNode.currentX, obstacleNode.currentY].color = Color.gray;
        }
        if (isDebugOn && isLineOn)
        {
            textMeshPro[obstacleNode.currentX, obstacleNode.currentY].color = Color.gray;
        }
        gridArray[obstacleNode.currentX, obstacleNode.currentY].traversable = false;
    }
    private void RedoObstacle(Node obstacleNode)
    {
        if (isDebugOn && isImageOn)
        {
            if (imgArray[obstacleNode.currentX, obstacleNode.currentY].color == Color.gray)
            {
                imgArray[obstacleNode.currentX, obstacleNode.currentY].color = Color.white;
            }
        }
        if (isDebugOn && isLineOn)
        {
            if (textMeshPro[obstacleNode.currentX, obstacleNode.currentY].color == Color.gray)
            {
                textMeshPro[obstacleNode.currentX, obstacleNode.currentY].color = Color.white;
            }
        }
        gridArray[obstacleNode.currentX, obstacleNode.currentY].traversable = true;
    }

    public void TracePathWithImg(int debugIndex)
    {
        if (imgArray[debugingPathList[debugIndex].currentX, debugingPathList[debugIndex].currentY].color == Color.black)
        {
            return;
        }
        if (imgArray[debugingPathList[debugIndex].currentX, debugingPathList[debugIndex].currentY].color == Color.blue)
        {
            return;
        }
        if (imgArray[debugingPathList[debugIndex].currentX, debugingPathList[debugIndex].currentY].color == Color.red)
        {
            return;
        }
        imgArray[debugingPathList[debugIndex].currentX, debugingPathList[debugIndex].currentY].color -= new Color(0.25f, 0.3f, 0, 0);
    }
    public void TracePathWithTMP(int debugIndex)
    {
        if (textMeshPro[debugingPathList[debugIndex].currentX, debugingPathList[debugIndex].currentY].color == Color.black)
        {
            return;
        }
        if (textMeshPro[debugingPathList[debugIndex].currentX, debugingPathList[debugIndex].currentY].color == Color.blue)
        {
            return;
        }
        if (textMeshPro[debugingPathList[debugIndex].currentX, debugingPathList[debugIndex].currentY].color == Color.red)
        {
            return;
        }
        textMeshPro[debugingPathList[debugIndex].currentX, debugingPathList[debugIndex].currentY].color -= new Color(0.25f, 0.3f, 0, 0);
        textMeshPro[debugingPathList[debugIndex].currentX, debugingPathList[debugIndex].currentY].fontStyle = FontStyles.Bold;
    }
}
