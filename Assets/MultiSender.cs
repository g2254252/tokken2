using UnityEngine;
using NativeWebSocket;
using System;
using System.Text;

#region Serializable Classes
[Serializable]
public class ObjectState {
    public string name;
    public Vector3 position;
    public Quaternion rotation;
    public float timestamp;   // 遅延測定用
    public bool isManual;     // 手動フラグ（黄色ログ用）
}

[Serializable]
public class SceneState {
    public ObjectState[] objects;
}
#endregion

public class MultiSender : MonoBehaviour {
    [Header("同期対象オブジェクト")]
    public Transform[] targetObjects;

    [Header("送信設定")]
    public float sendInterval = 0.05f; // 20Hz（1秒に20回）
    private float timer = 0f;

    private WebSocket websocket;

    // 帯域幅計測用
    private int bytesThisSecond = 0;
    private float bandwidthTimer = 0f;

    async void Start() {
        websocket = new WebSocket("ws://192.168.11.3:8081");

        // 生徒から戻ってきたパケットで遅延を計算（RTT方式）
        websocket.OnMessage += (bytes) => {
            string json = Encoding.UTF8.GetString(bytes);
            SceneState echoedData = JsonUtility.FromJson<SceneState>(json);
            
            if (echoedData.objects != null && echoedData.objects.Length > 0) {
                float rtt = Time.time - echoedData.objects[0].timestamp;
                float latency = (rtt / 2.0f) * 1000f; // 片道ミリ秒

                // 手動フラグを見て色を変える
                if (echoedData.objects[0].isManual) {
                    Debug.Log($"<color=yellow>【手動操作中】反映時間: {latency:F1}ms</color>");
                } else {
                    Debug.Log($"[自動運動中] 反映時間: {latency:F1}ms");
                }
            }
        };

        websocket.OnOpen += () => Debug.Log("WebSocket Connected (Sender)");
        websocket.OnError += (e) => Debug.LogError("WebSocket Error (Sender): " + e);
        
        await websocket.Connect();
    }

    void Update() {
        // メッセージキューの処理（常に実行）
#if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null) websocket.DispatchMessageQueue();
#endif

        // 1. 接続・初期化チェック（ガード節）
        if (websocket == null || websocket.State != WebSocketState.Open) return;
        if (targetObjects == null || targetObjects.Length == 0) return;

        // 2. ★帯域幅タイマーの更新（送信間隔の外側で確実に1秒を測る）
        bandwidthTimer += Time.deltaTime;
        if (bandwidthTimer >= 1.0f) {
            float kbps = bytesThisSecond / 1024f;
            Debug.Log($"<color=white>[Sender 統計] Bandwidth: {bytesThisSecond} bytes/s ({kbps:F2} KB/s)</color>");
            
            bytesThisSecond = 0;
            bandwidthTimer = 0f;
        }

        // 3. 送信間隔の制御
        timer += Time.deltaTime;
        if (timer < sendInterval) return;
        timer = 0f;

        // 4. MoonOrbitScriptから「手動操作中か」を取得
        var orbitScript = FindObjectOfType<MoonOrbitScript>();
        bool manualFlag = (orbitScript != null && !orbitScript.autoOrbit);

        // 5. 送信データの作成
        SceneState sceneState = new SceneState();
        sceneState.objects = new ObjectState[targetObjects.Length];
        float currentTimestamp = Time.time;

        for (int i = 0; i < targetObjects.Length; i++) {
            if (targetObjects[i] == null) continue;
            sceneState.objects[i] = new ObjectState {
                name = targetObjects[i].name,
                position = targetObjects[i].position,
                rotation = targetObjects[i].rotation,
                timestamp = currentTimestamp,
                isManual = manualFlag // 今のモードをパケットに入れる
            };
        }

        // 6. JSON化して送信
        string json = JsonUtility.ToJson(sceneState);
        byte[] sendBytes = Encoding.UTF8.GetBytes(json);
        websocket.Send(sendBytes);

        // 7. 送信したバイト数を加算
        bytesThisSecond += sendBytes.Length;
    }

    async void OnApplicationQuit() {
        if (websocket != null) await websocket.Close();
    }
}