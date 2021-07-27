namespace CatalogConverter.Data
{
    using System;
    using System.Collections;
    using System.Xml.Serialization;

    internal class XmlSerializerCache
    {
        private static readonly Hashtable Serializers = new Hashtable();

        public static XmlSerializer GetXmlSerializer(Type type, params Type[] extraTypes)
        {
            string fullName = type.FullName;
            XmlSerializer xmlSerializer = (XmlSerializer)XmlSerializerCache.Serializers[(object)fullName];
            if (xmlSerializer == null)
            {
                lock (XmlSerializerCache.Serializers.SyncRoot)
                {
                    xmlSerializer = (XmlSerializer)XmlSerializerCache.Serializers[(object)fullName];
                    if (xmlSerializer == null)
                    {
                        xmlSerializer = extraTypes == null || extraTypes.Length == 0 ? new XmlSerializer(type) : new XmlSerializer(type, extraTypes);
                        XmlSerializerCache.Serializers[(object)fullName] = (object)xmlSerializer;
                    }
                }
            }

            return xmlSerializer;
        }
    }
}

