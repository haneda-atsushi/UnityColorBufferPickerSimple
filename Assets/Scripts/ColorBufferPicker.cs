using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ColorBufferPicker : MonoBehaviour
{
    public bool  m_Enable      = true;
    public Color m_LineColor   = Color.black;
    public Color m_FontColor   = Color.white;

    public enum Mode
    {
        Float,
        LinearRGB,
        sRGB
    };
    public Mode m_Mode = Mode.Float;

    Camera    m_Camera;
    Texture2D m_TargetTexture;
    Color     m_PickedColor;

    Shader    m_VertexColorShader;
    Material  m_VertexColorMaterial;
    int       m_SrcWidth;
    int       m_SrcHeight;
    int       m_SrcCenterX;
    int       m_SrcCenterY;
    GUIStyle  m_GUIStyle;

    void Start()
    {
        m_Camera = GetComponent<Camera>();
        Assert.IsNotNull( m_Camera );

        m_TargetTexture = new Texture2D( 8, 8, TextureFormat.RGBAFloat, false );
        Assert.IsNotNull( m_TargetTexture );

        m_VertexColorShader = Shader.Find( "Hidden/VertexColor" );
        Assert.IsNotNull( m_VertexColorShader );

        m_VertexColorMaterial = new Material( m_VertexColorShader );
        Assert.IsNotNull( m_VertexColorMaterial );

        m_GUIStyle          = new GUIStyle();
        m_GUIStyle.fontSize = 40;
    }

    void OnRenderImage( RenderTexture src, RenderTexture dest )
    {
        if ( m_Enable )
        {
            RenderTexture.active = src;

            m_SrcWidth   = src.width;
            m_SrcHeight  = src.height;
            m_SrcCenterX = m_SrcWidth / 2;
            m_SrcCenterY = m_SrcHeight / 2;

            m_TargetTexture.ReadPixels( new Rect( m_SrcCenterX, m_SrcCenterY, 8, 8 ), 0, 0 );
            m_TargetTexture.Apply();

            m_PickedColor = m_TargetTexture.GetPixel( 0, 0 );
        }

        DrawCrossCenter_();

        Graphics.Blit( src, dest );
    }

    void OnGUI()
    {
        if ( m_Enable )
        {
            m_GUIStyle.normal.textColor = m_FontColor;

            string label = "";
            if ( m_Mode == Mode.Float )
            {
                label = string.Format( "HDR:( {0,2:F2}, {1,2:F2}, {2,2:F2} )",
                                        m_PickedColor.r, m_PickedColor.g, m_PickedColor.b );
            }
            else if ( m_Mode == Mode.LinearRGB )
            {
                label = string.Format( "linearRGB:( {0,2:F2}, {1,2:F2}, {2,2:F2})",
                                       m_PickedColor.r * 255.0f,
                                       m_PickedColor.b * 255.0f,
                                       m_PickedColor.b * 255.0f );
            }
            else if ( m_Mode == Mode.sRGB )
            {
                label = string.Format( "sRGB:( {0,2:F2}, {1,2:F2}, {2,2:F2})",
                                       Mathf.Pow( m_PickedColor.r, 1.0f / 2.2f ) * 255.0f,
                                       Mathf.Pow( m_PickedColor.g, 1.0f / 2.2f ) * 255.0f,
                                       Mathf.Pow( m_PickedColor.b, 1.0f / 2.2f ) * 255.0f );
            }

            GUI.Label( new Rect( 10, 10, 100, 50 ), label, m_GUIStyle );
        }
    }

    void DrawCrossCenter_()
    {
        const int   cCrossLength = 24;
        const float z_value      = 1.0f;

        Vector3 screen_pos0 = new Vector3( m_SrcCenterX - cCrossLength, m_SrcCenterY               , z_value );
        Vector3 screen_pos1 = new Vector3( m_SrcCenterX + cCrossLength, m_SrcCenterY               , z_value );
        Vector3 screen_pos2 = new Vector3( m_SrcCenterX               , m_SrcCenterY - cCrossLength, z_value );
        Vector3 screen_pos3 = new Vector3( m_SrcCenterX               , m_SrcCenterY + cCrossLength, z_value );

        Vector3 world_pos0  = m_Camera.ScreenToWorldPoint( screen_pos0 );
        Vector3 world_pos1  = m_Camera.ScreenToWorldPoint( screen_pos1 );
        Vector3 world_pos2  = m_Camera.ScreenToWorldPoint( screen_pos2 );
        Vector3 world_pos3  = m_Camera.ScreenToWorldPoint( screen_pos3 );

        m_VertexColorMaterial.SetPass( 0 );
        GL.PushMatrix();
        {
            GL.Begin( GL.LINES );
            GL.Color( m_LineColor );
            GL.Vertex3( world_pos0.x, world_pos0.y, world_pos0.z );

            GL.Color( m_LineColor );
            GL.Vertex3( world_pos1.x, world_pos1.y, world_pos1.z );

            GL.Color( m_LineColor );
            GL.Vertex3( world_pos2.x, world_pos2.y, world_pos2.z );

            GL.Color( m_LineColor );
            GL.Vertex3( world_pos3.x, world_pos3.y, world_pos3.z );
            GL.End();
        }
        GL.PopMatrix();
    }
}
