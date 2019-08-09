using System;

namespace BookmarksManager.Firefox.Base
{
    internal static class FfTimestamp
    {
        /// <summary>
        /// Return timestamp in Unix Epoch format (microsecond)
        /// </summary>
        static internal long Get() =>
            (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds * 1000;
    }
}
