namespace BookmarksHandler.Firefox.Base
{
    /// <summary>
    /// Firefox 50+ database url hash
    /// This code is a C# port of :
    /// C version https://github.com/bencaradocdavies/sqlite-mozilla-url-hash
    /// Python version https://gist.github.com/boppreh/a9737acb2abf015e6e828277b40efe71
    /// Thanks to bencaradocdavies and boppreh.
    /// </summary>
    internal static class FfHash
    {
        private const uint magicConst = 2654435769;

        private static uint RotateLeft5(uint value) =>
            (value << 5) | (value >> 27) & uint.MaxValue;

        private static uint AddToHash(uint hash, uint value) =>
            (magicConst * (RotateLeft5(hash) ^ value)) & uint.MaxValue;

        private static uint Hash(string url)
        {
            uint hash = 0;
            foreach (char c in url)
                hash = AddToHash(hash, c);
            return hash;
        }
        internal static long Get(string url) =>
            ((long)(Hash(url.Substring(0, url.IndexOf(':'))) & 65535) << 32) + Hash(url);
    }
}
