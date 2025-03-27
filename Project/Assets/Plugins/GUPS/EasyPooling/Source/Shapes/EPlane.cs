namespace GUPS.EasyPooling.Shapes
{
    /// <summary>
    /// Orientation in 2D space. The default 2D plane is on the XY axis.
    /// </summary>
    public enum EPlane : byte
    {
        /// <summary>
        /// The orientation is on the XY plane.
        /// </summary>
        XY = 0,

        /// <summary>
        /// The orientation is on the XZ plane.
        /// </summary>
        XZ = 1,

        /// <summary>
        /// The orientation is on the YZ plane.
        /// </summary>
        YZ = 2,
    }
}
