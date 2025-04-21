using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMove(ref ChessPiece[,] piece, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1;

        //Movimiento normal una casilla hacia adelante
        int x = currentX;
        int y = currentY + direction;

        if (y >= 0 && y < tileCountY && piece[x, y] == null)
        {
            r.Add(new Vector2Int(x, y));

            // Movimiento inicial dos casillas hacia adelante
            if ((team == 0 && currentY == 1) || (team == 1 && currentY == tileCountY - 2))
            {
                int doubleMoveY = currentY + (2 * direction);
                if (piece[x, doubleMoveY] == null)
                {
                    r.Add(new Vector2Int(x, doubleMoveY));
                }
            }
        }

        //Captura en diagonal izquierda
        x = currentX - 1;
        y = currentY + direction;
        if (x >= 0 && y >= 0 && x < tileCountX && y < tileCountY)
        {
            if (piece[x, y] != null && piece[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }

        //Captura en diagonal derecha
        x = currentX + 1;
        y = currentY + direction;
        if (x >= 0 && y >= 0 && x < tileCountX && y < tileCountY)
        {
            if (piece[x, y] != null && piece[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }

        return r;
    }

    public override SpecialMove GetSpecialMove(ref ChessPiece[,] piece, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        int direction = (team == 0) ? 1 : -1;

        //Promoción de peones
        if ((team == 0 && currentY == 6) || (team == 1 && currentY == 1))
        {
            return SpecialMove.Promotion;
        }

        //EnPassant
        if (moveList.Count > 0)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            ChessPiece lastPiece = piece[lastMove[1].x, lastMove[1].y];

            if (lastPiece.type == ChessPieceType.Pawn)
            {
                if (Mathf.Abs(lastMove[0].y - lastMove[1].y) == 2)
                {
                    if (lastMove[1].y == currentY)
                    {
                        if (lastMove[1].x == currentX - 1)
                        {
                            availableMoves.Add(new Vector2Int(currentX - 1, currentY + direction));
                            return SpecialMove.EnPassant;
                        }
                        else if (lastMove[1].x == currentX + 1)
                        {
                            availableMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
                            return SpecialMove.EnPassant;
                        }
                    }
                }
            }
        }

        return SpecialMove.None;
    }
}
