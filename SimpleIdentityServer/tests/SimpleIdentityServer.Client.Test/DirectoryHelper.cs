using System.IO;

namespace SimpleIdentityServer.Client.Test
{
    public class DirectoryHelper
    {
        public static void DropAndCreate(string folder)
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }

            var info = Directory.CreateDirectory(folder);
        }
    }
}
