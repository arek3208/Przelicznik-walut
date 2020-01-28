using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using App2.Interface;
using App2.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Refit;
//DODAĆ PLN!!!
namespace Przelicznik_walut
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        INbpApi nbpApi;
        Spinner spinner1;
        List<Currency> currencies;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            spinner1 = FindViewById<Spinner>(Resource.Id.spinner1);
            spinner1.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner1_ItemSelected);

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = { new StringEnumConverter() }
            };

            nbpApi = RestService.For<INbpApi>(@"http://api.nbp.pl");
            currencies = await GetData();

            List<string> currencyNames = new List<string>();
            foreach(var i in currencies)
            {
                currencyNames.Add(i.name);
            }
            spinner1.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, currencyNames);
             
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
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
                return items[0].rates;
            }
            catch (Exception e)
            {
                Toast.MakeText(this, Resource.String.connection_error, ToastLength.Long).Show();
                Android.Util.Log.Error("api", e.Message);
                return new List<Currency>();
            }
        }

        private void spinner1_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
        }

    }
}

