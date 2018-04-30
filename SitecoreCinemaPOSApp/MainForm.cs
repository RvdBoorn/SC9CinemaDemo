using MetroFramework.Forms;
using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Client.WebApi;
using Sitecore.XConnect.Collection.Model;
using Sitecore.XConnect.Schema;
using Sitecore.Xdb.Common.Web;
using SitecoreCinema.Models.Model.Collection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SitecoreCinemaPOSApp
{
	public partial class MainForm : MetroForm
	{
		private readonly string xdbUrl = ConfigurationManager.AppSettings["XconnectUrl"];
		private readonly Guid registrationGoal = Guid.Parse("{8FFB183B-DA1A-4C74-8F3A-9729E9FCFF6A}");
		private readonly Guid pocChanel = Guid.Parse("{B6C1EFF9-51E3-422F-8994-E9E72C50D7A1}");
		private readonly Guid purchasedFoodOutcome = Guid.Parse("{6DDAAC80-61B1-46C1-B433-70E09A56034F}");
		private readonly Guid purchasedTicketOutcome = Guid.Parse("{4C641625-C1E7-489C-8492-346AA95C41E2}");
		private readonly Guid reservedTicketOutcome = Guid.Parse("{7FC83103-8824-46AE-A7FE-914164D47D00}");

		private Contact contact;

		public MainForm()
		{
			InitializeComponent();
		}

		private void btnLookupContact_Click(object sender, EventArgs e)
		{
			this.RetrieveContact();
		}		

		private void btnChocolate_Click(object sender, EventArgs e)
		{
			bool result = AsyncHelpers.RunSync(() => this.BuySweets());
		}

		private void btnPopcorn_Click(object sender, EventArgs e)
		{
			bool result = AsyncHelpers.RunSync(() => this.BuyPopcorn());
		}

		private void btnGDPR_Click(object sender, EventArgs e)
		{
			bool result = AsyncHelpers.RunSync(() => this.DeleteContact());
		}
		
		private void btnTickets_Click(object sender, EventArgs e)
		{
			bool result = AsyncHelpers.RunSync(() => this.PickupTickets());
		}
		
		private async void Client_OperationCompleted(object sender, Sitecore.XConnect.Operations.XdbOperationEventArgs e)
		{
			// Stupid fix for xDB processing.
			// Executing it as an async task to not freeze the form itself while we wait for the result.
			await Task.Run(async () =>
			{
				await Task.Delay(1500);
				if (this.InvokeRequired)
				{
					this.Invoke(new MethodInvoker(this.RetrieveContact));
				}
				else
				{
					this.RetrieveContact();
				}
			});			
		}

		private void RetrieveContact()
		{
			this.contact = AsyncHelpers.RunSync(this.GetContact);

			if (this.contact != null)
			{
				this.lblFirstName.Text = this.contact.Personal().FirstName.Trim();
				this.lblLastName.Text = this.contact.Personal().LastName.Trim();
				this.lblEmail.Text = this.contact.Emails().PreferredEmail.SmtpAddress;
				this.lblPhone.Text =
					$"{this.contact.PhoneNumbers().PreferredPhoneNumber.CountryCode}{this.contact.PhoneNumbers().PreferredPhoneNumber.Number}";
				Tuple<int, int, DateTime?, List<string>, int, int, List<string>> expData = AsyncHelpers.RunSync(() => this.GetData());
				this.lblEngagementValue.Text = expData.Item2.ToString();
				this.lblReservedTickets.Text = expData.Item5.ToString();
				this.lblPurchasedTickets.Text = expData.Item6.ToString();
				this.lblInteractions.Text = expData.Item1.ToString();
				this.lblLastInteraction.Text = expData.Item3.HasValue ? $"{expData.Item3.Value.ToLongDateString()}\n{expData.Item3.Value.ToLongTimeString()}" : string.Empty;

				this.btnChocolate.Enabled = true;
				this.btnPopcorn.Enabled = true;
				this.btnGDPR.Enabled = true;
				this.btnTickets.Enabled = expData.Item7.Any();

				this.lvOutcomes.Clear();
				this.lvReserved.Clear();
				this.lvOutcomes.View = View.List;
				this.lvOutcomes.ForeColor = Color.Black;
				this.lvOutcomes.Items.AddRange(expData.Item4.Select(s => new ListViewItem(s) { ForeColor = Color.Black }).ToArray());
				this.lvOutcomes.Columns.Add(new ColumnHeader { Name = "column1", Text = "" });
				this.lvReserved.View = View.List;
				this.lvReserved.ForeColor = Color.Black;
				this.lvReserved.Items.AddRange(expData.Item7.Select(s => new ListViewItem(s) { ForeColor = Color.Black }).ToArray());
				this.lvReserved.Columns.Add(new ColumnHeader { Name = "column1", Text = "" });
			}
			else
			{
				this.lblFirstName.Text = string.Empty;
				this.lblLastName.Text = string.Empty;
				this.lblEmail.Text = string.Empty;
				this.lblPhone.Text = string.Empty;
				this.lblEngagementValue.Text = string.Empty;
				this.lblReservedTickets.Text = string.Empty;
				this.lblPurchasedTickets.Text = string.Empty;
				this.lblInteractions.Text = string.Empty;
				this.lblLastInteraction.Text = string.Empty;
				this.btnChocolate.Enabled = false;
				this.btnPopcorn.Enabled = false;
				this.btnTickets.Enabled = false;
				this.btnGDPR.Enabled = false;
				this.lvOutcomes.Clear();
				this.lvReserved.Clear();
			}
		}

		private async Task<Contact> GetContact()
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
			catch (XdbModelConflictException ex)
			{
				LogException(ex);
				return null;
			}

			using (var client = new XConnectClient(cfg))
			{
				try
				{

					if (this.txtBoxSearch.Text.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
					{
						return null;
					}
					else
					{
						var identifier = new IdentifiedContactReference("sitecoreextranet", this.txtBoxSearch.Text);
						contact = await client.GetAsync<Contact>(identifier, new ContactExpandOptions(PersonalInformation.DefaultFacetKey, CinemaVisitorInfo.DefaultFacetKey, EmailAddressList.DefaultFacetKey, PhoneNumberList.DefaultFacetKey)
						{
							Interactions = new RelatedInteractionsExpandOptions(new string[] { CinemaInfo.DefaultFacetKey })
							{
								StartDateTime = DateTime.Today.AddDays(-1000)
							}
						});

						if (contact == null)
						{
							MessageBox.Show("Customer not found.", "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
						}

						return contact;
					}
				}
				catch (XdbExecutionException ex)
				{
					LogException(ex);
				}
			}

			return null;
		}

		private async Task<Tuple<int, int, DateTime?, List<string>, int, int, List<string>>> GetData()
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
			catch (XdbModelConflictException ex)
			{
				LogException(ex);
				return null;
			}

			using (var client = new XConnectClient(cfg))
			{
				try
				{
					var presonalInfo = contact.GetFacet<PersonalInformation>();
					var cinemaInfo = contact.GetFacet<CinemaVisitorInfo>() ?? new CinemaVisitorInfo()
					{
						NumberOfReservedTickets = 0
					};

					var interactions = await client.Interactions.WithExpandOptions(new InteractionExpandOptions(new string[] { CinemaInfo.DefaultFacetKey })).Where(i => i.Contact.Id.Value == this.contact.Id.Value).ToList();
					var lastInteraction = interactions.OrderByDescending(i => i.StartDateTime).FirstOrDefault();
					var interactionsWithOutcomes = interactions.Where(x => x.Events.OfType<Outcome>().Any());
					var listOfOutcomes = interactionsWithOutcomes.Select(i => new Tuple<List<Outcome>, CinemaInfo>(i.Events.OfType<Outcome>().ToList(), i.GetFacet<CinemaInfo>(CinemaInfo.DefaultFacetKey)));
				
					var outcomes = new List<string>();
					foreach (var setOfOutcomes in listOfOutcomes)
					{
						foreach (var outcome in setOfOutcomes.Item1)
						{
							if (outcome.DefinitionId.Equals(this.purchasedFoodOutcome) && outcome.GetType() == typeof(PurchasedFood))
							{
								var e = outcome as PurchasedFood;

								outcomes.Add($"Purchased food: {e.FoodType} on {e.Timestamp.ToLongDateString()} for {e.CurrencyCode} {e.MonetaryValue.ToString("F2")} at {setOfOutcomes.Item2.Cinema}");
							}
							else if (outcome.DefinitionId.Equals(this.purchasedTicketOutcome) && outcome.GetType() == typeof(PurchasedCinemaTicket))
							{
								var e = outcome as PurchasedCinemaTicket;

								outcomes.Add($"Purchased ticket: {e.MovieName} on {e.Timestamp.ToLongDateString()} for {e.CurrencyCode} {e.MonetaryValue.ToString("F2")} at {setOfOutcomes.Item2.Cinema}");
							}
							else if (outcome.DefinitionId.Equals(this.reservedTicketOutcome) && outcome.GetType() == typeof(ReservedCinemaTicket))
							{
								var e = outcome as ReservedCinemaTicket;

								outcomes.Add($"Reserved ticket: {e.MovieName} on {e.Timestamp.ToLongDateString()} at {setOfOutcomes.Item2.Cinema}");
							}
						}
					}

					var reservedTickets = new List<string>();

					if (cinemaInfo.NumberOfReservedTickets > 0)
					{
						var allcinemainteraction = contact.Interactions.Where(i => i.GetFacet<CinemaInfo>(CinemaInfo.DefaultFacetKey) != null).OrderBy(d => d.EndDateTime);
						foreach (var inter in allcinemainteraction)
						{
							foreach (var evv in inter.Events)
							{
								if (evv.GetType() == typeof(ReservedCinemaTicket))
								{
									var interactionEvent = evv as ReservedCinemaTicket;
									reservedTickets.Add($"{interactionEvent.MovieName} on {interactionEvent.Timestamp.ToLongDateString()}");
								}
							}
						}
					}

					return new Tuple<int, int, DateTime?, List<string>, int, int, List<string>>(interactions.Count, interactions.Sum(i => i.EngagementValue), lastInteraction != null ? lastInteraction.StartDateTime : (DateTime?)null, outcomes, cinemaInfo.NumberOfReservedTickets, cinemaInfo.NumberOfPurchasedTickets, reservedTickets);
				}
				catch (XdbExecutionException ex)
				{
					LogException(ex);
				}
			}

			return null;
		}

		private async Task<bool> BuySweets()
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
			catch (XdbModelConflictException ex)
			{
				LogException(ex);
			}

			using (var client = new XConnectClient(cfg))
			{				
				try
				{
					var cinemaVisitorInfo = contact.GetFacet<CinemaVisitorInfo>();
					if (cinemaVisitorInfo == null)
					{
						cinemaVisitorInfo = new CinemaVisitorInfo() { FoodDiscountApplied = false };
					}
					else
					{
						cinemaVisitorInfo.FoodDiscountApplied = false;
					}

					var interaction = new Interaction(contact, InteractionInitiator.Contact, pocChanel, "");
					client.SetFacet<CinemaInfo>(interaction, CinemaInfo.DefaultFacetKey, new CinemaInfo() { Cinema = "St Katharines Dock" });
					interaction.Events.Add(new Goal(Guid.Parse("{9CA11D99-39C5-45C0-A5AE-C0AFE28DCA68}"), DateTime.UtcNow)); 
					interaction.Events.Add(new PurchasedFood(DateTime.UtcNow, "GBP", 2.50m) { FoodType = "Chocolate", EngagementValue = 200 });
					client.AddInteraction(interaction);

					client.OperationCompleted += Client_OperationCompleted;

					await client.SubmitAsync();

					return true;
				}
				catch (XdbExecutionException ex)
				{
					LogException(ex);
				}
			}

			return false;
		}

		private async Task<bool> BuyPopcorn()
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
			catch (XdbModelConflictException ex)
			{
				LogException(ex);
			}

			using (var client = new XConnectClient(cfg))
			{
				try
				{
					var cinemaVisitorInfo = contact.GetFacet<CinemaVisitorInfo>();
					if (cinemaVisitorInfo == null)
					{
						cinemaVisitorInfo = new CinemaVisitorInfo() { FoodDiscountApplied = false };
					}
					else
					{
						cinemaVisitorInfo.FoodDiscountApplied = false;
					}

					var interaction = new Interaction(contact, InteractionInitiator.Contact, pocChanel, "");
					client.SetFacet<CinemaInfo>(interaction, CinemaInfo.DefaultFacetKey, new CinemaInfo() { Cinema = "St Katharines Dock" });
					interaction.Events.Add(new Goal(Guid.Parse("{9CA11D99-39C5-45C0-A5AE-C0AFE28DCA68}"), DateTime.UtcNow));
					interaction.Events.Add(new PurchasedFood(DateTime.UtcNow, "GBP", 5.00m) { FoodType = "Popcorn", EngagementValue = 200 });
					client.AddInteraction(interaction);

					client.OperationCompleted += Client_OperationCompleted;

					await client.SubmitAsync();

					return true;
				}
				catch (XdbExecutionException ex)
				{
					LogException(ex);
				}
			}

			return false;
		}

		private async Task<bool> DeleteContact()
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
			catch (XdbModelConflictException ex)
			{
				LogException(ex);
			}

			using (var client = new XConnectClient(cfg))
			{
				try
				{
					client.ExecuteRightToBeForgotten(contact);

					client.OperationCompleted += Client_OperationCompleted;

					await client.SubmitAsync();

					return true;
				}
				catch (XdbExecutionException ex)
				{
					LogException(ex);
				}
			}

			return false;
		}

		private async Task<bool> PickupTickets()
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
			catch (XdbModelConflictException ex)
			{
				LogException(ex);
			}

			using (var client = new XConnectClient(cfg))
			{
				try
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

					string movieName = string.Empty;

					if (cinemaInfo.NumberOfReservedTickets > 0)
					{
						var allcinemainteraction = contact.Interactions.Where(i => i.GetFacet<CinemaInfo>(CinemaInfo.DefaultFacetKey) != null).OrderBy(d => d.EndDateTime);
						foreach (var inter in allcinemainteraction)
						{
							foreach (var evv in inter.Events)
							{
								if (evv.GetType() == typeof(ReservedCinemaTicket))
								{
									var interactionEvent = evv as ReservedCinemaTicket;
									movieName = interactionEvent.MovieName;
									break;
								}
							}
						}
					}

					if (cinemaInfo.NumberOfReservedTickets > 0)
					{
						int tickets = 1;
						int reservedTickets = 0;

						tickets = cinemaInfo.NumberOfPurchasedTickets + 1;
						reservedTickets = cinemaInfo.NumberOfReservedTickets - 1;

						cinemaInfo.NumberOfPurchasedTickets = tickets;
						cinemaInfo.NumberOfReservedTickets = reservedTickets;


						var interaction = new Interaction(contact, InteractionInitiator.Contact, pocChanel, "");
						client.SetFacet<CinemaInfo>(interaction, CinemaInfo.DefaultFacetKey, new CinemaInfo() { Cinema = "St Katharines Dock" });
						client.SetFacet<CinemaVisitorInfo>(contact, CinemaVisitorInfo.DefaultFacetKey, cinemaInfo);
						interaction.Events.Add(new PurchasedCinemaTicket(DateTime.UtcNow, "GBP", 9.99m) { MovieName = movieName, EngagementValue = 500 });

						if (movieName.Equals("It", StringComparison.OrdinalIgnoreCase) || movieName.Equals("Jigsaw", StringComparison.OrdinalIgnoreCase) || movieName.StartsWith("Insidious", StringComparison.OrdinalIgnoreCase))
						{
							interaction.Events.Add(new Goal(Guid.Parse("{025FB9C8-A7ED-4F51-B659-A7654C565FAE}"), DateTime.UtcNow)); // Watched Horror Movie
						}

						client.AddInteraction(interaction);

						client.OperationCompleted += Client_OperationCompleted;
						
						await client.SubmitAsync();
					}

					return true;
				}
				catch (XdbExecutionException ex)
				{
					LogException(ex);
				}
			}

			return false;
		}

		private void LogException(Exception ex)
		{
			if (ex == null)
			{
				return;
			}

			Log(string.Empty);
			Log(ex.Message);
			Log(ex.StackTrace);
			Log(ex.Source);

			var innerException = ex.InnerException;
			while (innerException != null)
			{
				Log(innerException.Message);
				Log(innerException.StackTrace);
				Log(innerException.Source);

				innerException = innerException.InnerException;
			}
		}

		private void Log(string message)
		{
			using (var sw = new StreamWriter($"{AppDomain.CurrentDomain.BaseDirectory}errors.txt", true))
			{
				sw.WriteLine($"[{DateTime.Now.ToShortDateString()}, {DateTime.Now.ToShortTimeString()}] {message}");
			}
		}
	}
}
