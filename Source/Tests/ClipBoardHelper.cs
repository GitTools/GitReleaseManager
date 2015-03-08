namespace ReleaseNotesCompiler.Tests
{
    using System.Threading;
    using System.Windows.Forms;

    class ClipBoardHelper
    {
        public static void SetClipboard(string result)
        {
            var thread = new Thread(() => Clipboard.SetText(result));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
    }
}
