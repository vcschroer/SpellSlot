using UnityEngine;
using UnityEngine.InputSystem;

public class SlotMachine : MonoBehaviour
{
    public enum TipoRecompensa { Dinheiro, TamanhoEspada, VelocidadeAtaque }

    [Header("Referências do Player")]
    [SerializeField] private PlayerController scriptPlayer;
    [SerializeField] private Sword scriptEspada;

    [Header("Conexão com a Parte Visual")]
    [SerializeField] private SlotMachineVisual scriptVisual;
    [Header("Valores dos Bônus - DINHEIRO")]
    [SerializeField] private int dinheiroPequeno = 50;
    [SerializeField] private int dinheiroGrande = 200;

    [Header("Valores dos Bônus - TAMANHO DA ESPADA")]
    [SerializeField] private int gomosPequeno = 1;
    [SerializeField] private int gomosGrande = 3;

    [Header("Valores dos Bônus - VELOCIDADE DE ATAQUE")]
    [SerializeField] private float velAtaquePequeno = 0.25f;
    [SerializeField] private float velAtaqueGrande = 0.75f;

    private bool menuAberto = false;

    void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            AlternarCaçaNiquel();
        }
    }

    private void AlternarCaçaNiquel()
    {
        menuAberto = !menuAberto;
        if (menuAberto)
        {
            GirarCaçaNiquel();
        }
    }

    private void GirarCaçaNiquel()
    {
        TipoRecompensa slot1 = (TipoRecompensa)Random.Range(0, 3);
        TipoRecompensa slot2 = (TipoRecompensa)Random.Range(0, 3);
        TipoRecompensa slot3 = (TipoRecompensa)Random.Range(0, 3);


        if (scriptVisual != null)
        {
            scriptVisual.AtualizarVisualDosSlots(slot1, slot2, slot3);
        }

        int qtdDinheiro = 0, qtdTamanho = 0, qtdVelocidade = 0;

        ContarSlot(slot1, ref qtdDinheiro, ref qtdTamanho, ref qtdVelocidade);
        ContarSlot(slot2, ref qtdDinheiro, ref qtdTamanho, ref qtdVelocidade);
        ContarSlot(slot3, ref qtdDinheiro, ref qtdTamanho, ref qtdVelocidade);

        AplicarRecompensaDinheiro(qtdDinheiro);
        AplicarRecompensaTamanho(qtdTamanho);
        AplicarRecompensaVelocidade(qtdVelocidade);

        menuAberto = false;
    }

    private void ContarSlot(TipoRecompensa slot, ref int din, ref int tam, ref int vel)
    {
        switch (slot)
        {
            case TipoRecompensa.Dinheiro: din++; break;
            case TipoRecompensa.TamanhoEspada: tam++; break;
            case TipoRecompensa.VelocidadeAtaque: vel++; break;
        }
    }

    private void AplicarRecompensaDinheiro(int quantidade)
    {
        if (quantidade == 0 || scriptPlayer == null) return;

        int bonusGanhado = quantidade switch
        {
            1 => dinheiroPequeno,
            2 => dinheiroGrande,
            3 => dinheiroPequeno + dinheiroGrande,
            _ => 0
        };

        scriptPlayer.GanharDinheiro(bonusGanhado);

    }

    private void AplicarRecompensaTamanho(int quantidade)
    {
        if (quantidade == 0 || scriptEspada == null) return;

        int gomosParaAdicionar = quantidade switch
        {
            1 => gomosPequeno,
            2 => gomosGrande,
            3 => gomosPequeno + gomosGrande,
            _ => 0
        };

        int tamanhoAtual = scriptEspada.QuantidadeSegmentosMeio;
        scriptEspada.MudarQuantidadeSegmentos(tamanhoAtual + gomosParaAdicionar);

    }

    private void AplicarRecompensaVelocidade(int quantidade)
    {
        if (quantidade == 0 || scriptPlayer == null) return;

        float velParaAdicionar = quantidade switch
        {
            1 => velAtaquePequeno,
            2 => velAtaqueGrande,
            3 => velAtaquePequeno + velAtaqueGrande,
            _ => 0f
        };

        scriptPlayer.attackSpeed += velParaAdicionar;

    }
}
