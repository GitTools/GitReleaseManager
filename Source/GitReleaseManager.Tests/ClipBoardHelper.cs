//-----------------------------------------------------------------------
// <copyright file="ClipBoardHelper.cs" company="GitTools Contributors">
//     Copyright (c) 2015 - Present - GitTools Contributors
// </copyright>
//-----------------------------------------------------------------------

namespace GitReleaseManager.Tests
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