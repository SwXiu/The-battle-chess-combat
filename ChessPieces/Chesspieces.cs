using System;
using System.Collections.Generic;
using UnityEngine;

public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop= 4,
    Queen = 5,
    King = 6
}

public class ChessPiece : MonoBehaviour
{
    public int team;
    public int currentX;
    public int currentY;
    public ChessPieceType type;

    private Vector3 desiredPosition;
    private Vector3 desiredScale;
    private Vector3 defaultScale;

    private void Start()
    {
        defaultScale = transform.localScale;
        desiredScale = defaultScale;
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
    }

    public virtual List<Vector2Int> GetAvailableMove(ref ChessPiece[,] piece, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();


        return r;
    }

    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if (force)
        {
            transform.position = desiredPosition;
        }
    }
    
    public virtual void SetScale(float scale, bool force = false)
    {
        desiredScale = defaultScale * scale;
        if (force)
        {
            transform.localScale = desiredScale;
        }
    }

    public virtual SpecialMove GetSpecialMove(ref ChessPiece[,] chessPieces, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        return SpecialMove.None;
    }
}

