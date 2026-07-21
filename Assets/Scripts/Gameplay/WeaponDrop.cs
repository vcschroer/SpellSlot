using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WeaponDrop : MonoBehaviour
{
    [System.Serializable]
    public struct ArmaDropData
    {
        [Tooltip("Nome apenas para organizar no Inspector")]
        public string nomeArma;

        [Tooltip("Prefab original da arma")]
        public GameObject prefabArma;

        [Tooltip("Imagem/Sprite exclusivo dessa arma que aparecerá no chão ao dropar")]
        public Sprite spriteIcone;
    }

    [Header("Configuração de Armas e Ícones")]
    [SerializeField] private ArmaDropData[] armasDisponiveis;

    [Header("Animação Flutuante")]
    [SerializeField] private float velocidadeFlutuar = 3f;
    [SerializeField] private float alturaFlutuar = 0.15f;

    [Header("Efeitos Visuais/Sonoros")]
    [SerializeField] private GameObject prefabEfeitoColeta;

    private Vector3 posicaoInicial;
    private SpriteRenderer spriteRenderer;
    private GameObject armaParaDar;

    private void Start()
    {
        posicaoInicial = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            ConfigurarDropParaPlayer(playerObj);
        }
    }

    private void Update()
    {
        float novoY = posicaoInicial.y + Mathf.Sin(Time.time * velocidadeFlutuar) * alturaFlutuar;
        transform.position = new Vector3(transform.position.x, novoY, transform.position.z);
    }

    private void ConfigurarDropParaPlayer(GameObject playerObj)
    {
        BaseWeapon[] armasAtuais = playerObj.GetComponentsInChildren<BaseWeapon>(true);

        foreach (ArmaDropData dadosArma in armasDisponiveis)
        {
            if (dadosArma.prefabArma == null) continue;

            BaseWeapon scriptPrefab = dadosArma.prefabArma.GetComponent<BaseWeapon>();
            if (scriptPrefab == null) continue;

            bool playerJaPossuiEstaArma = false;

            foreach (BaseWeapon armaAtual in armasAtuais)
            {
                if (armaAtual.GetType() == scriptPrefab.GetType())
                {
                    playerJaPossuiEstaArma = true;
                    break;
                }
            }

            if (!playerJaPossuiEstaArma)
            {
                armaParaDar = dadosArma.prefabArma;

                if (dadosArma.spriteIcone != null)
                {
                    spriteRenderer.sprite = dadosArma.spriteIcone;
                }
                else
                {
                    SpriteRenderer srArma = dadosArma.prefabArma.GetComponentInChildren<SpriteRenderer>();
                    if (srArma != null) spriteRenderer.sprite = srArma.sprite;
                }

                break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && armaParaDar != null)
        {
            ColetarSegundaArma(collision.gameObject);
        }
    }

    private void ColetarSegundaArma(GameObject playerObj)
    {
        GameObject novaArma = Instantiate(armaParaDar, playerObj.transform);
        novaArma.transform.localPosition = Vector3.zero;
        novaArma.transform.localRotation = Quaternion.identity;

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlaySFX("powerup");
        }

        if (prefabEfeitoColeta != null)
        {
            Instantiate(prefabEfeitoColeta, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}