using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Hawkeye.ComponentModel
{
    public static class CustomTypeDescriptors
    {
        private static readonly Dictionary<Type, TypeDescriptionProvider> providers =
            new Dictionary<Type, TypeDescriptionProvider>();

        static CustomTypeDescriptors()
        {
            //var type = typeof(IProxy);
            //var proxyProvider = new ProxyDescriptionProvider();
            //TypeDescriptor.AddProvider(proxyProvider, type);
            //providers.Add(type, proxyProvider);
        }

        /// <summary>
        ///     Adds the <paramref name="type" /> of the generic provider to.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="type" />
        /// </exception>
        public static void AddGenericProviderToType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (providers.ContainsKey(type))
            {
                return;
            }

            TypeDescriptionProvider previousProvider = TypeDescriptor.GetProvider(type);
            var typeDescriptor = new GenericTypeDescriptor(type, previousProvider.GetTypeDescriptor(type));
            var newProvider = new GenericTypeDescriptionProvider(typeDescriptor);

            TypeDescriptor.RemoveProvider(previousProvider, type);
            TypeDescriptor.AddProvider(newProvider, type);
        }

        private class GenericTypeDescriptionProvider : TypeDescriptionProvider
        {
            private readonly ICustomTypeDescriptor _typeDescriptor;

            /// <summary>
            ///     <para>
            ///         Initializes a new instance of the
            ///         <see cref="CustomTypeDescriptors.GenericTypeDescriptionProvider" />
            ///     </para>
            ///     <para>class.</para>
            /// </summary>
            /// <param name="customTypeDescriptor">
            ///     <para>
            ///         The custom type descriptor returned by
            ///         <see cref="CustomTypeDescriptors.GenericTypeDescriptionProvider.GetTypeDescriptor" />
            ///     </para>
            ///     <para>.</para>
            /// </param>
            /// <exception cref="ArgumentNullException">
            ///     <paramref name="customTypeDescriptor" />
            /// </exception>
            /// <inheritdoc />
            public GenericTypeDescriptionProvider(ICustomTypeDescriptor customTypeDescriptor)
            {
                _typeDescriptor = customTypeDescriptor ?? throw new ArgumentNullException(nameof(customTypeDescriptor));
            }

            /// <summary>
            ///     Gets a custom type descriptor for the given type and object.
            /// </summary>
            /// <param name="objectType">
            ///     The type of object for which to retrieve the type descriptor.
            /// </param>
            /// <param name="instance">
            ///     An instance of the type. Can be <see langword="null" /> if no
            ///     instance was passed to the <see cref="TypeDescriptor" /> .
            /// </param>
            /// <returns>
            ///     An <see cref="ICustomTypeDescriptor" /> that can provide
            ///     metadata for the type.
            /// </returns>
            /// <inheritdoc />
            public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
            {
                return _typeDescriptor;
            }
        }
    }
}