using HiveLib.Models;
using HiveLib.Models.Pieces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoardScript : MonoBehaviour
{

    private List<GameObject> futureMoveObjects = new List<GameObject>();
    public Board currentBoard { get; set; }
    public Piece selectedPiece { get; set; }
    public Vector3 cameraDestination { get; set; }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            HandleInput();
        }
        CenterCameraOnBoard();
        MovePiecesInHand();
    }

    private void MovePiecesInHand()
    {
        Hex centerHex = GameService.GetCenterHex(currentBoard);
        Dictionary<Hex, Piece> unplayedPiecesPosition = new Dictionary<Hex, Piece>();
        foreach(Piece p in currentBoard.unplayedPieces)
        {

        }
    }

    void MoveCamera()
    {
        float distance = Vector3.Distance(Camera.main.transform.position, cameraDestination);
        if (distance < 50f) return;
        var diff = cameraDestination - Camera.main.transform.position;
        float xSign = Math.Sign(diff.x);
        float ySign = Math.Sign(diff.y);
        float z = 0f;
        Camera.main.transform.position += new Vector3(xSign * 5, ySign * 5, z);
    }

    private void CenterCameraOnBoard()
    {
        Hex centerHex = GameService.GetCenterHex(currentBoard);
        cameraDestination = GameService.HexCoordToCenterPoint(centerHex, GameService._drawSize, Vector3.zero);
        MoveCamera();
    }



    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D [] hits = Physics2D.RaycastAll(inputRay.origin, inputRay.direction);

        GameObject target = hits.Select(h => h.collider.transform.gameObject)
                      .FirstOrDefault(o => o.tag == "FutureMoveSpot");
        if(target == null)
        {
            target = hits.Select(h => h.collider.transform.gameObject)
                      .FirstOrDefault(o => o.tag == "GamePiece");
        }

        if (target != null)
        {
            if (target.tag == "FutureMoveSpot")
            {
                FutureMoveSpotScript futureSpotScript = target.GetComponent<FutureMoveSpotScript>();
                MakeMove(futureSpotScript.Move);
            }
            else if (target.tag == "GamePiece" && selectedPiece == null)
            {
                Piece thisPiece = NotationParser.GetPieceByNotation(target.name);
                if (currentBoard.ColorToPlay != thisPiece.color || !currentBoard.GetMoves().Any(m => m.pieceToMove.Equals(thisPiece))) return;
                bool alreadyPlaced = !currentBoard.unplayedPieces.Contains(thisPiece);
                List<Hex> hexes = null;

                if (alreadyPlaced)
                {
                    hexes = currentBoard.AllMoves.Where(m => thisPiece.Equals(m.pieceToMove)).Select(m => m.hex).ToList();
                }
                else
                {
                    if (currentBoard.hivailableHexes.Count == 0) currentBoard.RefreshDependantBoardData();
                    if (thisPiece.color == PieceColor.White)
                        hexes = currentBoard.hivailableHexes.Where(kvp => kvp.Value.WhiteCanPlace).Select(kvp => kvp.Key).ToList();
                    else
                        hexes = currentBoard.hivailableHexes.Where(kvp => kvp.Value.BlackCanPlace).Select(kvp => kvp.Key).ToList();
                }

                ShowFutureMoves(thisPiece, hexes);
            }
        }
        else
        {
            ClearFutureMoveObjects();
        }

    }

    public void ShowFutureMoves(Piece piece, List<Hex> hexes)
    {
        ClearFutureMoveObjects();

        UnhighlightSelectedPiece();
        selectedPiece = piece;
        HighlightSelectedPiece();

        GameObject futureSpotObject = GameObject.Find("FutureMoveSpot");
        foreach (Hex hex in hexes)
        {
            GameObject futureSpotInstance = Instantiate(futureSpotObject);
            FutureMoveSpotScript script = futureSpotInstance.GetComponent<FutureMoveSpotScript>();
            var hexDrawing = GameService.GetFutureMoveDrawing(hex, 100, new Vector3());
            futureSpotInstance.transform.SetPositionAndRotation(hexDrawing._center, Quaternion.identity);
            futureMoveObjects.Add(futureSpotInstance);
            script.Move = Move.GetMove(piece, hex);
        }
    }

    private void ClearFutureMoveObjects()
    {
        // clear the futureMove hexes
        foreach (GameObject futureMoveObject in futureMoveObjects)
        {
            Destroy(futureMoveObject);
        }
        futureMoveObjects.Clear();
        UnhighlightSelectedPiece();
        selectedPiece = null;
    }

    public void MakeMove(Move move)
    {
        if (currentBoard.TryMakeMove(move))
        {
            DrawBoard();
            ClearFutureMoveObjects();
        }
    }

    private void HighlightSelectedPiece()
    {
        SetAlphaOnSelectedPieceBackground(105);
    }

    private void UnhighlightSelectedPiece()
    {
        SetAlphaOnSelectedPieceBackground(0);
    }

    private void SetAlphaOnSelectedPieceBackground(float alpha)
    {
        if (selectedPiece != null)
        {
            string pieceString = NotationParser.GetNotationForPiece(selectedPiece);
            var pieceGameObject = GameObject.Find(pieceString);
            GameObject existingSpotObject;
            if (pieceGameObject.transform.childCount == 0)
            {
                existingSpotObject = GameObject.Find("ExistingMoveSpot");
                GameObject backgroundHex = Instantiate(existingSpotObject);
                backgroundHex.transform.parent = pieceGameObject.transform;
                backgroundHex.transform.SetPositionAndRotation(pieceGameObject.transform.position, Quaternion.identity);
            }

            existingSpotObject = GameObject.Find(pieceString).transform.GetChild(0).gameObject;
            SpriteRenderer spriteRenderer = existingSpotObject.GetComponents<SpriteRenderer>()[0];
            spriteRenderer.color = new Color(255, 0, 0, alpha);
        }
    }

    // Use this for initialization
    void Start()
    {
        currentBoard = Board.GetNewBoard();
        List<Move> moves = new List<Move>();
        currentBoard.TryMakeMove(Move.GetMove(@"wG1 ."));// 
        currentBoard.TryMakeMove(Move.GetMove(@"bS1 wG1-"));// 
        currentBoard.TryMakeMove(Move.GetMove(@"wQ -wG1"));// 
        currentBoard.TryMakeMove(Move.GetMove(@"bQ bS1/"));// 
        currentBoard.TryMakeMove(Move.GetMove(@"wA1 \wG1"));// 
        currentBoard.TryMakeMove(Move.GetMove(@"bQ /bS1")); // queen move
        currentBoard.TryMakeMove(Move.GetMove(@"wA1 bQ/")); // ant move
        currentBoard.TryMakeMove(Move.GetMove(@"bB1 bS1\")); // 
        currentBoard.TryMakeMove(Move.GetMove(@"wS1 wQ\")); // 
        currentBoard.TryMakeMove(Move.GetMove(@"bB1 wS1-")); //
        currentBoard.TryMakeMove(Move.GetMove(@"wB1 \wS1")); // 
        currentBoard.TryMakeMove(Move.GetMove(@"bB2 bB1-")); //
        currentBoard.TryMakeMove(Move.GetMove(@"wB1 wS1")); // beetle climb
        currentBoard.TryMakeMove(Move.GetMove(@"bB2 bS1")); // beetle climb
                                                            //_board.TryMakeMove(Move.GetMove(@"wB2 -wB1")); // last beetle 
                                                            //_board.TryMakeMove(Move.GetMove(@"bB2 wG1")); // beetle climb
                                                            //_board.TryMakeMove(Move.GetMove(@"wB2 wQ")); // beetle climb
                                                            //_board.TryMakeMove(Move.GetMove(@"bB1 bB2")); // beetle climb
                                                            //_board.TryMakeMove(Move.GetMove(@"wB1 bB1")); // beetle climb
                                                            //_board.TryMakeMove(Move.GetMove(@"bA1 bS1-")); // 
                                                            //_board.TryMakeMove(Move.GetMove(@"wB2 wB1")); // beetle climb
                                                            //_board.TryMakeMove(Move.GetMove(@"bA1 -bQ")); // 
                                                            //_board.TryMakeMove(Move.GetMove(@"wB2 bQ")); // 
                                                            //_board.TryMakeMove(Move.GetMove(@"bG1 bS1-")); // 
                                                            //_board.TryMakeMove(Move.GetMove(@"wB2 bQ-")); // 
                                                            //_board.TryMakeMove(Move.GetMove(@"bS2 bG1-")); // 
                                                            //_board.TryMakeMove(Move.GetMove(@"wB1 wS1-")); // 
                                                            //_board.TryMakeMove(Move.GetMove(@"bB1 wQ")); // 
                                                            //_board.TryMakeMove(Move.GetMove(@"wA1 /bQ")); // 
                                                            //_board.TryMakeMove(Move.GetMove(@"bB2 wS1")); // 
                                                            //_board.TryMakeMove(Move.GetMove(@"wG1 bQ/")); // 
                                                            //_board.TryMakeMove(Move.GetMove(@"bB1 \bQ")); // 

        DrawBoard();
        CenterCameraOnBoard();
    }

    private void DrawBoard()
    {
        GameService._drawSize = 100;
        var hexes = GameService.GetHexagonsForBoard(currentBoard);
        GameObject existingSpotObject = GameObject.Find("ExistingMoveSpot");

        foreach (HexagonDrawing hexDrawing in hexes)
        {
            BeetleStack stack = hexDrawing.piece as BeetleStack;
            if (stack != null)
            {
                for (int i = 0; i < stack.Pieces.Count; i++)
                {
                    if (stack.Pieces[i] == null) continue;
                    SetPiecePosition(stack.Pieces[i], hexDrawing.center, i);
                }
            }
            else
            {
                SetPiecePosition(hexDrawing.piece, hexDrawing._center);
            }
        }
    }



    private static void SetPiecePosition(Piece piece, Vector3 vector3, int height = 0)
    {
        string pieceObjectName = NotationParser.GetNotationForPiece(piece);
        GameObject pieceGameObject = GameObject.Find(pieceObjectName);
        if (pieceGameObject == null) throw new System.Exception($"Could not find game piece named {pieceObjectName}");
        Vector3 heightAdjustedV = new Vector3(vector3.x, vector3.y, -height);
        pieceGameObject.transform.SetPositionAndRotation(heightAdjustedV, Quaternion.identity);
        SpriteRenderer spriteRenderer = pieceGameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = height;
    }

}


public class FutureMoveDrawing : HexagonDrawing
{
    public FutureMoveDrawing() { }

}

public class HexagonDrawing
{
    public float height;
    public float width;

    public Piece _piece;
    public Piece piece { get { return _piece; } }

    public Vector3 _center;
    public Vector3 center { get { return _center; } }

    public HexagonDrawing() { }
}

public static class GameService
{
    public static float _drawSize = 50;
    public static Vector3 _mainCanvasOffsetPoint = new Vector3();

    public static Vector3 GetOffsetPointFromCenter(Vector3 centerPoint, float size)
    {
        HexagonDrawing calculatedCenter = GetHexagonDrawing(new Hex(24, 24), size, new Vector3(0, 0));
        return new Vector3(calculatedCenter._center.x - centerPoint.x, calculatedCenter._center.y - centerPoint.y);
    }

    public static FutureMoveDrawing GetFutureMoveDrawing(Hex hex, float size, Vector3 offsetPoint)
    {
        HexagonDrawing hexDrawing = GetHexagonDrawing(hex, size, offsetPoint);
        FutureMoveDrawing drawing = new FutureMoveDrawing();
        drawing._center = hexDrawing.center;
        drawing._piece = hexDrawing.piece;
        drawing.height = hexDrawing.height;
        drawing.width = hexDrawing.width;
        return drawing;
    }

    public static HexagonDrawing GetHexagonDrawing(Hex hex, float size, Vector3 offsetPoint)
    {
        HexagonDrawing drawing = new HexagonDrawing();
        drawing._center = HexCoordToCenterPoint(hex, size, offsetPoint);
        drawing.height = size * 2;
        drawing.width = (float)Math.Sqrt(3) / 2 * drawing.height;
        return drawing;
    }

    public static HexagonDrawing GetHexagonDrawing(Hex hex, float size, Piece piece, Vector3 offsetPoint)
    {
        HexagonDrawing drawing = GetHexagonDrawing(hex, size, offsetPoint);
        drawing._piece = piece;
        //double imageXOffset = drawing._image.Source.Width / 2;
        //double imageYOffset = drawing._image.Source.Height / 2;
        //float scale = drawing.width / drawing._image.Source.Width + .25;
        //drawing._image.RenderTransform = new ScaleTransform(scale, scale, imageXOffset, imageYOffset);
        //Canvas.SetZIndex(drawing._image, -1);
        //Canvas.SetLeft(drawing._image, drawing.center.X - imageXOffset );
        //Canvas.SetTop(drawing._image, drawing.center.Y - imageYOffset);
        return drawing;
    }

    public static Vector3 HexCorner(Vector3 center, float size, int corner_number)
    {
        double angle_deg = 60 * corner_number + 30;
        double angle_rad = Math.PI / 180 * angle_deg;
        return new Vector3(center.x + size * (float)Math.Cos(angle_rad), center.y + size * (float)Math.Sin(angle_rad));
    }

    public static Vector3 HexCoordToCenterPoint(Hex hex, float size, Vector3 offsetPoint)
    {
        double height = size * 2;
        double width = Math.Sqrt(3) / 2 * height;
        //var basis = new Matrix(size * Math.Sqrt(3), size * Math.Sqrt(3) / 2, 0, size * 3 / 2, 0, 0);
        var basis = new Matrix4x4();
        basis.m11 = size * (float)Math.Sqrt(3);
        basis.m12 = size * (float)Math.Sqrt(3) / 2;
        basis.m21 = 0;
        basis.m22 = size * 3 / 2;

        var rowColMatrix = new Matrix4x4();
        rowColMatrix.m11 = hex.column;
        rowColMatrix.m21 = hex.row;

        var xy = basis * rowColMatrix;
        //Matrix xy = Matrix.Multiply(basis, new Matrix(hex.column, 0, hex.row, 0, 0, 0));

        return new Vector3(xy.m11 - offsetPoint.x, xy.m21 - offsetPoint.y);
    }

    public static Hex GetCenterHex(Board board)
    {
        int maxColumn = int.MinValue;
        int minColumn = int.MaxValue;
        int maxRow = int.MinValue;
        int minRow = int.MaxValue;
        foreach (Hex hex in board.playedPieces.Select(p => p.Value))
        {
            minColumn = Math.Min(hex.column, minColumn);
            maxColumn = Math.Max(hex.column, maxColumn);
            minRow = Math.Min(hex.row, minRow);
            maxRow = Math.Max(hex.row, maxRow);
        }

        Hex centerHex = new Hex(minColumn + ((maxColumn - minColumn) / 2), minRow + ((maxRow - minRow) / 2));
        return centerHex;
    }

    public static Hex GetMinVisibleHex(Board board)
    {
        float yCameraMin = Camera.main.transform.position.y - Camera.main.orthographicSize;
        float xCameraMin = Camera.main.transform.position.x - (Camera.main.orthographicSize * Screen.width / Screen.height);

        Hex currentHex = GetCenterHex(board);
        HexagonDrawing d;
        do
        {
            currentHex = new Hex(currentHex.column - 1, currentHex.row);
            d = GetHexagonDrawing(currentHex, _drawSize, Vector3.zero);
        }
        while (d.center.x > xCameraMin);
        int minColumn = currentHex.column + 3;

        currentHex = GetCenterHex(board);
        do
        {
            currentHex = new Hex(currentHex.column, currentHex.row - 1);
            d = GetHexagonDrawing(currentHex, _drawSize, Vector3.zero);
        }
        while (d.center.y > yCameraMin);
        int minRow = currentHex.row + 3;

        return new Hex(minColumn, minRow);
    }

    public static List<HexagonDrawing> GetHexagonsForBoard(Board board)
    {
        try
        {
            List<HexagonDrawing> results = new List<HexagonDrawing>();
            foreach (var kvp in board.playedPieces)
            {
                HexagonDrawing hexWithImage = GetHexagonDrawing(kvp.Value, _drawSize, kvp.Key, _mainCanvasOffsetPoint);
                results.Add(hexWithImage);
            }

            Hex minVisibleHex = GetMinVisibleHex(board);

            int minRow = minVisibleHex.row;
            int minColumn = minVisibleHex.column;
            int maxColumn = minColumn + 10;
            int midCol = minColumn + 5;
            int endCol = midCol + ((maxColumn - minColumn) / 2);


            int currentCol = minVisibleHex.column;
            int currentRow = minVisibleHex.row;
            foreach (Piece unplayedPiece in board.unplayedPieces.Where(p => p.color == PieceColor.White))
            {
                Hex sideHex = new Hex(currentCol, currentRow);
                HexagonDrawing hexWithImage = GetHexagonDrawing(sideHex, _drawSize, unplayedPiece, _mainCanvasOffsetPoint);
                results.Add(hexWithImage);

                currentCol++;
                if (currentCol == midCol)
                {
                    currentCol = minColumn;
                    currentRow -= 1;
                }
            }

            currentCol = midCol + 1;
            currentRow = minRow;
            foreach (Piece unplayedPiece in board.unplayedPieces.Where(p => p.color == PieceColor.Black))
            {
                Hex sideHex = new Hex(currentCol, currentRow);
                HexagonDrawing hexWithImage = GetHexagonDrawing(sideHex, _drawSize, unplayedPiece, _mainCanvasOffsetPoint);
                results.Add(hexWithImage);

                currentCol++;
                if (currentCol == endCol)
                {
                    currentCol = midCol + 1;
                    currentRow -= 1;
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
