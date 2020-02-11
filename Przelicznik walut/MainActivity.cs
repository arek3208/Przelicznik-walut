using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Text.Method;
using Android.Widget;
using App2.Interface;
using App2.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Refit;

namespace Przelicznik_walut
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        INbpApi nbpApi;
        Spinner spinner1, spinner2;
        List<Currency> currencies;
        TextView textView;
        Button reverseButton, refreshButton;
        EditText editText;
        ArrayAdapter<Currency> adapter;
        ViewFlipper viewFlipper;
        RelativeLayout loadingLayout;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = { new StringEnumConverter() }
            };
            editText = FindViewById<EditText>(Resource.Id.editText1);
            textView = FindViewById<TextView>(Resource.Id.textView1);
            reverseButton = FindViewById<Button>(Resource.Id.reverse);
            refreshButton = FindViewById<Button>(Resource.Id.refresh);
            spinner1 = FindViewById<Spinner>(Resource.Id.spinner1);
            spinner2 = FindViewById<Spinner>(Resource.Id.spinner2);
            viewFlipper = FindViewById<ViewFlipper>(Resource.Id.viewFlipper);
            refreshButton.Click += RefreshButton_Click;
            loadingLayout = FindViewById<RelativeLayout>(Resource.Id.loadingLayout);
            loadingLayout.Visibility = Android.Views.ViewStates.Gone;

            if (savedInstanceState != null)
            {
                currencies = JsonConvert.DeserializeObject<List<Currency>>(savedInstanceState.GetString("currencies"));

                editText.Text = savedInstanceState.GetCharSequence("edittext");
                textView.Text = savedInstanceState.GetCharSequence("textview");

                adapter = new ArrayAdapter<Currency>(this, Android.Resource.Layout.SimpleSpinnerItem, currencies);
                spinner1.Adapter = spinner2.Adapter = adapter;
                spinner1.SetSelection(savedInstanceState.GetInt("spinner1"));
                spinner2.SetSelection(savedInstanceState.GetInt("spinner2"));
            }

            else await LoadData();
            AddHandlersOrRefresh();
        }

        private async void RefreshButton_Click(object sender, EventArgs e)
        {
            await LoadData();
            AddHandlersOrRefresh();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public async Task<List<Currency>> GetData()
        {
            nbpApi = RestService.For<INbpApi>(@"http://api.nbp.pl");
            try
            {
                List<ResponseItem> items = await nbpApi.GetRates();
                items[0].Rates.Add(new Currency("złoty polski", "PLN", 1));
                return items[0].Rates;
            }
            catch (Exception e)
            {
                Android.Util.Log.Error("api", e.Message);
                return new List<Currency>();
            }
        }

        private async Task LoadData()
        {
            loadingLayout.Visibility = Android.Views.ViewStates.Visible;
            currencies = await GetData();
            adapter = new ArrayAdapter<Currency>(this, Android.Resource.Layout.SimpleSpinnerItem, currencies);
            spinner1.Adapter = spinner2.Adapter = adapter;
            spinner2.SetSelection(spinner2.Count - 1);
            loadingLayout.Visibility = Android.Views.ViewStates.Gone;
        }

        private void Calculate<T>(object sender, T e)
        {
            var source = adapter.GetItem(spinner1.SelectedItemPosition);
            var target = adapter.GetItem(spinner2.SelectedItemPosition);
            double amount;
            if (editText.Text == "") amount = 1;
            else amount = double.Parse('0' + editText.Text);
            textView.Text = $"{amount} {source.Code} = {Math.Round(amount*source.Rate/target.Rate,4,MidpointRounding.AwayFromZero)} {target.Code}";
            
        }

        private void ReverseButtonClick(object sender, EventArgs e)
        {
            var tmp = spinner1.SelectedItemPosition;
            spinner1.SetSelection(spinner2.SelectedItemPosition);
            spinner2.SetSelection(tmp);
            Calculate(sender, e);
        }

        private void AllowOneSeparator(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            if(e.Editable.ToString().Contains(',')) editText.KeyListener = DigitsKeyListener.GetInstance("0123456789");
            else editText.KeyListener = DigitsKeyListener.GetInstance("0123456789,");
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString("currencies", JsonConvert.SerializeObject(currencies));
            outState.PutCharSequence("edittext", editText.Text);
            outState.PutCharSequence("textview", textView.Text);
            outState.PutInt("spinner1", spinner1.SelectedItemPosition);
            outState.PutInt("spinner2", spinner2.SelectedItemPosition);
            base.OnSaveInstanceState(outState);
        }

        private void AddHandlersOrRefresh()
        {
            if (currencies.Count > 0)
            {
                reverseButton.Click += new EventHandler(ReverseButtonClick);
                var spinnerHandler = new EventHandler<AdapterView.ItemSelectedEventArgs>(Calculate);
                editText.AfterTextChanged += new EventHandler<Android.Text.AfterTextChangedEventArgs>(Calculate);
                spinner1.ItemSelected += spinnerHandler;
                spinner2.ItemSelected += spinnerHandler;
                editText.AfterTextChanged += new EventHandler<Android.Text.AfterTextChangedEventArgs>(AllowOneSeparator);
                viewFlipper.DisplayedChild = 0;
            }
            else
            {
                viewFlipper.DisplayedChild = 1;
            }
        }
    }
}

