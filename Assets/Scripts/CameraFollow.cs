using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform alvoPlayer;
    private float offsetZ = -10f;

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

        Vector3 posicaoBase = new Vector3(alvoPlayer.position.x, alvoPlayer.position.y, offsetZ);

        if (scriptShake != null)
        {
            posicaoBase += scriptShake.ObterDeslocamento();
        }

        transform.position = posicaoBase;
    }
}