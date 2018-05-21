using HiveLib.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FutureMoveSpotScript : MonoBehaviour
{
    public Move Move { get; set; }

    private void OnMouseDown()
    {
        GameObject builderGameObject = GameObject.Find("BuilderGameObject");
        GameBoardScript script = builderGameObject.GetComponent<GameBoardScript>();

        script.MakeMove(Move);
    }

}
