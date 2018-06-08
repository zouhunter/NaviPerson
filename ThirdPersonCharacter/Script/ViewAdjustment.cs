using System;
using UnityEngine;
using NaviPerson.CrossPlatformInput;

namespace NaviPerson
{


    public enum FollowViewType
    {
        First,
        Third
    }

    public class ViewAdjustment : MonoBehaviour
    {
        public FollowViewType ViewType;
        public bool IsSwitching { get; private set; }
        public event Action OnSwitchCompleted;
        public bool supportSwitch;
        public Transform m_Camera;
        private ProtectCameraFromWallClip m_Clip;

        private Vector3 m_LastPosition;
        private Vector3 m_LastEuler;

        private CharacterControl m_CharacterCtrl;

        CharacterControl CharacterCtrl
        {
            get
            {
                if (m_CharacterCtrl == null)
                {
                    m_CharacterCtrl = GetComponent<CharacterControl>();
                }

                return m_CharacterCtrl;
            }
        }


        void Start()
        {
            m_Clip = GetComponent<ProtectCameraFromWallClip>();
            Switch(ViewType);
        }
        public void AdjustHeight(float height)
        {
            if (ViewType == FollowViewType.First)
            {
                return;
            }

            if (IsSwitching)
            {
                return;
            }

            m_Clip.m_OriginalHeight = height;
        }

        public void AdjustDistance(float distance)
        {
            if (ViewType == FollowViewType.First)
            {
                return;
            }

            if (IsSwitching)
            {
                return;
            }

            m_Clip.m_OriginalDist = distance;
        }

        public void AdjustAngle(float angle)
        {
            if (IsSwitching)
            {
                return;
            }

            angle = ClampAngle(angle);
            m_Clip.m_OriginalRotate = angle;
        }

        float ClampAngle(float angle)
        {
            if (angle > 360)
            {
                angle -= 360;
            }

            if (angle < -360)
            {
                angle += 360;
            }

            return angle;
        }

        public void Switch(FollowViewType type)
        {
            ViewType = type;
            Vector3 pos = GetPosition(type);
            Vector3 rot = GetRotate();

            m_LastPosition = m_Camera.localPosition;
            m_LastEuler = m_Camera.localEulerAngles;
            IsSwitching = true;

            SwitchType(type);
            m_Camera.localPosition = pos;
            m_Camera.localEulerAngles = rot;
        }

        void SwitchType(FollowViewType type)
        { 
            if (type == FollowViewType.First)
            {
                m_Clip.CanClip = false;
                OnFirstSwitchCompeleted();
            }
            else
            {
                OnThirdSwithcCompeleted();
            }

        }

        void OnFirstSwitchCompeleted()
        {
            IsSwitching = false;
            if (OnSwitchCompleted != null)
                OnSwitchCompleted.Invoke();

            CharacterCtrl.SwitchFollowView(FollowViewType.First);
        }

        void OnThirdSwithcCompeleted()
        {
            m_Clip.CanClip = true;
            IsSwitching = false;
            if (OnSwitchCompleted != null)
            {
                OnSwitchCompleted.Invoke();
            }
            CharacterCtrl.SwitchFollowView(FollowViewType.Third);
        }

        Vector3 GetPosition(FollowViewType type)
        {
            if (m_LastPosition != Vector3.zero)
            {
                return m_LastPosition;
            }
            else
            {
                float distance = 0.3f;
                float height = 0.2f;
                if (type == FollowViewType.Third)
                {
                    distance = -3.7f;
                    height = 1.56f;
                }

                return new Vector3(m_Camera.localPosition.x, height, distance);
            }
        }

        Vector3 GetRotate()
        {
            if (m_LastEuler != Vector3.zero)
            {
                return m_LastEuler;
            }
            else
            {
                return m_Camera.localEulerAngles;
            }
        }

        void Update()
        {
            if (supportSwitch && CrossPlatformInputManager.GetKeyDown(KeyCode.Tab))
            {
                if (IsSwitching)
                    return;

                switch (ViewType)
                {
                    case FollowViewType.First:
                        Switch(FollowViewType.Third);
                        break;
                    case FollowViewType.Third:
                        Switch(FollowViewType.First);
                        break;
                }
            }
        }
    }
}