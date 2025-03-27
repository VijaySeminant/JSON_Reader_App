using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


namespace JSON_Reader.ViewModel
{
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty] static System.Collections.ObjectModel.ObservableCollection<string> source_item_collection;

        [ObservableProperty]
        System.Collections.ObjectModel.ObservableCollection<string> field_item_collection;


        private string[] arr_default_sources = ["Linkedin", "Google", "Facebook"];

        private string[] arr_default_fields = ["Name", "Email", "Phone", "Profile", "Website", "Location", "Address"];

        //private string[] mandatory_arr_fields = ["Name", "Email", "Phone", "Website"];

        public static int GetSourceCount()
        {
            int cntt = source_item_collection.Count;
            return cntt;
        }



        private void SetSource()
        {
            source_item_collection = new System.Collections.ObjectModel.ObservableCollection<string>();
            source_item_collection.Clear();
            var pref_source_items = Preferences.Default.Get("pref_sources", "");

            if (string.IsNullOrWhiteSpace(pref_source_items))
            {
                ResetSource();

            }
            else
            {

                foreach (var src in pref_source_items.Split(","))
                {
                    source_item_collection.Add(src);
                }
            }
        }

        private void SetField()
        {
            field_item_collection = new System.Collections.ObjectModel.ObservableCollection<string>();
            field_item_collection.Clear();

            var pref_field_items = Preferences.Default.Get("pref_fields", "");

            if (string.IsNullOrWhiteSpace(pref_field_items))
            {
                ResetField();
            }
            else
            {
                foreach (var ffld in pref_field_items.Split(","))
                {
                    field_item_collection.Add(ffld);
                }
            }
        }

        //CONSTRUCTOR
        public SettingsViewModel()
        {
            SetSource();
            SetField();
        }

        [ObservableProperty]
        string newsource;

        [RelayCommand]
        void AddSource()
        {
            if (string.IsNullOrWhiteSpace(Newsource))
                return;

            source_item_collection.Add(Newsource);
            Newsource = string.Empty;
        }

        [RelayCommand]
        void SaveSource()
        {
            //if (source_item_collection.Count == 0)
            //    return;

            //Preferences.Default.Clear();

            Preferences.Default.Set("pref_sources", "");

            string strSource = "";

            foreach (var sourceItem in source_item_collection)
            {
                strSource = sourceItem + "," + strSource;

            }
            Preferences.Default.Set("pref_sources", strSource.TrimEnd(','));
        }


        [RelayCommand]
        void SaveField()
        {
            //Preferences.Default.Clear();
            Preferences.Default.Set("pref_fields", "");

            string strField = "";

            foreach (var fieldItem in field_item_collection)
            {
                strField = fieldItem + "," + strField;

            }
            Preferences.Default.Set("pref_fields", strField.TrimEnd(','));
        }


        [RelayCommand]
        void ResetSource()
        {
            source_item_collection.Clear();

            foreach (var src in arr_default_sources)
            {
                source_item_collection.Add(src);
            }

            SaveSource();

            Newsource = string.Empty;
        }


        [RelayCommand]
        void ResetField()
        {
            field_item_collection.Clear();
            foreach (var ffld in arr_default_fields)
            {
                field_item_collection.Add(ffld);
            }


            //Newfield = string.Empty;
        }

        [RelayCommand]
        void DeleteSource(string source)
        {

            if (source_item_collection.Contains(source))
            {
                source_item_collection.Remove(source);
            }
        }

        [RelayCommand]
        async void DeleteField(string field)
        {

            //if(!mandatory_arr_fields.Contains(field))
            //{
            if (field_item_collection.Contains(field))
            {
                field_item_collection.Remove(field);
            }
            //}

        }










    }
}
