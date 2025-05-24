using System;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public enum OpCode
{
    KEEP_ALIVE = 1,
    WELCOME = 2,
    START_GAME = 3,
    MAKE_MOVE = 4,
    REMATCH = 5,
}
public static class NetUtility
{
    public static void OnData(DataStreamReader stream, NetworkConnection connection, Server server = null)
    {
        NetMessage message = null;
        var opCode = (OpCode)stream.ReadByte();
        switch (opCode)
        {
            case OpCode.KEEP_ALIVE:
                message = new NetKeepAlive(stream);
                break;
            case OpCode.WELCOME:
                message = new NetWelcome(stream);
                break;
            case OpCode.START_GAME:
                message = new NetStartGame(stream);
                break;
            case OpCode.MAKE_MOVE:
                message = new NetMakeMove(stream);
                break;
            case OpCode.REMATCH:
                message = new NetRematch(stream);
                break;
            default:
                Debug.LogError("Unknown OpCode: " + opCode);
                break;
        }
        
        if (server != null)
        {
            message.ReceivedOnServer(connection);
        }
        else
        {
            message.ReceivedOnClient();
        }
    }
    
    public static Action<NetMessage> C_KEEP_ALIVE;
    public static Action<NetMessage> C_WELCOME;
    public static Action<NetMessage> C_START_GAME;
    public static Action<NetMessage> C_MAKE_MOVE;
    public static Action<NetMessage> C_REMATCH;
    public static Action<NetMessage, NetworkConnection> S_KEEP_ALIVE;
    public static Action<NetMessage, NetworkConnection> S_WELCOME;
    public static Action<NetMessage, NetworkConnection> S_START_GAME;
    public static Action<NetMessage, NetworkConnection> S_MAKE_MOVE;
    public static Action<NetMessage, NetworkConnection> S_REMATCH;
}


