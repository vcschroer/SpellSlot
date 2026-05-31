using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform alvoPlayer;
    private float offsetZ = -10f;
    private float offsetY = -1.25f; // câmera 1.25 unidades abaixo

    private CameraShake scriptShake;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            alvoPlayer = playerObj.transform;
        }

        scriptShake = GetComponent<CameraShake>();
    }

    void LateUpdate()
    {
        if (alvoPlayer == null) return;

        Vector3 posicaoBase = new Vector3(
            alvoPlayer.position.x,
            alvoPlayer.position.y + offsetY,
            offsetZ
        );

        if (scriptShake != null)
        {
            posicaoBase += scriptShake.ObterDeslocamento();
        }

        transform.position = posicaoBase;
    }
}