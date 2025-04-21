using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece
{
        public override List<Vector2Int> GetAvailableMove(ref ChessPiece[,] piece, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        //Direcciones de movimiento del alfil
        int[,] directions = {
            {   1,  1   },  //Arriba derecha
            {  -1,  1   },  //Arriba izquierda
            {   1, -1   },  //Abajo derecha
            {  -1, -1   }   //Abajo izquierda
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
