using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using App2.Interface;
using App2.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Refit;

//BRAK SIECI

namespace Przelicznik_walut
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        INbpApi nbpApi;
        Spinner spinner1, spinner2;
        List<Currency> currencies;
        TextView textView;
        Button reverseButton;
        EditText editText;
        ArrayAdapter<Currency> adapter;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            editText = FindViewById<EditText>(Resource.Id.editText1);
            textView = FindViewById<TextView>(Resource.Id.textView1);
            reverseButton = FindViewById<Button>(Resource.Id.button1);
            spinner1 = FindViewById<Spinner>(Resource.Id.spinner1);
            spinner2 = FindViewById<Spinner>(Resource.Id.spinner2);

            if(savedInstanceState != null)
            {
                currencies = JsonConvert.DeserializeObject<List<Currency>>(savedInstanceState.GetString("currencies"));

                editText.Text = savedInstanceState.GetCharSequence("edittext");
                textView.Text = savedInstanceState.GetCharSequence("textview");

                adapter = new ArrayAdapter<Currency>(this, Android.Resource.Layout.SimpleSpinnerItem, currencies);
                spinner1.Adapter = spinner2.Adapter = adapter;
                spinner1.SetSelection(savedInstanceState.GetInt("spinner1"));
                spinner2.SetSelection(savedInstanceState.GetInt("spinner2"));
            }

            else
            {
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Converters = { new StringEnumConverter() }
                };

                nbpApi = RestService.For<INbpApi>(@"http://api.nbp.pl");
                currencies = await GetData();
                adapter = new ArrayAdapter<Currency>(this, Android.Resource.Layout.SimpleSpinnerItem, currencies);
                spinner1.Adapter = spinner2.Adapter = adapter;

                spinner2.SetSelection(spinner2.Count - 1);
            }

            editText.AfterTextChanged += new EventHandler<Android.Text.AfterTextChangedEventArgs>(AllowOneSeparator);

            reverseButton.Click += new EventHandler(ReverseButtonClick) + new EventHandler(Calculate);

            var spinnerHandler = new EventHandler<AdapterView.ItemSelectedEventArgs>(Calculate);
            editText.AfterTextChanged += new EventHandler<Android.Text.AfterTextChangedEventArgs>(Calculate);
            spinner1.ItemSelected += spinnerHandler;
            spinner2.ItemSelected += spinnerHandler;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public async Task<List<Currency>> GetData()
        {
            try
            {
                List<ResponseItem> items = await nbpApi.GetRates();
                items[0].rates.Add(new Currency("złoty polski", "PLN", 1));
                return items[0].rates;
            }
            catch (Exception e)
            {
                Toast.MakeText(this, Resource.String.connection_error, ToastLength.Long).Show();
                Android.Util.Log.Error("api", e.Message);
                return new List<Currency>();
            }
        }

        private void Calculate<T>(object sender, T e)
        {
            var source = adapter.GetItem(spinner1.SelectedItemPosition);
            var target = adapter.GetItem(spinner2.SelectedItemPosition);
            double amount;
            if (editText.Text == "") amount = 1;
            else amount = double.Parse('0' + editText.Text);
            textView.Text = $"{amount} {source.code} = {Math.Round(amount*source.rate/target.rate,4,MidpointRounding.AwayFromZero)} {target.code}";
            
        }

        private void ReverseButtonClick(object sender, EventArgs e)
        {
            var tmp = spinner1.SelectedItemPosition;
            spinner1.SetSelection(spinner2.SelectedItemPosition);
            spinner2.SetSelection(tmp);
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

    }
}

