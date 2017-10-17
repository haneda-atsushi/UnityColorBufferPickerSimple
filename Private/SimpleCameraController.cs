using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

namespace GFX
{

    public class SimpleCameraController : MonoBehaviour
    {
        public enum CameraMode
        {
            EditorCamera,
            ViewerCamera
        };

        public enum UpdateMode
        {
            LastUpdate,
            Update
        };

        [Header("General")]
        public CameraMode m_CameraMode = CameraMode.EditorCamera;
        public UpdateMode m_UpdateMode = UpdateMode.LastUpdate;

        [Space(1)]
        [Header("Editor mode")]

        [Range(0.0f, 1.0f)]
        public float m_MoveSpeed  = 0.01f;
        float        rotationY    = 0.0f;

        float        rotationZ    = 0.0f;

        [Range(1.0f, 100.0f)]
        public float m_MoveShiftScale = 10.0f;

        [Space(1)]
        [Header("Viewer mode")]
        [Range( 0.0f, 100.0f )]
        public float m_Distance          = 10.0f;

        [Range( 0.0f, 10.0f )]
        public float m_MinDistance       = 0.1f;

        [Range( 0.0f, 1000.0f )]
        public float m_MaxDistance       = 100.0f;

        float horizontalAngle            = 0.0f;
        float verticalAngle              = 10.0f;
        public Transform m_LookAtTarget  = null;
        public Vector3   m_LookAtOffsetPos = Vector3.zero;
        float cameraMoveSpeed          = 0.1f;

        [Range( -20.0f, 10.0f )]
        public float m_MinVerticalAngle = - 20.0f;
        [Range( 0.0f, 80.0f )]
        public float m_MaxVerticalAngle = 60.0f;



        [Range( 10.0f, 500.0f )]
        public float m_DeltaTouchPosToZ = 300.0f;
        float        m_Interval = 0.0f;

        const int    cMaxTouchIgnoreFrameCount = 30;
        int          m_TouchIgnoreFrameCount = 0;

        SimpleInputManager m_SimpleInputManager;
        Transform          m_MainCameraTransform;

        void InitParam_( Transform camera_transform )
        {
            Vector3 euler_angles = camera_transform.rotation.eulerAngles;
            verticalAngle   = euler_angles.x;
            horizontalAngle = euler_angles.y;

            // Debug.Log( "euler_angles = " + euler_angles );
            //distance        = 8.0f;
            //public float horizontalAngle = 0.0f;
            //public float verticalAngle   = 10.0f;
        }

        void Start()
        {
            m_SimpleInputManager = FindObjectOfType<SimpleInputManager>();
            Assert.IsNotNull( m_SimpleInputManager );

            m_MainCameraTransform = Camera.main.transform;
            Assert.IsNotNull( m_MainCameraTransform );

            /*
            Vector3 look_at_pos = lookAtOffsetPos;
            if ( lookAtTarget != null )
            {
                look_at_pos += lookAtTarget.position;
            }

            // transform.position = mainCamera.transform.position;
            transform.LookAt( look_at_pos );

            Vector3 init_euler = transform.rotation.eulerAngles;
            verticalAngle      = init_euler.x;
            horizontalAngle    = init_euler.y;

            Debug.Log( "init_euler = " + init_euler.x + "," + init_euler.y );
            */

            InitParam_( m_MainCameraTransform );
        }

        void Update()
        {
            if ( m_UpdateMode == UpdateMode.Update )
            {
                UpdateCamera_();
            }
        }

        void LateUpdate()
        {
            if ( m_UpdateMode == UpdateMode.LastUpdate )
            {
                UpdateCamera_();
            }
        }

        void UpdateCamera_()
        {
            if ( m_CameraMode == CameraMode.EditorCamera )
            {
                UpdateEditorCamera_();
            }
            else if ( m_CameraMode == CameraMode.ViewerCamera )
            {
                UpdateViewerCamera_();
            }
        }

        void UpdateEditorCamera_()
        {
            float move_scale = 1.0f;

            // キーボード用
            if ( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift ) )
            {
                move_scale = m_MoveShiftScale;
            }

            //--------------------------------------------------
            // カメラの並進移動
            //--------------------------------------------------

            Vector2 left_input  = Vector2.zero;
            left_input         += m_SimpleInputManager.GetKeyboardMoveInput();
            left_input         += m_SimpleInputManager.GetLeftAnalogStickInput();

            Vector2 delta_xy;
            delta_xy.x          = left_input.x;
            delta_xy.y          = left_input.y;

            Vector3 world_pos        = m_MainCameraTransform.position;
            world_pos               += m_MainCameraTransform.forward * delta_xy.y * m_MoveSpeed * move_scale;
            world_pos               += m_MainCameraTransform.right   * delta_xy.x * m_MoveSpeed * move_scale;

            // ゲームコントローラ
            //if ( ( ! m_SimpleInputManager.IsCursorMoved() ) && 
            //     ( Input.touchCount == 0                  ) &&
            //     ( ! m_SimpleInputManager.GetMouseButton() ) )
            {
                if ( Input.GetKey( KeyCode.E ) )
                {
                    world_pos.y += m_MoveSpeed;
                }

                if ( Input.GetKey( KeyCode.Q ) )
                {
                    world_pos.y -= m_MoveSpeed;
                }

                if ( Input.GetButton( "Fire1" ) )
                {
                }

                if ( Input.GetButton( "Jump" ) )
                {
                }
            }

            m_MainCameraTransform.position = world_pos;



            //--------------------------------------------------
            // カメラの向き
            //--------------------------------------------------
            Vector2 delta_rot_xy = new Vector2( 0.0f, 0.0f );

            // 右のアナログスティック
            Vector2 right_analog_input  = m_SimpleInputManager.GetRightAnalogStickInput();
            delta_rot_xy               += right_analog_input;

            // カーソル
            Vector2 touch_input         = m_SimpleInputManager.GetCursorDeltaInput();
            if ( Input.touchCount >= 2 )
            {
                touch_input = Vector2.zero;
            }
            delta_rot_xy               += touch_input;

            // マウス用
            if ( Input.GetButton( m_SimpleInputManager.m_DragButtonName ) )
            {
                Vector2 mouse_move  = m_SimpleInputManager.GetMouseMoveInput();
                delta_rot_xy       += mouse_move;
            }

            rotationY        = ( - m_MainCameraTransform.localEulerAngles.x );
            rotationZ        = m_MainCameraTransform.localEulerAngles.z;

            float rotationX  = m_MainCameraTransform.localEulerAngles.y + delta_rot_xy.x;
            rotationY       += delta_rot_xy.y;

            m_MainCameraTransform.localEulerAngles = new Vector3( - rotationY, rotationX, rotationZ );
        }

        void UpdateViewerCamera_()
        {
            //--------------------------------------------------
            // カメラの向き
            //--------------------------------------------------

            Vector2 right_analog_input  = m_SimpleInputManager.GetRightAnalogStickInput();

            // タッチ入力
            if ( m_SimpleInputManager.IsCursorMoved() && 
                 m_SimpleInputManager.IsCursorValid() )
            {
                Vector2 delta        = m_SimpleInputManager.GetCursorDeltaInput();

                if ( ( Input.touchCount        >= 2 ) || 
                     ( m_TouchIgnoreFrameCount >  0 ) )
                {
                    delta = Vector2.zero;
                }

                horizontalAngle     += delta.x;
                horizontalAngle      = Mathf.Repeat(horizontalAngle, 360.0f);

                verticalAngle       -= delta.y;
                verticalAngle        = Mathf.Clamp(verticalAngle, m_MinVerticalAngle, m_MaxVerticalAngle );
            }
            horizontalAngle -= right_analog_input.x;

            Quaternion look_quat  = Quaternion.Euler( verticalAngle, horizontalAngle, 0 );

            //--------------------------------------------------
            // カメラの並進移動
            //--------------------------------------------------
            Vector2 left_input    = Vector2.zero;
            left_input           += m_SimpleInputManager.GetKeyboardMoveInput();
            left_input           += m_SimpleInputManager.GetLeftAnalogStickInput();

            Vector2 delta_xy;
            delta_xy.x  = left_input.x;
            delta_xy.y  = - left_input.y;

            m_LookAtOffsetPos += look_quat * new Vector3( delta_xy.x * cameraMoveSpeed,
                                                           0.0f,
                                                         - delta_xy.y * cameraMoveSpeed );


            // ゲームコントローラ
            if ( ( ! m_SimpleInputManager.IsCursorMoved()  ) && 
                 ( Input.touchCount == 0                   ) &&
                 ( ! m_SimpleInputManager.GetMouseButton() ) )
            {
                if ( Input.GetButton( "Fire2" ) )
                {
                    // Do nothing
                    m_LookAtOffsetPos.y += m_MoveSpeed;
                }

                if ( Input.GetButton( "Fire3" ) )
                {
                    m_LookAtOffsetPos.y -= m_MoveSpeed;
                }

                if ( Input.GetButton( "Fire1" ) )
                {
                }

                if ( Input.GetButton( "Jump" ) )
                {
                }
            }

            Vector3 lookAtPosition = m_LookAtOffsetPos;
            if ( m_LookAtTarget != null )
            {
                lookAtPosition += m_LookAtTarget.position;
            }

            float delta_z = CalcCameraDeltaPosZ( m_MainCameraTransform );
            if ( ( delta_z != 0.0f ) && ( Input.touchCount > 0 ) )
            {
                m_TouchIgnoreFrameCount = cMaxTouchIgnoreFrameCount;
            }

            if ( Input.GetKey( KeyCode.O ) )
            {
                delta_z = - 0.1f;
            }

            if ( Input.GetKey( KeyCode.P ) )
            {
                delta_z = 0.1f;
            }

            m_Distance   -= delta_z;
            m_Distance   -= right_analog_input.y * 0.01f;
            m_Distance    = Mathf.Clamp( m_Distance, m_MinDistance, m_MaxDistance );

            Vector3 relativePos = look_quat * new Vector3( 0, 0, - m_Distance );

            m_MainCameraTransform.position = lookAtPosition + relativePos;
            m_MainCameraTransform.LookAt( lookAtPosition );


            if ( m_TouchIgnoreFrameCount > 0 )
            {
                m_TouchIgnoreFrameCount--;
            }

            /*
            RaycastHit hitInfo;
            if ( Physics.Linecast( lookPosition, transform.position,
                                    out hitInfo, 1 << LayerMask.NameToLayer( "Ground" ) ) )
            {
                transform.position = hitInfo.point;
            }
            */
        }

        public float CalcCameraDeltaPosZ( Transform camera_transform )
        {
            if ( Input.touchCount == 2 )
            {
                // [Unity]:ドラッグ＆ピンチイン・ピンチアウトの入力コントロール
                // http://mokuapps.com/develop/?p=162

                if ( ( Input.touches[0].phase == TouchPhase.Began ) ||
                     ( Input.touches[1].phase == TouchPhase.Began ) )
                {
                    m_Interval = Vector2.Distance( Input.touches[0].position, Input.touches[1].position );
                }
                float tmpInterval = Vector2.Distance( Input.touches[0].position, Input.touches[1].position );
                float delta_z     = ( tmpInterval - m_Interval ) / m_DeltaTouchPosToZ;

                m_Interval                = tmpInterval;

                return delta_z;
            }
            else
            {
                return 0.0f;
            }
        }
    }
}