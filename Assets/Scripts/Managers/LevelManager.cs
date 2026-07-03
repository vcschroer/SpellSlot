using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Configuracao de Spawn")]
    [Tooltip("Arraste um objeto vazio aqui para guiar onde o Player vai nascer")]
    [SerializeField] private Transform pontoDeSpawn;

    void Start()
    {
        SpawnarPersonagemSelecionado();
    }

    private void SpawnarPersonagemSelecionado()
    {
        if (GameManager.Instance == null || GameManager.Instance.personagemEscolhido == null)
        {
            Debug.LogError("[LevelManager] Nao foi possivel spawnar: GameManager ou personagemEscolhido estao nulos!");
            return;
        }

        GameObject prefabDoHeroi = GameManager.Instance.personagemEscolhido.prefabDoPersonagem;

        if (prefabDoHeroi != null)
        {
            Vector3 posicao = pontoDeSpawn != null ? pontoDeSpawn.position : Vector3.zero;
            Quaternion rotacao = pontoDeSpawn != null ? pontoDeSpawn.rotation : Quaternion.identity;

            GameObject heroiInstanciado = Instantiate(prefabDoHeroi, posicao, rotacao);
            Debug.Log($"[LevelManager] {heroiInstanciado.name} foi criado no mundo com sucesso!");
        }
        else
        {
            Debug.LogError("[LevelManager] O CharacterData selecionado nao tem um 'prefabDoPlayer' atribuido no Inspetor!");
        }
    }
}