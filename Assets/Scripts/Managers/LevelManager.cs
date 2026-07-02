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
        // Garante que o GameManager e o dado do personagem existem
        if (GameManager.Instance == null || GameManager.Instance.personagemEscolhido == null)
        {
            Debug.LogError("[LevelManager] Nao foi possivel spawnar: GameManager ou personagemEscolhido estao nulos!");
            return;
        }

        // Pega o prefab do Player de dentro do CharacterData
        GameObject prefabDoHeroi = GameManager.Instance.personagemEscolhido.prefabDoPersonagem;

        if (prefabDoHeroi != null)
        {
            // Calcula posicao e rotacao baseada no ponto de spawn (ou no zero se nao houver ponto)
            Vector3 posicao = pontoDeSpawn != null ? pontoDeSpawn.position : Vector3.zero;
            Quaternion rotacao = pontoDeSpawn != null ? pontoDeSpawn.rotation : Quaternion.identity;

            // Cria o personagem no mundo!
            GameObject heroiInstanciado = Instantiate(prefabDoHeroi, posicao, rotacao);
            Debug.Log($"[LevelManager] {heroiInstanciado.name} foi criado no mundo com sucesso!");
        }
        else
        {
            Debug.LogError("[LevelManager] O CharacterData selecionado nao tem um 'prefabDoPlayer' atribuido no Inspetor!");
        }
    }
}