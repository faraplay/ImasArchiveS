﻿using Imas.Records;
using Imas.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;

namespace Imas
{
    internal class CommuToXlsx : IDisposable
    {
        private readonly XlsxWriter xlsx;

        #region IDisposable

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                xlsx?.Dispose();
            }
            disposed = true;
        }

        #endregion IDisposable

        public CommuToXlsx(string fileName, bool overwrite)
        {
            xlsx = new XlsxWriter(fileName, overwrite);
        }

        private IEnumerable<CommuLine> GetCommuFromStream(Stream binStream, string fileName)
        {
            try
            {
                List<CommuLine> lines = new List<CommuLine>();
                Binary binary = new Binary(binStream, true);
                if (binary.ReadUInt32() != 0x004D0053 ||
                    binary.ReadUInt32() != 0x00470000)
                {
                    throw new InvalidDataException();
                }

                int msgCount = (int)binary.ReadUInt32();

                if (binary.ReadUInt32() != 0)
                {
                    throw new InvalidDataException();
                }

                byte[] namebuf = new byte[32];
                byte[] msgbuf = new byte[128];
                for (int i = 0; i < msgCount; i++)
                {
                    CommuLine commuLine = new CommuLine { file = fileName };
                    commuLine.Deserialise(binStream);
                    lines.Add(commuLine);
                }
                return lines;
            }
            catch (EndOfStreamException)
            {
                throw new InvalidDataException();
            }
        }

        private void WriteCommuToXlsx(IEnumerable<CommuLine> lines, string fileName)
        {
            string sheetName;
            if (fileName[11] == '_')
                sheetName = "other";
            else
                sheetName = fileName[11..14];
            foreach (CommuLine line in lines)
            {
                xlsx.AppendRow(sheetName, line);
            }
        }

        public void GetAndWriteCommu(Stream binStream, string fileName) => WriteCommuToXlsx(GetCommuFromStream(binStream, fileName), fileName);
    }
}