namespace GitReleaseManager.IntegrationTests
{
    using System.Threading;
    using TextCopy;

    public static class ClipBoardHelper
    {
        public static void SetClipboard(string result)
        {
            var thread = new Thread(() => new Clipboard().SetText(result));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
    }
}