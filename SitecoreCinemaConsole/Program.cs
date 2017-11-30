using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SitecoreCinemaConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Generating Model......");
            var model = SitecoreCinema.Model.Collection.SitecoreCinemaModel.Model;

            var seralizedModel = Sitecore.XConnect.Serialization.XdbModelWriter.Serialize(model);

            File.WriteAllText("d:\\" + model.FullName + ".json", seralizedModel);

            System.Console.WriteLine("Press any key to continue! Your model is here: " + "d:\\" + model.FullName + ".json");
            System.Console.ReadKey();
        }
    }
}
