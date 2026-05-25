namespace UrlShortener.Infrastructure.Utilities
{
    // Convierte un ID entero a un string corto en base 62 (0-9, a-z, A-Z) para generar los
    // short codes de las URLs. Al usar 62 caracteres, un ID de 7 dígitos decimal se representa
    // en 4-5 caracteres. La codificación usa un buffer en el stack (Span<char>) para evitar
    // allocations en el heap y mantener el método eficiente bajo alta carga.
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
