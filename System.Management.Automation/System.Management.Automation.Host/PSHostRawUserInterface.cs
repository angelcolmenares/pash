namespace System.Management.Automation.Host
{
    using System;
    using System.Management.Automation;

    public abstract class PSHostRawUserInterface
    {
        private const string AllNullOrEmptyStringsErrorTemplateResource = "AllNullOrEmptyStringsErrorTemplate";
        private const string BufferCellLengthErrorTemplateResource = "BufferCellLengthErrorTemplate";
        private const string NonPositiveNumberErrorTemplateResource = "NonPositiveNumberErrorTemplate";
        private const string StringsBaseName = "MshHostRawUserInterfaceStrings";

        protected PSHostRawUserInterface()
        {
        }

        public abstract void FlushInputBuffer();
        public abstract BufferCell[,] GetBufferContents(Rectangle rectangle);
        public virtual int LengthInBufferCells(char source)
        {
            return 1;
        }

        public virtual int LengthInBufferCells(string source)
        {
            if (source == null)
            {
                throw PSTraceSource.NewArgumentNullException("source");
            }
            return source.Length;
        }

        public virtual int LengthInBufferCells(string source, int offset)
        {
            if (source == null)
            {
                throw PSTraceSource.NewArgumentNullException("source");
            }
            string str = source.Substring(offset);
            return this.LengthInBufferCells(str);
        }

        public BufferCell[,] NewBufferCellArray(Size size, BufferCell contents)
        {
            return this.NewBufferCellArray(size.Width, size.Height, contents);
        }

		public abstract void Clear (int code);

        public BufferCell[,] NewBufferCellArray(int width, int height, BufferCell contents)
        {
            if (width <= 0)
            {
                throw PSTraceSource.NewArgumentOutOfRangeException("width", width, "MshHostRawUserInterfaceStrings", "NonPositiveNumberErrorTemplate", new object[] { "width" });
            }
            if (height <= 0)
            {
                throw PSTraceSource.NewArgumentOutOfRangeException("height", height, "MshHostRawUserInterfaceStrings", "NonPositiveNumberErrorTemplate", new object[] { "height" });
            }
            BufferCell[,] cellArray = new BufferCell[height, width];
            switch (this.LengthInBufferCells(contents.Character))
            {
                case 1:
                    for (int i = 0; i < cellArray.GetLength(0); i++)
                    {
                        for (int j = 0; j < cellArray.GetLength(1); j++)
                        {
                            cellArray[i, j] = contents;
                            cellArray[i, j].BufferCellType = BufferCellType.Complete;
                        }
                    }
                    return cellArray;

                case 2:
                {
                    int num4 = ((width % 2) == 0) ? width : (width - 1);
                    for (int k = 0; k < height; k++)
                    {
                        for (int m = 0; m < num4; m++)
                        {
                            cellArray[k, m] = contents;
                            cellArray[k, m].BufferCellType = BufferCellType.Leading;
                            m++;
                            cellArray[k, m] = new BufferCell('\0', contents.ForegroundColor, contents.BackgroundColor, BufferCellType.Trailing);
                        }
                        if (num4 < width)
                        {
                            cellArray[k, num4] = contents;
                            cellArray[k, num4].BufferCellType = BufferCellType.Leading;
                        }
                    }
                    break;
                }
            }
            return cellArray;
        }

        public BufferCell[,] NewBufferCellArray(string[] contents, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            if (contents == null)
            {
                throw PSTraceSource.NewArgumentNullException("contents");
            }
            byte[][] bufferArray = new byte[contents.Length][];
            int num = 0;
            for (int i = 0; i < contents.Length; i++)
            {
                if (!string.IsNullOrEmpty(contents[i]))
                {
                    int num3 = 0;
                    bufferArray[i] = new byte[contents[i].Length];
                    for (int k = 0; k < contents[i].Length; k++)
                    {
                        bufferArray[i][k] = (byte) this.LengthInBufferCells(contents[i][k]);
                        num3 += bufferArray[i][k];
                    }
                    if (num < num3)
                    {
                        num = num3;
                    }
                }
            }
            if (num <= 0)
            {
                throw PSTraceSource.NewArgumentException("contents", "MshHostRawUserInterfaceStrings", "AllNullOrEmptyStringsErrorTemplate", new object[0]);
            }
            var cellArray = new BufferCell[contents.Length, num];
            for (int j = 0; j < contents.Length; j++)
            {
                int num6 = 0;
                int index = 0;
                while (index < contents[j].Length)
                {
                    if (bufferArray[j][index] == 1)
                    {
                        cellArray[j, num6] = new BufferCell(contents[j][index], foregroundColor, backgroundColor, BufferCellType.Complete);
                    }
                    else if (bufferArray[j][index] == 2)
                    {
                        cellArray[j, num6] = new BufferCell(contents[j][index], foregroundColor, backgroundColor, BufferCellType.Leading);
                        num6++;
                        cellArray[j, num6] = new BufferCell('\0', foregroundColor, backgroundColor, BufferCellType.Trailing);
                    }
                    index++;
                    num6++;
                }
                while (num6 < num)
                {
                    cellArray[j, num6] = new BufferCell(' ', foregroundColor, backgroundColor, BufferCellType.Complete);
                    num6++;
                }
            }
            return cellArray;
        }

        public KeyInfo ReadKey()
        {
            return this.ReadKey(ReadKeyOptions.IncludeKeyUp | ReadKeyOptions.IncludeKeyDown);
        }

        public abstract KeyInfo ReadKey(ReadKeyOptions options);
        public abstract void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill);
        public abstract void SetBufferContents(Coordinates origin, BufferCell[,] contents);
        public abstract void SetBufferContents(Rectangle rectangle, BufferCell fill);

        public abstract ConsoleColor BackgroundColor { get; set; }

        public abstract Size BufferSize { get; set; }

        public abstract Coordinates CursorPosition { get; set; }

        public abstract int CursorSize { get; set; }

        public abstract ConsoleColor ForegroundColor { get; set; }

        public abstract bool KeyAvailable { get; }

        public abstract Size MaxPhysicalWindowSize { get; }

        public abstract Size MaxWindowSize { get; }

        public abstract Coordinates WindowPosition { get; set; }

        public abstract Size WindowSize { get; set; }

        public abstract string WindowTitle { get; set; }
    }
}

