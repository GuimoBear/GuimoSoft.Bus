using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GuimoSoft.Core.Serialization.Interfaces;

namespace GuimoSoft.Core.Serialization
{
    internal sealed class MessageSerializerManager : IMessageSerializerManager
    {
        internal static readonly MessageSerializerManager Instance
            = new MessageSerializerManager();

        private IDefaultSerializer _defaultSerializer = JsonMessageSerializer.Instance;
        private readonly IDictionary<Type, IDefaultSerializer> _typedSerializers
            = new ConcurrentDictionary<Type, IDefaultSerializer>();

        internal void SetDefaultSerializer(IDefaultSerializer defaultSerializer)
        {
            _defaultSerializer = defaultSerializer ?? throw new ArgumentNullException(nameof(defaultSerializer));
        }

        internal void AddTypedSerializer<TEvent>(TypedSerializer<TEvent> serializer)
        {
            if (serializer is null)
                throw new ArgumentNullException(nameof(serializer));
            _typedSerializers[typeof(TEvent)] = serializer;
        }

        public IDefaultSerializer GetSerializer(Type messageType)
        {
            if (_typedSerializers.TryGetValue(messageType, out var serializer))
                return serializer;
            return _defaultSerializer;
        }
    }
}
