using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaviPerson.CrossPlatformInput;
using UnityEngine.Assertions.Comparers;
namespace NaviPerson
{
    [RequireComponent(typeof(PersonCharacter))]
    public class CharacterControl : MonoBehaviour
    {
        public GameObject[] bodys;
        public Transform Camera;

        [SerializeField]
        private MouseLook m_mouseLook = new MouseLook();
        private PersonCharacter m_character;
        private UnityEngine.AI.NavMeshAgent m_agent;
        private Vector3 m_camForward;
        private Vector3 m_move;

        private bool m_isNavigate;

        private Vector3 m_TargetPosition;
        private ViewAdjustment m_Adjustment;
        private ViewAdjustment Adjustment
        {
            get
            {
                if (m_Adjustment == null)
                {
                    m_Adjustment = GameObject.FindObjectOfType<ViewAdjustment>();
                }

                return m_Adjustment;
            }
        }
        private FollowViewType viewType;
        public event UnityAction onPlayerStop;

        public MouseLook mouseLook { get { return m_mouseLook; } }
        private void Awake()
        {
            m_character = GetComponent<PersonCharacter>();
            m_agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
        }
        IEnumerator Start()
        {
            m_agent.updateRotation = false;
            m_agent.updatePosition = true;
            m_mouseLook.Init(transform, Camera.transform);
            yield return null;
        }
        private void Update()
        {
            if (CanFirstViewControl())
            {
                m_mouseLook.LookRotation(transform, Camera.transform);
            }
        }

        private void FixedUpdate()
        {
            CalcuteAndMove();
        }

        public void SwitchFollowView(FollowViewType type)
        {
            this.viewType = type;
            switch (type)
            {
                case FollowViewType.First:
                    HideBodys();
                    m_mouseLook.Init(transform, Camera.transform);
                    //if (!m_isNavigate){
                    //    m_mouseLook.SetCursorLock(true);
                    //}
                    break;
                case FollowViewType.Third:
                    ShowBodys();
                    //m_mouseLook.SetCursorLock(false);
                    break;
            }
        }

        public void ImmediateMove(Vector3 pos, Quaternion dir)
        {
            transform.position = pos;
            transform.rotation = dir;
            if (m_agent.isActiveAndEnabled)
            {
                m_agent.updatePosition = false;
                m_agent.Warp(transform.position);
                m_agent.updatePosition = true;
            }
            InternalImmediateStop();
            MovePlayer(Vector3.zero);
        }

        public void ImmediateStop()
        {
            if (onPlayerStop != null)
                onPlayerStop.Invoke();
            InternalImmediateStop();
        }

        #region PrivateFunctions
        private void ShowBodys()
        {
            foreach (var b in bodys)
            {
                b.gameObject.SetActive(true);
            }
        }

        private void CalcuteAndMove()
        {
            float h;
            float v;
            float r;
            bool isInput = IsInputControl(out h, out v, out r);
            m_move = Vector3.zero;
            if (isInput)
            {
                if (m_isNavigate)
                {
                    InternalImmediateStop();
                }
                m_move = GetInputMovement(h, v, r);
            }
            else if (m_isNavigate)
            {
                m_agent.SetDestination(m_TargetPosition);
                if (m_agent.pathPending)
                {
                    Debug.Log("m_agent.pathPending");
                }
                else if (m_agent.remainingDistance < m_agent.stoppingDistance)
                {
                    ImmediateStop();
                }
                else
                {
                    m_move = m_agent.desiredVelocity;
                }
            }
            MovePlayer(m_move);
        }

        private void MovePlayer(Vector3 move)
        {
            switch (viewType)
            {
                case FollowViewType.First:
                    m_character.FirstPersonMove(move);
                    break;
                case FollowViewType.Third:
                    m_character.ThirdPersonMove(move);
                    break;
                default:
                    break;
            }
        }

        private void InternalImmediateStop()
        {
            if (m_agent.isActiveAndEnabled)
            {
                m_agent.isStopped = true;
            }
            m_isNavigate = false;
            Debug.Log("InternalImmediateStop");
        }

        private void HideBodys()
        {
            foreach (var b in bodys)
            {
                b.gameObject.SetActive(false);
            }
        }

        public void SetTargetPosition(Vector3 position)
        {
            ImmediateStop();
            DetectNavMoved(position);
        }

        private void DetectNavMoved(Vector3 position)
        {
            if (m_agent != null)
            {
                m_agent.enabled = true;
                m_isNavigate = true;
                m_TargetPosition = position;
                m_agent.isStopped = false;
            }
            else
            {
                ImmediateMove(position, transform.rotation);
            }

            m_mouseLook.lockCursor = false;
        }

        private bool CanFirstViewControl()
        {
            if (Adjustment == null)
            {
                return true;
            }

            if (m_isNavigate)
            {
                return false;
            }

            return true;
        }

        private Vector3 GetInputMovement(float h, float v, float r)
        {
            Vector3 movement;
            if (Camera != null)
            {
                var calculateForward = transform.forward;
                var calculateRight = transform.right;
                if (v < 0f)
                {
                    calculateForward = Camera.forward;
                    calculateRight = Camera.right;
                }
                m_camForward = Vector3.Scale(calculateForward, new Vector3(1, 0, 1)).normalized;
                movement = v * m_camForward + h * calculateRight;
            }
            else
            {
                movement = v * Vector3.forward + h * Vector3.right;
            }

#if !MOBILE_INPUT
            if (!CrossPlatformInputManager.GetKey(KeyCode.LeftShift))
            {
                movement *= 0.5f;
                r *= 0.5f;
            }
#endif
            return movement;
        }

        private bool IsInputControl(out float h, out float v, out float r)
        {
            h = CrossPlatformInputManager.GetAxis("Horizontal");
            v = CrossPlatformInputManager.GetAxis("Vertical");
            r = 0f;

            if (!FloatComparer.s_ComparerWithDefaultTolerance.Equals(h, 0))
            {
                return true;
            }

            if (!FloatComparer.s_ComparerWithDefaultTolerance.Equals(v, 0))
            {
                return true;
            }

            return false;
        }
        #endregion
    }
}
