using System;
using System.Net;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq.Expressions;
using System.Collections.Generic;

public class UDPReceiver : MonoBehaviour
{
    UdpClient client;
    Thread receiveThread;
    public int port = 5005;

    private string lastJson = "";
    private object lockObject = new();

    public Dictionary<string, HandData> Hands = new Dictionary<string, HandData>();
    public string currentGesture = null;
    public float currentPinchDistance;

    private int udpMessageCount = 0;
    private float lastReportTime = 0;
    private int frameCount = 0;

    void Start()
    {
        client = new UdpClient(port);
        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        lastReportTime = Time.time;
    }

    void ReceiveData()
    {
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
        while (true)
        {
            try
            {
                byte[] data = client.Receive(ref endPoint);
                string text = Encoding.UTF8.GetString(data);
                
                udpMessageCount++;

                lock(lockObject){
                    lastJson = text; // vai ser usado como semáforo
                        } 

            }
            catch (SocketException e)
            {
                Debug.Log("UDP Error: " + e.Message);
            }
        }
    }

    void Update()
    {
        string json;
        lock (lockObject) { json = lastJson; }

        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                  // convert a string JSOn em objetos
                HandDataList dataList = JsonUtility.FromJson<HandDataList>(json);

                if (dataList.hands != null)
                {
                    // Clear the dictionary before updating
                    Hands.Clear();
                    foreach (var hand in dataList.hands)
                    {
                        if (hand.on_screen)
                        {
                            // Use handedness ("Right" or "Left") as the key
                            Hands[hand.handed] = hand;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Failing in parsing json object: " + e.Message);
            }
        }
        frameCount++;
        if(Time.time - lastReportTime > 2f)
        {
            float elapsed = Time.time - lastReportTime;
            float fps = frameCount / elapsed;
            //Debug.Log($"[PERFORMANCE] FPS:{fps:F1},UDP msg / s:{ udpMessageCount}");

            lastReportTime = Time.time;
            frameCount = 0;
            udpMessageCount = 0;
        }
    }

    void OnApplicationQuit()
    {
        receiveThread?.Abort();
        client?.Close();
}

}


[Serializable]
public class Landmark
{
    public float x, y, z;
}


[Serializable]
public class HandData
{
    public Landmark[] landmarks;
    public string gesture;
    public float pinch_distance;
    public string handed;
    public bool on_screen;
}

[Serializable]
public class HandDataList
{
    public HandData[] hands;
}
