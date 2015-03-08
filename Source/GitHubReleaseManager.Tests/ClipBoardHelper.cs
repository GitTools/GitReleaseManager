//-----------------------------------------------------------------------
// <copyright file="ClipBoardHelper.cs" company="gep13">
//     Copyright (c) gep13. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace GitHubReleaseManager.Tests
{
    using System.Threading;
    using System.Windows.Forms;

    public class ClipBoardHelper
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