// System
using System;

namespace GUPS.EasyPooling.Blueprint
{
    /// <summary>
    /// Defines a common interface for blueprints, providing information about an object's configuration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="IBlueprint"/> interface serves as a common structure for objects that represent configuration details,
    /// offering a standardized way to retrieve the name associated with a blueprint.
    /// </para>
    /// </remarks>
    public interface IBlueprint
    {
        /// <summary>
        /// The name of the blueprint.
        /// </summary>
        String Name { get; }
    }
}
