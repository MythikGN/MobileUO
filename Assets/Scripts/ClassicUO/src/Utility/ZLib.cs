#region license
// Copyright (C) 2020 ClassicUO Development Community on Github
// 
// This project is an alternative client for the game Ultima Online.
// The goal of this is to develop a lightweight client considering
// new technologies.
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.IO;
using System.Runtime.InteropServices;
using zlib;

namespace ClassicUO.Utility
{
    internal static class ZLib
    {
        public static void Decompress(IntPtr source, int sourceLength, int offset, IntPtr dest, int length)
        {
            var byteArray = new byte[sourceLength];
            System.Runtime.InteropServices.Marshal.Copy(source, byteArray, 0, sourceLength);
            //uncompress(dest, ref length, source, sourceLength - offset);
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream))
            using (Stream inMemoryStream = new MemoryStream(byteArray))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                var outData = outMemoryStream.ToArray();
                Marshal.Copy(outData,0,dest,outData.Length);
                length = outData.Length;
            }
        }

        public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        } 

        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [DllImport("zlib")]
        #else
        [DllImport("libz")]
        #endif
        private static extern ZLibError uncompress(IntPtr dest, ref int destLen, IntPtr source, int sourceLen);

        private enum ZLibError
        {
            VersionError = -6,
            BufferError = -5,
            MemoryError = -4,
            DataError = -3,
            StreamError = -2,
            FileError = -1,
            Okay = 0,
            StreamEnd = 1,
            NeedDictionary = 2
        }
    }
}