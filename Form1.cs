using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Extensions.Configuration;
using Azure.AI.Language.QuestionAnswering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab1_AInML
{
    public partial class Form1 : Form
    {
        public TextAnalyticsClient CogSvcClient;

        public Hotel HotelGood = new Hotel
        {
            HotelName = "HotelGood",
            Reviews = new List<Review>
            {
                new Review { title = "Good hotel downtown", review = "The rooms were clean, very comfortable, and the staff was amazing. They went over and beyond to help make our stay enjoyable. I highly recommend this hotel for anyone visiting downtown", user = "HotelMaster123", published = "2022-05-23" },
                new Review { title = "Great Hotel, Exceptional service", review = "They were extremely accommodating and allowed us to check in early at like 10am. We got to hotel super early and I didn’t wanna wait. So this was a big plus. The sevice was exceptional as well. Would definitely send a friend there.", user = "AwesomeHotelStayer1979", published = "2022-9-27" },
                new Review { title = "Bon hôtel, bon emplacement", review = "J'ai profité de l'emplacement du centre-ville pour aller dîner à pied, visiter quelques galeries et prendre un verre. C'était super. Service au top comme toujours. Le confort du lit est imbattable.", user = "FrançaisSéjournantDansDesHôtels", published = "2022-7-26" },
                new Review{title = "Uno de los peores hoteles alrededor", review = "Uno de los peores hoteles en los que me he alojado, la comida era demasiado sabrosa, las piscinas para mojar y las habitaciones, mi señor, tan limpias, ¡FUEGO a esos limpiadores!", user = "Enojadohoteleroespañol", published = "2022-5-4"}
            }
        };

        public Hotel Hotelschlecht = new Hotel
        {
            HotelName = "Hotelschlecht",
            Reviews = new List<Review>
            {
                new Review{title = "Gutes Hotel, gute Erfahrung", review = "nsgesamt hatte ich eine tolle Erfahrung mit dem Hotel; Das Personal war unglaublich hilfsbereit und die Annehmlichkeiten waren großartig. Das Zimmer war wunderbar, sauber und perfekt, um ein Geburtstagswochenende zu feiern.", user = "VerstopfungDeutsch83", published = "2022-3-3"},
                new Review{title = "Horrible, stay away from this hotel!", review = "Everything ! The place is horrendous and disgusting… I’m surprised they even have a business .. nothing seems up to code for 95$. There was blood on the sheets!", user = "AngryEnglishmanNo1", published = "2022-5-2"},
                new Review{title = "Bon hôtel pour fumer du crack.", review = "La chambre était dégoûtante ! Très sale. On pouvait voir des empreintes de pas sur le sol où d'autres personnes marchaient ! On aurait dit qu'il n'avait pas été balayé depuis très longtemps ! Cela semblait être une bonne pièce pour fumer du crack.", user = "HautFrançais", published = "2021-12-12"},
            }
        };

        public Hotel HôtelMoche = new Hotel
        {
            HotelName = "HôtelMoche",
            Reviews = new List<Review>
            {
                new Review{title = "Worst experience ever.", review = "I didn’t like anything. This place was so disgusting. Bugs everywhere, horrible customer service, worst experience ever. DO NOT EVER WASTE YOUR MONEY", user = "DisappointedEnglishman", published = "2022-4-13"},
                new Review{title = "Loud drugdealers, cannot recommend.", review = "The drug dealer next door having cars pull up with loud music and banging on their door. The shower was broke and the drain was clogged. The side of the tub had cigarette burns and the toilet seat swung back and forth.", user = "AnnoyedEnglishman23", published = "2022-7-6"},
                new Review{title = "L'établissement manque de personnel, les chambres sont en mauvais état", review = "L'établissement manque de personnel, les chambres sont en mauvais état et tout est très sale. La première fois que j'ai vu qu'un motel avait besoin d'un agent de sécurité avec un gilet pare-balles sur la propriété. Tant d'invités ont eu tant de plaintes de cafards dans la chambre que la clé de leur chambre ne fonctionnait pas", user = "FrançaisQuiEstFrançais", published = "2022-4-28"},
            }
        };

        public List<Hotel> HotelList = new List<Hotel>();
        public List<Review> SortedReviews = new List<Review>();
        public Hotel hotel;

        QuestionAnsweringClient client;
        QuestionAnsweringProject project;

        public void ConfigureSettings()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();
            string cogSvcEndpoint = configuration["CognitiveServicesEndpoint"];
            string cogSvcKey = configuration["CognitiveServiceKey"];

            AzureKeyCredential credentials = new AzureKeyCredential(cogSvcKey);
            Uri endpoint = new Uri(cogSvcEndpoint);
            TextAnalyticsClient CogClient = new TextAnalyticsClient(endpoint, credentials);
            CogSvcClient = CogClient;

            Uri endpoint_2 = new Uri("https://ls1219.cognitiveservices.azure.com/");
            AzureKeyCredential credential = new AzureKeyCredential("2a4467cb691545788ed657d31988c493");
            string projectName = "LearnFAQ";
            string deploymentName = "production";

            client = new QuestionAnsweringClient(endpoint_2, credential);
            project = new QuestionAnsweringProject(projectName, deploymentName);

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;

            comboBox2.Items.Add("All");
            comboBox2.Items.Add("English");
            comboBox2.Items.Add("Spanish");
            comboBox2.Items.Add("French");
            comboBox2.Items.Add("German");

            comboBox3.Items.Add("All");
            comboBox3.Items.Add("Positive");
            comboBox3.Items.Add("Negative");
            comboBox3.Items.Add("Mixed");
        }

        public void GetHotelsAndReviews()
        {
            HotelList.Add(HotelGood);
            HotelList.Add(Hotelschlecht);
            HotelList.Add(HôtelMoche);

            foreach(var hotels in HotelList)
            {
                comboBox1.Items.Add(hotels.HotelName);
            }
        }

        public Form1()
        {
            InitializeComponent();
            ConfigureSettings();
            GetHotelsAndReviews();
            DetectReviewLanguagesAndRatings();
        }

        public void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                label2.Visible = true;
                timer1.Start();
            }
            else
            {
                string question = textBox1.Text;
                Response<AnswersResult> response = client.GetAnswers(question, project);

                foreach (KnowledgeBaseAnswer answer in response.Value.Answers)
                {
                    richTextBox1.AppendText($"{answer.Answer}\n\n");
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label2.Visible = false;
            timer1.Stop();
        }

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            hotel = HotelList.Where(h => h.HotelName.Contains($"{comboBox1.SelectedItem}")).FirstOrDefault();

            listBox1.Items.Clear();
            if (comboBox2.SelectedItem == null)
            {
                if(comboBox3.SelectedItem == null)
                {
                    foreach (var review in hotel.Reviews)
                    {
                        listBox1.Items.Add(review.title);
                    }
                }
                else
                {
                    foreach (var review in hotel.Reviews.Where(r => r.Rating == comboBox3.SelectedItem.ToString()))
                    {
                        listBox1.Items.Add(review.title);
                    }
                }   
            }
            else
            {
                if (comboBox3.SelectedItem == null)
                {
                    foreach (var review in hotel.Reviews.Where(r => r.Language == comboBox2.SelectedItem.ToString()))
                    {
                        listBox1.Items.Add(review.title);
                    }
                }
                else
                {
                    foreach (var review in hotel.Reviews.Where(r => r.Language == comboBox2.SelectedItem.ToString() && r.Rating == comboBox3.SelectedItem.ToString()))
                    {
                        listBox1.Items.Add(review.title);
                    }
                }   
            }
        }

        public void DetectReviewLanguagesAndRatings()
        {
            foreach(var hotel in HotelList)
            {
                foreach(var review in hotel.Reviews)
                {
                    var lang = CogSvcClient.DetectLanguage(review.review);
                    var rating = CogSvcClient.AnalyzeSentiment(review.review);
                    review.Language = lang.Value.Name;
                    review.Rating = $"{rating.Value.Sentiment}";
                }
            }


        }

        private void listBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            var Review = hotel.Reviews.Where(t => t.title.Contains($"{listBox1.SelectedItem}")).FirstOrDefault();
            var lang = CogSvcClient.DetectLanguage(Review.review);

            richTextBox2.Text = $"{Review.title}\n by {Review.user}\n {Review.published}\n\n {Review.review}\n\n Rating: {Review.Rating}";
        }

        private void comboBox2_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if(comboBox2.SelectedItem.ToString() == "All")
            {
                comboBox2.SelectedItem = null;
                listBox1.Items.Clear();

                if(comboBox1.SelectedItem == null) { }
                else
                {
                    if(comboBox3.SelectedItem == null)
                    {
                        foreach (var review in hotel.Reviews)
                        {
                            listBox1.Items.Add(review.title);
                        }
                    }
                    else
                    {
                        foreach (var review in hotel.Reviews.Where(r => r.Rating == comboBox3.SelectedItem.ToString()))
                        {
                            listBox1.Items.Add(review.title);
                        }
                    }
                } 
            }
            else
            {
                if (comboBox1.SelectedItem == null) { }
                else
                {
                    hotel = HotelList.Where(h => h.HotelName.Contains($"{comboBox1.SelectedItem}")).FirstOrDefault();
                    if (comboBox3.SelectedItem == null)
                    {
                        listBox1.Items.Clear();
                        foreach (var review in hotel.Reviews.Where(r => r.Language == comboBox2.SelectedItem.ToString()))
                        {
                            listBox1.Items.Add(review.title);
                        }
                    }
                    else
                    {
                        listBox1.Items.Clear();
                        foreach (var review in hotel.Reviews.Where(r => r.Language == comboBox2.SelectedItem.ToString() && r.Rating == comboBox3.SelectedItem.ToString()))
                        {
                            listBox1.Items.Add(review.title);
                        }
                    }
                }
            }  
        }

        private void comboBox3_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if(comboBox3.SelectedItem.ToString() == "All")
            {
                comboBox3.SelectedItem = null;
                listBox1.Items.Clear();

                if (comboBox1.SelectedItem == null) { }
                else
                {
                    if (comboBox2.SelectedItem == null)
                    {
                        foreach (var review in hotel.Reviews)
                        {
                            listBox1.Items.Add(review.title);
                        }
                    }
                    else
                    {
                        foreach (var review in hotel.Reviews.Where(r => r.Language == comboBox2.SelectedItem.ToString()))
                        {
                            listBox1.Items.Add(review.title);
                        }
                    }
                }
            }
            else
            {
                if (comboBox1.SelectedItem == null) { }
                else
                {
                    hotel = HotelList.Where(h => h.HotelName.Contains($"{comboBox1.SelectedItem}")).FirstOrDefault();
                    if (comboBox2.SelectedItem == null)
                    {
                        listBox1.Items.Clear();
                        foreach (var review in hotel.Reviews.Where(r => r.Rating == comboBox3.SelectedItem.ToString()))
                        {
                            listBox1.Items.Add(review.title);
                        }
                    }
                    else
                    {
                        listBox1.Items.Clear();
                        foreach (var review in hotel.Reviews.Where(r => r.Rating == comboBox3.SelectedItem.ToString() && r.Language == comboBox2.SelectedItem.ToString()))
                        {
                            listBox1.Items.Add(review.title);
                        }
                    }
                }
            } 
        }
    }
}
