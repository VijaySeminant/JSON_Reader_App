using CommunityToolkit.Maui.Storage;
using JSON_Reader.QuickType;
using Microsoft.Office.Interop.Excel;

namespace JSON_Reader
{
    public partial class MainPage : ContentPage
    {

        //private List<string> lead_json_field_lst = ["Name", "Email", "Phone"];


        //string[] lead_website_lst = new string[] { "linkedin", "facebook", "google" };

        List<string> lead_website_lst = new List<string>();
        List<string> lead_json_field_lst = new List<string>();

        List<FileInfo> processedFiles = new List<FileInfo>();
        List<FileInfo> not_processedFiles = new List<FileInfo>();
        List<Lead> lstLeads = new List<Lead>();

        //string ConfigFile = "PathConfig.txt";
        //string ConfigFile = "C:\\Users\\Seminant\\Desktop\\PathConfig.txt";

        string ConfigFile = "";
        string userBrowsedFolderPath = "";

        //DateTime date = DateTime.Now;
        private string dateString = "";

        private int jsonFilesCnt = 0;
        //private IEnumerable<FileInfo> jsonFiles=[];
        private List<FileInfo> jsonFiles = [];

        string newExcelFilename = "";

        private string[] arr_default_sources = ["Linkedin", "Google", "Facebook"];
        private string[] arr_default_field = ["Name","Email", "Phone", "Profile", "Website", "Location", "Source", "Address"];

        List<string> alphabet_lst = new List<string>();

        public MainPage()
        {
            InitializeComponent();
            SetAlphabetChar();
        }

        void ResetSource()
        {
            lead_website_lst.Clear();

            foreach (var src in arr_default_sources)
            {
                lead_website_lst.Add(src);
            }
        }

        void ResetField()
        {
            lead_json_field_lst.Clear();

            foreach (var ffield in arr_default_field)
            {
                lead_json_field_lst.Add(ffield);
            }
        }

        private void SetField()
        {

            lead_json_field_lst.Clear();
            var pref_field_items = Preferences.Default.Get("pref_fields", "");

            if (string.IsNullOrWhiteSpace(pref_field_items))
            {
                ResetField();

            }
            else
            {

                foreach (var ffield in pref_field_items.Split(","))
                {
                    if (ffield != null) lead_json_field_lst.Add(ffield);
                }

                //Console.WriteLine(lead_json_field_lst);
            }
        }

        private void SetSource()
        {

            lead_website_lst.Clear();
            var pref_source_items = Preferences.Default.Get("pref_sources", "");

            if (string.IsNullOrWhiteSpace(pref_source_items))
            {
                ResetSource();

            }
            else
            {

                foreach (var src in pref_source_items.Split(","))
                {
                    if (src != null) lead_website_lst.Add(src);
                }

                //Console.WriteLine(lead_website_lst);
            }
        }

        async private void OnBrowseClicked(object sender, EventArgs e)
        {
            processedFiles.Clear();
            not_processedFiles.Clear();
            lstLeads.Clear();

            SetSource();
            SetField();

            var buttonWidth = (uint)ExportBtn.BorderWidth;
            await BrowseBtn.ScaleTo(2, buttonWidth + 20);
            await BrowseBtn.ScaleTo(1, buttonWidth);

            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            PickFolder(token);
        }

        async private void OnExportClicked(object sender, EventArgs e)
        {

            var buttonWidth = (uint)ExportBtn.BorderWidth;
            await ExportBtn.ScaleTo(2, buttonWidth + 20);
            await ExportBtn.ScaleTo(1, buttonWidth);


            ExportBtn.IsEnabled = false;



            if (!string.IsNullOrEmpty(userBrowsedFolderPath))
            {

                PathEntry.Text = userBrowsedFolderPath;

                MainFunction();

                SemanticScreenReader.Announce("Completed reading and exported json files");

                string info = "Completed reading and exported \n";

                info = info + processedFiles.Count + " json files are Processed sucessfully \n";

                int notprocessedCount = not_processedFiles.Count;

                if (notprocessedCount > 0)
                {
                    info = info + notprocessedCount + " json files are NOT processed";
                }


                bool answer = await DisplayAlert("Completed", info, "Open Excel sheet", "Cancel");
                if (answer)
                {
                    if (!string.IsNullOrEmpty(newExcelFilename))
                    {
                        OpenExcelFileAsRead(newExcelFilename);
                    }
                }

            }
            else
            {
                await DisplayAlert("Path should not be empty", "Please select a json folder to Export", "OK");
            }


        }

        void Move_NonProcessed_JsonFileToFolder()
        {
            if (string.IsNullOrEmpty(dateString))
            {
                DateTime date = DateTime.Now;
                dateString = date.ToString("dd_MM_yyyy_HH_mm_ss");
            }
         

            string notprocessed_DirPath = userBrowsedFolderPath + "Not_Processed";

            DirectoryInfo NotProcessed_Dir = new DirectoryInfo(notprocessed_DirPath);

            try
            {
                if (!NotProcessed_Dir.Exists)
                {
                    Directory.CreateDirectory(notprocessed_DirPath);
                }

                foreach (var not_processedFile in not_processedFiles)
                {
                    string destnationFullPath = notprocessed_DirPath + "\\" + not_processedFile.Name;

                    FileInfo fileinfoo = new FileInfo(destnationFullPath);

                    if (fileinfoo.Exists)
                    {
                        File.Delete(destnationFullPath);
                    }

                    File.Move(not_processedFile.FullName, destnationFullPath);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }



        }

        void MoveJsonFileToCompletedFolder()
        {
            if (string.IsNullOrEmpty(dateString))
            {
                DateTime date = DateTime.Now;
                dateString = date.ToString("dd_MM_yyyy_HH_mm_ss");
            }

            string compDirPath = userBrowsedFolderPath + "completed_" + dateString;

            DirectoryInfo Processed_Dir = new DirectoryInfo(compDirPath);

            if (!Processed_Dir.Exists)
            {
                Directory.CreateDirectory(compDirPath);
            }

            foreach (var processedFile in processedFiles)
            {
                string destnationFullPath = compDirPath + "\\" + processedFile.Name;

                FileInfo fileinfoo = new FileInfo(destnationFullPath);

                if (fileinfoo.Exists)
                {
                    File.Delete(destnationFullPath);
                }

                File.Move(processedFile.FullName, destnationFullPath);



                //File.Move(processedFile.FullName, compDirPath + "\\" + processedFile.Name);
            }

        }

        bool IsLeadWebsite(string filename)
        {
            foreach (var leadWebsite in lead_website_lst)
            {
                if (filename.ToLower().Trim().Contains(leadWebsite.ToLower().Trim()))
                {
                    return true;
                }
            }
            return false;
        }

        bool IsCompletedorUnprocessedFolder(string filename)
        {
            //don't read files from the completed and unprocessed folder.
            if (filename.ToLower().Trim().Contains("completed") || filename.ToLower().Trim().Contains("not_processed"))
            {
                return false;
            }
            return true;
        }

        bool isNOTNullorEmpty(string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        bool ReadConfigPath()
        {

            try
            {
                if (File.Exists(ConfigFile))
                {
                    using (StreamReader strTxtRead = new StreamReader(ConfigFile))
                    {
                        var firstLine = strTxtRead.ReadLine();
                        if (!string.IsNullOrEmpty(firstLine))
                        {
                            userBrowsedFolderPath = firstLine.Trim();
                            if (!userBrowsedFolderPath.EndsWith("\\"))
                            {
                                userBrowsedFolderPath = userBrowsedFolderPath + "\\";
                            }
                        }
                        else
                        {
                            Console.WriteLine("Path not found in " + ConfigFile);
                            Console.ReadLine();
                        }
                    }
                }
                else
                {
                    Console.WriteLine("File name PathConfig.txt is not found in the Application directory.");
                    Console.ReadLine();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;

        }

        async Task PickFolder(CancellationToken cancellationToken)
        {
            var result = await FolderPicker.Default.PickAsync(cancellationToken);

            if (result.IsSuccessful)
            {
                //await Toast.Make($"The folder was picked: Name - {result.Folder.Name}, Path - {result.Folder.Path}", ToastDuration.Long).Show(cancellationToken);
                userBrowsedFolderPath = result.Folder.Path;
                if (!userBrowsedFolderPath.EndsWith("\\"))
                {
                    userBrowsedFolderPath = userBrowsedFolderPath + "\\";


                }

                PathEntry.Text = userBrowsedFolderPath;
                DirectoryInfo dir = new DirectoryInfo(userBrowsedFolderPath);

                if (dir.Exists)
                {

                    string[] AllJSONfiles = Directory.GetFiles(userBrowsedFolderPath, "*.json", SearchOption.AllDirectories);

                    var requiredFiles = AllJSONfiles.Where(ss => IsLeadWebsite(ss)).Where(kk => IsCompletedorUnprocessedFolder(kk));

                    jsonFiles = [];

                    foreach (string file in requiredFiles)
                    {
                        if (jsonFiles != null)
                        {
                            jsonFiles.Add(new FileInfo(file));

                        }
                    }

                    jsonFilesCnt = jsonFiles.Count();

                    LblJsonCount.Text = "Total JSON files found " + jsonFilesCnt.ToString();

                    if (jsonFilesCnt == 0)
                    {
                        ExportBtn.IsEnabled = false;
                        await DisplayAlert("No JSON files found", "Please select a folder contains json files", "OK");

                    }
                    else
                    {
                        ExportBtn.IsEnabled = true;
                    }


                }

            }
            else
            {
                //await Toast.Make($"The folder was not picked with error: {result.Exception.Message}").Show(cancellationToken);
                //Console.WriteLine();
            }
        }

        async void MainFunction()
        {
            //   Define the cancellation token.
            //   CancellationTokenSource source = new CancellationTokenSource();
            //   CancellationToken token = source.Token;
            //   PickFolder(token);

            foreach (FileInfo flInfo in jsonFiles)
            {
                try
                {

                    ReadJsonFile(flInfo.FullName, flInfo.CreationTime);
                    processedFiles.Add(flInfo);
                    //Console.WriteLine($"{processedFiles.Count()}");
                }
                catch (Newtonsoft.Json.JsonReaderException jsonEx)
                {
                    not_processedFiles.Add(flInfo);
                    Console.WriteLine(jsonEx);
                }
                catch (Exception generalEx)
                {
                    not_processedFiles.Add(flInfo);
                    Console.WriteLine(generalEx);
                }

            }

            if (processedFiles.Count > 0)
            {
                //WriteToNewCsv();

                WriteToNewExcel();

                MoveJsonFileToCompletedFolder();

            }

            if (not_processedFiles.Count > 0)
            {
                Move_NonProcessed_JsonFileToFolder();
            }



        }

        void WriteToNewCsv()
        {
            DateTime date = DateTime.Now;
            dateString = date.ToString("dd_MM_yyyy_HH_mm_ss");

            try
            {
                var file = userBrowsedFolderPath + dateString + ".csv";


                using (var stream = File.CreateText(file))
                {

                    //string csvHeaderRow = string.Format("{0},{1},{2},{3},{4},{5},{6}", "Name", "Profile", "Website", "Phone", "Email", "Location", "Source");

                    string csvHeaderRow = "";

                    Dictionary<string, string> dict_row =
                        new Dictionary<string, string>();
                    for (int i = 0; i < lead_json_field_lst.Count; i++)
                    {
                        csvHeaderRow = lead_json_field_lst[i] + "," + csvHeaderRow;
                        dict_row.Add(alphabet_lst[i], lead_json_field_lst[i]);
                    }





                    stream.WriteLine(csvHeaderRow.TrimEnd(','));

                    // Loop through your variables and write them to CSV file
                    foreach (var leadddd in lstLeads)
                    {
                        string csvRow = string.Format("{0},{1},{2},{3},{4},{5},{6}", leadddd.Name, leadddd.Profile, leadddd.Website, leadddd.Phone, leadddd.Email, leadddd.Location, leadddd.Source);

                        stream.WriteLine(csvRow);
                    }
                }

            }
            catch (Exception generalEx)
            {
                Console.WriteLine(generalEx);

            }

        }

        private void SetAlphabetChar()
        {
            for (int i = 'A'; i <= 'Z'; i++)
            {
                alphabet_lst.Add(string.Format("{0}", Convert.ToChar(i)));
            }
        }

        void WriteToNewExcel()
        {
            DateTime date = DateTime.Now;
            dateString = date.ToString("dd_MM_yyyy_HH_mm_ss");

            try
            {

                // Create a new instance of Excel application
                var excelApp = new Microsoft.Office.Interop.Excel.Application();

                // Open an existing workbook or create a new one
                var workbook = excelApp.Workbooks.Add();

                // Get the active worksheet
                var worksheets = workbook.Worksheets.Add();

                Microsoft.Office.Interop.Excel.Worksheet worksheet = (Worksheet)workbook.ActiveSheet;

                // Initialize row counter
                // Header
                int row = 1;
                //worksheet.Cells[row, "A"] = "Name";
                //worksheet.Cells[row, "B"] = "Profile";
                //worksheet.Cells[row, "C"] = "Website";
                //worksheet.Cells[row, "D"] = "Phone";
                //worksheet.Cells[row, "E"] = "Email";
                //worksheet.Cells[row, "F"] = "Location";
                //worksheet.Cells[row, "G"] = "Source";




                Dictionary<string, string> dict_row =
                    new Dictionary<string, string>();
                for (int i = 0; i < lead_json_field_lst.Count; i++)
                {
                    //worksheet.Cells[row, "A"] = "Name";
                    worksheet.Cells[row, alphabet_lst[i]] = lead_json_field_lst[i];
                    dict_row.Add(alphabet_lst[i], lead_json_field_lst[i]);
                }







                // ROW
                row = 2;

                // Loop through your variables and write them to Excel
                foreach (var leadddd in lstLeads)
                {
                    if (isNOTNullorEmpty(leadddd.Phone) || isNOTNullorEmpty(leadddd.Email))
                    {
                        foreach (var pair in dict_row)
                        {
                            switch (pair.Value)
                            {
                                case "Name":
                                    worksheet.Cells[row, pair.Key] = leadddd.Name;
                                    break;
                                case "Profile":
                                    worksheet.Cells[row, pair.Key] = leadddd.Profile;
                                    break;
                                case "Website":
                                    worksheet.Cells[row, pair.Key] = leadddd.Website;
                                    break;
                                case "Phone":
                                    worksheet.Cells[row, pair.Key] = "'" + leadddd.Phone;
                                    break;
                                case "Email":
                                    worksheet.Cells[row, pair.Key] = leadddd.Email;
                                    break;
                                case "Location":
                                    worksheet.Cells[row, pair.Key] = leadddd.Location;
                                    break;
                                case "Source":
                                    worksheet.Cells[row, pair.Key] = leadddd.Source;
                                    break;
                                case "Address":
                                    worksheet.Cells[row, pair.Key] = leadddd.Address;
                                    break;

                                default:
                                    worksheet.Cells[row, pair.Key] = "default";
                                    break;
                            }

                        }

                        //worksheet.Cells[row, "A"] = leadddd.Name;
                        //worksheet.Cells[row, "B"] = leadddd.Profile;
                        //worksheet.Cells[row, "C"] = leadddd.Website;
                        //worksheet.Cells[row, "D"] = "'" + leadddd.Phone;
                        //worksheet.Cells[row, "E"] = leadddd.Email;
                        //worksheet.Cells[row, "F"] = leadddd.Location;
                        //worksheet.Cells[row, "G"] = leadddd.Source;
                        row++;
                    }

                }

                newExcelFilename = userBrowsedFolderPath + dateString + ".xlsx";

                // Save the workbook
                workbook.SaveAs(newExcelFilename);

                // Close Excel and release resources
                workbook.Close(false);
                excelApp.Quit();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }



        }

        public void OpenExcelFileAsRead(string filePath)
        {
            Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(filePath, ReadOnly: true);
            xlApp.Visible = true;
        }

        void ReadJsonFile(string filePath, DateTime creationTime)
        {
            //LblStatus.Text = "Reading " + filePath;

           

            using (StreamReader strRead = new StreamReader(filePath))
            {

                string jsonString = strRead.ReadToEnd();

                // use below syntax to access JSON file
                var jsonFile = Lead.FromJson(jsonString);

                //var profileee = jsonFile.Profile;
                //var websitee = jsonFile.Website;
                //var namee = jsonFile.Name;
                //var phonee = jsonFile.Phone;
                //var emailee = jsonFile.Email;
                //var locationnn = jsonFile.Location;


                var leadd = new Lead();
                leadd.Name = jsonFile.Name;
                leadd.Profile = jsonFile.Profile;
                leadd.Website = jsonFile.Website;
                leadd.Phone = jsonFile.Phone;
                leadd.Email = jsonFile.Email;
                leadd.Location = jsonFile.Location;
                leadd.Source = jsonFile.Source;
                leadd.Address = jsonFile.Address;

                lstLeads.Add(leadd);

                //Console.WriteLine("=================================================================");

                //Console.WriteLine(namee);//Name
                //Console.WriteLine(profileee);//Profile
                //Console.WriteLine(websitee);//Website
                //Console.WriteLine(phonee);//Phone
                //Console.WriteLine(emailee);//Email
                //Console.WriteLine(locationnn);//Location


            }

        }

    }


    //reference https://app.quicktype.io/
    namespace QuickType
    {
        using System.Globalization;
        using Newtonsoft.Json;
        using Newtonsoft.Json.Converters;

        public partial class Lead
        {
            [JsonProperty("Profile")] public string Profile { get; set; }

            [JsonProperty("Website")] public string Website { get; set; }

            [JsonProperty("Name")] public string Name { get; set; }

            [JsonProperty("Phone")] public string Phone { get; set; }

            [JsonProperty("Email")] public string Email { get; set; }

            [JsonProperty("Location")] public string Location { get; set; }

            [JsonProperty("Source")] public string Source { get; set; }

            [JsonProperty("Address")] public string Address { get; set; }
        }

        public partial class Lead
        {
            public static Lead FromJson(string json) =>
                JsonConvert.DeserializeObject<Lead>(json, QuickType.Converter.Settings);
        }

        public static class Serialize
        {
            public static string ToJson(this Lead self) =>
                JsonConvert.SerializeObject(self, QuickType.Converter.Settings);
        }

        internal static class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters =
                {
                    new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
                },
            };
        }

    }

}
