namespace Microsoft.ActiveDirectory.Management
{
    using System;
    using System.IO;

    internal class Yylex
    {
        private static int[] accept_dispatch = new int[] { 
            0, -1, -1, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 
            0x10, 0x11, 0x12, 0x13, 20, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, -1, 0x1f, 
            0x20, 0x21, 0x22, 0x23, -1, 0x25, 0x26, -1, 40, 0x29, -1, 0x2b, -1, 0x2d, -1, 0x2f, 
            -1, 0x31, -1, 0x33, -1, 0x35, -1, 0x37, -1, 0x39, 0x3a, 0x3b, 60, 0x3d, 0x3e, 0x3f, 
            0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 70, 0x47, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 
            80
         };
        private static int[] yy_acpt = new int[] { 
            4, 4, 0, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 
            4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 0, 4, 
            4, 4, 4, 4, 0, 4, 4, 0, 4, 4, 0, 4, 0, 4, 0, 4, 
            0, 4, 0, 4, 0, 4, 0, 4, 0, 4, 4, 4, 4, 4, 4, 4, 
            4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 
            4
         };
        private bool yy_at_bol;
        private const int YY_BOL = 0x80;
        private char[] yy_buffer;
        private int yy_buffer_end;
        private int yy_buffer_index;
        private int yy_buffer_read;
        private const int YY_BUFFER_SIZE = 0x200;
        private int yy_buffer_start;
        private static int[] yy_cmap = new int[] { 
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 
            0x27, 40, 3, 2, 0x22, 2, 40, 1, 0x20, 0x21, 2, 9, 2, 5, 7, 2, 
            10, 6, 6, 6, 6, 6, 6, 6, 6, 6, 2, 2, 40, 40, 40, 2, 
            2, 0x15, 0x18, 0x1a, 0x19, 8, 12, 15, 0x1f, 0x12, 0x23, 0x13, 13, 30, 0x11, 20, 
            0x16, 0x10, 0x17, 0x1c, 14, 0x1b, 0x1d, 0x23, 11, 0x23, 0x23, 2, 2, 2, 2, 0x24, 
            4, 0x15, 0x18, 0x1a, 0x19, 8, 12, 15, 0x1f, 0x12, 0x23, 0x13, 13, 30, 0x11, 20, 
            0x16, 0x10, 0x17, 0x1c, 14, 0x1b, 0x1d, 0x23, 11, 0x23, 0x23, 0x25, 40, 0x26, 2, 2, 
            0, 0
         };
        private const int YY_E_INTERNAL = 0;
        private const int YY_E_MATCH = 1;
        private const int YY_END = 2;
        private const int YY_EOF = 0x81;
        private static string[] yy_error_string = new string[] { "Error: Internal error.\n", "Error: Unmatched input.\n" };
        private const int YY_F = -1;
        private int yy_lexical_state;
        private const int YY_NO_ANCHOR = 4;
        private const int YY_NO_STATE = -1;
        private const int YY_NOT_ACCEPT = 0;
        private static int[,] yy_nxt = new int[,] { 
            { 
                1, 2, -1, 30, -1, 0x24, 3, -1, 4, -1, 0x1f, 4, 4, 4, 4, 4, 
                4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 
                5, 6, 7, 4, -1, -1, -1, 8, 0x1d
             }, { 
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, -1, -1, -1
             }, { 
                -1, 9, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 
                2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 
                2, 2, 2, 2, 2, 2, 2, 2, 2
             }, { 
                -1, -1, -1, -1, -1, -1, 3, 0x2a, 0x2c, -1, 3, -1, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 4, 4, -1, 4, -1, 4, 4, 4, 4, 4, 4, 
                4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 
                -1, -1, -1, 4, 4, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, -1, 0x20, 0x30, 0x20, -1, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 
                0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 
                -1, -1, -1, 0x20, 0x20, 50, -1, -1, -1
             }, { 
                -1, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                12, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, -1, 0x13, -1, 0x13, -1, 0x13, -1, 0x13, -1, -1, -1, 
                -1, -1, -1, -1, -1, 0x13, -1, -1, 0x13, 0x13, 0x13, -1, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, -1, -1, -1
             }, { 
                -1, 50, 50, 50, 0x36, 50, 50, 0x38, 50, 50, 50, 50, 50, 50, 50, 50, 
                50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 
                50, 50, 50, 50, 50, -1, 20, 50, 50
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x44, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, -1, -1, 0x1d
             }, { 
                -1, 30, 30, 10, 0x27, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 
                30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 
                30, 30, 30, 30, 30, 30, 30, 30, 30
             }, { 
                -1, -1, -1, -1, -1, -1, 3, 0x2a, 0x2c, -1, 3, 0x2e, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, -1, 0x20, 0x30, 0x20, -1, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 
                0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 
                -1, -1, -1, 0x20, 0x20, -1, -1, -1, -1
             }, 
            { 
                -1, 50, 50, 50, 0x36, 50, 0x23, 0x38, 0x23, 50, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 
                0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 
                50, 50, 50, 0x23, 0x23, -1, 20, 50, 50
             }, { 
                -1, -1, -1, -1, -1, -1, 3, -1, 11, -1, 3, 0x22, 0x22, 0x26, 0x22, 0x29, 
                0x22, 0x2b, 0x22, 0x22, 0x2d, 60, 0x22, 80, 0x3d, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, -1, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, -1, 0x25, -1, 0x2c, -1, 0x25, -1, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 13, -1, 0x22, 0x22, 0x22, 0x22, 14, 0x22, 
                0x22, 0x22, 0x3e, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, 30, 30, 0x21, 0x27, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 
                30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 
                30, 30, 30, 30, 30, 30, 30, 30, 30
             }, { 
                -1, -1, -1, -1, -1, -1, 40, -1, -1, -1, 40, -1, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 15, -1, 0x22, 0x22, 0x22, 0x22, 0x10, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, -1, 0x25, -1, -1, -1, 0x25, -1, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x11, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x2f, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x34, 40, -1, -1, 0x34, 40, -1, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x12, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x15, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, -1, 0x20, -1, 0x20, -1, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 
                0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 
                -1, -1, -1, 0x20, 0x20, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x16, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, 50, 50, 50, 0x36, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 
                50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 
                50, 50, 50, 50, 50, -1, 20, 50, 50
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x17, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, 
            { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x18, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, 50, 50, 50, 0x36, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 
                50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 
                50, 50, 50, 50, 50, 50, 20, 50, 50
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x19, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, 50, 50, 50, 0x36, 50, 0x23, 50, 0x23, 50, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 
                0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 0x23, 
                50, 50, 50, 0x23, 0x23, -1, 20, 50, 50
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x1a, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x1b, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x1c, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x31, 0x22, 0x22, 0x22, 0x22, 70, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x33, 0x3f, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x35, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x37, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x39, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x3a, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x3b, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x40, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x41, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, 
            { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x42, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x43, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x45, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x47, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x48, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x49, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x4a, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x4b, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x4c, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x4d, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x22, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x4e, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }, { 
                -1, -1, -1, -1, -1, 0x22, 0x22, -1, 0x4f, -1, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 0x22, 
                -1, -1, -1, 0x22, 0x22, -1, -1, -1, -1
             }
         };
        private TextReader yy_reader;
        private static int[] yy_rmap = new int[] { 
            0, 1, 2, 3, 4, 1, 1, 5, 1, 6, 1, 7, 8, 8, 8, 8, 
            8, 8, 8, 9, 10, 11, 8, 8, 8, 8, 8, 8, 8, 12, 13, 14, 
            15, 13, 8, 0x10, 0x11, 0x12, 0x13, 20, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 9, 0x1b, 
            0x1c, 0x1d, 30, 0x1f, 0x15, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 40, 0x29, 0x2a, 
            0x2b, 0x2c, 0x2d, 0x2e, 0x2f, 0x30, 0x31, 50, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 
            0x3b
         };
        private const int YY_START = 1;
        private static int[] yy_state_dtrans = new int[1];
        private int yychar;
        private const int YYINITIAL = 0;

        private Yylex()
        {
            this.yy_buffer = new char[0x200];
            this.yy_buffer_read = 0;
            this.yy_buffer_index = 0;
            this.yy_buffer_start = 0;
            this.yy_buffer_end = 0;
            this.yychar = 0;
            this.yy_at_bol = true;
            this.yy_lexical_state = 0;
        }

        public Yylex(FileStream instream) : this()
        {
            if (instream == null)
            {
                throw new ADException("Error: Bad input stream initializer.");
            }
            this.yy_reader = new StreamReader(instream);
        }

        public Yylex(TextReader reader) : this()
        {
            if (reader == null)
            {
                throw new ADException("Error: Bad input stream initializer.");
            }
            this.yy_reader = reader;
        }

        private object Accept(int rule)
        {
            switch (rule)
            {
                case 0:
                    return this.Accept_0();

                case 3:
                    return this.Accept_3();

                case 4:
                    return this.Accept_4();

                case 5:
                    return this.Accept_5();

                case 6:
                    return this.Accept_6();

                case 7:
                    return this.Accept_7();

                case 8:
                    return this.Accept_8();

                case 9:
                    return this.Accept_9();

                case 10:
                    return this.Accept_10();

                case 11:
                    return this.Accept_11();

                case 12:
                    return this.Accept_12();

                case 13:
                    return this.Accept_13();

                case 14:
                    return this.Accept_14();

                case 15:
                    return this.Accept_15();

                case 0x10:
                    return this.Accept_16();

                case 0x11:
                    return this.Accept_17();

                case 0x12:
                    return this.Accept_18();

                case 0x13:
                    return this.Accept_19();

                case 20:
                    return this.Accept_20();

                case 0x15:
                    return this.Accept_21();

                case 0x16:
                    return this.Accept_22();

                case 0x17:
                    return this.Accept_23();

                case 0x18:
                    return this.Accept_24();

                case 0x19:
                    return this.Accept_25();

                case 0x1a:
                    return this.Accept_26();

                case 0x1b:
                    return this.Accept_27();

                case 0x1c:
                    return this.Accept_28();

                case 0x1d:
                    return this.Accept_29();

                case 0x1f:
                    return this.Accept_31();

                case 0x20:
                    return this.Accept_32();

                case 0x21:
                    return this.Accept_33();

                case 0x22:
                    return this.Accept_34();

                case 0x23:
                    return this.Accept_35();

                case 0x25:
                    return this.Accept_37();

                case 0x26:
                    return this.Accept_38();

                case 40:
                    return this.Accept_40();

                case 0x29:
                    return this.Accept_41();

                case 0x2b:
                    return this.Accept_43();

                case 0x2d:
                    return this.Accept_45();

                case 0x2f:
                    return this.Accept_47();

                case 0x31:
                    return this.Accept_49();

                case 0x33:
                    return this.Accept_51();

                case 0x35:
                    return this.Accept_53();

                case 0x37:
                    return this.Accept_55();

                case 0x39:
                    return this.Accept_57();

                case 0x3a:
                    return this.Accept_58();

                case 0x3b:
                    return this.Accept_59();

                case 60:
                    return this.Accept_60();

                case 0x3d:
                    return this.Accept_61();

                case 0x3e:
                    return this.Accept_62();

                case 0x3f:
                    return this.Accept_63();

                case 0x40:
                    return this.Accept_64();

                case 0x41:
                    return this.Accept_65();

                case 0x42:
                    return this.Accept_66();

                case 0x43:
                    return this.Accept_67();

                case 0x44:
                    return this.Accept_68();

                case 0x45:
                    return this.Accept_69();

                case 70:
                    return this.Accept_70();

                case 0x47:
                    return this.Accept_71();

                case 0x48:
                    return this.Accept_72();

                case 0x49:
                    return this.Accept_73();

                case 0x4a:
                    return this.Accept_74();

                case 0x4b:
                    return this.Accept_75();

                case 0x4c:
                    return this.Accept_76();

                case 0x4d:
                    return this.Accept_77();

                case 0x4e:
                    return this.Accept_78();

                case 0x4f:
                    return this.Accept_79();

                case 80:
                    return this.Accept_80();
            }
            return null;
        }

        private object Accept_0()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_10()
        {
            return QueryParser.STRVAL2;
        }

        private object Accept_11()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_12()
        {
            return QueryParser.OP_EQ;
        }

        private object Accept_13()
        {
            return QueryParser.OP_LE;
        }

        private object Accept_14()
        {
            return QueryParser.OP_LT;
        }

        private object Accept_15()
        {
            return QueryParser.OP_GE;
        }

        private object Accept_16()
        {
            return QueryParser.OP_GT;
        }

        private object Accept_17()
        {
            return QueryParser.OP_NE;
        }

        private object Accept_18()
        {
            return QueryParser.OR;
        }

        private object Accept_19()
        {
            return QueryParser.HEXNUMVAL;
        }

        private object Accept_20()
        {
            return QueryParser.VAR2;
        }

        private object Accept_21()
        {
            return QueryParser.NOT;
        }

        private object Accept_22()
        {
            return QueryParser.AND;
        }

        private object Accept_23()
        {
            return QueryParser.OP_BOR;
        }

        private object Accept_24()
        {
            return QueryParser.OP_LIKE;
        }

        private object Accept_25()
        {
            return QueryParser.OP_BAND;
        }

        private object Accept_26()
        {
            return QueryParser.OP_APPROX;
        }

        private object Accept_27()
        {
            return QueryParser.OP_NOTLIKE;
        }

        private object Accept_28()
        {
            return QueryParser.OP_RECURSIVEMATCH;
        }

        private object Accept_29()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_3()
        {
            return QueryParser.NUMVAL;
        }

        private object Accept_31()
        {
            return QueryParser.NUMVAL;
        }

        private object Accept_32()
        {
            return QueryParser.VAR1;
        }

        private object Accept_33()
        {
            return QueryParser.STRVAL2;
        }

        private object Accept_34()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_35()
        {
            return QueryParser.VAR2;
        }

        private object Accept_37()
        {
            return QueryParser.NUMVAL;
        }

        private object Accept_38()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_4()
        {
            return QueryParser.PROP;
        }

        private object Accept_40()
        {
            return QueryParser.NUMVAL;
        }

        private object Accept_41()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_43()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_45()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_47()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_49()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_5()
        {
            return QueryParser.LPAREN;
        }

        private object Accept_51()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_53()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_55()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_57()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_58()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_59()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_6()
        {
            return QueryParser.RPAREN;
        }

        private object Accept_60()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_61()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_62()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_63()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_64()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_65()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_66()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_67()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_68()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_69()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_7()
        {
            return QueryParser.VAR1;
        }

        private object Accept_70()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_71()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_72()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_73()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_74()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_75()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_76()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_77()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_78()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_79()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_8()
        {
            return null;
        }

        private object Accept_80()
        {
            return QueryParser.OP_NOTSUP;
        }

        private object Accept_9()
        {
            return QueryParser.STRVAL1;
        }

        private char yy_advance()
        {
            if (this.yy_buffer_index >= this.yy_buffer_read)
            {
                int num;
                if (this.yy_buffer_start != 0)
                {
                    int index = this.yy_buffer_start;
                    int num3 = 0;
                    while (index < this.yy_buffer_read)
                    {
                        this.yy_buffer[num3] = this.yy_buffer[index];
                        index++;
                        num3++;
                    }
                    this.yy_buffer_end -= this.yy_buffer_start;
                    this.yy_buffer_start = 0;
                    this.yy_buffer_read = num3;
                    this.yy_buffer_index = num3;
                    num = this.yy_reader.Read(this.yy_buffer, this.yy_buffer_read, this.yy_buffer.Length - this.yy_buffer_read);
                    if (num <= 0)
                    {
                        return '\x0081';
                    }
                    this.yy_buffer_read += num;
                }
                while (this.yy_buffer_index >= this.yy_buffer_read)
                {
                    if (this.yy_buffer_index >= this.yy_buffer.Length)
                    {
                        this.yy_buffer = this.yy_double(this.yy_buffer);
                    }
                    num = this.yy_reader.Read(this.yy_buffer, this.yy_buffer_read, this.yy_buffer.Length - this.yy_buffer_read);
                    if (num <= 0)
                    {
                        return '\x0081';
                    }
                    this.yy_buffer_read += num;
                }
            }
            return yy_translate.translate(this.yy_buffer[this.yy_buffer_index++]);
        }

        internal int yy_char()
        {
            return this.yychar;
        }

        private char[] yy_double(char[] buf)
        {
            char[] chArray = new char[2 * buf.Length];
            for (int i = 0; i < buf.Length; i++)
            {
                chArray[i] = buf[i];
            }
            return chArray;
        }

        private void yy_error(int code, bool fatal)
        {
            if (fatal)
            {
                throw new ADException("Fatal Error.\n");
            }
            throw new ADException(yy_error_string[code]);
        }

        private void yy_mark_end()
        {
            this.yy_buffer_end = this.yy_buffer_index;
        }

        private void yy_mark_start()
        {
            this.yychar = (this.yychar + this.yy_buffer_index) - this.yy_buffer_start;
            this.yy_buffer_start = this.yy_buffer_index;
        }

        private void yy_move_end()
        {
            if ((this.yy_buffer_end > this.yy_buffer_start) && ('\n' == this.yy_buffer[this.yy_buffer_end - 1]))
            {
                this.yy_buffer_end--;
            }
            if ((this.yy_buffer_end > this.yy_buffer_start) && ('\r' == this.yy_buffer[this.yy_buffer_end - 1]))
            {
                this.yy_buffer_end--;
            }
        }

        private void yy_to_mark()
        {
            this.yy_buffer_index = this.yy_buffer_end;
            this.yy_at_bol = (this.yy_buffer_end > this.yy_buffer_start) && ((this.yy_buffer[this.yy_buffer_end - 1] == '\r') || (this.yy_buffer[this.yy_buffer_end - 1] == '\n'));
        }

        private void yybegin(int state)
        {
            this.yy_lexical_state = state;
        }

        private int yylength()
        {
            return (this.yy_buffer_end - this.yy_buffer_start);
        }

        public object yylex()
        {
            int num = 4;
            int index = yy_state_dtrans[this.yy_lexical_state];
            int num3 = -1;
            int num4 = -1;
            bool flag = true;
            this.yy_mark_start();
            if (yy_acpt[index] != 0)
            {
                num4 = index;
                this.yy_mark_end();
            }
            while (true)
            {
                char ch;
                if (flag && this.yy_at_bol)
                {
                    ch = '\x0080';
                }
                else
                {
                    ch = this.yy_advance();
                }
                num3 = yy_nxt[yy_rmap[index], yy_cmap[ch]];
                if (('\x0081' == ch) && flag)
                {
                    return null;
                }
                if (-1 != num3)
                {
                    index = num3;
                    flag = false;
                    if (yy_acpt[index] != 0)
                    {
                        num4 = index;
                        this.yy_mark_end();
                    }
                }
                else
                {
                    if (-1 == num4)
                    {
                        throw new ADException("Lexical Error: Unmatched Input.");
                    }
                    num = yy_acpt[num4];
                    if ((2 & num) != 0)
                    {
                        this.yy_move_end();
                    }
                    this.yy_to_mark();
                    if (num4 < 0)
                    {
                        if (num4 < 0x51)
                        {
                            this.yy_error(0, false);
                        }
                    }
                    else
                    {
                        int rule = accept_dispatch[num4];
                        if (rule != -1)
                        {
                            object obj2 = this.Accept(rule);
                            if (obj2 != null)
                            {
                                return obj2;
                            }
                        }
                    }
                    flag = true;
                    index = yy_state_dtrans[this.yy_lexical_state];
                    num3 = -1;
                    num4 = -1;
                    this.yy_mark_start();
                    if (yy_acpt[index] != 0)
                    {
                        num4 = index;
                        this.yy_mark_end();
                    }
                }
            }
        }

        internal string yytext()
        {
            return new string(this.yy_buffer, this.yy_buffer_start, this.yy_buffer_end - this.yy_buffer_start);
        }
    }
}

