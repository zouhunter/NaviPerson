using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaviPerson.CrossPlatformInput;
using UnityEngine.Assertions.Comparers;
namespace NaviPerson
{
    public delegate void ImmediateMoveAction(Vector3 pos);

    [RequireComponent(typeof(PersonCharacter))]
    public class CharacterControl : MonoBehaviour
    {
        public GameObject[] bodys;
        public Transform Camera;

        [SerializeField]
        private MouseLook m_MouseLook = new MouseLook();
        private PersonCharacter m_Character;
        private UnityEngine.AI.NavMeshAgent m_Agent;
        private Vector3 m_CamForward;
        private Vector3 m_Move;

        private bool m_IsNavigate;
        private bool m_NavMoved;
        private Vector3 m_TargetPosition;
        private ViewAdjustment m_Adjustment;
        private Coroutine m_NavDetectedCo;

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

        private void Awake()
        {
            m_Character = GetComponent<PersonCharacter>();
            m_Agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
        }
        IEnumerator Start()
        {
            m_Agent.updateRotation = false;
            m_Agent.updatePosition = true;
            m_MouseLook.Init(transform, Camera.transform);
            yield return null;
        }
        private void ShowBodys()
        {
            foreach (var b in bodys)
            {
                b.gameObject.SetActive(true);
            }
        }

        public event ImmediateMoveAction immediateMoveAction;
        public void ImmediateMove(Vector3 pos, Quaternion dir)
        {
            //Facade.Instance.SendNotification(NotiConst.TargetDisable);
            //Facade.Instance.SendNotification(NotiConst.ChangeLookType, LookType.Free);
            m_Agent.updatePosition = false;
            transform.position = pos;
            transform.rotation = dir;
            m_Agent.Warp(transform.position);
            m_Agent.updatePosition = true;
            ImmediateStop();
            m_Character.Move(Vector3.zero);
            if (immediateMoveAction != null)
            {
                immediateMoveAction(pos);
            }
        }

        public void ImmediateStop()
        {
            InternalImmediateStop();
            if (m_NavDetectedCo != null)
            {
                StopCoroutine(m_NavDetectedCo);
                m_NavDetectedCo = null;
            }
        }

        void InternalImmediateStop()
        {
            m_Agent.isStopped = true;
            m_IsNavigate = false;
        }

        private void HideBodys()
        {
            foreach (var b in bodys)
            {
                b.gameObject.SetActive(false);
            }
        }

        public void SetTargetPositionFree(Vector3 position)
        {
            SetTargetPosition(position);
            //Facade.Instance.SendNotification(NotiConst.ChangeLookType, LookType.Free);
        }

        public void SetTargetPositionFix(Vector3 position)
        {
            SetTargetPosition(position);
            //Facade.Instance.SendNotification(NotiConst.TargetDisable);
        }

        void SetTargetPosition(Vector3 position)
        {
            if (m_NavDetectedCo != null)
            {
                StopCoroutine(m_NavDetectedCo);
                m_NavDetectedCo = null;
            }

            m_NavMoved = false;
            ImmediateStop();
            m_NavDetectedCo = StartCoroutine(DetectNavMoved(position));
        }

        IEnumerator DetectNavMoved(Vector3 position)
        {
            int count = 0;
            while (true)
            {
                if (m_NavMoved)
                {
                    yield break;
                }
                else
                {
                    m_IsNavigate = true;
                    m_TargetPosition = position;
                    m_Agent.isStopped = false;
                    m_MouseLook.SetCursorLock(false);
                }

                count++;
                if (count > 10)
                {
                    ImmediateStop();
                    //Facade.Instance.SendNotification(NotiConst.TargetDisable);
                    yield break;
                }

                yield return null;
            }
        }

        public void SwitchFollowView(FollowViewType type)
        {
            switch (type)
            {
                case FollowViewType.First:
                    HideBodys();
                    m_MouseLook.Init(transform, Camera.transform);
                    if (!m_IsNavigate)
                    {
                        m_MouseLook.SetCursorLock(true);
                    }
                    break;
                case FollowViewType.Third:
                    ShowBodys();
                    m_MouseLook.SetCursorLock(false);
                    break;
            }
        }

        void Update()
        {
            if (CanFirstViewControl())
            {
                m_MouseLook.LookRotation(transform, Camera.transform);
            }
        }

        private void FixedUpdate()
        {
            float h;
            float v;
            float r;
            bool isInput = IsInputControl(out h, out v, out r);
            m_Move = Vector3.zero;

            if (isInput)
            {
                if (m_IsNavigate)
                {
                    //Facade.Instance.SendNotification(NotiConst.ChangeLookType, LookType.Free);
                    //Facade.Instance.SendNotification(NotiConst.TargetDisable);
                }

                m_Move = GetInputMovement(h, v, r);
                ImmediateStop();
            }
            else if (m_IsNavigate)
            {
                m_Agent.SetDestination(m_TargetPosition);
                if (!m_Agent.pathPending &&
                    m_Agent.remainingDistance < m_Agent.stoppingDistance)
                {
                    InternalImmediateStop();
                    if (m_NavMoved)
                    {
                        //Facade.Instance.SendNotification(NotiConst.TargetDisable);
                    }
                }
                else
                {
                    m_NavMoved = true;
                    m_Move = m_Agent.desiredVelocity;
                }
            }

            m_Character.Move(m_Move);
        }

        bool CanFirstViewControl()
        {
            if (Adjustment == null)
            {
                return true;
            }

            if (Adjustment.ViewType == FollowViewType.Third)
            {
                return false;
            }

            if (m_IsNavigate)
            {
                return false;
            }

            return true;
        }

        Vector3 GetInputMovement(float h, float v, float r)
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
                m_CamForward = Vector3.Scale(calculateForward, new Vector3(1, 0, 1)).normalized;
                movement = v * m_CamForward + h * calculateRight;
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

        bool IsInputControl(out float h, out float v, out float r)
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
    }
}
