using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Assertions.Comparers;

namespace NaviPerson
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class PersonCharacter : MonoBehaviour
    {
        private bool haveAnimator;
        [SerializeField]
        private float m_MovingTurnSpeed = 100;
        [SerializeField]
        private float m_StationaryTurnSpeed = 50;
        [SerializeField]
        private float m_MoveSpeedMultiplier = 1f;
        [SerializeField]
        private float m_AnimSpeedMultiplier = 1f;
        public float MoveTurnSpeed
        {
            get { return m_MovingTurnSpeed; }
            set
            {
                m_MovingTurnSpeed = value * 2;
                m_StationaryTurnSpeed = value;
            }
        }
        public float MoveSpeedMultiplier
        {
            get { return m_MoveSpeedMultiplier; }
            set
            {
                m_MoveSpeedMultiplier = value;
                m_AnimSpeedMultiplier = value;
            }
        }

        private Rigidbody m_Rigidbody;

        private float m_TurnAmount;
        private float m_ForwardAmount;
        private Animator _animator;
        private Animator animator
        {
            get
            {
                if (_animator == null)
                {
                    _animator = GetComponent<Animator>();
                }
                return _animator;
            }
        }
        // Use this for initialization
        void Awake()
        {
            haveAnimator = animator != null;
            m_Rigidbody = GetComponent<Rigidbody>();

            m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX
                                      | RigidbodyConstraints.FreezeRotationY
                                      | RigidbodyConstraints.FreezeRotationZ;
        }

        #region First Person Move
        public void FirstPersonMove(Vector3 vec)
        {
            if (vec.magnitude > 1f)
            {
                vec.Normalize();
            }

            Vector3 desiredMove = Vector3.ProjectOnPlane(vec, Vector3.up);
            m_Rigidbody.velocity = desiredMove * m_MoveSpeedMultiplier * 4f * m_MovingTurnSpeed * 0.01f;
        }
        #endregion

        #region Third Person Move
        public void ThirdPersonMove(Vector3 vec)
        {
            if (vec.magnitude > 1f)
            {
                vec.Normalize();
            }

            vec = transform.InverseTransformDirection(vec);
            animator.applyRootMotion = true;
            vec = Vector3.ProjectOnPlane(vec, Vector3.up);

            if (!FloatComparer.AreEqual(vec.x, 0f, Mathf.Epsilon))//    || !FloatComparer.AreEqual(vec.z, 0f, Mathf.Epsilon))
            {
                m_TurnAmount = vec.x;/*Mathf.Atan2(vec.x, vec.z)*/;
            }
            else
            {
                m_TurnAmount = 0f;
            }

            m_ForwardAmount = vec.z * 2;
            ApplyExtraTurnRotation();

            UpdateAnimator(vec);
        }

        void ApplyExtraTurnRotation()
        {
            float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
            transform.Rotate(0, m_TurnAmount * turnSpeed * Time.deltaTime, 0);
        }

        void UpdateAnimator(Vector3 move)
        {
            animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
            animator.SetFloat("Turn", m_TurnAmount, 0.1f, Time.deltaTime);
            animator.SetBool("Crouch", false);
            animator.SetBool("OnGround", true);

            animator.speed = m_AnimSpeedMultiplier;
        }

        void OnAnimatorMove()
        {
            if (Time.deltaTime > 0 && animator)
            {
                Vector3 v = (animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;
                v.y = m_Rigidbody.velocity.y;
                m_Rigidbody.velocity = v;
            }
        }
        #endregion
    }
}