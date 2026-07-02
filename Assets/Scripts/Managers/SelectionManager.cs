using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectionManager : MonoBehaviour
{
    [Header("Configuração de Painéis")]
    [SerializeField] private GameObject painelPersonagens;
    [SerializeField] private GameObject painelMundos;

    [Header("Botões dos Mundos")]
    [SerializeField] private Button botaoMundo1;
    [SerializeField] private Button botaoMundo2;
    [SerializeField] private Button botaoMundo3;

    [Header("Nomes das Cenas de Gameplay")]
    [SerializeField] private string cenaMundo1 = "Mundo1";
    [SerializeField] private string cenaMundo2 = "Mundo2";
    [SerializeField] private string cenaMundo3 = "Mundo3";

    [Header("Voltar para o Menu")]
    [SerializeField] private string cenaMenuPrincipal = "Menu";

    private void Start()
    {
        painelPersonagens.SetActive(true);
        painelMundos.SetActive(false);
    }

    public void SelecionarPersonagem(CharacterData personagem)
    {
        if (GameManager.Instance != null)
        {
            GameKeyManager(personagem);
        }

        painelPersonagens.SetActive(false);
        painelMundos.SetActive(true);

        AtualizarPainelDeMundos();
    }

    public void VoltarParaMenuPrincipal()
    {
        CarregarCena(cenaMenuPrincipal);
    }

    public void AtualizarPainelDeMundos()
    {
        if (ProgressManager.Instance == null) return;

        int mundoLiberado = ProgressManager.Instance.GetMundoMaximoLiberado();

        botaoMundo1.interactable = true;
        botaoMundo2.interactable = (mundoLiberado >= 2);
        botaoMundo3.interactable = (mundoLiberado >= 3);
    }

    public void EntrarNoMundo(int numeroDoMundo)
    {
        string cenaAlvo = cenaMundo1;
        if (numeroDoMundo == 2) cenaAlvo = cenaMundo2;
        if (numeroDoMundo == 3) cenaAlvo = cenaMundo3;

        CarregarCena(cenaAlvo);
    }

    public void VoltarParaPersonagens()
    {
        painelMundos.SetActive(false);
        painelPersonagens.SetActive(true);
    }

    private void GameKeyManager(CharacterData data)
    {
        GameManager.Instance.personagemEscolhido = data;
        Debug.Log($"Personagem {data.nomeDoPersonagem} salvo!");
    }

    private void CarregarCena(string nomeDaCena)
    {
        if (TransitionManager.Instance != null)
            TransitionManager.Instance.CarregarCena(nomeDaCena);
        else
            SceneManager.LoadScene(nomeDaCena);
    }
}