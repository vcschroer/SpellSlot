using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instancia { get; private set; }

    private Vector3 deslocamentoTremor = Vector3.zero;

    void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Tremer(float duracao, float magnitude)
    {
        StartCoroutine(RotinaTremor(duracao, magnitude));
    }

    private IEnumerator RotinaTremor(float duracao, float magnitude)
    {
        float tempoPassado = 0f;

        while (tempoPassado < duracao)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            deslocamentoTremor = new Vector3(x, y, 0f);

            tempoPassado += Time.deltaTime;
            yield return null; 
        }

        deslocamentoTremor = Vector3.zero;
    }

    public Vector3 ObterDeslocamento()
    {
        return deslocamentoTremor;
    }
}
