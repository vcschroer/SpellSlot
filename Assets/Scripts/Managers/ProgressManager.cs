using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int GetMundoMaximoLiberado()
    {
        return PlayerPrefs.GetInt("MundoMaximoLiberado", 1);
    }

    public void DesbloquearProximoMundo(int mundoConcluido)
    {
        int proximoMundo = mundoConcluido + 1;
        int atualLiberado = GetMundoMaximoLiberado();

        if (proximoMundo > atualLiberado)
        {
            PlayerPrefs.SetInt("MundoMaximoLiberado", proximoMundo);
            PlayerPrefs.Save();
            Debug.Log($"Parabéns! Mundo {proximoMundo} foi desbloqueado!");
        }
    }

    public void ResetarProgresso()
    {
        PlayerPrefs.SetInt("MundoMaximoLiberado", 1);
        PlayerPrefs.Save();
    }
}