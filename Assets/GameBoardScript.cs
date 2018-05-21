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
            HashSet<Tuple<PieceColor, string>> shown = new HashSet<Tuple<PieceColor, string>>();
            foreach (Piece unplayedPiece in board.unplayedPieces.OrderBy(p => p.number))
            {
                var pieceTypeTuple = new Tuple<PieceColor, string>(unplayedPiece.color, unplayedPiece.GetPieceNotation());
                if (shown.Contains(pieceTypeTuple)) continue;
                shown.Add(pieceTypeTuple);
            }

            return results;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
