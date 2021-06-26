using PushOrders.CDSAPIService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace PushOrders
{
    public partial class frmTest : Form
    {
        private static readonly Random random = new Random(0);
        private static readonly List<string> forenames = new List<string>
                                                             {
                                                                 "John",
                                                                 "Peter",
                                                                 "Mary",
                                                                 "Jane",
                                                                 "James",
                                                                 "Robert",
                                                                 "Michael",
                                                                 "William",
                                                                 "David",
                                                                 "Said",
                                                                 "Ali",
                                                                 "Lamar",
                                                                 "Hala",
                                                                 "Milica",
                                                                 "Lucía",
                                                                 "Sofía",
                                                                 "Olivia",
                                                                 "Mary",
                                                                 "Anya",
                                                                 "Ruby",
                                                                 "Ferrari",
                                                                 "Popa"
                                                             };

        private static readonly List<string> surnames = new List<string>
                                                            {
                                                                "González",
                                                                "Rodríguez",
                                                                "Gómez",
                                                                "Flores",
                                                                "Brown",
                                                                "Lee",
                                                                "Wilson",
                                                                "Martin",
                                                                "Patel",
                                                                "Hernández",
                                                                "Wong",
                                                                "Anderson",
                                                                "Hodžić",
                                                                "Jensen",
                                                                "Müller",
                                                                "Bērziņš",
                                                                "De Jong",
                                                                "Kovačič",
                                                                "Popescu",
                                                                " De Vries",
                                                                "García",
                                                                "Brown"
                                                            };

        private static readonly List<string> taxTypes = new List<string>
                                                            {
                                                                "Excise",
                                                                "Tariff",
                                                                "Entertainment",
                                                                "Dangerous waste",
                                                                "Embossment",
                                                                "Stamp"
                                                            };

        //Enums for this?
        private static readonly List<string> decisions = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "99" };
        private static readonly List<string> natureTypes = new List<string> { "", "21", "31", "32", "91", "991", "999" };


        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static readonly string defaultCurrency = GetFromConfig("DefaultCurrency");

        private static string GetFromConfig(string configKey)
        {
            return ConfigurationManager.AppSettings[configKey];
        }

        private static readonly string postalOrganization = "12345";
        private static readonly string customsOrganization = "12";
        private static readonly string defaultCountry = "VN";
        public const string MAILITEM = "MI";

        private static readonly List<string> mailflow = new List<string> { "I", "O", };
        private static readonly List<string> mailclasses = new List<string> { "U", "C", "E" };
        private static readonly List<string> mailClassCodes = new List<string> { "LE", "CP", "EE" };

        private static readonly List<string> countries = new List<string>(GetFromConfig("partnerPostCountries").Split(new[] { ',' }));

        private static readonly List<string> currencies = new List<string>(GetFromConfig("partnerPostCurrencies").Split(new[] { ',' }));

        private static readonly List<string> posts = new List<string>(GetFromConfig("parnterPosts").Split(new[] { ',' }));


        public frmTest()
        {
            InitializeComponent();
        }

        private CDSAPIService.MailObject AddMailObject(DateTime datetime, string country, string post, string count,
                                          bool inbound, string mailclass, string mailclassCode)
        {
            string bound = (inbound) ? country : defaultCountry;

            return new MailObject
            {
                Id = string.Format("{2}{0}X{1}", count, bound, mailclassCode),
                PostingDt = datetime,
                OrigPostOrgCd = (inbound) ? post : postalOrganization,
                DestPostOrgCd = (inbound) ? postalOrganization : post,
                MailClassCd = mailclass,
                MailStateCd = 5, //send to customs
                TypeCd = MAILITEM
            };
        }

        private Declaration AddDeclarationItem(string country, string currency, bool inbound,
                                             string recipientfirstname,
                                             string senderfirstname, string recipientlastname, string senderlastname
                                            )
        {
            int postage = random.Next(-100, 300);

            string senderCountryCd = (inbound) ? country : defaultCountry;
            string receiptCountry = (inbound) ? customsOrganization : country;
            string senderCurrency = (inbound) ? currency : defaultCurrency;

            string natureType = natureTypes[new Random().Next(natureTypes.Count)];
            natureType = natureType.Length > 0 ? string.Format(" NTyp='{0}'", natureType) : "";

            int grossWgt = 0;
            var decl = new Declaration //fill declaration
            {
                PostOrganizationCd = postalOrganization,
                CustOrganizationCd = customsOrganization,
                Data = new Declaration.DeclarationData
                {
                    SenderLastName = senderlastname,
                    SenderFirstName = senderfirstname,
                    SenderAddressLine1 = GetRandomString(),
                    SenderCountryCd = senderCountryCd,
                    SenderZIP = GetRandomStringZip(),
                    SenderCity = GetRandomString(),
                    SenderState = GetRandomString(),
                    RecipientLastName = recipientlastname,
                    RecipientFirstName = recipientfirstname,
                    RecipientAddressLine1 = GetRandomString(),
                    RecipientCountryCd = receiptCountry,
                    RecipientZIP = GetRandomStringZip(),
                    RecipientCity = GetRandomString(),
                    RecipientState = GetRandomString(),

                    NatureTypeCd = natureType,
                    Postage = (postage > 0)
                   ? postage
                   : new decimal?(),
                    PostageCurrencyCd = (postage > 0)
                    ? senderCurrency
                    : "",

                    ContentPieces = GenerateContentPieces(senderCountryCd, senderCurrency, ref grossWgt)
                }
            };

            decl.Data.GrossWeight = grossWgt;

            return decl;
        }
        private int getSeedValue(int limit)
        {
            var currentSeed = 0;

            string xmlFragment = "data.xml";
            if (File.Exists(xmlFragment))
            {
                // Create an isntance of XmlTextReader and call Read method to read the file
                XmlTextReader textReader = new XmlTextReader(xmlFragment);
                // If the node has value

                while (textReader.Read())
                {
                    // Move to fist element
                    textReader.MoveToElement();
                    int.TryParse(textReader.ReadElementString(), out currentSeed);
                }

                textReader.Close();
                currentSeed += limit;
            }
            else
            {
                currentSeed = 0;
            }

            using (XmlWriter writer = XmlWriter.Create("data.xml"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("seed");
                writer.WriteValue(currentSeed);
                writer.Close();
            }

            return currentSeed;
        }

        public static string GetRandomString()
        {
            string path = Path.GetRandomFileName();
            path = path.Replace(".", ""); // Remove period.
            return path;
        }

        public static string GetRandomStringZip()
        {
            var stringChars = new char[6];

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = Chars[random.Next(Chars.Length)];
            }

            return new String(stringChars);


        }

        private Declaration.DeclarationData.ContentPiece[] GenerateContentPieces(string country, string currency, ref int grossWgt)
        {
            var pieces = new List<Declaration.DeclarationData.ContentPiece>();
            int index = new Random().Next(1, 4);

            int totalWgt = 0;

            for (int i = 1; i < index; i++)
            {
                int amount = random.Next(1, 300);
                var hscode = random.Next(100000, 999999).ToString(CultureInfo.InvariantCulture);
                int wgt = random.Next(1, 20);
                int number = random.Next(1, 20);

                pieces.Add(new Declaration.DeclarationData.ContentPiece		 	//content pieces
                {
                    Number = number,
                    Description = "item" + i,
                    Amount = amount,
                    CurrencyCd = currency,
                    NetWeight = wgt,
                    OrigCountryCd = country,
                    HS = hscode,
                });

                totalWgt = totalWgt + wgt;
            }
            grossWgt += totalWgt + random.Next(1, 100);

            return pieces.ToArray();
        }

        private void btnSendData_Click(object sender, EventArgs e)
        {
            string strUser = "vnpost_api";
            var token = new Guid(GetFromConfig("PostalGUID"));
            CDSAPIServiceClient client = new CDSAPIServiceClient("APIwsEndpointNoSec");

            var cdsViews = new List<CDSAPIService.CDSView>();
            var limit = 1;
            var seed = getSeedValue(limit);

            var max = seed + limit;

            for (int i = seed; i <= max; i++)
            {
                DateTime dateTime = DateTime.Now;

                string surname1 = surnames[random.Next(surnames.Count)];
                string forename1 = forenames[random.Next(forenames.Count)];
                string surname2 = surnames[random.Next(surnames.Count)];
                string forename2 = forenames[random.Next(forenames.Count)];

                string count = i.ToString(CultureInfo.InvariantCulture).PadLeft(8, '0');

                int index = random.Next(0, countries.Count);
                string partnerCountry = countries[index];
                string partnerCurrency = currencies[index];
                int mailClassIndex = random.Next(0, mailclasses.Count);
                string mailclass = mailclasses[mailClassIndex];
                string mailclassCode = mailClassCodes[mailClassIndex];
                string post = posts[index];

                bool inbound = new Random().Next(100) % 2 == 0;

                cdsViews.Add(new CDSView
                {
                    MailObject =
                        AddMailObject(dateTime, partnerCountry, post, count, inbound,
                                      mailclass, mailclassCode),
                    Declaration =
                        AddDeclarationItem(partnerCurrency, partnerCountry, inbound, forename1,
                                                           forename2, surname1, surname2)
                });

                client.CreateOrUpdateDeclarations(token, cdsViews.ToArray(), "vnpost_api");
            }
        }
    }
}
