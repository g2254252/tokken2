using UnityEngine;

// MoonOrbitScript: 月が地球の周りを回転する動きを制御するスクリプト。
// 教師操作時には自動回転を無効化できるようにする。
public class MoonOrbitScript : MonoBehaviour
{
    // earth: 地球のTransform
    public Transform earth;

    // rotationSpeed: 回転速度（度/秒）
    public float rotationSpeed = 10f;

    // autoOrbit: 自動公転を行うかどうか
    // true  : 自動で公転
    // false : 教師の操作を優先
    public bool autoOrbit = true;

    void Update()
    {
        // 教師操作中は自動回転を行わない
        if (!autoOrbit) return;

        transform.RotateAround(
            earth.position,
            Vector3.up,
            rotationSpeed * Time.deltaTime
        );
    }
}
