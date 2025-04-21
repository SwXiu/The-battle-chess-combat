using System.Collections.Generic;
using UnityEngine;

public class Queen : ChessPiece
{
    public override List<Vector2Int> GetAvailableMove(ref ChessPiece[,] piece, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        //Direcciones de movimiento de la reina
        int[,] directions = {
            {  0,  1  },  //Arriba
            {  0, -1  },  //Abajo
            {  1,  0  },  //Derecha
            { -1,  0  },  //Izquierda
            {  1,  1  },  //Arriba derecha
            { -1,  1  },  //Arriba izquierda
            {  1, -1  },  //Abajo derecha
            { -1, -1  }   //Abajo izquierda
        };

        for (int i = 0; i < directions.GetLength(0); i++)
        {
            int x = currentX;
            int y = currentY;

            while (true)
            {
                x += directions[i, 0];
                y += directions[i, 1];

                if (x < 0 || x >= tileCountX || y < 0 || y >= tileCountY)
                {
                    break;
                }
                if (piece[x, y] == null)
                {
                    r.Add(new Vector2Int(x, y));
                }
                else
                {
                    if (piece[x, y].team != team)
                    {
                        r.Add(new Vector2Int(x, y));
                    }
                    break;
                }
            }
        }

        return r;
    }
}
