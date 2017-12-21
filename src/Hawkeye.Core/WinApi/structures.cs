using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Hawkeye.WinApi
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT : IEquatable<POINT>
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static POINT FromPoint(Point pt)
        {
            return new POINT(pt.X, pt.Y);
        }

        public Point ToPoint()
        {
            return new Point(X, Y);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="Object" /> is equal to
        ///     this instance.
        /// </summary>
        /// <param name="obj">
        ///     The <see cref="Object" /> to compare with this instance.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="Object" /> is equal to this
        ///     instance; otherwise, <c>false</c> .
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is POINT && Equals((POINT) obj);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing
        ///     algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public override string ToString()
        {
            return $"{{X={X}, Y={Y}}}";
        }

        /// <inheritdoc />
        public bool Equals(POINT other)
        {
            return X == other.X && Y == other.Y;
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT : IEquatable<RECT>
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RECT" /> struct.
        /// </summary>
        /// <param name="left">The left coordinate.</param>
        /// <param name="top">The top coordinate.</param>
        /// <param name="right">The right coordinate.</param>
        /// <param name="bottom">The bottom coordinate.</param>
        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Height => Bottom - Top;

        public int Width => Right - Left;

        public Size Size => new Size(Width, Height);

        public Point Location => new Point(Left, Top);

        public Rectangle ToRectangle()
        {
            return Rectangle.FromLTRB(Left, Top, Right, Bottom);
        }

        public static RECT FromRectangle(Rectangle rectangle)
        {
            return new RECT(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
        }

        /// <inheritdoc />
        public bool Equals(RECT other)
        {
            return Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is RECT rect && Equals(rect);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Left;
                hashCode = (hashCode * 397) ^ Top;
                hashCode = (hashCode * 397) ^ Right;
                hashCode = (hashCode * 397) ^ Bottom;
                return hashCode;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MODULEENTRY32 : IEquatable<MODULEENTRY32>
    {
        public uint dwSize;
        public uint th32ModuleID;
        public uint th32ProcessID;
        public uint GlblcntUsage;
        public uint ProccntUsage;
        public IntPtr modBaseAddr;
        public uint modBaseSize;
        public IntPtr hModule;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szModule;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szExePath;

        /// <inheritdoc />
        public bool Equals(MODULEENTRY32 other)
        {
            return dwSize == other.dwSize && th32ModuleID == other.th32ModuleID &&
                   th32ProcessID == other.th32ProcessID && GlblcntUsage == other.GlblcntUsage &&
                   ProccntUsage == other.ProccntUsage && modBaseAddr.Equals(other.modBaseAddr) &&
                   modBaseSize == other.modBaseSize && hModule.Equals(other.hModule) &&
                   string.Equals(szModule, other.szModule) && string.Equals(szExePath, other.szExePath);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is MODULEENTRY32 moduleentry32 && Equals(moduleentry32);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) dwSize;
                hashCode = (hashCode * 397) ^ (int) th32ModuleID;
                hashCode = (hashCode * 397) ^ (int) th32ProcessID;
                hashCode = (hashCode * 397) ^ (int) GlblcntUsage;
                hashCode = (hashCode * 397) ^ (int) ProccntUsage;
                hashCode = (hashCode * 397) ^ modBaseAddr.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) modBaseSize;
                hashCode = (hashCode * 397) ^ hModule.GetHashCode();
                hashCode = (hashCode * 397) ^ (szModule != null ? szModule.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (szExePath != null ? szExePath.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}