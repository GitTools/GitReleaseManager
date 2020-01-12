public bool FileHashEquals(FilePath y, FilePath x)
{
    using (var sha512 = System.Security.Cryptography.SHA512.Create())
    using (System.IO.Stream
        yStream = System.IO.File.OpenRead(y.FullPath),
        xStream = System.IO.File.OpenRead(x.FullPath))
    {
        var yHash = sha512.ComputeHash(yStream);
        var xHash = sha512.ComputeHash(xStream);
        return Enumerable.SequenceEqual(yHash, xHash);
    }
}
