namespace System;

public static class UriHelper
{
    #region Utilities

    private static Uri Concat(this Uri uri, string path)
    {
        return new Uri(new Uri(uri.AbsoluteUri.TrimEnd('/') + "/"), path.TrimStart('/'));
    }

    #endregion

    #region Methods

    public static Uri Concat(this Uri uri, params string[] paths)
    {
        foreach (var path in paths)
            uri = Concat(uri, path);

        return uri;
    }

    #endregion
}
