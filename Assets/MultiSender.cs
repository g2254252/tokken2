using UnityEngine;
using NativeWebSocket;
using System;
using System.Text;   // byte数計測

[Serializable]
public class ObjectState {
    public string name;
    public Vector3 position;
    public Quaternion rotation;
    public float timestamp;   // 遅延測定
}

[Serializable]
public class SceneState {
    public ObjectState[] objects;
}

public class MultiSender : MonoBehaviour {

    [Header("同期対象オブジェクト")]
    public Transform[] targetObjects;

    [Header("送信設定")]
    public float sendInterval = 0.05f;   // 20Hz（1秒に20回）
    private float timer = 0f;

    private WebSocket websocket;

    //帯域幅計測
    private int bytesThisSecond = 0;
    private float bandwidthTimer = 0f;


    async void Start() {
        websocket = new WebSocket("ws://192.168.11.3:8081");

        websocket.OnOpen += () => {
            Debug.Log("WebSocket Connected (Sender)");
        };

        websocket.OnError += (e) => {
            Debug.LogError("WebSocket Error (Sender): " + e);
        };

        websocket.OnClose += (e) => {
            Debug.Log("WebSocket Closed (Sender)");
        };

        await websocket.Connect();
    }

    void Update() {
        if (websocket == null || websocket.State != WebSocketState.Open) return;
        if (targetObjects == null || targetObjects.Length == 0) return;

        //送信間隔制御
        timer += Time.deltaTime;
        if (timer < sendInterval) return;
        timer = 0f;


        //シーン状態作成
        SceneState sceneState = new SceneState();
        sceneState.objects = new ObjectState[targetObjects.Length];

        for (int i = 0; i < targetObjects.Length; i++) {
            sceneState.objects[i] = new ObjectState {
                name = targetObjects[i].name,
                position = targetObjects[i].position,
                rotation = targetObjects[i].rotation,
                timestamp = Time.time   // 遅延測定
            };
        }

        // JSON化
        string json = JsonUtility.ToJson(sceneState);

        //データサイズ計測,帯域幅測定
        int byteSize = Encoding.UTF8.GetByteCount(json);
        bytesThisSecond += byteSize;


        // 送信
        websocket.SendText(json);

        //1秒ごとに帯域幅表示
        bandwidthTimer += sendInterval;
        if (bandwidthTimer >= 1.0f) {
            Debug.Log(
                $"[Sender] Bandwidth: {bytesThisSecond} bytes/s " +
                $"({bytesThisSecond / 1024f:F2} KB/s)"
            );
            bytesThisSecond = 0;
            bandwidthTimer = 0f;
        }


#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }

    async void OnApplicationQuit() {
        if (websocket != null)
            await websocket.Close();
    }
}

