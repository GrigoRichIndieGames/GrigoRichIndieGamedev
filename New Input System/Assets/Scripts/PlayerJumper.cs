using Unity.Mathematics;
using UnityEngine;

namespace InputSubscribers
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerJumper : MonoBehaviour
    {
        [SerializeField] private float _jumpForce = 10;

        private Inputs _input;
        private Rigidbody _rigidbody;

        private void Awake()
        {
            _input = new Inputs();
            _rigidbody = gameObject.GetComponent<Rigidbody>();

            _input.Player.Jump.performed += _ =>
            {
                float2 direction = _input.Player.Move.ReadValue<Vector2>() * 2;
                float3 jumpVector = math.up();
                jumpVector = math.normalize(jumpVector) * _jumpForce;
                _rigidbody.AddForce(jumpVector, ForceMode.Impulse);
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
    }
}