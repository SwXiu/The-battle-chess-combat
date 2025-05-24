using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEditor.PackageManager;
using UnityEngine;

public class Client : MonoBehaviour
{
    public NetworkDriver driver;
    private NetworkConnection connection;
    private bool isActive = false;
    public Action connectionDropped;

    public void Init(string ip, ushort port)
    {
        driver = NetworkDriver.Create();
        NetworkEndpoint endpoint = NetworkEndpoint.Parse(ip, port);

        connection = driver.Connect(endpoint);

        Debug.Log("Connecting to server at " + ip + ":" + port);

        isActive = true;

        RegisterToEvent();
    }
    public void Shutdown()
    {
        UnregisterToEvent();
        driver.Dispose();
        isActive = false;
        connection = default(NetworkConnection);
    }

    public void OnDestroy()
    {
        Shutdown();
    }

    public void Update()
    {
        if (!isActive)
        {
            return;
        }

        driver.ScheduleUpdate().Complete();

        CheckAlive();
        UpdateMessages();
    }

    private void CheckAlive()
    {
        if (!connection.IsCreated && isActive)
        {
            Debug.Log("Connection dropped");
            connectionDropped?.Invoke();
            Shutdown();
        }
    }

    private void UpdateMessages()
    {
        DataStreamReader stream;

        NetworkEvent.Type cmd;
        while ((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty)
        {
            switch (cmd)
            {
                case NetworkEvent.Type.Connect:
                    Debug.Log("Client connected");
                    // SendToServer(new NetWelcome());
                    break;
                case NetworkEvent.Type.Data:
                    Debug.Log("Data received");
                    NetUtility.OnData(stream, default(NetworkConnection));
                    break;
                case NetworkEvent.Type.Disconnect:
                    Debug.Log("Client disconnected");
                    connection = default(NetworkConnection);
                    connectionDropped?.Invoke();
                    Shutdown();
                    break;
            }
        }

    }

    public void SendToServer(NetMessage message)
    {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        message.Serialize(ref writer);
        driver.EndSend(writer);
    }

    private void RegisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE += OnKeepAlive;
    }

    private void UnregisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE -= OnKeepAlive;
    }

    private void OnKeepAlive(NetMessage message)
    {
        SendToServer(message);
    }
}
