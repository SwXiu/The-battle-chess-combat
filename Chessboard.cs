using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpecialMove
{
    None,
    EnPassant,
    Castling,
    Promotion
}

public class ChessBoard : MonoBehaviour
{
    [Header("Chess Board Settings")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deathSize = 0.7f;
    [SerializeField] private float deathSpacing = 0.3f;
    [SerializeField] private float dragOffset = 1.5f;

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    [Header("UI")]
    [SerializeField] private GameObject promotionScreen;

    private ChessPiece[,] chessPieces;
    private ChessPiece currentlyDragging;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<ChessPiece> deadWhitePieces = new List<ChessPiece>();
    private List<ChessPiece> deadBlackPieces = new List<ChessPiece>();
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector2Int previousMove;
    private Vector3 bounds;
    private bool isWhiteTurn;
    private bool isStartChangeCamera = false;
    private SpecialMove specialMove;
    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();
    private int promotionX, promotionY, promotionTeam;

    private void Start()
    {
        previousMove = -Vector2Int.one;
        isWhiteTurn = true;
        currentCamera = Camera.main;
        GenerateAllTiles(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPieces();
        PositionAllPieces();
    }

    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (chessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    if ((chessPieces[hitPosition.x, hitPosition.y].team == 0 && isWhiteTurn) || (chessPieces[hitPosition.x, hitPosition.y].team == 1 && !isWhiteTurn))
                    {
                        currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];

                        availableMoves = currentlyDragging.GetAvailableMove(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);

                        specialMove = currentlyDragging.GetSpecialMove(ref chessPieces, ref moveList, ref availableMoves);

                        PreventCheck();

                        HighlightTiles();
                    }
                }
            }

            if (currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(currentlyDragging.currentX, currentlyDragging.currentY);

                bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);
                if (!validMove)
                {
                    currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                }
                else
                {
                    if (previousMove != -Vector2Int.one)
                    {
                        tiles[previousMove.x, previousMove.y].layer = LayerMask.NameToLayer("Tile");
                    }

                    previousMove = previousPosition;
                    tiles[previousMove.x, previousMove.y].layer = LayerMask.NameToLayer("PreviousMove");
                }

                currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }
        else
        {
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(availableMoves, currentHover)) ? LayerMask.NameToLayer("Highlight") : LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }

            if (currentlyDragging && Input.GetMouseButtonUp(0))
            {
                currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }

        if (currentlyDragging)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if (horizontalPlane.Raycast(ray, out distance))
            {
                Vector3 point = ray.GetPoint(distance) + Vector3.up * dragOffset;
                currentlyDragging.SetPosition(point);
            }
        }
    }

    //GENERACION DE TABLERO Y PIEZAS
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY) //Funcion para generar todas las casillas
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileCountX / 2) * tileSize, 0, (tileCountX / 2) * tileSize) + boardCenter;

        tiles = new GameObject[tileCountX, tileCountY];
        for (int x = 0; x < tileCountX; x++)
        {
            for (int y = 0; y < tileCountY; y++)
            {
                tiles[x, y] = GenerateTile(tileSize, x, y);
            }
        }
    }

    private GameObject GenerateTile(float tileSize, int x, int y) //Funcion para generar una casilla
    {
        char column = (char)('A' + y);
        int row = x + 1;

        GameObject tile = new GameObject(string.Format("{0}{1}", column, row));
        tile.transform.SetParent(transform);

        Mesh mesh = new Mesh();
        tile.AddComponent<MeshFilter>().mesh = mesh;
        tile.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        tile.layer = LayerMask.NameToLayer("Tile");
        tile.AddComponent<BoxCollider>();

        return tile;
    }

    private void SpawnAllPieces() //Funcion para generar todas las piezas
    {
        chessPieces = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];

        int whiteTeam = 0;
        int blackTeam = 1;

        //Piezas blancas
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
        }

        //Piezas negras
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        for (int i = 0; i < TILE_COUNT_X; i++)
        {
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
        }

    }

    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team) //Funcion para generar una pieza
    {
        ChessPiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();
        cp.type = type;
        cp.team = team;
        cp.GetComponent<MeshRenderer>().material = teamMaterials[team];

        if (team == 1)
        {
            cp.transform.rotation = Quaternion.Euler(0, 180, 0) * cp.transform.rotation;
        }

        return cp;
    }

    //POSICIONAMIENTO
    private void PositionAllPieces() //Funcion para posicionar todas las piezas
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    PositionSinglePiece(x, y, true);
                }
            }
        }
    }
    private void PositionSinglePiece(int x, int y, bool force = false) //Funcion para posicionar una pieza
    {
        chessPieces[x, y].currentX = x;
        chessPieces[x, y].currentY = y;
        chessPieces[x, y].SetPosition(GetTileCenter(x, y), force);
    }
    private Vector3 GetTileCenter(int x, int y) //Funcion para obtener el centro de una casilla
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    //ILUMINACION DE CASILLAS
    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }
    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }

        availableMoves.Clear();
    }
    
    //MOVIMIENTOS ESPECIALES
    private void ProcessSpecialMove()
    {
        if (specialMove == SpecialMove.Promotion)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            ChessPiece targetPawn = chessPieces[lastMove[1].x, lastMove[1].y];

            if (targetPawn.type == ChessPieceType.Pawn)
            {
                if (targetPawn.team == 0 && lastMove[1].y == 7 || targetPawn.team == 1 && lastMove[1].y == 0)
                {
                    promotionX = lastMove[1].x;
                    promotionY = lastMove[1].y;
                    promotionTeam = targetPawn.team;
                    promotionScreen.SetActive(true);
                    Time.timeScale = 0f;
                    return;
                }
            }
        }

        if (specialMove == SpecialMove.EnPassant)
        {
            var newMove = moveList[moveList.Count - 1];
            ChessPiece myPawn = chessPieces[newMove[1].x, newMove[1].y];
            var targetPawnPosition = moveList[moveList.Count - 2];
            ChessPiece enemyPawn = chessPieces[targetPawnPosition[1].x, targetPawnPosition[1].y];

            if (myPawn.currentX == enemyPawn.currentX)
            {
                if (myPawn.currentY == enemyPawn.currentY - 1 || myPawn.currentY == enemyPawn.currentY + 1)
                {
                    if (enemyPawn.team == 0)
                    {
                        deadWhitePieces.Add(enemyPawn);
                        enemyPawn.SetScale(deathSize);
                        enemyPawn.SetPosition(new Vector3(8 * tileSize, yOffset * 1.5f, 8 * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2) + (Vector3.left * deathSpacing) * deadWhitePieces.Count);
                    }
                    else
                    {
                        deadBlackPieces.Add(enemyPawn);
                        enemyPawn.SetScale(deathSize);
                        enemyPawn.SetPosition(new Vector3(8 * tileSize, yOffset * 1.5f, 8 * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2) + (Vector3.left * deathSpacing) * deadBlackPieces.Count);
                    }

                    chessPieces[enemyPawn.currentX, enemyPawn.currentY] = null;
                }
            }
        }

        if (specialMove == SpecialMove.Castling)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];

            if (lastMove[1].x == 2) //Enroque largo
            {
                if (lastMove[1].y == 0)
                {
                    ChessPiece rook = chessPieces[0, 0];
                    chessPieces[3, 0] = rook;
                    PositionSinglePiece(3, 0);
                    chessPieces[0, 0] = null;
                }
                else if (lastMove[1].y == 7)
                {
                    ChessPiece rook = chessPieces[0, 7];
                    chessPieces[3, 7] = rook;
                    PositionSinglePiece(3, 7);
                    chessPieces[0, 7] = null;
                }
            }
            else if (lastMove[1].x == 6) //Enroque corto
            {
                if (lastMove[1].y == 0)
                {
                    ChessPiece rook = chessPieces[7, 0];
                    chessPieces[5, 0] = rook;
                    PositionSinglePiece(5, 0);
                    chessPieces[7, 0] = null;
                }
                else if (lastMove[1].y == 7)
                {
                    ChessPiece rook = chessPieces[7, 7];
                    chessPieces[5, 7] = rook;
                    PositionSinglePiece(5, 7);
                    chessPieces[7, 7] = null;
                }
            }
        }
    }

    public void OnPromotionButtonClicked(int pieceType)
    {
        ChessPieceType newType = ChessPieceType.Pawn;
        switch (pieceType)
        {
            case 1:
                newType = ChessPieceType.Queen;
                break;
            case 2:
                newType = ChessPieceType.Rook;
                break;
            case 3:
                newType = ChessPieceType.Bishop;
                break;
            case 4:
                newType = ChessPieceType.Knight;
                break;
        }
        PromotePawn(promotionX, promotionY, promotionTeam, newType);
        promotionScreen.SetActive(false);
        Time.timeScale = 1f;
    }

    private void PromotePawn(int x, int y, int team, ChessPieceType newType)
    {
        Destroy(chessPieces[x, y].gameObject);
        ChessPiece newPiece = SpawnSinglePiece(newType, team);
        chessPieces[x, y] = newPiece;
        PositionSinglePiece(x, y);
    }

    //JAQUE
    private void PreventCheck()
    {
        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    if (chessPieces[x, y].type == ChessPieceType.King)
                    {
                        if (chessPieces[x, y].team == currentlyDragging.team)
                        {
                            targetKing = chessPieces[x, y];
                        }
                    }
                }
            }
        }

        SimulateMoveForSinglePiece(currentlyDragging, ref availableMoves, targetKing);

    }

    private void SimulateMoveForSinglePiece(ChessPiece cp, ref List<Vector2Int> moves, ChessPiece targetKing)
    {
        int actualX = cp.currentX;
        int actualY = cp.currentY;

        List<Vector2Int> movesToRemove = new List<Vector2Int>();

        for (int i = 0; i < moves.Count; i++)
        {
            int simX = moves[i].x;
            int simY = moves[i].y;

            Vector2Int kingPositionThisSim = new Vector2Int(targetKing.currentX, targetKing.currentY);
            if (cp.type == ChessPieceType.King)
            {
                kingPositionThisSim = new Vector2Int(simX, simY);
            }

            ChessPiece[,] simulation = new ChessPiece[TILE_COUNT_X, TILE_COUNT_Y];
            List<ChessPiece> simAttakingPieces = new List<ChessPiece>();
            for (int x = 0; x < TILE_COUNT_X; x++)
            {
                for (int y = 0; y < TILE_COUNT_Y; y++)
                {
                    if (chessPieces[x, y] != null)
                    {
                        simulation[x, y] = chessPieces[x, y];
                        if (simulation[x, y].team != cp.team)
                        {
                            simAttakingPieces.Add(simulation[x, y]);
                        }
                    }
                }
            }

            simulation[actualX, actualY] = null;
            cp.currentX = simX;
            cp.currentY = simY;
            simulation[simX, simY] = cp;

            var deadPiece = simAttakingPieces.Find(c => c.currentX == simX && c.currentY == simY);
            if (deadPiece != null)
            {
                simAttakingPieces.Remove(deadPiece);
            }

            List<Vector2Int> simMoves = new List<Vector2Int>();
            for (int n = 0; n < simAttakingPieces.Count; n++)
            {
                var pieceMoves = simAttakingPieces[n].GetAvailableMove(ref simulation, TILE_COUNT_X, TILE_COUNT_Y);
                for (int m = 0; m < pieceMoves.Count; m++)
                {
                    simMoves.Add(pieceMoves[m]);
                }
            }

            if (ContainsValidMove(simMoves, kingPositionThisSim))
            {
                movesToRemove.Add(moves[i]);
            }

            cp.currentX = actualX;
            cp.currentY = actualY;


        }

        for (int i = 0; i < movesToRemove.Count; i++)
        {
            moves.Remove(movesToRemove[i]);
        }
    }

    private bool CheckForCheckmate()
    {
        var lastMove = moveList[moveList.Count - 1];
        int targetTeam = (chessPieces[lastMove[1].x, lastMove[1].y].team == 0) ? 1 : 0;

        List<ChessPiece> attackingPiece = new List<ChessPiece>();
        List<ChessPiece> defendingPiece = new List<ChessPiece>();
        ChessPiece targetKing = null;
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    if (chessPieces[x, y].team != targetTeam)
                    {
                        defendingPiece.Add(chessPieces[x, y]);
                        if (chessPieces[x, y].type == ChessPieceType.King)
                        {
                            targetKing = chessPieces[x, y];
                        }
                    }
                    else
                    {
                        attackingPiece.Add(chessPieces[x, y]);
                    }
                }
            }
        }

        List<Vector2Int> currentAvailableMoves = new List<Vector2Int>();
        for (int i = 0; i < attackingPiece.Count; i++)
        {
            var pieceMoves = attackingPiece[i].GetAvailableMove(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
            for (int m = 0; m < pieceMoves.Count; m++)
            {
                currentAvailableMoves.Add(pieceMoves[m]);
            }
        }

        if (ContainsValidMove(currentAvailableMoves, new Vector2Int(targetKing.currentX, targetKing.currentY)))
        {
            for (int i = 0; i < defendingPiece.Count; i++)
            {
                List<Vector2Int> defendingMoves = defendingPiece[i].GetAvailableMove(ref chessPieces, TILE_COUNT_X, TILE_COUNT_Y);
                SimulateMoveForSinglePiece(defendingPiece[i], ref defendingMoves, targetKing);

                if (defendingMoves.Count != 0)
                {
                    return false;
                }

                return true;
            }
        }

        return false;
    }

    //OPERAIONES DEL JUEGO
    private Vector2Int LookupTileIndex(GameObject hitInfo) //Funcion para buscar la posicion de una casilla
    {
        for (int x = 0; x < TILE_COUNT_X; x++)
        {
            for (int y = 0; y < TILE_COUNT_Y; y++)
            {
                if (tiles[x, y] == hitInfo)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return -Vector2Int.one;
    }
    private bool MoveTo(ChessPiece piece, int x, int y) //Funcion para mover una pieza
    {
        if (!ContainsValidMove(availableMoves, new Vector2(x, y)))
        {
            return false;
        }

        Vector2Int previousPosition = new Vector2Int(piece.currentX, piece.currentY);

        if (chessPieces[x, y] != null)
        {
            ChessPiece ocp = chessPieces[x, y];

            if (piece.team == ocp.team)
            {
                return false;
            }

            if (ocp.team == 0)
            {
                if (ocp.type == ChessPieceType.King)
                {
                    UnityEngine.Debug.Log("Black Wins");
                }

                deadWhitePieces.Add(ocp);
                ocp.SetScale(deathSize);
                ocp.SetPosition(new Vector3(8 * tileSize, yOffset * 1.5f, 8 * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2) + (Vector3.left * deathSpacing) * deadWhitePieces.Count);
            }
            else
            {
                if (ocp.type == ChessPieceType.King)
                {
                    UnityEngine.Debug.Log("White Wins");
                }

                deadBlackPieces.Add(ocp);
                ocp.SetScale(deathSize);
                ocp.SetPosition(new Vector3(-1 * tileSize, yOffset * 1.5f, -1 * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2) + (Vector3.right * deathSpacing) * deadBlackPieces.Count);
            }

        }

        chessPieces[x, y] = piece;
        chessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y);

        isWhiteTurn = !isWhiteTurn;

        moveList.Add(new Vector2Int[] { previousPosition, new Vector2Int(x, y) });

        ProcessSpecialMove();

        if (CheckForCheckmate())
        {
            //Saltar a la parte de combate
        }

        return true;
    }
    private bool ContainsValidMove(List<Vector2Int> moves, Vector2 position) //Funcion para verificar si una casilla esta en la lista de movimientos validos
    {
        for (int i = 0; i < moves.Count; i++)
        {
            if (moves[i].x == position.x && moves[i].y == position.y)
            {
                return true;
            }
        }
        return false;
    }
}
