namespace Packages.realityflow_package.Runtime.scripts
{
    /// <summary>
    /// The purpose of this class is to copy the properties of one object into another
    /// </summary>
    /// <typeparam name="TParent">The type of the parent object (The source information)</typeparam>
    /// <typeparam name="TChild">The type of the child object (The destination, these properties will get overwritten with those values that are in the parent)</typeparam>
    public class PropertyCopier<TParent, TChild> where TParent : class
                                            where TChild : class
    {
        /// <summary>
        /// Copy the properties of a parent object into the corresponsing property of the child
        /// </summary>
        /// <param name="parent">Source information object</param>
        /// <param name="child">Destination object (These properties get overwritten)</param>
        public static void Copy(TParent parent, TChild child)
        {
            var parentProperties = parent.GetType().GetProperties();
            var childProperties = child.GetType().GetProperties();

            foreach (var parentProperty in parentProperties)
            {
                foreach (var childProperty in childProperties)
                {
                    if (parentProperty.Name == childProperty.Name && parentProperty.PropertyType == childProperty.PropertyType)
                    {
                        childProperty.SetValue(child, parentProperty.GetValue(parent));
                        break;
                    }
                }
            }
        }
    }
}