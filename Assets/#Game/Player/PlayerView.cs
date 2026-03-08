using UnityEngine;

/// <summary>
/// Управляет анимацией персонажа по направлению движения.
/// Использует Blend Tree 2D (MoveX, MoveY): в Animator должен быть стейт с Blend Tree типа
/// 2D Simple Directional или 2D Freeform Directional и параметры MoveX, MoveY.
/// При движении передаёт нормализованное направление; при стоянии — (0, 0).
///
/// Настройка в Unity:
/// 1. Window → Animation → Animator. Добавить параметры Float: MoveX, MoveY.
/// 2. Создать новый стейт, по нему ПКМ → Create State → From New Blend Tree; войти в Blend Tree.
/// 3. В Blend Tree: Blend Type = 2D Simple Directional (или Freeform Directional).
///    Параметры: MoveX, MoveY. Добавить motion: Idle в (0, 0); движения вверх/вниз/влево/вправо
///    в (0,1), (0,-1), (-1,0), (1,0); при 8 направлениях — диагонали (1,1), (-1,1) и т.д.
/// 4. Сделать этот стейт стейтом по умолчанию (синяя стрелка Entry).
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerView : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField]
    [Tooltip("Если не задан — берётся GetComponent на этом объекте")]
    private Animator animator;

    [SerializeField]
    [Tooltip("Отсюда берётся направление движения. Если не задан — GetComponent")]
    private TopDownMovement2D topDownMovement;

    [Header("Параметры аниматора (Blend Tree 2D)")]
    [SerializeField]
    [Tooltip("Имя параметра по горизонтали (-1..1), обычно MoveX")]
    private string moveXParameterName = "MoveX";

    [SerializeField]
    [Tooltip("Имя параметра по вертикали (-1..1), обычно MoveY")]
    private string moveYParameterName = "MoveY";

    private int _moveXParameterHash;
    private int _moveYParameterHash;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (topDownMovement == null)
            topDownMovement = GetComponent<TopDownMovement2D>();

        _moveXParameterHash = Animator.StringToHash(moveXParameterName);
        _moveYParameterHash = Animator.StringToHash(moveYParameterName);
    }

    private void Update()
    {
        if (animator == null)
            return;

        Vector2 direction = topDownMovement != null ? topDownMovement.MovementDirection : Vector2.zero;

        animator.SetFloat(_moveXParameterHash, direction.x);
        animator.SetFloat(_moveYParameterHash, direction.y);
    }
}
