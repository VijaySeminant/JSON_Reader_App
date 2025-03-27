using JSON_Reader.ViewModel;

namespace JSON_Reader.View;

public partial class Settings : ContentPage
{
    public Settings(SettingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

    }

    async private void OnSaveSourceClicked(object sender, EventArgs e)
    {


        var buttonWidth = (uint)SaveSourceBtn.BorderWidth;
        await SaveSourceBtn.ScaleTo(2, buttonWidth + 20);
        await SaveSourceBtn.ScaleTo(1, buttonWidth);

        if (SettingsViewModel.GetSourceCount() == 0)
        {
            await DisplayAlert("Source is Empty", "Please add atleast one source.", "OK");

        }


    }

    async private void OnSaveFieldClicked(object sender, EventArgs e)
    {


        var buttonWidth = (uint)SaveFieldBtn.BorderWidth;
        await SaveFieldBtn.ScaleTo(2, buttonWidth + 20);
        await SaveFieldBtn.ScaleTo(1, buttonWidth);

        //if (SettingsViewModel.GetSourceCount() == 0)
        //{
        //    await DisplayAlert("Source is Empty", "Please add atleast one source.", "OK");

        //}


    }
    async private void OnAddSourceClicked(object sender, EventArgs e)
    {


        var buttonWidth = (uint)AddNewSourceBtn.BorderWidth;
        await AddNewSourceBtn.ScaleTo(2, buttonWidth + 20);
        await AddNewSourceBtn.ScaleTo(1, buttonWidth);



    }
}