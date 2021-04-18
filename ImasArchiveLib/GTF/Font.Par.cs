using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Imas.Gtf
{
    public partial class Font : IDisposable
    {
        private readonly byte[] zeros = new byte[256];
        private byte[] parHeader;

        public async Task ReadFontPar(Stream stream)
        {
            using MemoryStream memStream = new MemoryStream();
            await stream.CopyToAsync(memStream);
            memStream.Position = 0;
            Binary binary = new Binary(memStream, true);
            parHeader = new byte[16];
            memStream.Read(parHeader);
            int gtfPos = binary.ReadInt32();
            _ = binary.ReadInt32();
            int nxpPos = binary.ReadInt32();

            memStream.Position = gtfPos;
            SetGtf(GTF.CreateFromGtfStream(memStream));

            memStream.Position = nxpPos + 8;
            int charCount = binary.ReadInt32();
            root = binary.ReadInt32();
            chars = new CharData[charCount];
            memStream.Position = nxpPos + 48;
            for (int i = 0; i < charCount; i++)
            {
                chars[i] = new CharData
                {
                    index = i,
                    key = binary.ReadUInt16(),
                    datawidth = binary.ReadByte(),
                    dataheight = binary.ReadByte(),
                    datax = binary.ReadInt16(),
                    datay = binary.ReadInt16(),
                    offsetx = binary.ReadInt16(),
                    offsety = binary.ReadInt16(),
                    width = binary.ReadInt16(),
                    blank = binary.ReadInt16(),
                    left = binary.ReadInt32(),
                    right = binary.ReadInt32(),
                    isEmoji = binary.ReadUInt16()
                };
                memStream.Position += 6;
            }
        }

        public async Task WriteFontPar(Stream stream, bool nxFixedWidth = true)
        {
            int gtfPad, nxPad, nxpPad;
            using (MemoryStream memStream = new MemoryStream())
            {
                UseBigBitmap();
                BuildTree();

                long pos = 0;
                parHeader = new byte[16] {
                    0x50, 0x41, 0x52, 0x02, 0x00, 0x00, 0x00, 0x03,
                    0x00, 0x00, 0x00, 0x03, 0x03, 0x00, 0x00, 0x00,
                };

                memStream.Write(parHeader);

                int gtfPos = 0x200;
                int gtfLen = 0x800080;
                gtfPad = (-gtfLen) & 0x7F;
                int nxPos = gtfPos + gtfLen + gtfPad;
                int nxLen = 0x30 + 0x20 * chars.Length;
                nxPad = (-nxLen) & 0x7F;
                int nxpPos = nxPos + nxLen + nxPad;
                int nxpLen = 0x30 + 0x20 * chars.Length;
                nxpPad = (-nxpLen) & 0x7F;

                Binary binary = new Binary(memStream, true);

                binary.WriteInt32(gtfPos);
                binary.WriteInt32(nxPos);
                binary.WriteInt32(nxpPos);
                binary.WriteInt32(0);

                string gtfName = "im2nx.gtf";
                string nxName = "im2nx.paf";
                string nxpName = "im2nxp.paf";
                memStream.Write(Encoding.ASCII.GetBytes(gtfName));
                memStream.Write(zeros, 0, 0x80 - gtfName.Length);
                memStream.Write(Encoding.ASCII.GetBytes(nxName));
                memStream.Write(zeros, 0, 0x80 - nxName.Length);
                memStream.Write(Encoding.ASCII.GetBytes(nxpName));
                memStream.Write(zeros, 0, 0x80 - nxpName.Length);

                binary.WriteUInt32(0);
                binary.WriteUInt32(1);
                binary.WriteUInt32(2);
                binary.WriteUInt32(0);

                binary.WriteInt32(gtfLen);
                binary.WriteInt32(nxLen);
                binary.WriteInt32(nxpLen);
                binary.WriteInt32(0);

                int pad = (int)(pos - memStream.Position) & 0x7F;
                memStream.Write(zeros, 0, pad);

                memStream.Position = 0;
                await memStream.CopyToAsync(stream);
            }

            await GTF.WriteGTF(stream, BigBitmap, 0x83);
            await stream.WriteAsync(zeros, 0, gtfPad);

            await WritePaf(stream, false, nxFixedWidth);
            await stream.WriteAsync(zeros, 0, nxPad);

            await WritePaf(stream, true, nxFixedWidth);
            await stream.WriteAsync(zeros, 0, nxpPad);
        }

        private async Task WritePaf(Stream stream, bool isNxp, bool nxFixedWidth = true)
        {
            using MemoryStream memStream = new MemoryStream();
            Binary binary = new Binary(memStream, true);
            binary.WriteUInt32(0x70616601);
            binary.WriteUInt32(0x0201001D);
            binary.WriteInt32(chars.Length);
            binary.WriteInt32(root);
            memStream.Write(Encoding.ASCII.GetBytes("im2nx"));
            if (isNxp)
                memStream.WriteByte(0x70);
            else
                memStream.WriteByte(0);
            memStream.Write(zeros, 0, 0xA);

            binary.WriteInt16(0x30);
            binary.WriteInt16(0x100);
            memStream.Write(zeros, 0, 0xC);

            foreach (CharData c in chars)
            {
                binary.WriteUInt16(c.key);
                binary.WriteByte(c.datawidth);
                binary.WriteByte(c.dataheight);
                binary.WriteInt16(c.datax);
                binary.WriteInt16(c.datay);
                binary.WriteInt16(c.offsetx);
                binary.WriteInt16(c.offsety);
                binary.WriteInt16((isNxp || !nxFixedWidth) ? c.width : (short)0x20);
                binary.WriteInt16(c.blank);
                binary.WriteInt32(c.left);
                binary.WriteInt32(c.right);
                binary.WriteUInt16(c.isEmoji);
                memStream.Write(zeros, 0, 6);
            }

            memStream.Position = 0;
            await memStream.CopyToAsync(stream);
        }
    }
}