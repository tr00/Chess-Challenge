using System;
using System.Numerics;

class DecimalPacker {

public static byte[] DecimalsToBytes(decimal[] data) {
    var buffer = new byte[data.Length * 12];

    int offset = 0;

    foreach (var dec in data)
        Buffer.BlockCopy(decimal.GetBits(dec), 0,
            buffer, 12 * offset++, 12);

    return buffer;
}

public static decimal[] BytesToDecimals(byte[] data) {
    
    if (data.Length % 12 != 0)
        throw new ArgumentException("pad byte[] to length of 12 pls");

    var buffer = new decimal[data.Length / 12];

    int offset = 0;

    int[] bits = new int[4];
    bits[3] = 0;

    for (int i = 0; i < data.Length; i += 12) {
        Buffer.BlockCopy(data, i, bits, 0, 12);

        buffer[offset++] = new decimal(bits);
    }

    return buffer;
}

    public static void Main() {
        byte[] bytes = { 
            0x00, 0x11, 0x22, 0x33, 
            0x44, 0x55, 0x66, 0x77,
            0x88, 0x99, 0xaa, 0xbb,

            0x88, 0x99, 0xaa, 0xbb,
            0x44, 0x55, 0x66, 0x77,
            0x00, 0x11, 0x22, 0x33, 
        };

        var encoded = BytesToDecimals(bytes);
        var decoded = DecimalsToBytes(encoded);

        Console.Write("source: ");
        foreach (var value in decoded)
            Console.Write("{0:x2} ", value);

        Console.WriteLine();

        Console.Write("decode: ");
        foreach (var value in decoded)
            Console.Write("{0:x2} ", value);

        Console.WriteLine();

        Console.Write("encode:\n{\n");
        foreach(var value in encoded)
            Console.WriteLine("\t{0}m,", value);

        Console.WriteLine("}");
        
        for (int i = 0; i < bytes.Length; ++i)
            if (bytes[i] != decoded[i])
                throw new ArgumentException("test failed!!");

    }

}
