﻿using Imas.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImasArchiveLibTest
{
    [TestClass]
    public class DeserialiserTest
    {

        [TestMethod]
        public void DeserialiseSpriteTest()
        {
            byte[] spriteData = new byte[]
            {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x42, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x42, 0x74, 0x00, 0x00,
                0x42, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46, 0x1C, 0x40, 0x00,
                0x46, 0x1C, 0x40, 0x00, 0x3F, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02,
                0xFF, 0xFF, 0xFF, 0xFF, 0x3F, 0x42, 0x00, 0x00, 0x3B, 0x80, 0x00, 0x00, 0x3F, 0x7F, 0x00, 0x00,
                0x3E, 0x78, 0x00, 0x00,
            };
            using MemoryStream memoryStream = new MemoryStream(spriteData);
            var obj = Deserialiser.Deserialise(new Imas.Binary(memoryStream, true), typeof(Sprite));
            using MemoryStream memoryStream1 = new MemoryStream();
            Serialiser.Serialise(new Imas.Binary(memoryStream1, true), obj);
        }

        [TestMethod]
        public void DeserialiseSpriteGroupTest()
        {
            byte[] spriteGroupData = new byte[]
            {
                0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x42, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x42, 0x74, 0x00, 0x00, 0x42, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x46, 0x1C, 0x40, 0x00, 0x46, 0x1C, 0x40, 0x00, 0x3F, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x02, 0xFF, 0xFF, 0xFF, 0xFF, 0x3F, 0x42, 0x00, 0x00, 0x3B, 0x80, 0x00, 0x00,
                0x3F, 0x7F, 0x00, 0x00, 0x3E, 0x78, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x42, 0x74, 0x00, 0x00, 0x42, 0x74, 0x00, 0x00, 0x42, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x46, 0x1C, 0x40, 0x00, 0x46, 0x1C, 0x40, 0x00, 0x3F, 0x80, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0xFF, 0xFF, 0xFF, 0xFF, 0x3F, 0x7F, 0x00, 0x00,
                0x3E, 0x78, 0x00, 0x00, 0x3F, 0x42, 0x00, 0x00, 0x3B, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x42, 0x74, 0x00, 0x00, 0x42, 0x74, 0x00, 0x00, 0x42, 0x74, 0x00, 0x00, 0x42, 0x74, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46, 0x1C, 0x40, 0x00, 0x46, 0x1C, 0x40, 0x00,
                0x3F, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0xFF, 0xFF, 0xFF, 0xFF,
                0x3F, 0x42, 0x00, 0x00, 0x3E, 0x78, 0x00, 0x00, 0x3F, 0x7F, 0x00, 0x00, 0x3B, 0x80, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x42, 0x74, 0x00, 0x00,
                0x42, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46, 0x1C, 0x40, 0x00,
                0x46, 0x1C, 0x40, 0x00, 0x3F, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02,
                0xFF, 0xFF, 0xFF, 0xFF, 0x3F, 0x7F, 0x00, 0x00, 0x3B, 0x80, 0x00, 0x00, 0x3F, 0x42, 0x00, 0x00,
                0x3E, 0x78, 0x00, 0x00,
            };
            using MemoryStream memoryStream = new MemoryStream(spriteGroupData);
            var obj = Deserialiser.Deserialise(new Imas.Binary(memoryStream, true), typeof(SpriteGroup));
        }

        [TestMethod]
        public void DeserialiseGroupControlTest()
        {
            byte[] spriteData = new byte[]
            {
                0x00, 0x00, 0x00, 0x04, 0x43, 0x6F, 0x75, 0x6E, 0x74, 0x46, 0x78, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x42, 0x54, 0x00, 0x00,
                0x42, 0x1C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x46, 0x1C, 0x40, 0x00, 0x46, 0x1C, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x3F, 0x80, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x42, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x42, 0x74, 0x00, 0x00,
                0x42, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46, 0x1C, 0x40, 0x00,
                0x46, 0x1C, 0x40, 0x00, 0x3F, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02,
                0xFF, 0xFF, 0xFF, 0xFF, 0x3F, 0x42, 0x00, 0x00, 0x3B, 0x80, 0x00, 0x00, 0x3F, 0x7F, 0x00, 0x00,
                0x3E, 0x78, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x42, 0x74, 0x00, 0x00,
                0x42, 0x74, 0x00, 0x00, 0x42, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x46, 0x1C, 0x40, 0x00, 0x46, 0x1C, 0x40, 0x00, 0x3F, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x02, 0xFF, 0xFF, 0xFF, 0xFF, 0x3F, 0x7F, 0x00, 0x00, 0x3E, 0x78, 0x00, 0x00,
                0x3F, 0x42, 0x00, 0x00, 0x3B, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x42, 0x74, 0x00, 0x00,
                0x42, 0x74, 0x00, 0x00, 0x42, 0x74, 0x00, 0x00, 0x42, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x46, 0x1C, 0x40, 0x00, 0x46, 0x1C, 0x40, 0x00, 0x3F, 0x80, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0xFF, 0xFF, 0xFF, 0xFF, 0x3F, 0x42, 0x00, 0x00,
                0x3E, 0x78, 0x00, 0x00, 0x3F, 0x7F, 0x00, 0x00, 0x3B, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x42, 0x74, 0x00, 0x00, 0x42, 0x74, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46, 0x1C, 0x40, 0x00, 0x46, 0x1C, 0x40, 0x00,
                0x3F, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0xFF, 0xFF, 0xFF, 0xFF,
                0x3F, 0x7F, 0x00, 0x00, 0x3B, 0x80, 0x00, 0x00, 0x3F, 0x42, 0x00, 0x00, 0x3E, 0x78, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
            };
            using MemoryStream memoryStream = new MemoryStream(spriteData);
            var obj = Deserialiser.Deserialise(new Imas.Binary(memoryStream, true), typeof(Control));
        }

        [TestMethod]
        public void DeserialiseControlTest()
        {
            using FileStream fileStream = new FileStream("playground/topS4U.pau", FileMode.Open);
            fileStream.Seek(4, SeekOrigin.Begin);
            var obj = Deserialiser.Deserialise(new Imas.Binary(fileStream, true), typeof(Control));
            using FileStream memoryStream1 = new FileStream("playground/topS4Uout.pau", FileMode.Create);
            Serialiser.Serialise(new Imas.Binary(memoryStream1, true), obj);
        }
    }
}