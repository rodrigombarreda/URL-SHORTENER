namespace UrlShortener.Infrastructure.Utilities
{
    public class Base62Converter
    {
        private const string Base62Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string Encode(int id)
        {
            if (id == 0) return "0";

            string result = "";
            while (id > 0)
            {
                result = Base62Chars[id % 62] + result;
                id /= 62;
            }
            return result;
        }
    }
}
