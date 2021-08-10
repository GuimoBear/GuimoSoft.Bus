using System;
using System.Collections.Generic;

namespace GuimoSoft.Bus.Benchmarks
{
    public static class BenchmarkContext
    {
        private static readonly object _lock = new();

        private static readonly Queue<Guid> _ids = new();

        public static void Add(Guid id)
        {
            lock (_lock)
                _ids.Enqueue(id);
        }

        public static bool TryGet(out Guid id)
        {
            lock (_lock)
                return _ids.TryDequeue(out id);
        }
    }
}
