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
    [RequireComponent(typeof(CharacterControl))]
    public class ViewAdjustment : MonoBehaviour
    {
        public FollowViewType ViewType { get; private set; }
        private Vector3 m_LastPosition;
        private Vector3 m_LastEuler;

        private CharacterControl m_CharacterCtrl;

        Camera m_camera;
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
            m_camera = CharacterCtrl.Camera.GetComponent<Camera>();
            ViewType = FollowViewType.Third;
        }
        public void AdjustHeight(float height)
        {
            if (ViewType == FollowViewType.First)
            {
                return;
            }
        }

        public void AdjustDistance(float distance)
        {
            if (ViewType == FollowViewType.First)
            {
                return;
            }
        }

        public void AdjustAngle(float angle)
        {
            angle = ClampAngle(angle);
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
            m_LastPosition = m_camera.transform.localPosition;
            m_LastEuler = m_camera.transform.localEulerAngles;
            CharacterCtrl.SwitchFollowView(type);
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

                return new Vector3(m_camera.transform.localPosition.x, height, distance);
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
                return m_camera.transform.localEulerAngles;
            }
        }

        void Update()
        {
            if (CrossPlatformInputManager.GetKeyDown(KeyCode.Tab))
            {
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