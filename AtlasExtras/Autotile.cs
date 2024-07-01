namespace AtlasExtras;

public interface IAutotile {

    public bool CheckTile(int x, int y);
}

public static class Autotile {
    public static byte GetState(this IAutotile self, int x, int y) {
        bool[] bits = [
            self.CheckTile(x - 1, y - 1),
            self.CheckTile(x, y - 1),
            self.CheckTile(x + 1, y - 1),
            self.CheckTile(x - 1, y),
            self.CheckTile(x + 1, y),
            self.CheckTile(x - 1, y + 1),
            self.CheckTile(x, y + 1),
            self.CheckTile(x + 1, y + 1)
        ];

        for (byte i = 0; i < Adjacencies.Length/2; i++) {
            byte adjMask = Adjacencies[i * 2];
            byte ignoreMask = Adjacencies[i * 2 + 1];

            bool success = true;
            for (int j = 0; j < bits.Length; j++) {
                int bitIndex = 1 << (7-j);
                
                if ((ignoreMask & bitIndex) != 0) continue;
                if (((adjMask & bitIndex) != 0) != bits[j]) {
                    success = false;
                    break;
                }
            }

            if (success) return i;
        }
        
        throw new Exception($"Wang autotile failed at: {x}, {y}");
    }
    
    internal static readonly byte[] Adjacencies = [
        /*  0 */ 0b_00001011, 0b_10100100,               //    ADJACENCY DIAGRAM
        /*  1 */ 0b_00011111, 0b_10100000,               //        ┊       ┊       
        /*  2 */ 0b_00010110, 0b_10100001,               //    0   ┊   1   ┊   2    
        /*  3 */ 0b_00000010, 0b_10100101,               //        ┊       ┊        
        /*  4 */ 0b_00001010, 0b_10100100,               // ┈┈┈┈┈┈┈┼┈┈┈┈┈┈┈┼┈┈┈┈┈┈┈
        /*  5 */ 0b_00011110, 0b_10100000,               //        ┊  YOU  ┊       
        /*  6 */ 0b_00011011, 0b_10100000,               //    3   ┊  ARE  ┊   4   
        /*  7 */ 0b_00010010, 0b_10100001,               //        ┊ HERE  ┊       
        /*  8 */ 0b_00011010, 0b_10100000,               // ┈┈┈┈┈┈┈┼┈┈┈┈┈┈┈┼┈┈┈┈┈┈┈
        /*  9 */ 0b_11011011, 0b_00000000,               //        ┊       ┊       
        /* 10 */ 0b_01101011, 0b_10000100,               //    5   ┊   6   ┊   7   
        /* 11 */ 0b_11111111, 0b_00000000,               //        ┊       ┊      
        /* 12 */ 0b_11010110, 0b_00100001,               // KEY: AAA_AA_AAA, BBB_BB_BBB 
        /* 13 */ 0b_01000010, 0b_10100101,               // A (Adjacency mask): 0 -> Empty,  1 -> Tile
        /* 14 */ 0b_01101010, 0b_10000100,               // B (Ignore mask):    0 -> Normal, 1 -> Act same if tile or not
        /* 15 */ 0b_11111110, 0b_00000000,               
        /* 16 */ 0b_11111011, 0b_00000000,
        /* 17 */ 0b_11010010, 0b_00100001,
        /* 18 */ 0b_11111010, 0b_00000000,
        /* 19 */ 0b_01111110, 0b_00000000,
        /* 20 */ 0b_01101000, 0b_10000101,
        /* 21 */ 0b_11111000, 0b_00000101,
        /* 22 */ 0b_11010000, 0b_00100101,
        /* 23 */ 0b_01000000, 0b_10100101,
        /* 24 */ 0b_01001010, 0b_10000101,
        /* 25 */ 0b_11011111, 0b_00000000,
        /* 26 */ 0b_01111111, 0b_00000000,
        /* 27 */ 0b_01010110, 0b_00100001,
        /* 28 */ 0b_01011111, 0b_00000000,
        /* 29 */ 0b_01011011, 0b_00000000,
        /* 20 */ 0b_01011110, 0b_00000000,
        /* 31 */ 0b_00001000, 0b_10100101,
        /* 32 */ 0b_00011000, 0b_10100101,
        /* 33 */ 0b_00010000, 0b_10100101,
        /* 34 */ 0b_00000000, 0b_10100101,
        /* 35 */ 0b_01001000, 0b_10000101,
        /* 36 */ 0b_11011000, 0b_00000101,
        /* 37 */ 0b_01111000, 0b_00000101,
        /* 38 */ 0b_01010000, 0b_00100101,
        /* 39 */ 0b_01011000, 0b_00000101,
        /* 40 */ 0b_01111010, 0b_00000000,
        /* 41 */ 0b_11011010, 0b_00000000,
        /* 42 */ 0b_01001010, 0b_10000100,
        /* 43 */ 0b_11011110, 0b_00000000,
        /* 44 */ 0b_01111011, 0b_00000000,
        /* 45 */ 0b_01010010, 0b_00100001,
        /* 46 */ 0b_01011010, 0b_00000000,
    ];
}