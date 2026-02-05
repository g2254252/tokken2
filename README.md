## Implemented Programs (Author)

### Unity (C#)
- Assets/MoonOrbitScript.cs
  月が地球の周囲を自動公転する挙動を制御するスクリプト。
  教師操作時には自動回転を停止し、教材操作を優先できる。 
  

- Assets/MultiSender.cs  
  教師端末側で複数オブジェクトの状態（位置・回転）を取得し、
  WebSocketを用いて一定周期で送信する同期用スクリプト。
  送信データ量から通信帯域幅を算出し、送信時刻を用いた遅延測定も行う。
  

- Assets/MQTTSender.cs
  MQTT通信を用いて、Unityシーン内のカメラおよび天体オブジェクトの
  位置・回転情報をJSON形式で送信する同期用スクリプト。
  

- Assets/MATTReceiver.cs
  MQTTで受信した同期データを解析し、
  対応するUnityオブジェクトのTransformを更新する受信用スクリプト。
  

### WebSocket / Server (JavaScript)
- websocket-server/server.js  
  クライアントから送信された状態データを中継するWebSocketサーバ


### slides
- slides/特別研究II 発表資料.pptx
  発表資料


## External Libraries
- NativeWebSocket  

- M2Mqtt (Eclipse Paho MQTT C#)  

