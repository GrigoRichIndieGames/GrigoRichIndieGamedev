using DG.Tweening;
using UnityEngine;

namespace InputSubscribers
{
    [RequireComponent(typeof(PlayerJumper)), RequireComponent(typeof(PlayerMover))]
    public class HotSwapMenuPresenter : MonoBehaviour
    {
        [SerializeField] private RectTransform _select;
        [SerializeField] private RectTransform _barFull;
        [SerializeField] private RectTransform _center;
        [SerializeField] private float duration = 0.75f;

        private Inputs _input;

        private PlayerJumper _jumper;
        private PlayerMover _mover;

        private void Awake()
        {
            _input = new Inputs();

            _jumper = gameObject.GetComponent<PlayerJumper>();
            _mover = gameObject.GetComponent<PlayerMover>();

            Vector3 startPoint = _barFull.position;
            Sequence sequence = DOTween.Sequence();

            _input.Player.PieMenuInvoke.performed += _ =>
            {
                PieInvoke(_center.position, sequence);
            };

            _input.Player.PieMenuInvoke.canceled += _ =>
            {
                PieMenuCancel(startPoint, sequence);
            };
        }

        private void OnEnable()
        {
            _input.Enable();
        }

        private void OnDisable()
        {
            _input.Disable();
        }

        private void PieMenuCancel(Vector3 closePosition, Sequence sequence)
        {
            Time.timeScale = 1;
            sequence.Append(_barFull.DOMove(closePosition, duration));
            _mover.enabled = true;
            _jumper.enabled = true;
        }

        private void PieInvoke(Vector3 openPosition, Sequence sequence)
        {
            sequence.Append(_barFull.DOMove(openPosition, duration));
            _mover.enabled = false;
            _jumper.enabled = false;
            sequence.onComplete += () =>
            {
                Time.timeScale = 0;
            };
        }
    }
}