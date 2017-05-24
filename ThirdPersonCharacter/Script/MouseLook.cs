using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace NaviPerson
{
    [Serializable]
    public class MouseLook
    {
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public bool smooth;
        public float smoothTime = 5f;
        //public bool lockCursor = true;


        private Quaternion m_CharacterTargetRot;
        private Quaternion m_CameraTargetRot;
        private bool m_cursorIsLocked = false;
        private PointerEventData pointData;
        public void Init(Transform character, Transform camera)
        {
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
            pointData = new PointerEventData(EventSystem.current);
        }


        public void LookRotation(Transform character, Transform camera)
        {
            UpdateCursorLock();

            if (!m_cursorIsLocked) 
            {
                return;
            }

            float yRot = Input.GetAxis("Mouse X") * XSensitivity;
            float xRot = Input.GetAxis("Mouse Y") * YSensitivity;

            m_CharacterTargetRot *= Quaternion.Euler (0f, yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler (-xRot, 0f, 0f);

            if(clampVerticalRotation)
                m_CameraTargetRot = ClampRotationAroundXAxis (m_CameraTargetRot);

            if(smooth)
            {
                character.localRotation = Quaternion.Slerp (character.localRotation, m_CharacterTargetRot,
                    smoothTime * Time.deltaTime);
                camera.localRotation = Quaternion.Slerp (camera.localRotation, m_CameraTargetRot,
                    smoothTime * Time.deltaTime);
            }
            else
            {
                character.localRotation = m_CharacterTargetRot;
                camera.localRotation = m_CameraTargetRot;
            }

            //UpdateCursorLock();
        }

        public void SetCursorLock(bool value)
        {
            m_cursorIsLocked = value;

            if (!m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
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
                if (Cursor.visible)
                {
                    pointData.position = Input.mousePosition;
                    List<RaycastResult> hits = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(pointData, hits);
                    bool hitUI = false;
                    foreach (var item in hits)
                    {
                        hitUI = item.gameObject.GetComponent<RectTransform>();
                        if (hitUI){
                            break;
                        }
                    }
                    if(!hitUI) m_cursorIsLocked = true;
                }
                else
                {
                    m_cursorIsLocked = false;
                }
            }

            if (m_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);

            angleX = Mathf.Clamp (angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }
}
