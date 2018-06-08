using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace NaviPerson
{
    [Serializable]
    public class MouseLook
    {
        [SerializeField]
        private float XSensitivity = 2f;
        [SerializeField]
        private float YSensitivity = 2f;
        [SerializeField]
        private bool clampVerticalRotation = true;
        [SerializeField]
        private float MinimumX = -90F;
        [SerializeField]
        private float MaximumX = 90F;
        [SerializeField]
        private bool smooth;
        [SerializeField]
        private float smoothTime = 5f;


        private bool _cursorLock = false;
        private Quaternion m_CharacterTargetRot;
        private Quaternion m_CameraTargetRot;
        /// <summary>
        /// Òþ²ØÊó±ê
        /// </summary>
        public bool lockCursor { get
            {
                return _cursorLock;
            }
            set {
                _cursorLock = value;
                Cursor.visible = !_cursorLock;
            }
        }
        private PointerEventData pointData;
        public void Init(Transform character, Transform camera)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
            pointData = new PointerEventData(EventSystem.current);
        }

        private Vector3 mouse_position;

        public void LookRotation(Transform character, Transform camera)
        {
            UpdateCursorLock();

            if (!lockCursor)
            {
                mouse_position = Input.mousePosition;
                return;
            }

            if (mouse_position == Vector3.zero)
                mouse_position = Input.mousePosition;

            var vec_span = Input.mousePosition - mouse_position;
            float yRot = vec_span.x * XSensitivity;
            float xRot = vec_span.y * YSensitivity;
            mouse_position = Input.mousePosition;

            m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            if (smooth)
            {
                character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot,
                    smoothTime * Time.deltaTime);
                camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot,
                    smoothTime * Time.deltaTime);
            }
            else
            {
                character.localRotation = m_CharacterTargetRot;
                camera.localRotation = m_CameraTargetRot;
            }
        }

        public void UpdateCursorLock()
        {
            InternalLockUpdate();
        }

        private void InternalLockUpdate()
        {
            if (Input.GetMouseButtonUp(1))
            {
                if (!lockCursor)
                {
                    pointData.position = Input.mousePosition;
                    List<RaycastResult> hits = new List<RaycastResult>();
                    if (EventSystem.current == null)
                    {
                        lockCursor = true;
                    }
                    else
                    {
                        EventSystem.current.RaycastAll(pointData, hits);
                        bool hitUI = false;
                        foreach (var item in hits)
                        {
                            hitUI = item.gameObject.GetComponent<RectTransform>();
                            if (hitUI)
                            {
                                break;
                            }
                        }
                        if (!hitUI)
                        {
                            lockCursor = true;
                        }
                    }

                }
                else
                {
                    lockCursor = false;
                }
            }
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }
}
