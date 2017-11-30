using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Client.WebApi;
using Sitecore.XConnect.Collection.Model;
using Sitecore.XConnect.Schema;
using Sitecore.Xdb.Common.Web;
using SitecoreCinema.Model.Collection;
using SitecoreCinemaPOS.AsciArt;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SitecoreCinemaPOS
{
    class Program
    {
        private static string xdbUrl = ConfigurationManager.AppSettings["XconnectUrl"];
        private static string customerEmail = string.Empty;
        private static Contact contact = new Contact();
        private static Guid POCChanel =        Guid.Parse("{B6C1EFF9-51E3-422F-8994-E9E72C50D7A1}");
        private static Guid RegistrationGoal = Guid.Parse("{8FFB183B-DA1A-4C74-8F3A-9729E9FCFF6A}");

        static void Main(string[] args)
        {
            System.Console.WindowWidth = 160;
            if(xdbUrl.Last() != '/') { xdbUrl = xdbUrl + "/"; }

            WriteHeader(Art.XConnect, ConsoleColor.DarkGreen);
            WriteHeader(Art.Cinema, ConsoleColor.Red);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Please enter customer name, or press enter for new customer");
            Console.ForegroundColor = ConsoleColor.Gray;
            customerEmail = Console.ReadLine().Trim(); ;

            Console.ForegroundColor = ConsoleColor.White;

            var workerTask = Task.Factory.StartNew(() => AsyncWorker(args)).Unwrap();

            try
            {

                Task.WaitAll(workerTask);
            }               
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Oh no...");
                Console.WriteLine(ex.InnerException);
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("");
            Console.WriteLine("Shutting down Sitecore Cinema POS System......");
            Console.ReadLine();
            Environment.Exit(0);

        }

        private static async Task AsyncWorker(string[] args)
        {
            CertificateWebRequestHandlerModifierOptions options = CertificateWebRequestHandlerModifierOptions.Parse(ConfigurationManager.AppSettings["CertPath"]);

            var certificateModifier = new Sitecore.Xdb.Common.Web.CertificateWebRequestHandlerModifier(options);

            List<IHttpClientModifier> clientModifiers = new List<IHttpClientModifier>();
            var timeoutClientModifier = new TimeoutHttpClientModifier(new TimeSpan(0, 0, 20));
            clientModifiers.Add(timeoutClientModifier);

            var collectionClient = new CollectionWebApiClient(new Uri(xdbUrl + "odata"), clientModifiers, new[] { certificateModifier });
            var searchClient = new SearchWebApiClient(new Uri(xdbUrl + "odata"), clientModifiers, new[] { certificateModifier });
            var configurationClient = new ConfigurationWebApiClient(new Uri(xdbUrl + "configuration"), clientModifiers, new[] { certificateModifier });

            var cfg = new XConnectClientConfiguration(new XdbRuntimeModel(SitecoreCinemaModel.Model), collectionClient, searchClient, configurationClient);

            try
            {
                await cfg.InitializeAsync();
            }
            catch (XdbModelConflictException ce)
            {
                Console.WriteLine("ERROR:" + ce.Message);
                return;
            }

            using (var client = new XConnectClient(cfg))
            {
                try
                {

                    if (customerEmail == string.Empty)
                    {
                        await CreateCustomer(client);
                    }
                    else
                    {
                        var identifier = new IdentifiedContactReference("sitecoreextranet", customerEmail);
                        contact = await client.GetAsync<Contact>(identifier, new ContactExpandOptions(PersonalInformation.DefaultFacetKey, CinemaVisitorInfo.DefaultFacetKey));


                        if (contact == null)
                        {
                            WriteMessage("Customer not found... Create new customer.", ConsoleColor.DarkRed);
                            await CreateCustomer(client);
                        }
                    }
                    int result = 0;
                    while (result != 9)
                    {
                        result = await MainApplication(client);
                    }

                }
                catch (XdbExecutionException ex)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Oh no...");
                    Console.WriteLine(ex.InnerException);
                }


            }
        }



        private static async Task<int> MainApplication(XConnectClient client)
        {
            Console.Clear();
            WriteHeader(AsciArt.Art.Cinema, ConsoleColor.Red);

            CinemaVisitorInfo visit = contact.GetFacet<CinemaVisitorInfo>();
            if (visit != null)
            {
                if (visit.FoodDiscountApplied)
                {
                    WriteMessage("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@", ConsoleColor.Yellow);
                    WriteMessage("@         Customer Has 20% Discount off food offer         @", ConsoleColor.Yellow);
                    WriteMessage("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@", ConsoleColor.Yellow);
                }
            }

            WriteMenu();

            var option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    {
                        await PickupTicket(client);
                        break;
                    }
                case "2":
                    {
                        await BuyTicket(client);
                        break;
                    }
                case "3":
                    {
                        await BuyPopcorn(client);
                        break;
                    }
                case "4":
                    {
                        await BuySweets(client);
                        break;
                    }
                case "5":
                    {
                        await GetData(client);
                        break;
                    }
                case "8":
                    {
                        await DeleteContact(client);
                        Console.Clear();
                        Main(null);
                        break;
                    }
                case "9":
                    {
                        break;
                    }
                default:
                    {
                        Console.Clear();
                        Main(null);
                        break;
                    }
            }


            return Int32.Parse(option);
        }

        private static async Task CreateCustomer(XConnectClient client)
        {
            WriteMessage("================================================", ConsoleColor.Blue);
            WriteMessage("First Name:", ConsoleColor.White);
            var first = Console.ReadLine();
            WriteMessage("Last Name:", ConsoleColor.White);
            var last = Console.ReadLine();
            WriteMessage("email:", ConsoleColor.White);
            var email = Console.ReadLine();

            if (email == string.Empty) { return; }

            customerEmail = email;

            ContactIdentifier identifier = new ContactIdentifier("sitecoreextranet", email, ContactIdentifierType.Known);
            Contact newcontact = new Contact(new ContactIdentifier[] { identifier });

            PersonalInformation personalInfo = new PersonalInformation()
            {
                FirstName = first,
                LastName = last,
            };

            EmailAddressList emailFacet = new EmailAddressList(new EmailAddress(email, true), "Work");

            client.AddContact(newcontact);
            client.SetFacet(newcontact, emailFacet);
            client.SetFacet<PersonalInformation>(newcontact, PersonalInformation.DefaultFacetKey, personalInfo);

            var offlineChannel = Guid.NewGuid(); //
            var registrationGoalId = Guid.NewGuid();

            Interaction interaction = new Interaction(newcontact, InteractionInitiator.Brand, POCChanel, String.Empty);

            interaction.Events.Add(new Goal(registrationGoalId, DateTime.UtcNow));
            client.AddInteraction(interaction);

            await client.SubmitAsync();
            WriteMessage("Contact Created... Contact ID:" + identifier.Identifier, ConsoleColor.White);

            var id = new IdentifiedContactReference("sitecoreextranet", email);
            contact = await client.GetAsync<Contact>(id, new ContactExpandOptions(PersonalInformation.DefaultFacetKey, CinemaVisitorInfo.DefaultFacetKey));

            Thread.Sleep(1000);
            return;

        }


        private static async Task PickupTicket(XConnectClient client)
        {
            var cinemaInfo = contact.GetFacet<CinemaVisitorInfo>();
            string movieName = string.Empty;
            if(cinemaInfo!=null && cinemaInfo.NumberOfReservedTickets > 0)
            {
                var identifier = new IdentifiedContactReference("sitecoreextranet", customerEmail);
                var contactAgain = await client.GetAsync<Contact>(identifier, new ContactExpandOptions( CinemaVisitorInfo.DefaultFacetKey)
                {
                    Interactions = new RelatedInteractionsExpandOptions(new string[] { CinemaInfo.DefaultFacetKey })
                    {
                        StartDateTime = DateTime.Today.AddDays(-7)
                    }
                });

                if (contactAgain.Interactions.Count() > 0)
                {
                    var allcinemainteraction = contactAgain.Interactions.Where(i => i.GetFacet<CinemaInfo>(CinemaInfo.DefaultFacetKey) != null).OrderBy(d=>d.EndDateTime);
                    foreach(var inter in allcinemainteraction)
                    {
                        foreach(var evv in inter.Events)
                        {
                            if(evv.GetType() == typeof(SitecoreCinema.Model.Collection.ReservedCinemaTicket))
                            {
                                var interactionEvent = evv as ReservedCinemaTicket;
                                movieName = interactionEvent.MovieName;
                                Console.WriteLine("Picked Up Tickets Reserved for '" + movieName + "'");
                                break;
                            }
                        }
                    }

                    int tickets = 1;
                    int reservedTickets = 0;
                    if (cinemaInfo != null)
                    {
                        tickets = cinemaInfo.NumberOfPurchasedTickets + 1;
                        reservedTickets = cinemaInfo.NumberOfReservedTickets - 1;
                    }
                    else
                    {
                        cinemaInfo = new CinemaVisitorInfo();
                    }

                    cinemaInfo.NumberOfPurchasedTickets = tickets;
                    cinemaInfo.NumberOfReservedTickets = reservedTickets;


                    var interaction = new Interaction(contact, InteractionInitiator.Contact, POCChanel, "");
                    client.SetFacet<CinemaInfo>(interaction, CinemaInfo.DefaultFacetKey, new CinemaInfo() { Cinema = "St Katharines Dock" });
                    client.SetFacet<CinemaVisitorInfo>(contact, CinemaVisitorInfo.DefaultFacetKey, cinemaInfo);
                    interaction.Events.Add(new PurchasedCinemaTicket(DateTime.UtcNow, "GBP", 9.99m) { MovieName = movieName, EngagementValue = 500 });
                    client.AddInteraction(interaction);
                    await client.SubmitAsync();

                    Console.WriteLine("Picked Up Reserved Ticket");

                }                    
            }
            else
            {
                WriteMessage("No Tickets Have Been Reserved by Customer", ConsoleColor.Red);
            }

            Console.WriteLine("Press Enter To Continue.....");
            Console.ReadLine();
        }

        private static async Task GetData(XConnectClient client)
        {
            var presonalInfo = contact.GetFacet<PersonalInformation>();
            var cinemaInfo = contact.GetFacet<CinemaVisitorInfo>();

            if (cinemaInfo == null)
            {
                cinemaInfo = new CinemaVisitorInfo()
                {
                    NumberOfReservedTickets = 0
                };
            }

            System.Console.WriteLine("Customer " + presonalInfo.FirstName + " " + presonalInfo.LastName + ", has reserved " + cinemaInfo.NumberOfReservedTickets.ToString() + " tickets");

            var identifier = new IdentifiedContactReference("sitecoreextranet", customerEmail);

            var contactAgain = await client.GetAsync<Contact>(identifier, new ContactExpandOptions(PersonalInformation.DefaultFacetKey, CinemaVisitorInfo.DefaultFacetKey)
            {
                Interactions = new RelatedInteractionsExpandOptions(new string[] { CinemaInfo.DefaultFacetKey })
                {
                    StartDateTime = DateTime.Today.AddDays(-7)
                }
            });

            if (contactAgain.Interactions != null)
            {
                foreach (var interaction in contactAgain.Interactions)
                {
                    if (interaction.GetFacet<CinemaInfo>(CinemaInfo.DefaultFacetKey) != null)
                    {
                        var cinema = interaction.GetFacet<CinemaInfo>(CinemaInfo.DefaultFacetKey).Cinema;
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine("Interaction at cinema: " + cinema);
                        Console.WriteLine("Events:");
                        Console.ForegroundColor = ConsoleColor.White;
                        foreach (var evv in interaction.Events)
                        {
                            if(evv.GetType() == typeof(SitecoreCinema.Model.Collection.PurchasedCinemaTicket))
                            {
                                var interactionEvent = evv as PurchasedCinemaTicket;                                
                                Console.WriteLine("Purchased Cinema Ticket for '" + interactionEvent.MovieName + "' on " + evv.Timestamp.ToShortDateString() + ", price: " + interactionEvent.MonetaryValue + " " + interactionEvent.CurrencyCode);
                            }
                            if (evv.GetType() == typeof(SitecoreCinema.Model.Collection.PurchasedFood))
                            {
                                var interactionEvent = evv as PurchasedFood;
                                Console.WriteLine("Purchased food '" + interactionEvent.FoodType + "' on" + evv.Timestamp.ToShortDateString() + ", price: " + interactionEvent.MonetaryValue + " " + interactionEvent.CurrencyCode);

                            }
                            if (evv.GetType() == typeof(SitecoreCinema.Model.Collection.ReservedCinemaTicket))
                            {
                                var interactionEvent = evv as ReservedCinemaTicket;
                                Console.WriteLine("Reserveed Cinema Ticket for '" + interactionEvent.MovieName + "' on" + evv.Timestamp.ToShortDateString());

                            }

                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("No Previous History");
            }


            WriteMessage("Press Enter To Return To Menu", ConsoleColor.White);
            Console.ReadLine();


            return;
        }

        private static async Task BuyTicket(XConnectClient client)
        {
            Console.Clear();
            WriteHeader(AsciArt.Art.Ticket, ConsoleColor.DarkYellow);
            WriteMessage("#####################################################################", ConsoleColor.White);
            WriteMessage("#                         Please Select Movie                       #", ConsoleColor.White);
            WriteMessage("#####################################################################", ConsoleColor.White);
            var movie = WriteMovieList();

            if (movie != null)
            {
                var cinemaVisitorInfo = contact.GetFacet<CinemaVisitorInfo>();

                int tickets = 1;
                if (cinemaVisitorInfo != null)
                {
                    tickets = cinemaVisitorInfo.NumberOfPurchasedTickets + 1;
                }
                else
                {
                    cinemaVisitorInfo = new CinemaVisitorInfo();
                }

                cinemaVisitorInfo.NumberOfPurchasedTickets = tickets;


                var interaction = new Interaction(contact, InteractionInitiator.Contact, POCChanel, "");
                client.SetFacet<CinemaInfo>(interaction, CinemaInfo.DefaultFacetKey, new CinemaInfo() { Cinema = "St Katharines Dock" });
                client.SetFacet<CinemaVisitorInfo>(contact, CinemaVisitorInfo.DefaultFacetKey, cinemaVisitorInfo);
                interaction.Events.Add(new PurchasedCinemaTicket(DateTime.UtcNow, "GBP", 9.99m) { MovieName = movie.Name, EngagementValue = 500 } );
                client.AddInteraction(interaction);

                if(movie.horror > 1)
                {
                    interaction.Events.Add(new Goal(Guid.Parse("{025FB9C8-A7ED-4F51-B659-A7654C565FAE}"), DateTime.UtcNow)); // Watched Horror Movie
                }
                if(movie.sciFi > 0)
                {
                    interaction.Events.Add(new Goal(Guid.Parse("{02FCDF50-0675-44B6-A428-962EBF0C41B7}"), DateTime.UtcNow)); // Watched Sc Fi Movie
                }

    
                await client.SubmitAsync();

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Ticket for '" + movie.Name + "' purchased, press enter to continue");
                Console.ReadLine();
            }

        }


        private static async Task BuyPopcorn(XConnectClient client)
        {
            WriteHeader(AsciArt.Art.Popcorn, ConsoleColor.Yellow);

            var cinemaVisitorInfo = contact.GetFacet<CinemaVisitorInfo>();
            if (cinemaVisitorInfo == null)
            {
                cinemaVisitorInfo = new CinemaVisitorInfo() { FoodDiscountApplied = false };
            }
            else
            {
                cinemaVisitorInfo.FoodDiscountApplied = false;
            }

            var interaction = new Interaction(contact, InteractionInitiator.Contact, POCChanel, "");
            client.SetFacet<CinemaInfo>(interaction, CinemaInfo.DefaultFacetKey, new CinemaInfo() { Cinema = "St Katharines Dock" });
          //  client.SetFacet<CinemaVisitorInfo>(interaction, CinemaVisitorInfo.DefaultFacetKey, cinemaVisitorInfo);
            interaction.Events.Add(new Goal(Guid.Parse("{9CA11D99-39C5-45C0-A5AE-C0AFE28DCA68}"), DateTime.UtcNow)); // Recieved Food Discount Goal
            interaction.Events.Add(new PurchasedFood(DateTime.UtcNow, "GBP", 2.50m) {  FoodType = "Popcorn", EngagementValue = 200 });
            client.AddInteraction(interaction);
            await client.SubmitAsync();

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Popcorn Purchased, press enter to continue");
            Console.ReadLine();
        }

        private static async Task BuySweets(XConnectClient client)
        {
            WriteHeader(AsciArt.Art.Chocolate, ConsoleColor.Yellow);

            var cinemaVisitorInfo = contact.GetFacet<CinemaVisitorInfo>();
            if(cinemaVisitorInfo==null)
            {
                cinemaVisitorInfo = new CinemaVisitorInfo() { FoodDiscountApplied = false };
            }
            else
            {
                cinemaVisitorInfo.FoodDiscountApplied = false;
            }

            var interaction = new Interaction(contact, InteractionInitiator.Contact, POCChanel, "");
            client.SetFacet<CinemaInfo>(interaction, CinemaInfo.DefaultFacetKey, new CinemaInfo() { Cinema = "St Katharines Dock" });
         //   client.SetFacet<CinemaVisitorInfo>(interaction, CinemaVisitorInfo.DefaultFacetKey, cinemaVisitorInfo);
            interaction.Events.Add(new Goal(Guid.Parse("{9CA11D99-39C5-45C0-A5AE-C0AFE28DCA68}"), DateTime.UtcNow)); // Recieved Food Discount Goal
            interaction.Events.Add(new PurchasedFood(DateTime.UtcNow, "GBP", 2.50m) { FoodType = "Chocolate", EngagementValue = 200 });
            client.AddInteraction(interaction);
            await client.SubmitAsync();

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Chocolate Purchased, press enter to continue");
            Console.ReadLine();
        }

        private static async Task DeleteContact(XConnectClient client)
        {

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("#####################################################################");
            Console.WriteLine("#                                                                   #");
            Console.WriteLine("#                                                                   #");
            Console.WriteLine("#                    Execute Right to Be Forgotten                  #");
            Console.WriteLine("#                                                                   #");
            Console.WriteLine("#                                                                   #");
            Console.WriteLine("#####################################################################");

            client.ExecuteRightToBeForgotten(contact);
            await client.SubmitAsync();

            Console.WriteLine("Contact has been forgotten..... who are you anyway?");
            Console.ReadLine();


        }

        private static void WriteHeader(string[] text, System.ConsoleColor color)
        {
            System.Console.ForegroundColor = color;

            foreach (string line in text)
            {
                System.Console.WriteLine(line);
            }
            System.Console.WriteLine();
        }

        private static void WriteMessage(string text, System.ConsoleColor color)
        {
            var prevColour = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = prevColour;
        }

        private static void WriteMenu()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("#####################################################################");
            Console.WriteLine("#                    Sitecore Cinema Main Menu                      #");
            Console.WriteLine("#####################################################################");
            Console.WriteLine("#  Customer : " + customerEmail);
            Console.WriteLine("#####################################################################");
            Console.WriteLine("#                            Options                                #");
            Console.WriteLine("#     1: Pickup Reserved Ticket                                     #");
            Console.WriteLine("#     2: Buy New Ticket                                             #");
            Console.WriteLine("#     3: Buy Popcorn                                                #");
            Console.WriteLine("#     4: Buy Chocolate                                              #");
            Console.WriteLine("#     5: View History                                               #");
            Console.WriteLine("#                                                                   #");
            Console.WriteLine("#     8: Delete Customer                                            #");
            Console.WriteLine("#                                                                   #");
            Console.WriteLine("#     9: Exit                                                       #");
            Console.WriteLine("#    10: Restart                                                    #");
            Console.WriteLine("#####################################################################");
        }

        private static MovieData WriteMovieList()
        {
            Console.WriteLine("#     1: Star Wars: The Last Jedi                                   #");
            Console.WriteLine("#     2: Paddington 2                                               #");
            Console.WriteLine("#     3: Jigsaw                                                     #");
            Console.WriteLine("#     4: The Snowman                                                #");
            Console.WriteLine("#     5: Incedious: The Last Key                                    #");
            Console.WriteLine("#     6: Blade Runner 2149                                          #");
            Console.WriteLine("#     7: Justice League                                             #");
            Console.WriteLine("#     8: Thor: Ragnorock                                            #");
            Console.WriteLine("#####################################################################");
            Console.WriteLine("Please Select Movie:");
            var movie = Console.ReadLine();
            switch (movie)
            {
                case "1":
                    {
                        return new MovieData() { Name = "Star Wars: The Last Jedi",  sciFi=5};
                    }
                case "2":
                    {
                        return new MovieData() { Name = "Paddington 2", family = 5 };
                    }
                case "3":
                    {
                        return new MovieData() { Name = "Jigsaw", horror = 5 };
                    }
                case "4":
                    {
                        return new MovieData() { Name = "The Snowman", horror = 5 };
                    }
                case "5":
                    {
                        return new MovieData() { Name = "Incedious: The Last Key", horror = 5 };
                    }
                case "6":
                    {
                        return new MovieData() { Name = "Blade Runner 2149", sciFi = 5 };
                    }
                case "7":
                    {
                        return new MovieData() { Name = "Justice League", sciFi = 5 };
                    }
                case "8":
                    {
                        return new MovieData() { Name = "Thor: Rangorock", sciFi = 5 };
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        public class MovieData
        {
            public string Name { get; set; }
            public int horror { get; set; } = 0;
            public int family { get; set; } = 0;
            public int action { get; set; } = 0;
            public int comedy { get; set; } = 0;
            public int drama { get; set; } = 0;
            public int sciFi { get; set; } = 0;
        }
    }
}


