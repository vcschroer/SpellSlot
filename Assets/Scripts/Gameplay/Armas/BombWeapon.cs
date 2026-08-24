using System.Collections;
using UnityEngine;

public class BombWeapon : BaseWeapon
{
    [Header("Configurações do Prefab")]
    [SerializeField] private GameObject prefabBomba;

    [Header("Configurações da Bomba")]
    [SerializeField] private int danoBomba = 25;
    [SerializeField] private float tamanhoBomba = 1f;
    [SerializeField] private float raioExplosao = 2.5f;
    [SerializeField] private float tempoParaExplodir = 3f;

    [Header("Configurações de Disparo Múltiplo")]
    [SerializeField] private int quantidadeBombas = 1;
    [SerializeField] private float delayEntreBombas = 0.3f;

    [Header("Filtro de Inimigos")]
    [SerializeField] private LayerMask layerInimigos;

    [Header("Configurações de Jackpot")]
    [SerializeField] private float duracaoJackpot = 5f;
    [SerializeField] private float tempoEntreBombasJackpot = 0.15f;

    public int QuantidadeBombas
    {
        get => quantidadeBombas;
        set => quantidadeBombas = Mathf.Max(1, value);
    }

    protected override void Start()
    {
        tipoArma = TipoArma.Bomba;
        tempoEntreAtaquesBase = 2f; 
        base.Start();
    }

    protected override void DispararAtaqueAutomatico()
    {
        StartCoroutine(RotinaSoltarBombas());
    }

    private IEnumerator RotinaSoltarBombas()
    {
        for (int i = 0; i < quantidadeBombas; i++)
        {
            SoltarUmaBomba();

            if (i < quantidadeBombas - 1)
            {
                yield return new WaitForSeconds(delayEntreBombas);
            }
        }
    }

    private void SoltarUmaBomba()
    {
        if (prefabBomba == null || player == null) return;

        Vector3 posInstanciar = player.transform.position;
        GameObject bombaObj = Instantiate(prefabBomba, posInstanciar, Quaternion.identity);

        BombItem bombScript = bombaObj.GetComponent<BombItem>();
        if (bombScript != null)
        {
            bombScript.Inicializar(danoBomba, raioExplosao, tempoParaExplodir, tamanhoBomba, layerInimigos);
        }
    }

    public override void AtivarJackpot(Vector2 offset, float raio)
    {
        EstaEmModoJackpot = true;
        StartCoroutine(RotinaJackpotBomba());
    }

    private IEnumerator RotinaJackpotBomba()
    {
        float timerJackpot = duracaoJackpot;

        while (timerJackpot > 0)
        {
            timerJackpot -= UnityEngine.Time.deltaTime;
            SoltarUmaBomba();
            yield return new WaitForSeconds(tempoEntreBombasJackpot);
        }

        EstaEmModoJackpot = false;
    }
}