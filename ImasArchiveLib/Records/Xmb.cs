using DocumentFormat.OpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Imas.Records
{
    class Xmb
    {
        Record[] tagStrings;
        List<Element> elements;
        List<Attr> attrs;
        List<IdStringOff> idStringOffs;
        public void ReadXmb(Stream stream)
        {
            Binary binary = new Binary(stream, true);
            if (binary.ReadInt32() != 0x584D4220)
            {
                return;
            }
            int elementCount = binary.ReadInt32();
            int attrCount = binary.ReadInt32();
            int tagStringCount = binary.ReadInt32();
            int idCount = binary.ReadInt32();

            int tagStringOffsetPos = binary.ReadInt32();
            int elementPos = binary.ReadInt32();
            int attrPos = binary.ReadInt32();
            int idStringOffPos = binary.ReadInt32();
            int tagStringPos = binary.ReadInt32();
            int dataStringPos = binary.ReadInt32();

            stream.Position = tagStringOffsetPos;
            tagStrings = new Record[tagStringCount];
            for (int i = 0; i < tagStringCount; i++)
            {
                tagStrings[i] = new Record("iX");
                tagStrings[i].Deserialise(stream);
            }

            stream.Position = elementPos;
            elements = new List<Element>(elementCount);
            for (int i = 0; i < elementCount; i++)
            {
                elements.Add(Element.GetElement(stream));
            }

            stream.Position = attrPos;
            attrs = new List<Attr>(attrCount);
            for (int i = 0; i < attrCount; i++)
                attrs.Add(Attr.GetAttr(stream));

            stream.Position = idStringOffPos;
            idStringOffs = new List<IdStringOff>(idCount);
            for (int i = 0; i < idCount; i++)
            {
                idStringOffs.Add(IdStringOff.GetIdStringOff(stream));
            }

            for (int i = 0; i < tagStringCount; i++)
            {
                stream.Position = tagStringPos + (int)tagStrings[i][0];
                char c;
                StringBuilder sb = new StringBuilder();
                while ((c = (char)binary.ReadByte()) != 0)
                {
                    sb.Append(c);
                }
                tagStrings[i][1] = sb.ToString();
            }

            for (int i = 0; i < attrCount; i++)
            {
                stream.Position = tagStringPos + attrs[i].keyOffset;
                char c;
                StringBuilder sb = new StringBuilder();
                while ((c = (char)binary.ReadByte()) != 0)
                {
                    sb.Append(c);
                }
                attrs[i].key = sb.ToString();

                sb.Clear();
                stream.Position = dataStringPos + attrs[i].valOffset;
                if (attrs[i].key == "_text")
                {
                    while ((c = (char)binary.ReadUInt16()) != 0)
                    {
                        sb.Append(c);
                    }
                }
                else
                {
                    while ((c = (char)binary.ReadByte()) != 0)
                    {
                        sb.Append(c);
                    }
                }
                attrs[i].value = sb.ToString();
            }

            for (int i = 0; i < elementCount; i++)
            {
                stream.Position = tagStringPos + elements[i].tagStringOffset;
                char c;
                StringBuilder sb = new StringBuilder();
                while ((c = (char)binary.ReadByte()) != 0)
                {
                    sb.Append(c);
                }
                elements[i].type = sb.ToString();
            }
        }

        public void WriteXml(Stream stream)
        {
            XmlWriter xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true });
            xmlWriter.WriteStartDocument();
            WriteXmlNode(xmlWriter, 0);
            xmlWriter.WriteEndDocument();
            xmlWriter.Flush();
        }

        void WriteXmlNode(XmlWriter xmlWriter, int elementIndex)
        {
            Element r = elements[elementIndex];
            xmlWriter.WriteStartElement(r.type);
            for (int i = 0; i < r.attrCount; i++)
                xmlWriter.WriteAttributeString(
                    attrs[r.firstAttrIndex + i].key, attrs[r.firstAttrIndex + i].value);
            for (int i = 0; i < r.childCount; i++)
                WriteXmlNode(xmlWriter, r.firstChildIndex + i);
            xmlWriter.WriteEndElement();
        }

        #region Read XML
        StringData typeData;
        StringData valueData;
        public void ReadXml(Stream stream)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(stream);
            elements = new List<Element>();
            attrs = new List<Attr>();
            idStringOffs = new List<IdStringOff>();
            typeData = new StringData();
            valueData = new StringData();
            ReadNodeAndDescendants(xml.DocumentElement, -1);
        }

        void ReadNodeAndDescendants(XmlNode xmlNode, int parentIndex)
        {
            int index = elements.Count;
            ReadNode(xmlNode, parentIndex);
            UpdateElementAndReadDescendants(xmlNode, index);
        }

        void ReadNode(XmlNode xmlNode, int parentIndex)
        {
            Element element = new Element();
            int elementIndex = elements.Count;
            element.type = xmlNode.LocalName;
            element.tagStringOffset = typeData.GetOffset(element.type, false);
            element.attrCount = (ushort)xmlNode.Attributes.Count;
            element.childCount = (ushort)xmlNode.ChildNodes.Count;

            if (element.attrCount == 0)
                element.firstAttrIndex = 0xFFFF;
            else
                element.firstAttrIndex = (ushort)attrs.Count;

            for (int i = 0; i < element.attrCount; i++)
            {
                Attr attr = new Attr();
                attr.key = xmlNode.Attributes[i].LocalName;
                attr.keyOffset = typeData.GetOffset(attr.key, false);
                attr.value = xmlNode.Attributes[i].Value;
                attr.valOffset = valueData.GetOffset(attr.value, attr.key == "_text");
                attrs.Add(attr);

                if (attr.key == "id")
                {
                    idStringOffs.Add(new IdStringOff { offset = attr.valOffset, nodeIndex = elementIndex });
                }
            }
            element.parentIndex = (ushort)parentIndex;
            element.minusOne = -1;
            elements.Add(element);
        }

        void UpdateElementAndReadDescendants(XmlNode parentNode, int nodeIndex)
        {
            Element element = elements[nodeIndex];
            if (element.type == "text_node")
                element.firstChildIndex = 0xFFFF;
            else
            {
                element.firstChildIndex = (ushort)elements.Count;
                ReadDescendantNodes(parentNode, nodeIndex);
            }
        }

        void ReadDescendantNodes(XmlNode parentNode, int parentIndex)
        {
            int firstChildIndex = elements.Count;
            for (int j = 0; j < parentNode.ChildNodes.Count; j++)
            {
                ReadNode(parentNode.ChildNodes[j], parentIndex);
            }
            for (int j = 0; j < parentNode.ChildNodes.Count; j++)
            {
                UpdateElementAndReadDescendants(parentNode.ChildNodes[j], firstChildIndex + j);
            }
        }
        #endregion

        public async Task WriteXmb(Stream stream)
        {
            Binary binary = new Binary(stream, true);
            binary.WriteUInt32(0x584D4220);
            binary.WriteInt32(elements.Count);
            binary.WriteInt32(attrs.Count);
            binary.WriteInt32(typeData.Count);
            binary.WriteInt32(idStringOffs.Count);

            int offset = 0x40;
            binary.WriteInt32(offset);
            offset += typeData.Count * 4;
            binary.WriteInt32(offset);
            offset += elements.Count * 16;
            binary.WriteInt32(offset);
            offset += attrs.Count * 8;
            binary.WriteInt32(offset);
            offset += idStringOffs.Count * 8;
            binary.WriteInt32(offset);
            offset += typeData.Length;
            while (offset % 4 != 0)
                offset++;
            binary.WriteInt32(offset);

            binary.WriteInt32(0);
            binary.WriteInt32(0);
            binary.WriteInt32(0);
            binary.WriteInt32(0);
            binary.WriteInt32(0);

            WriteXmbTypeStringOffset(stream);
            foreach (Element element in elements)
                element.Serialise(stream);
            foreach (Attr attr in attrs)
                attr.Serialise(stream);
            foreach (IdStringOff idStringOff in idStringOffs)
                idStringOff.Serialise(stream);
            await typeData.CopyToAsync(stream);
            while (stream.Position % 4 != 0)
                stream.WriteByte(0);
            await valueData.CopyToAsync(stream);
            while (stream.Position % 4 != 0)
                stream.WriteByte(0);
        }

        void WriteXmbTypeStringOffset(Stream stream)
        {
            var typeStringOffsets = typeData.GetKeyValuePairs();
            typeStringOffsets.Sort((pair1, pair2) => string.Compare(pair1.Key, pair2.Key));
            foreach (var pair in typeStringOffsets)
            {
                Binary.WriteInt32(stream, true, pair.Value);
            }
        }

        //void WriteXml(TextWriter writer, int elementIndex, int depth)
        //{
        //    Record r = elements[elementIndex];
        //    for (int i = 0; i < depth; i++)
        //    {
        //        writer.Write('\t');
        //    }
        //    writer.Write('<');
        //    writer.Write((string)r[7]);
        //    for (int i = 0; i < (short)r[1]; i++)
        //    {
        //        WriteAttribute(writer, (ushort)(short)r[3] + i);
        //    }
        //    writer.Write(">\n");

        //    for (int i = 0; i < (short)r[2]; i++)
        //    {
        //        WriteXml(writer, (ushort)(short)r[4] + i, depth + 1);
        //    }

        //    for (int i = 0; i < depth; i++)
        //    {
        //        writer.Write('\t');
        //    }
        //    writer.Write("</");
        //    writer.Write((string)r[7]);
        //    writer.Write(">\n");
        //}

        //void WriteAttribute(TextWriter writer, int attrIndex)
        //{
        //    writer.Write(' ');
        //    writer.Write((string)attrs[attrIndex][2]);
        //    writer.Write("=\"");
        //    string val = ((string)attrs[attrIndex][3]).Replace("\n", "&#10;");
        //    writer.Write(val);
        //    writer.Write('"');
        //}

        class StringData
        {
            MemoryStream stringData = new MemoryStream();
            Dictionary<string, int> asciiStringOffs = new Dictionary<string, int>();
            Dictionary<string, int> utf16StringOffs = new Dictionary<string, int>();

            public int Count => asciiStringOffs.Count + utf16StringOffs.Count;

            public int Length => (int)stringData.Length;

            public async Task CopyToAsync(Stream stream)
            {
                stringData.Position = 0;
                await stringData.CopyToAsync(stream);
            }

            public List<KeyValuePair<string, int>> GetKeyValuePairs()
            {
                List<KeyValuePair<string, int>> keyValuePairs = new List<KeyValuePair<string, int>>();
                keyValuePairs.AddRange(asciiStringOffs);
                keyValuePairs.AddRange(utf16StringOffs);
                return keyValuePairs;
            }

            public int GetOffset(string s, bool isWideChar)
            {
                if (isWideChar)
                {
                    if (utf16StringOffs.ContainsKey(s))
                    {
                        return utf16StringOffs[s];
                    }
                    else
                    {
                        int offset = (int)stringData.Position;
                        stringData.Write(ImasEncoding.Custom.GetBytes(s));
                        stringData.WriteByte(0);
                        stringData.WriteByte(0);
                        utf16StringOffs.Add(s, offset);
                        return offset;
                    }
                }
                else
                {
                    if (asciiStringOffs.ContainsKey(s))
                    {
                        return asciiStringOffs[s];
                    }
                    else
                    {
                        int offset = (int)stringData.Position;
                        stringData.Write(Encoding.ASCII.GetBytes(s));
                        stringData.WriteByte(0);
                        asciiStringOffs.Add(s, offset);
                        return offset;
                    }
                }
            }
        }

        class Element
        {
            public int tagStringOffset;
            public ushort attrCount;
            public ushort childCount;
            public ushort firstAttrIndex;
            public ushort firstChildIndex;
            public ushort parentIndex;
            public short minusOne;
            public string type;

            public static Element GetElement(Stream stream)
            {
                Binary binary = new Binary(stream, true);
                return new Element
                {
                    tagStringOffset = binary.ReadInt32(),
                    attrCount = binary.ReadUInt16(),
                    childCount = binary.ReadUInt16(),
                    firstAttrIndex = binary.ReadUInt16(),
                    firstChildIndex = binary.ReadUInt16(),
                    parentIndex = binary.ReadUInt16(),
                    minusOne = binary.ReadInt16()
                };
            }

            public void Serialise(Stream stream)
            {
                Binary.WriteInt32(stream, true, tagStringOffset);
                Binary.WriteUInt16(stream, true, attrCount);
                Binary.WriteUInt16(stream, true, childCount);
                Binary.WriteUInt16(stream, true, firstAttrIndex);
                Binary.WriteUInt16(stream, true, firstChildIndex);
                Binary.WriteUInt16(stream, true, parentIndex);
                Binary.WriteInt16(stream, true, minusOne);
            }
        }

        class Attr
        {
            public int keyOffset;
            public int valOffset;
            public string key;
            public string value;

            public static Attr GetAttr(Stream stream)
            {
                Binary binary = new Binary(stream, true);
                return new Attr
                {
                    keyOffset = binary.ReadInt32(),
                    valOffset = binary.ReadInt32()
                };
            }
            public void Serialise(Stream stream)
            {
                Binary.WriteInt32(stream, true, keyOffset);
                Binary.WriteInt32(stream, true, valOffset);
            }
        }

        class IdStringOff
        {
            public int offset;
            public int nodeIndex;
            public static IdStringOff GetIdStringOff(Stream stream)
            {
                Binary binary = new Binary(stream, true);
                return new IdStringOff
                {
                    offset = binary.ReadInt32(),
                    nodeIndex = binary.ReadInt32()
                };
            }
            public void Serialise(Stream stream)
            {
                Binary.WriteInt32(stream, true, offset);
                Binary.WriteInt32(stream, true, nodeIndex);
            }
        }
    }
}
