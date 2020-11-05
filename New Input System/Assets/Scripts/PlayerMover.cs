using Unity.Mathematics;
using UnityEngine;
using math = Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;

namespace InputSubscribers
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMover : MonoBehaviour
    {
        [SerializeField] private float _movingSpeed = 5;
        [SerializeField] private float _rotationSpeed = 3;
        [SerializeField] private Transform _viewTransform;

        private Calculator _calculator;
        private Inputs _inputs;

        private Rigidbody _rigidbody;
        private Transform _transform;


        private void Awake()
        {
            _inputs = new Inputs();

            _inputs.Player.Move.performed += _ =>
            {
                _calculator.Direction = _inputs.Player.Move.ReadValue<Vector2>();
            };

            _inputs.Player.Move.canceled += _ =>
            {
                _calculator.Direction = 0;
            };

            _rigidbody = gameObject.GetComponent<Rigidbody>();
            _transform = gameObject.GetComponent<Transform>();
            Transform cameraTransform = Camera.main.GetComponent<Transform>();
            _calculator = new Calculator(cameraTransform, _transform, _viewTransform);
        }

        private void OnEnable()
        {
            _inputs.Enable();
        }

        private void OnDisable()
        {
            _inputs.Disable();
        }

        private void FixedUpdate()
        {
            TranslatePlayer();
            RotatePlayer();
        }


        private void TranslatePlayer()
        {
            float3 translation = _calculator.Translation * Time.deltaTime * _movingSpeed;
            float3 position = _transform.position;
            _rigidbody.MovePosition(position + translation);
        }

        private void RotatePlayer()
        {
            quaternion rotation = _calculator.Rotation;
            _viewTransform.rotation = math.slerp(_viewTransform.rotation, rotation, _rotationSpeed * Time.deltaTime);
        }

        private struct Calculator
        {
            public float2 Direction { private get; set; }
            public float3 Translation
            {
                get
                {
                    float3 forward = math.cross(_cameraTransform.right, _moverTransform.up);
                    quaternion rotationAngle = FromToRotation(math.forward(), forward);

                    float3 translation = new float3(Direction.x, 0, Direction.y);

                    return _translate = math.rotate(rotationAngle, translation);
                }
            }
            public quaternion Rotation
            {
                get
                {
                    if (Equals(Direction, float2.zero))
                        return _viewTransform.rotation;
                    return Quaternion.LookRotation(_translate, math.up());
                }
            }

            private readonly Transform _cameraTransform;
            private readonly Transform _moverTransform;
            private readonly Transform _viewTransform;

            private float3 _translate;

            public Calculator(Transform cameraTransform, Transform moverTransform, Transform viewTransform)
            {
                _cameraTransform = cameraTransform;
                _moverTransform = moverTransform;
                _viewTransform = viewTransform;
                Direction = float2.zero;
                _translate = 0;
            }

            //May have hight math error
            private quaternion FromToRotation(float3 from, float3 to)
            {
                /// <summary>
                /// OpenGL realisation http://www.opengl-tutorial.org/ru/intermediate-tutorials/tutorial-17-quaternions/
                /// </summary>
                from = math.normalize(from);
                to = math.normalize(to);

                float dotProduct = math.dot(from, to); //= cos of angle between from and to
                float3 rotationAxis;

                if (dotProduct < -1 + 0.001f)
                {
                    ///<summary>
                    /// special case when vectors in opposite directions: 
                    ///there is no "ideal" rotation axis So guess one; 
                    ///any will do as long as it's perpendicular to start
                    ///</summary>
                    rotationAxis = math.cross(math.up(), from);

                    if (math.lengthsq(rotationAxis) < 0.01f) // bad luck, they were parallel, try again!
                        rotationAxis = math.cross(math.right(), from);

                    rotationAxis = math.normalize(rotationAxis);
                    return quaternion.AxisAngle(rotationAxis, math.radians(180f));
                }

                rotationAxis = math.cross(from, to);
                float sqrt = math.sqrt((1 + dotProduct) * 2);
                return new quaternion(rotationAxis.x / sqrt, rotationAxis.y / sqrt, rotationAxis.z / sqrt, sqrt * 0.5f);
            }
        }
    }
}