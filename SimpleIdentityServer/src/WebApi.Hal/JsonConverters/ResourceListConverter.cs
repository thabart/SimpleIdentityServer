using System;
using System.Linq;
using Newtonsoft.Json;
using WebApi.Hal.Interfaces;
using System.Reflection;

namespace WebApi.Hal.JsonConverters
{
    [Obsolete("use SimpleListRepresentation instead of RepresentationList")]
    public class ResourceListConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var representation = value as Representation;
            if (representation != null)
                representation.RepopulateHyperMedia();

            var list = (IRepresentationList)value;

            writer.WriteStartObject();
            writer.WritePropertyName("_links");
            serializer.Serialize(writer, list.Links);

            writer.WritePropertyName("_embedded");
            writer.WriteStartObject();
            writer.WritePropertyName(list.Rel);
            writer.WriteStartArray();
            foreach (Representation halResource in list)
            {
                serializer.Serialize(writer, halResource);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();

            var listType = list.GetType();
            var propertyInfos = typeof(RepresentationList<>).GetProperties().Select(p => p.Name);
            foreach (var property in listType.GetProperties().Where(p => !propertyInfos.Contains(p.Name)))
            {
                writer.WritePropertyName(property.Name.ToLower());
                serializer.Serialize(writer, property.GetValue(value, null));
            }

            writer.WriteEndObject();
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value;
        }

        public override bool CanConvert(Type objectType)
        {
            return IsResource(objectType) && IsResourceList(objectType);
        }

        static bool IsResourceList(Type objectType)
        {
            var type = typeof(IRepresentationList);
#if DNX451
            return type.IsAssignableFrom(objectType);
#endif
#if DNXCORE50
            return type.GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
#endif
        }

        static bool IsResource(Type objectType)
        {
            var type = typeof(Representation);
#if DNX451
            return type.IsAssignableFrom(objectType);
#endif
            return typeof(Representation).IsAssignableFrom(objectType);
        }
    }
}