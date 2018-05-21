using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HiveLib;
using HiveLib.Models;
using System.Linq;
using HiveLib.Models.Pieces;

public class PieceScript : MonoBehaviour {


    private void OnMouseDown()
    {
        GameObject builderGameObject = GameObject.Find("BuilderGameObject");
        GameBoardScript script = builderGameObject.GetComponent<GameBoardScript>();
        Board board = script.currentBoard;
        Piece thisPiece = NotationParser.GetPieceByNotation(this.name);
        bool alreadyPlaced = !board.unplayedPieces.Contains(thisPiece);
        List<Hex> hexes = null;

        if (alreadyPlaced)
        {
            hexes = board.AllMoves.Where(m => thisPiece.Equals(m.pieceToMove)).Select(m => m.hex).ToList();
        }
        else
        {
            if (board.hivailableHexes.Count == 0) board.RefreshDependantBoardData();
            if (thisPiece.color == PieceColor.White)
                hexes = board.hivailableHexes.Where(kvp => kvp.Value.WhiteCanPlace).Select(kvp => kvp.Key).ToList();
            else
                hexes = board.hivailableHexes.Where(kvp => kvp.Value.BlackCanPlace).Select(kvp => kvp.Key).ToList();
        }

        script.ShowFutureMoves(thisPiece, hexes);
    }
}
