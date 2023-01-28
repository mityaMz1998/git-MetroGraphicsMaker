using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public static class StorageExtentionMethods
    {

        public static List<Tuple<IEnumerable<Entity>, String, Type>> Add(
            this List<Tuple<IEnumerable<Entity>, String, Type>> storage, IEnumerable<Entity> collection, String name)
        {
            if (collection == null)
                return storage;

            var element = collection.SingleOrDefault();
            var type = element == null ? typeof (Entity) : element.GetType();
            storage.Add(Tuple.Create(collection, name, type));
            return storage;
        }

        public static List<Tuple<IEnumerable<Entity>, String, Type>> Add (
            this List<Tuple<IEnumerable<Entity>, String, Type>> storage, IEnumerable<Entity> collection, String name,
            Type type)
        {
            storage.Add(Tuple.Create(collection, name, type));
            return storage;
        }

        public static IEnumerable<Entity> GetCollectionByName(this List<Tuple<IEnumerable<Entity>, String, Type>> storage, String name)
        {
            var element = storage.SingleOrDefault(x => x.Item2 == name);
            return element == null ? Enumerable.Empty<Entity>() : element.Item1;
        }

        public static IEnumerable<T> GetCollectionByName<T>(this List<Tuple<IEnumerable<Entity>, String, Type>> storage, String name)
        {
            return storage.GetCollectionByName(name).OfType<T>();
        }
    }
}
