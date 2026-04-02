Public Module DDS_Common

    <Flags>
    Public Enum DDS_SurfaceFlags
        DDSD_CAPS = &H1
        DDSD_HEIGHT = &H2
        DDSD_WIDTH = &H4
        DDSD_PITCH = &H8
        DDSD_PIXELFORMAT = &H1000
        DDSD_MIPMAPCOUNT = &H20000
        DDSD_LINEARSIZE = &H80000
        DDSD_DEPTH = &H800000
    End Enum

    <Flags>
    Public Enum DDS_PixelFlags
        DDPF_ALPHAPIXELS = &H1
        DDPF_FOURCC = &H4
        DDPF_RGB = &H40
    End Enum

    <Flags>
    Public Enum DDS_Caps1
        DDSCAPS_COMPLEX = &H8
        DDSCAPS_TEXTURE = &H1000
        DDSCAPS_MIPMAP = &H400000
    End Enum

    <Flags>
    Public Enum DXGI_Format
        DXGI_FORMAT_UNKNOWN = &H0
        DXGI_FORMAT_BC1_UNORM = &H47
        DXGI_FORMAT_BC3_UNORM = &H4D
        DXGI_FORMAT_B8G8R8A8_UNORM = &H57
        DXGI_FORMAT_B8G8R8X8_UNORM = &H58
    End Enum

    <Flags>
    Public Enum DX10_ResourceDimension As Integer
        D3D10_RESOURCE_DIMENSION_UNKNOWN = &H0
        D3D10_RESOURCE_DIMENSION_BUFFER = &H1
        D3D10_RESOURCE_DIMENSION_TEXTURE1D = &H2
        D3D10_RESOURCE_DIMENSION_TEXTURE2D = &H3
        D3D10_RESOURCE_DIMENSION_TEXTURE3D = &H4
    End Enum

    <Flags>
    Public Enum DX10_MiscFlags As Integer
        D3D10_RESOURCE_MISC_NONE = &H0
        D3D10_RESOURCE_MISC_TEXTURECUBE = &H4
    End Enum

    <Flags>
    Public Enum DX10_AlphaMode As Integer
        DDS_ALPHA_MODE_UNKNOWN = &H0
        DDS_ALPHA_MODE_STRAIGHT = &H1
        DDS_ALPHA_MODE_PREMULTIPLIED = &H2
        DDS_ALPHA_MODE_OPAQUE = &H3
        DDS_ALPHA_MODE_CUSTOM = &H4
    End Enum

    Public Function Clamp(Value As Integer, MinValue As Integer, MaxValue As Integer) As Integer
        Return Math.Max(MinValue, Math.Min(Value, MaxValue))
    End Function

    Public Sub Swap(Of T)(ByRef Value1 As T, ByRef Value2 As T)
        Dim Temp As T = Value1
        Value1 = Value2
        Value2 = Temp
    End Sub

End Module
