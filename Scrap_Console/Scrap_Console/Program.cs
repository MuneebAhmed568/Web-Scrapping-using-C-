using System;
using IronXL;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System.Data;
using Microsoft.Data.SqlClient;

class Program
{
    public static List<int> errorRows = new List<int>();
    private static int _PagesCounter = 0;
    static void Main(string[] args)
    {
        // Set the path to the ChromeDriver executable
        //https://chromedriver.chromium.org   //Download from Here
        var chromeDriverPath = @"";

        // Initialize ChromeDriver
        var options = new ChromeOptions();
        options.AddArgument("no-sandbox");
        options.AddArgument("headless");    // for not opening instance of Chrome everytime
        options.AddArgument("--blink-settings=imagesEnabled=false");   // when loading a page in background 
        /* options.AddArgument("--headless");  // Run Chrome in headless mode (no GUI)*/
        

        
   
        string connectionString = "Server=localhost;Database=Demo;Trusted_Connection=False;TrustServerCertificate=True;User=sa;password=admin@123";
        SqlConnection connections = new SqlConnection(connectionString);
        List<string> mydict = new List<string>();





        //Part 1
        int counter = 0; 
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT column1 FROM Medicine ORDER BY column1";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string column1Value = reader.GetString(0); // Replace 1 with the actual column index
                        mydict.Add(reader.GetString(0)); 
                    }
                }
            }
            connection.Close();
        }

        foreach (string productName in mydict)
        {
            counter++;
            List<string[]> medicineList = new List<string[]>();
            medicineList = Searching(productName, chromeDriverPath, options, medicineList);
            foreach (string[] medDetail in medicineList)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string updateQuery = "Insert into NewMedicine (Name,MedicineName,MedPrice) Values (@value1,@value2,@value3)";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@value1", medDetail[0]);
                        command.Parameters.AddWithValue("@value2", medDetail[1]);
                        command.Parameters.AddWithValue("@value3", medDetail[2]);
                        int rowsAffected = command.ExecuteNonQuery();
                        //Console.WriteLine($"{key} Row Number");
                    }

                    connection.Close();


                }
            }
        }

        






        //Part 2
        Dictionary<int, string> mydict1 = new Dictionary<int, string>();
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            string query = "SELECT  Id, MedicineName from NewMedicine where  Id > 70803  and  (MedDescription is null or MedDescription = '' ) order by ID";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int column1Value = reader.GetInt32(0); // Replace 0 with the actual column index
                        string column2Value = reader.GetString(1); // Replace 1 with the actual column index
                        mydict1.Add(reader.GetInt32(0), reader.GetString(1));
                    }
                }
            }
            connection.Close();
        }

        foreach (var (key, value) in mydict1)
        {
            string Description = SpecificPageContent(value, chromeDriverPath, options);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string updateQuery = "UPDATE NewMedicine SET MedDescription = @value1 WHERE Id = @value2";

                using (SqlCommand command = new SqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@value1", Description);
                    command.Parameters.AddWithValue("@value2", key);
                    int rowsAffected = command.ExecuteNonQuery();
                    
                }
                connection.Close();
            }
        }

    }

    public static List<String[]> Searching(String SearchName,String chromeDriverPath ,ChromeOptions options, List<String[]> medicineList)
    {
        IWebDriver MainDriver = new ChromeDriver(chromeDriverPath, options);
        try
        {
            MainDriver.Navigate().GoToUrl("https://sehat.com.pk/search.php?search_query=" + SearchName);
            _PagesCounter = 0;
            medicineList = NextPageContent(MainDriver, medicineList, SearchName);
            MainDriver.Quit();
            return medicineList;
        }
        catch (Exception ex)
        {
            MainDriver.Quit();
            return medicineList;
        }
       
    }


    public static List<String[]> NextPageContent(IWebDriver MainDriver , List<String[]> medicineList , string SearchName)
    {
        
        try
        {
        IWebElement medicineElement = MainDriver.FindElement(By.ClassName("ProductList"));

        IReadOnlyCollection<IWebElement> medicineElementList = medicineElement.FindElements(By.TagName("li"));

        foreach (IWebElement specificElement in medicineElementList)
        {
            string name = specificElement.FindElement(By.ClassName("ProductName")).Text;
            IWebElement PriceDiv = specificElement.FindElement(By.ClassName("ProductDetails"));
            string price = PriceDiv.FindElement(By.ClassName("ProductPriceRating")).Text;


            medicineList.Add(new string[] { SearchName, name, price });
        }

        if(_PagesCounter < 6)
            {
                IWebElement productPagination = MainDriver.FindElement(By.ClassName("CategoryPagination"));

                IWebElement NextButtonDIv = productPagination.FindElement(By.ClassName("FloatRight"));

                if (NextButtonDIv.FindElement(By.TagName("a")).Displayed)
                {
                    _PagesCounter++;
                    IWebElement NextButton = NextButtonDIv.FindElement(By.TagName("a"));
                    NextButton.Click();
                    int milliseconds = 2000;
                    Thread.Sleep(milliseconds);
                    medicineList = NextPageContent(MainDriver, medicineList, SearchName);
                }
            }
        
        }
        catch(Exception ex)

        {
            Console.WriteLine("ERROR OCCURED" + ex.ToString());
        }
  
            return medicineList;
    }

    public static String SpecificPageContent(String ProductName, string chromeDriverPath, ChromeOptions options)
    {
        IWebDriver Maindriver = new ChromeDriver(chromeDriverPath, options);
        try
        {
            string stringWithoutSpaces = ProductName.Replace("%", "%25");
            stringWithoutSpaces = stringWithoutSpaces.Replace("-", "%252d");
            stringWithoutSpaces = stringWithoutSpaces.Replace(" ", "-");
            stringWithoutSpaces = stringWithoutSpaces.Replace("/", "%7B47%7D");
            string FinalURl = stringWithoutSpaces.Replace("'", "%27");
            string URl = "https://sehat.com.pk/products/" + FinalURl + ".html";
            Console.WriteLine("URL IS :" + URl);
            Maindriver.Navigate().GoToUrl(URl);
            int milliseconds = 2000;
            Thread.Sleep(milliseconds);
            IWebElement ProductDescriptionContainer = Maindriver.FindElement(By.ClassName("ProductDescriptionContainer"));
            string medicineDescriptionText = ProductDescriptionContainer.Text;
            medicineDescriptionText = medicineDescriptionText.Replace("'", " ");

            Maindriver.Quit();
            Console.WriteLine(medicineDescriptionText);
            return medicineDescriptionText;
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR OCCURED" + ex.ToString());
            Maindriver.Quit();
            return "";
        }


    }

}


