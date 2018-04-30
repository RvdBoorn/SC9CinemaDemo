using System.IO;

namespace SitecoreCinemaConsole
{
	class Program
    {
		static void Main(string[] args)
        {
            System.Console.WriteLine("Generating Model......");
			var model = SitecoreCinema.Models.Model.Collection.SitecoreCinemaModel.Model;

            var seralizedModel = Sitecore.XConnect.Serialization.XdbModelWriter.Serialize(model);

            File.WriteAllText("C:\\" + model.FullName + ".json", seralizedModel);

            System.Console.WriteLine("Press any key to continue! Your model is here: " + "C:\\" + model.FullName + ".json");
            System.Console.ReadKey();
        }
    }
}
