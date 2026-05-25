namespace UrlShortener.Infrastructure.Utilities
{
    public class Base62Converter
    {
        private const string Base62Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string Encode(int id)
        {
            if (id == 0) return "0";

            Span<char> buffer = stackalloc char[10];
            int pos = buffer.Length;
            while (id > 0)
            {
                buffer[--pos] = Base62Chars[id % 62];
                id /= 62;
            }
            return new string(buffer[pos..]);
        }
    }
}
