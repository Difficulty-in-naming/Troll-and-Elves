// System
using System;

namespace GUPS.EasyPooling.Strategy
{
    /// <summary>
    /// Specifies the pooling strategy for a GameObject pool.
    /// </summary>
    [Flags]
    public enum EPoolingStrategy
    {
        /// <summary>
        /// The pool will be empty on register, and the capacity will not grow or degrow over time.
        /// </summary>
        DEFAULT = 0,

        /// <summary>
        /// Fill the pool with the assigned capacity of objects on register. Useful when using loading scenes/screens.
        /// </summary>
        FILL = 1,

        /// <summary>
        /// The capacity of the pool will grow when the pool is often empty or degrow when the pool is at maximum capacity over long time.
        /// </summary>
        GROW = 2,
    }
}
