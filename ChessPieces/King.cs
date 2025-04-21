using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override List<Vector2Int> GetAvailableMove(ref ChessPiece[,] piece, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        //Direcciones de movimiento del rey 
        int[,] moves = {
            {  0,  1  }, //Arriba
            {  0, -1  }, //Abajo
            {  1,  0  }, //Derecha
            { -1,  0  }, //Izquierda
            {  1,  1  }, //Derecha arriba
            { -1,  1  }, //Izquierda arriba
            {  1, -1  }, //Derecha abajo
            { -1, -1  }  //Izquierda abajo
        };

        for (int i = 0; i < moves.GetLength(0); i++)
        {
            int x = currentX + moves[i, 0];
            int y = currentY + moves[i, 1];

            if (x >= 0 && x < tileCountX && y >= 0 && y < tileCountY)
            {
                if (piece[x, y] == null || piece[x, y].team != team)
                {
                    r.Add(new Vector2Int(x, y));
                }
            }
        }

        return r;
    }

    public override SpecialMove GetSpecialMove(ref ChessPiece[,] piece, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        SpecialMove specialMove = SpecialMove.None;

        bool kingMove = false;
        for (int i = 0; i < moveList.Count; i++)
        {
            if (moveList[i][0].x == 4 && moveList[i][0].y == (team == 0 ? 0 : 7))
            {
                kingMove = true;
                break;
            }
        }

        if (!kingMove && currentX == 4)
        {
            int y = (team == 0) ? 0 : 7;

            // Enroque corto (Rey lado derecho)
            if (CanCastle(piece, moveList, 7, y, new int[] { 5, 6 }))
            {
                availableMoves.Add(new Vector2Int(6, y));
                specialMove = SpecialMove.Castling;
            }

            // Enroque largo (Dama lado izquierdo)
            if (CanCastle(piece, moveList, 0, y, new int[] { 1, 2, 3 }))
            {
                availableMoves.Add(new Vector2Int(2, y));
                specialMove = SpecialMove.Castling;
            }
        }

        return specialMove;
    }

    private bool CanCastle(ChessPiece[,] piece, List<Vector2Int[]> moveList, int x, int y, int[] emptyTiles)
    {
        bool rookMove = false;
        for (int i = 0; i < moveList.Count; i++)
        {
            if (moveList[i][0].x == x && moveList[i][0].y == y)
            {
                rookMove = true;
                break;
            }
        }

        if (rookMove || piece[x, y] == null || piece[x, y].type != ChessPieceType.Rook || piece[x, y].team != team)
        {
            return false;
        }

        for (int i = 0; i < emptyTiles.Length; i++)
        {
            if (piece[emptyTiles[i], y] != null)
            {
                return false;
            }
        }

        return true;
    }
}