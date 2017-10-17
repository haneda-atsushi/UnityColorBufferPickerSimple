#if UNITY_ANDROID // && ( ! UNITY_EDITOR )
#define UNITY_ANDROID_STANDALONE
#endif

using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

namespace GFX
{

    public class SimpleInputManager : MonoBehaviour
    {
        Vector2 startCursorPos;
        Vector2 oldCursorPos;
        Vector2 deltaCursorPos = Vector2.zero;
        bool isCursorMoved     = false;

        public string m_DragButtonName  = "Fire2";
        public string m_TouchButtonName = "Fire1";

        public Rect   m_InvalidArea     = new Rect( 0, 160, 0, 0 );

        public float m_MouseSensitivityX = 4.0f;
        public float m_MouseSensitivityY = 4.0f;

        [Range(0.0f, 2.0f)]
        public float m_LeftAnalogSensitivityX = 1.0f;

        [Range(0.0f, 2.0f)]
        public float m_LeftAnalogSensitivityY = 1.0f;

        [Range(0.0f, 2.0f)]
        public float m_RightAnalogSensitivityX = 1.0f;

        [Range(0.0f, 2.0f)]
        public float m_RightAnalogSensitivityY = 1.0f;

        public float m_CursorRotAngle    = 180.0f;

        int m_CursorTouchNum = 0;

        public Vector2 GetDeltaCursorPos()
        {
            return deltaCursorPos;
        }

        public bool IsCursorMoved()
        {
            return isCursorMoved;
        }

        public int GetCursorTouchNum()
        {
            return m_CursorTouchNum;
        }

        public bool IsCursorValid()
        {
            Vector2 cursor_pos = GetCursorPos();
            bool is_invalid    = 
                ( cursor_pos.y < m_InvalidArea.y );

            return ( ! is_invalid );
        }

        public Vector2 GetCursorPos()
        {
            return Input.mousePosition;
        }

        void Update()
        {
            UpdateCursorInput_();
        }

        public bool Clicked()
        {
            if ( ( ! isCursorMoved ) && ( GetMouseButtonUp() || GetTouchButtonUp() ) )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Vector2 GetKeyboardMoveInput()
        {
            Vector2 left_analog_stick = Vector2.zero;

            if ( Input.GetKey( KeyCode.W ) )
            {
                left_analog_stick.y = 1.0f;
            }
            else if ( Input.GetKey( KeyCode.S ) )
            {
                left_analog_stick.y = - 1.0f;
            }

            if ( Input.GetKey( KeyCode.A ) )
            {
                left_analog_stick.x = - 1.0f;
            }
            else if ( Input.GetKey( KeyCode.D ) )
            {
                left_analog_stick.x = 1.0f;
            }

            return left_analog_stick;
        }

        public Vector2 GetMouseMoveInput()
        {
            Vector2 mouse_delta_xy = Vector2.zero;
            mouse_delta_xy.x = Input.GetAxis( "Mouse X" ) * m_MouseSensitivityX;
            mouse_delta_xy.y = Input.GetAxis( "Mouse Y" ) * m_MouseSensitivityY;

            return mouse_delta_xy;
        }

        public Vector2 GetLeftAnalogStickInput()
        {
            Vector2 left_analog_stick = Vector2.zero;

            float left_x         = Input.GetAxis( "Horizontal" );
            float left_y         = Input.GetAxis( "Vertical" );
            left_analog_stick.x  = left_x * m_LeftAnalogSensitivityX;
            left_analog_stick.y  = left_y * m_LeftAnalogSensitivityY;

            return left_analog_stick;
        }

        public Vector2 GetRightAnalogStickInput()
        {
            Vector2 right_analog_stick = Vector2.zero;
            right_analog_stick.x = Input.GetAxis( "Horizontal2" ) * m_RightAnalogSensitivityX;
            right_analog_stick.y = Input.GetAxis( "Vertical2"   ) * m_RightAnalogSensitivityY;

            return right_analog_stick;
        }

        public Vector2 GetCursorDeltaInput()
        {
            if ( IsCursorMoved() && 
                 IsCursorValid() )
            {
                float anglePerPixel  = m_CursorRotAngle / (float) Screen.width;
                Vector2 delta        = GetDeltaCursorPos() * anglePerPixel;

                return delta;
            }
            else
            {
                return Vector2.zero;
            }
        }

        void UpdateCursorInput_()
        {
            Vector2 cursor_pos = GetCursorPos();
            if ( GetMouseButtonDown() || GetTouchButtonDown() )
            {
                startCursorPos  = cursor_pos;

                m_CursorTouchNum = Input.touchCount;
            }

            if ( GetMouseButton() || GetTouchButton() )
            {
                float dist =
                    Vector2.Distance( startCursorPos, cursor_pos );
                if ( dist >= ( Screen.width * 0.1f ) )
                {
                    isCursorMoved = true;
                }
            }

            if ( ( ! ( GetMouseButtonUp() || GetTouchButtonUp() ) ) &&
                 ( ! ( GetMouseButton()   || GetTouchButton()   ) ) )
            {
                isCursorMoved = false;
            }

            if ( isCursorMoved )
            {
                deltaCursorPos = cursor_pos - oldCursorPos;
            }
            else
            {
                deltaCursorPos = Vector2.zero;
            }

            oldCursorPos = cursor_pos;
        }

        public bool GetMouseButtonDown()
        {
            bool flag = 
                ( Input.touchCount == 0 ) &&
                Input.GetButtonDown( m_DragButtonName );

            return flag;
        }

        public bool GetTouchButtonDown()
        {
            bool flag = 
                ( Input.touchCount == 1 ) &&
                ( Input.touches[ 0 ].phase == TouchPhase.Began );

            return flag;
        }

        public bool GetMouseButtonUp()
        {
            bool flag = 
                ( Input.touchCount == 0 ) &&
                Input.GetButtonUp( m_DragButtonName );

            return flag;
        }

        public bool GetTouchButtonUp()
        {
            bool flag = 
                ( Input.touchCount == 1 ) &&
                ( Input.touches[ 0 ].phase == TouchPhase.Ended );

            return flag;
        }

        public bool GetMouseButton()
        {
            bool flag = 
                ( Input.touchCount == 0 ) &&
                Input.GetButton( m_DragButtonName );
            return flag;
        }

        public bool GetTouchButton()
        {
            bool flag =
                ( Input.touchCount == 1 &&
                  ( ( Input.touches[ 0 ].phase == TouchPhase.Moved      ) ||
                    ( Input.touches[ 0 ].phase == TouchPhase.Stationary ) ) );
            return flag;
        }
    }
}
