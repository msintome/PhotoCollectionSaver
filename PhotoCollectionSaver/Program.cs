using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoCollectionSaver
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            if (args.Length > 0)
            {
                string firstArgument = args[0].ToLower().Trim();
                string secondArgument = null;

                // Handle cases where arguments are separated by colon.
                // Examples: /c:1234567 or /P:1234567
                if (firstArgument.Length > 2)
                {
                    secondArgument = firstArgument.Substring(3).Trim();
                    firstArgument = firstArgument.Substring(0, 2);
                }
                else if (args.Length > 1)
                    secondArgument = args[1];

                if (firstArgument == "/c")           // Configuration mode
                {
                    // TODO
                }
                else if (firstArgument == "/p")      // Preview mode
                {
                    // TODO
                }
                else if (firstArgument == "/s")      // Full-screen mode
                {
                    List<MarcusImage> images = getImagesList();
                    ShowScreenSaver(images);
                    Application.Run();
                }
                else    // Undefined argument
                {
                    MessageBox.Show("Sorry, but the command line argument \"" + firstArgument +
                        "\" is not valid.", "ScreenSaver",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else    // No arguments - treat like /c
            {
                // TODO
            }

        }

        static void ShowScreenSaver(List<MarcusImage> images)
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                ScreenSaverForm screensaver = new ScreenSaverForm(screen.Bounds);
                screensaver.Show();

                var mew = screensaver.CreateGraphics();

                Bitmap do1 = new Bitmap(images[0].filepath);
                mew.DrawImage(do1,1,1);
                
                
            }
        }

        static List<MarcusImage> getImagesList()
        {
            List<MarcusImage> retimages = new List<MarcusImage>();

            string mypath = @"D:\Users\Marcus\Pictures\Guys\mattrush";

            foreach(var filename in Directory.EnumerateFiles(mypath))
            {
                if (Path.GetExtension(filename).ToLower() == ".jpg")
                {
                    Bitmap img = new Bitmap(filename);
                    retimages.Add(new MarcusImage { filepath = filename, width = img.Width, height = img.Height });
                    img.Dispose();
                }
            }

            return retimages;
        }

    }

    

    
}
