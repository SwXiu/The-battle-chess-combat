using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    public override List<Vector2Int> GetAvailableMove(ref ChessPiece[,] piece, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        //Movimientos en L del caballo 
        int[,] moves = {
            {  1,  2  }, //Derecha arriba
            {  2,  1  }, //Derecha poco arriba
            { -1,  2  }, //Izquierda arriba
            { -2,  1  }, //Izquierda poco arriba
            {  1, -2  }, //Derecha abajo
            {  2, -1  }, //Derecha poco abajo
            { -1, -2  }, //Izquierda abajo
            { -2, -1  }  //Izquierda poco abajo
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

}
